﻿using Autofac;
using ComputationalCluster.Communication.Messages;
using ComputationalCluster.CommunicationServer.Repositories;
using ComputationalCluster.NetModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UCCTaskSolver;

namespace ComputationalCluster.TaskManager
{
    public class TaskManagerRunner
    {
        private ITaskSolversRepository _taskSolversRepository;
        private INetClient _client;

        private LinkedList<SolvePartialProblems> _parialProblems;
        private Semaphore _semaphorePartialProblems;
        private LinkedList<Solutions> _finalSolutions;
        private Semaphore _semaphoreFinalSolutions;
        private ulong _id;

        public TaskManagerRunner()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<TaskManagerModule>();
            var container = builder.Build();

            _taskSolversRepository = container.Resolve<ITaskSolversRepository>();
            _client = container.Resolve<INetClient>();

            _parialProblems = new LinkedList<SolvePartialProblems>();
            _semaphorePartialProblems = new Semaphore(1, 1);
            _finalSolutions = new LinkedList<Solutions>();
            _semaphoreFinalSolutions = new Semaphore(1, 1);
        }

        public void Start()
        {
            var response = _client.Send(new Register()
            {
                Type = RegisterType.TaskManager
            }) as RegisterResponse;
            _id = response.Id;
            Console.WriteLine("Register response: ID={0}", _id);

            while (true)
            {
                System.Threading.Thread.Sleep(new TimeSpan(0, 0, (int)(response.Timeout / 2)));
                var receivedMessage = _client.Send(new Status()
                {
                    Id = _id,
                });

                Consume(receivedMessage);
                SendPartialProblems();
                SendSolution();
            }
        }

        public void Stop()
        {
        }

        public void Consume(IMessage receivedMessage)
        {
            Console.WriteLine("Received message: {0}", receivedMessage.GetType().Name);
            if (receivedMessage.GetType() == typeof(DivideProblem))
            {
                Console.WriteLine("Received a problem to divide: ID={0}", (receivedMessage as DivideProblem).Id);
                Thread thread = new Thread(new ParameterizedThreadStart(Divide));
                thread.Start(receivedMessage);
            }
            else if (receivedMessage.GetType() == typeof(Solutions))
            {
                Console.WriteLine("Received partial problems to merge: ID={0}", (receivedMessage as Solutions).Id);
                Thread thread = new Thread(new ParameterizedThreadStart(Merge));
                thread.Start(receivedMessage);
            }
        }

        public void Divide(object problem)
        {
            Type solverType = _taskSolversRepository.GetSolverType((problem as DivideProblem).ProblemType);
            TaskSolver solver = (TaskSolver)Activator.CreateInstance(solverType, Convert.FromBase64String((problem as DivideProblem).Data));
            byte[][] partialProblems = solver.DivideProblem((int)(problem as DivideProblem).ComputationalNodes);

            var partialProblemsMessage = new SolvePartialProblems()
            {
                ProblemType = (problem as DivideProblem).ProblemType,
                Id = (problem as DivideProblem).Id,
                PartialProblems = new SolvePartialProblemsPartialProblem[partialProblems.Length],
            };
            for (int i=0; i<partialProblems.Length; i++)
            {
                partialProblemsMessage.PartialProblems[i] = new SolvePartialProblemsPartialProblem()
                {
                    Data = Convert.ToBase64String(partialProblems[i]),
                    TaskId = (ulong)i,
                    NodeID = _id
                };
            }

            _semaphorePartialProblems.WaitOne();
            _parialProblems.AddLast(partialProblemsMessage);
            _semaphorePartialProblems.Release();
        }

        public void SendPartialProblems()
        {
            _semaphorePartialProblems.WaitOne();
            while (_parialProblems.Count != 0)
            {
                Console.WriteLine("Sending partial problems: ID={0}, number of partial problems={1}", _parialProblems.First.Value.Id, _parialProblems.First.Value.PartialProblems.Count());
                var response = _client.Send(_parialProblems.First.Value);
                _parialProblems.RemoveFirst();
                Consume(response);
            }
            _semaphorePartialProblems.Release();
        }

        public void Merge(object problems)
        {
            Type solverType = _taskSolversRepository.GetSolverType((problems as Solutions).ProblemType);
            TaskSolver solver = (TaskSolver)Activator.CreateInstance(solverType);
            
            byte[][] partialSolutions = new byte[(problems as Solutions).Solutions1.Length][];
            for (int i = 0; i < partialSolutions.Length; i++)
                partialSolutions[i] = Convert.FromBase64String((problems as Solutions).Solutions1[i].Data);
                
            byte[] solution = solver.MergeSolution(partialSolutions);

            var solutionMessage = new Solutions()
            {
                ProblemType = (problems as Solutions).ProblemType,
                Id = (problems as Solutions).Id,
                Solutions1 = new SolutionsSolution[]
                { 
                    new SolutionsSolution()
                    {
                        TaskIdSpecified = false,
                        Data = Convert.ToBase64String(solution),
                        Type = SolutionsSolutionType.Final
                    }
                }
            };

            _semaphoreFinalSolutions.WaitOne();
            _finalSolutions.AddLast(solutionMessage);
            _semaphoreFinalSolutions.Release();
        }

        public void SendSolution()
        {
            _semaphoreFinalSolutions.WaitOne();
            while (_finalSolutions.Count != 0)
            {
                Console.WriteLine("Sending final solution: ID={0}", _finalSolutions.First.Value.Id);
                var response = _client.Send(_finalSolutions.First.Value);
                _finalSolutions.RemoveFirst();
                Consume(response);
            }
            _semaphoreFinalSolutions.Release();
        }
    }
}