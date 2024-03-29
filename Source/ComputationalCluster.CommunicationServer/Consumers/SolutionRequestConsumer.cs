﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using ComputationalCluster.Communication.Messages;
using ComputationalCluster.CommunicationServer.Repositories;
using ComputationalCluster.NetModule;
using log4net;

namespace ComputationalCluster.CommunicationServer.Consumers
{
    public class SolutionRequestConsumer : IMessageConsumer<SolutionRequest>
    {
        private readonly IProblemsRepository _problemsRepository;
        private ILog _log;

        public SolutionRequestConsumer(IProblemsRepository problemsRepository, ILog log)
        {
            _problemsRepository = problemsRepository;
            _log = log;
        }
        public ICollection<IMessage> Consume(SolutionRequest message, ConnectionInfo connectionInfo = null)
        {
            _log.InfoFormat("Consuming {0} = [{1}]", message.GetType().Name, message.ToString());
            var problemInstance = _problemsRepository.FindById((int)message.Id);
            if (problemInstance == null)
            {
                return new IMessage[] {new Error()
                {
                    ErrorType = ErrorErrorType.InvalidOperation,
                    ErrorMessage = "Cluster doesn't compute problem with Id="+message.Id.ToString(),
                }};
            }

            //ulong computationalTime = (ulong)(DateTime.Now - problemInstance.RequestDate).TotalMilliseconds;
            var solution = new SolutionsSolution
            {
                //ComputationsTime = computationalTime,
                //TimeoutOccured = problemInstance.Timeout == 0 ? false : (computationalTime > problemInstance.Timeout),
                ComputationsTime = problemInstance.ComputationsTime,
                TimeoutOccured = problemInstance.TimeoutOccured,
                TaskIdSpecified = false
            };

            if (problemInstance.IsDone)
            {
                solution.Type = SolutionsSolutionType.Final;
                solution.Data = problemInstance.OutputData;
            }
            else
            {
                solution.Type = SolutionsSolutionType.Ongoing;
            }

            var result = new Solutions
            {
                Id = message.Id,
                ProblemType = problemInstance.ProblemDefinition.Name,
                Solutions1 = new[] { solution }
            };

            return new[] { result };
        }

        public ICollection<IMessage> Consume(IMessage message, ConnectionInfo connectionInfo = null)
        {
            var solutionRequest = message as SolutionRequest;
            if (solutionRequest == null)
            {
                throw new NotSupportedException("SolutionRequestConsumer consumes SolutionRequest messages only.\n");
            }
            return Consume(solutionRequest);
        }
    }
}
