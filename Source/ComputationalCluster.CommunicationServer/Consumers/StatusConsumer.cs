﻿using ComputationalCluster.Communication.Messages;
using ComputationalCluster.CommunicationServer.Models;
using ComputationalCluster.CommunicationServer.Queueing;
using ComputationalCluster.CommunicationServer.Repositories;
using ComputationalCluster.NetModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autofac.Core;
using log4net;
using ComputationalCluster.Common;
using ComputationalCluster.CommunicationServer.Backup;

namespace ComputationalCluster.CommunicationServer.Consumers
{
    public class StatusConsumer : IMessageConsumer<Status>
    {
        private readonly IComponentsRepository _componentsRepository;
        private TaskQueue<OrderedPartialProblem> _partialProblemsQueue;
        private TaskQueue<Problem> _problems;
        private IPartialProblemsRepository _partialProblemsRepository;
        private IProblemsRepository _problemsRepository;
        private ILog _log;
        private ITimeProvider _timeProvider;
        private ISynchronizationQueue _synchronizationQueue;

        public StatusConsumer(IComponentsRepository componentsRepository, TaskQueue<OrderedPartialProblem> partialProblemsQueue, TaskQueue<Problem> problemsQueue,
            IPartialProblemsRepository partialProblemsRepository,ISynchronizationQueue synchronizationQueue, IProblemsRepository problemsRepository, ILog log, ITimeProvider timeProvider)
        {
            _componentsRepository = componentsRepository;
            _partialProblemsQueue = partialProblemsQueue;
            _problems = problemsQueue;
            _partialProblemsRepository = partialProblemsRepository;
            _problemsRepository = problemsRepository;
            _log = log;
            _timeProvider = timeProvider;
            _synchronizationQueue = synchronizationQueue;
        }

        public ICollection<IMessage> Consume(Status message, ConnectionInfo connectionInfo = null)
        {
            if (_componentsRepository.GetById(message.Id) == null)
                return new IMessage[] { new Error() 
                {
                    ErrorType=ErrorErrorType.UnknownSender,
                    ErrorMessage = "Component is not registered."
                }};

            _componentsRepository.UpdateLastStatusTimestamp(message.Id);

            if (message.Threads == null)
            {
                message.Threads = new StatusThread[] { };
            }

            if (_componentsRepository.GetById(message.Id).Type != RegisterType.CommunicationServer)
            {
                _synchronizationQueue.Enqueue(message);
            }

            _problemsRepository.StopSolvingTimedOutProblems();
            switch (_componentsRepository.GetById(message.Id).Type)
            {
                case RegisterType.TaskManager:
                {
                    return ConsumeFromTaskManager(message);
                }
                case RegisterType.ComputationalNode:
                {
                    return ConsumeFromNode(message);
                }
                case RegisterType.CommunicationServer:
                {
                    return ConsumeFromBackup(message);
                }
                default:
                {
                    return new IMessage[] {PrepareNoOperationMessage()};
                }
            }


        }

        private ICollection<IMessage> ConsumeFromBackup(Status message)
        {
            var messageList = _synchronizationQueue.DequeueAll().ToList<IMessage>();
            messageList.Add(new NoOperation());
            return messageList;
        }

        private ICollection<IMessage> ConsumeFromNode(Status message)
        {
            int threadsCount = message.Threads.Count(t => t.State == StatusThreadState.Idle);
            List<OrderedPartialProblem> partialProblems = new List<OrderedPartialProblem>();
            var component = _componentsRepository.GetById(message.Id);
            OrderedPartialProblem partialProblem = _partialProblemsQueue.GetNextTask(component.SolvableProblems);

            while (partialProblem != null && partialProblem.Done == false && threadsCount > 0)
            {
                _log.DebugFormat("GetNextTask Id: {0}", partialProblem.Id);
                partialProblem.IsAwaiting = false;
                partialProblem.AssignedTo = component;
                partialProblems.Add(partialProblem);
                threadsCount--;
                partialProblem = _partialProblemsQueue.GetNextTask(new []{partialProblem.ProblemDefinition});
            }

            if (partialProblems.Count != 0)
            {
                var problem = _problemsRepository.FindById((int)partialProblems[0].Id);
                var partialProblemsMessage = new SolvePartialProblems()
                {
                    ProblemType = partialProblems[0].ProblemDefinition.Name,
                    Id = partialProblems[0].Id,
                    CommonData = partialProblems[0].CommonData,
                    SolvingTimeout = problem.Timeout - (ulong)(_timeProvider.Now-problem.RequestDate).TotalMilliseconds,
                    SolvingTimeoutSpecified = (problem.Timeout != 0),
                    PartialProblems = new SolvePartialProblemsPartialProblem[partialProblems.Count]
                };
                for (int i = 0; i < partialProblems.Count; i++)
                {
                    partialProblems[i].IsAwaiting = false;
                    partialProblemsMessage.PartialProblems[i] = new SolvePartialProblemsPartialProblem()
                    {
                        TaskId = partialProblems[i].TaskId,
                        Data = partialProblems[i].Data,
                        NodeID = partialProblems[i].NodeId
                    };
                }

                _synchronizationQueue.Enqueue(partialProblemsMessage);
                return new IMessage[] {partialProblemsMessage, PrepareNoOperationMessage()};
            }
            else
                return new IMessage[] {PrepareNoOperationMessage() };
        }

