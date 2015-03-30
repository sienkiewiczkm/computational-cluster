﻿using ComputationalCluster.CommunicationServer.Models;
using ComputationalCluster.CommunicationServer.Queueing;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace ComputationalCluster.CommunicationServer.Repositories
{
    public class PartialProblemsInMemoryRepository : IQueuableTasksRepository<OrderedPartialProblem>, IPartialProblemsRepository
    {
        private Dictionary<ulong, OrderedPartialProblem> _orderedPartialProblems;
        private ulong _nextValidId = 1;

        public PartialProblemsInMemoryRepository(IProblemDefinitionsRepository repository)
        {
            _orderedPartialProblems = new Dictionary<ulong, OrderedPartialProblem>();
        }

        public ulong Add(OrderedPartialProblem problem)
        {
            problem.Id = _nextValidId++;
            problem.RequestDate = DateTime.Now;
            _orderedPartialProblems.Add(problem.Id, problem);
            return problem.Id;
        }


        public ICollection<OrderedPartialProblem> GetFinishedProblem(ICollection<ProblemDefinition> problemDefinitions)
        {
            var orderedPartialProblems = _orderedPartialProblems.Select(orderedPartialProblem => orderedPartialProblem.Value);

            var res2 =
                orderedPartialProblems.Where(tmp => problemDefinitions.Contains(tmp.ProblemDefinition))
                    .OrderBy(tmp => tmp.RequestDate)
                    .GroupBy(tmp => tmp.TaskId)
                    .Select(g => g);

            IEnumerable<IGrouping<ulong, OrderedPartialProblem>> res3;
            res3 = res2.Where(a => a.All(x => x.Done));


            IGrouping<ulong, OrderedPartialProblem> first = res3.FirstOrDefault();
            return first == null? res3.FirstOrDefault().ToArray(): null;

        }

        public ICollection<IQueueableTask> GetQueuableTasks()
        {
            return _orderedPartialProblems.Values.ToArray();
        }

        public void DequeueTask(IQueueableTask task)
        {
            task.IsAwaiting = false;
        }

    }
}