﻿using ComputationalCluster.Communication.Messages;
using ComputationalCluster.CommunicationServer.Models;
using ComputationalCluster.CommunicationServer.Queueing;
using ComputationalCluster.CommunicationServer.Repositories;
using ComputationalCluster.NetModule;
using ComputationalCluster.TaskSolver.ArithmeticProgressionSum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ComputationalCluster.CommunicationServer.Consumers
{
    public class StatusConsumer : IMessageConsumer<Status>
    {
        private readonly IComponentsRepository _componentsRepository;
        private TaskQueue<OrderedPartialProblem> _partialProblemsQueue;
        private TaskQueue<Problem> _problems;
        private IPartialProblemsRepository _partialProblemsRepository;

        public StatusConsumer(IComponentsRepository componentsRepository, TaskQueue<OrderedPartialProblem> partialProblemsQueue, TaskQueue<Problem> problemsQueue,
            IPartialProblemsRepository partialProblemsRepository)
        {
            _componentsRepository = componentsRepository;
            _partialProblemsQueue = partialProblemsQueue;
            _problems = problemsQueue;
            _partialProblemsRepository = partialProblemsRepository;
        }

        public ICollection<IMessage> Consume(Status message)
        {
            #region test
            /* // przykład, żeby przetestować jak node'y odbierają podproblemy
            if (new Random().Next(2) == 1)
            {
                int firstMember = 2;
                int difference = 3;
                int amount = 10;
                int threadsCount = 3;

                using (var ms = new MemoryStream())
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(firstMember);
                    writer.Write(difference);
                    writer.Write(amount);

                    var solver = new ArithmeticProgressionSumSolver(ms.GetBuffer());
                    var partialProblems = solver.DivideProblem(threadsCount);
                    byte[][] partialSolution = new byte[threadsCount][];
                    for (int i = 0; i < threadsCount; i++)
                        partialSolution[i] = solver.Solve(partialProblems[i], TimeSpan.Zero);
                    var mergedSolution = solver.MergeSolution(partialSolution);
                    var finalSolution = BitConverter.ToInt32(mergedSolution, 0);

                    int expectedSum = (amount * (2 * firstMember + difference * (amount - 1))) / 2;

                    for (int i = 0; i < threadsCount; i++)
                        Console.WriteLine("expected result of partial solution {0}: {1}", i, Convert.ToBase64String(partialSolution[i]));

                    var sppPartialProblems = new SolvePartialProblemsPartialProblem[threadsCount];
                    for (int i=0; i<threadsCount; i++)
                        sppPartialProblems[i] = new SolvePartialProblemsPartialProblem()
                        {
                            TaskId = (ulong)i,
                            Data = Convert.ToBase64String(partialProblems[i])
                        };

                    var partialProblemsResponse = new SolvePartialProblems()
                    {
                        ProblemType = "Arithmetic progression sum",
                        Id = (ulong)(new Random().Next(1000)),
                        CommonData = Convert.ToBase64String(new UTF8Encoding().GetBytes("commondata")),
                        PartialProblems = sppPartialProblems
                    };
                    
                    _componentsRepository.UpdateLastStatusTimestamp(message.Id);
                    return partialProblemsResponse;
                }
            }
            else
            {
                var noOperationResponse = new NoOperation();
                _componentsRepository.UpdateLastStatusTimestamp(message.Id);
                return noOperationResponse;
            }
            */

            /* // przykład, żeby przetestować jak task manager dzieli na podproblemy
            if (new Random().Next(2) == 1)
            {
                int firstMember = 2;
                int difference = 3;
                int amount = 10;
                int threadsCount = 3;

                using (var ms = new MemoryStream())
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(firstMember);
                    writer.Write(difference);
                    writer.Write(amount);

                    var solver = new ArithmeticProgressionSumSolver(ms.GetBuffer());

                    var divideProblemResponse = new DivideProblem()
                    {
                        ProblemType = "Arithmetic progression sum",
                        Id = (ulong)(new Random().Next(1000)),
                        Data = Convert.ToBase64String(ms.GetBuffer()),
                        ComputationalNodes = (ulong)threadsCount
                    };
                    _componentsRepository.UpdateLastStatusTimestamp(message.Id);
                    return divideProblemResponse;
                }
            }
            else
            {
                var noOperationResponse = new NoOperation();
                _componentsRepository.UpdateLastStatusTimestamp(message.Id);
                return noOperationResponse;
            }
            */


            /*// przykład, testujący czy task manager poprawnie łączy rozwiązania częściowe
            if (new Random().Next(2) == 0)
            {
                int firstMember = 2;
                int difference = 3;
                int amount = 10;
                int threadsCount = 3;

                using (var ms = new MemoryStream())
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(firstMember);
                    writer.Write(difference);
                    writer.Write(amount);

                    var solver = new ArithmeticProgressionSumSolver(ms.GetBuffer());
                    var partialProblems = solver.DivideProblem(threadsCount);
                    byte[][] partialSolutions = new byte[threadsCount][];
                    for (int i = 0; i < threadsCount; i++)
                        partialSolutions[i] = solver.Solve(partialProblems[i], TimeSpan.Zero);
                    var mergedSolution = solver.MergeSolution(partialSolutions);
                    var finalSolution = BitConverter.ToInt32(mergedSolution, 0);
                    int expectedSum = (amount * (2 * firstMember + difference * (amount - 1))) / 2;

                    Console.WriteLine("expected solution: {0} {1}", finalSolution, expectedSum);

                    var partialSolutionsMessage = new Solutions()
                    {
                        ProblemType = "Arithmetic progression sum",
                        Id = (ulong)(new Random().Next(1000)),
                        Solutions1 = new SolutionsSolution[partialSolutions.Length]
                    };
                    for (int i = 0; i < partialSolutions.Length; i++)
                        partialSolutionsMessage.Solutions1[i] = new SolutionsSolution()
                        {
                            Data = Convert.ToBase64String(partialSolutions[i]),
                            TaskId = (ulong)i,
                            TaskIdSpecified = true,
                            Type = SolutionsSolutionType.Partial
                        };
                    _componentsRepository.UpdateLastStatusTimestamp(message.Id);
                    return partialSolutionsMessage;
                }
            }
            else
            {
                var noOperationResponse = new NoOperation();
                _componentsRepository.UpdateLastStatusTimestamp(message.Id);
                return noOperationResponse;
            }
            */
            #endregion

            switch (_componentsRepository.GetById(message.Id).Type)
            {
                case RegisterType.TaskManager:
                    return ConsumeFromTaskManager(message);
                //case RegisterType.ComputationalNode:
                   //TODO: przenieść!!
                  ///  break;
            }

            if (_componentsRepository.GetById(message.Id).Type == RegisterType.ComputationalNode)
            {
                int threadsCount = 0;
                for (int i = 0; i < message.Threads.Length; i++)
                    if (message.Threads[i].State == StatusThreadState.Idle)
                        threadsCount++;

                List<OrderedPartialProblem> partialProblems = new List<OrderedPartialProblem>();
                OrderedPartialProblem problem = _partialProblemsQueue.GetNextTask(_componentsRepository.GetById(message.Id).SolvableProblems);
                while (problem != null && threadsCount > 0)
                {
                    partialProblems.Add(problem);
                    threadsCount--;
                    problem = _partialProblemsQueue.GetNextTask(new ProblemDefinition[] { problem.ProblemDefinition });
                }

                var noOperationResponse = new NoOperation();
                _componentsRepository.UpdateLastStatusTimestamp(message.Id);

                if (partialProblems.Count != 0)
                {
                    var partialProblemsMessage = new SolvePartialProblems()
                    {
                        ProblemType = partialProblems[0].ProblemDefinition.Name,
                        Id = partialProblems[0].Id,
                        CommonData = partialProblems[0].CommonData,
                        SolvingTimeout = partialProblems[0].Timeout,
                        SolvingTimeoutSpecified = (partialProblems[0].Timeout != 0),
                        PartialProblems = new SolvePartialProblemsPartialProblem[partialProblems.Count]
                    };
                    for (int i = 0; i < partialProblems.Count; i++)
                    {
                        partialProblemsMessage.PartialProblems[i] = new SolvePartialProblemsPartialProblem()
                        {
                            TaskId = partialProblems[i].TaskId,
                            Data = partialProblems[i].Data,
                            NodeID = partialProblems[i].NodeId
                        };
                    }

                    return new IMessage[] { noOperationResponse, partialProblemsMessage };
                }
                else
                    return new IMessage[] { noOperationResponse };
            }
            else
            {
                var noOperationResponse = new NoOperation();
                _componentsRepository.UpdateLastStatusTimestamp(message.Id);
                return new IMessage[] { noOperationResponse };
            }
        }

        public ICollection<IMessage> Consume(IMessage message)
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
                return new IMessage[] {new NoOperation() };
            }
            var mergeSolution = GetSolution(message.Id, threadsCount);
            if (mergeSolution != null)
            {
                return new IMessage[] {mergeSolution, new NoOperation()};
            }

            var divideMessage = GetProblems(message.Id, threadsCount);
            if (divideMessage != null)
            {
                return new IMessage[] {divideMessage, new NoOperation()};
            }

            return new IMessage[] { new NoOperation() };

        }

        private DivideProblem GetProblems(ulong componentId, int thredCount)
        {
            if (thredCount <= 0) return null;
            var component = _componentsRepository.GetById(componentId);

            var problem = _problems.GetNextTask(component.SolvableProblems);
            if (problem != null)
            {
                return new DivideProblem
                {
                    ProblemType = problem.ProblemDefinition.Name,
                    Id = problem.Id,
                    ComputationalNodes = (ulong) problem.ProblemDefinition.AvailableComputationalNodes,
                    NodeID = component.Id,
                };
            }
            return null;
        }

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
                    Id = partialSolutions.ElementAt(0).TaskId,
                };

                var solutionsList = new List<SolutionsSolution>();
                foreach (var orderedPartialProblem in partialSolutions)
                {
                    solutionsList.Add(new SolutionsSolution
                    {
                        Data = orderedPartialProblem.Data,
                        TaskId = orderedPartialProblem.TaskId,
                        TaskIdSpecified = true,
                        Type = SolutionsSolutionType.Partial,
                        //TODO: timeout i computatonTime
                    });
                }
                solution.Solutions1 = solutionsList.ToArray();

                return solution;
            }
            return null;
        }

    }
}
