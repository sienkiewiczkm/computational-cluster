﻿using ComputationalCluster.CommunicationServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputationalCluster.CommunicationServer.Repositories
{
    public interface IPartialProblemsRepository
    {
        ulong Add(OrderedPartialProblem problem);

        ICollection<OrderedPartialProblem> GetFinishedProblem(ICollection<ProblemDefinition> problemDefinitions);

        void RemoveFinishedProblems(ulong problemId);
        OrderedPartialProblem Find(ulong problemid, ulong taskId);
        bool IsProblemComputed(ulong ProblemId);
    }
}