        public ICollection<IMessage> Consume(IMessage message, ConnectionInfo connectionInfo = null)
        {
            var status = message as Status;
            if (status == null)
            {
                throw new NotSupportedException("StatusConsumer consumes Status messages only.\n");
            }

            return Consume(status);
        }

        private ICollection<IMessage> ConsumeFromTaskManager(Status message)
        {
            int threadsCount = message.Threads.Count(t => t.State == StatusThreadState.Idle);

            if (threadsCount <= 0)
            {
                return new IMessage[] { PrepareNoOperationMessage() };
            }
            var mergeSolution = GetSolution(message.Id, threadsCount);
            if (mergeSolution != null)
            {
                _synchronizationQueue.Enqueue(mergeSolution);
                return new IMessage[] {mergeSolution, PrepareNoOperationMessage()};
            }

            var divideMessage = GetProblems(message.Id, threadsCount);
            if (divideMessage != null)
            {
                _synchronizationQueue.Enqueue(divideMessage);
                return new IMessage[] {divideMessage, PrepareNoOperationMessage()};
            }
            
            return new IMessage[] { PrepareNoOperationMessage() };

        }

        private DivideProblem GetProblems(ulong componentId, int thredCount)
        {
            if (thredCount <= 0) return null;
            var component = _componentsRepository.GetById(componentId);

            var problem = _problems.GetNextTask(component.SolvableProblems);
            if (problem != null)
            {
                problem.IsAwaiting = false;
                problem.AssignedTo = component;
                return new DivideProblem
                {
                    ProblemType = problem.ProblemDefinition.Name,
                    Id = problem.Id,
                    ComputationalNodes = problem.ProblemDefinition.AvailableComputationalNodes != 0 ? (ulong)problem.ProblemDefinition.AvailableComputationalNodes : 1,
                    NodeID = component.Id,
                    Data = problem.InputData
                };
            }
            return null;
        }

        //TODO: jakoś to trzeb obsłużyć w backup
        private Solutions GetSolution(ulong componentId, int threadCount)
        {
            if (threadCount <= 0) return null;
            var component = _componentsRepository.GetById(componentId);
            var partialSolutions = _partialProblemsRepository.GetFinishedProblem(component.SolvableProblems);

            if (partialSolutions != null && partialSolutions.Count > 0)
            {
                Solutions solution = new Solutions
                {
                    ProblemType = partialSolutions.ElementAt(0).ProblemDefinition.Name,
                    CommonData = partialSolutions.ElementAt(0).CommonData,
                    Id = partialSolutions.ElementAt(0).Id,
                };

                solution.Solutions1 = partialSolutions.Select(orderedPartialProblem =>
                {
                    //TODO: dokładnie chodzi o to
                    orderedPartialProblem.IsAwaiting = false;
                    orderedPartialProblem.AssignedTo = component;
                    return new SolutionsSolution
                    {
                        Data = orderedPartialProblem.Data,
                        TaskId = orderedPartialProblem.TaskId,
                        TaskIdSpecified = true,
                        Type = SolutionsSolutionType.Partial,
                        //TODO: timeout i computatonTime
                    };
                }).ToArray();

                return solution;
            }
            return null;
        }


        private IMessage PrepareNoOperationMessage()
        {
            var backup = _componentsRepository.GetBackupServer();
            if (backup == null)
            {
                return new NoOperation();
            }

            return new NoOperation
            {
                BackupCommunicationServers = new NoOperationBackupCommunicationServers
                {
                    BackupCommunicationServer = new NoOperationBackupCommunicationServersBackupCommunicationServer
                    {
                        address = ((BackupComponent) backup).IpAddress.ToString(),
                        port = (ushort) ((BackupComponent) backup).Port,
                        portSpecified = true
                    }
                }
            };
        }
    }
}
