﻿using ComputationalCluster.Common;
using ComputationalCluster.Communication.Messages;
using ComputationalCluster.CommunicationServer.Models;
using ComputationalCluster.CommunicationServer.Repositories;
using ComputationalCluster.NetModule;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComputationalCluster.CommunicationServer.Consumers
{
    public class RegisterConsumer : IMessageConsumer<Register>
    {
        private readonly IComponentsRepository _componentsRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly ILog _log;

        public RegisterConsumer(IComponentsRepository componentsRepository, ITimeProvider timeProvider,
            ILog log)
        {
            _componentsRepository = componentsRepository;
            _timeProvider = timeProvider;
            _log = log;
        }

        public ICollection<IMessage> Consume(Register message, ConnectionInfo connectionInfo = null)
        {
            _log.InfoFormat("Consuming {0} = [{1}]", message.GetType().Name, message.ToString());

            var backupComponent = _componentsRepository.GetBackupServer() as BackupComponent;

            var response = new RegisterResponse()
            {
                Timeout = 30, // todo: config,
                BackupCommunicationServers = backupComponent != null ? new RegisterResponseBackupCommunicationServers
                        {
                            BackupCommunicationServer =
                                new RegisterResponseBackupCommunicationServersBackupCommunicationServer
                                {
                                    address = backupComponent.IpAddress.ToString(),
                                    port = (ushort)backupComponent.Port,
                                    portSpecified = true
                                }
                        } : null,
            };

            if (message.Type == RegisterType.CommunicationServer && backupComponent != null)
            {
                response.Id = 0;
                return new IMessage[] { response };
            }
            // Null object - dlaczego nie jest length=0 od razu?
            if (message.SolvableProblems == null)
            {
                message.SolvableProblems = new string[] { };
            }

            /* Backup feature
            if (message.DeregisterSpecified && message.Deregister)
            {
                if (!message.IdSpecified)
                {
                    _log.Error("Deregister requested without specified component Id.");
                    return null;
                }

                _componentsRepository.Deregister(message.Id);
                return null;
            }
            */

            if (message.SolvableProblems.Length == 0)
            {
                _log.Warn("Registering component with no solvable problems.");
            }
            Component component;
            if (message.Type == RegisterType.CommunicationServer)
            {
                component = new BackupComponent
                {
                    IpAddress = connectionInfo != null ? connectionInfo.IpAddress : null,
                    Port = connectionInfo != null ? connectionInfo.Port : 0,
                    Type = message.Type,
                    MaxThreads = message.ParallelThreads,
                    LastStatusTimestamp = _timeProvider.Now,
                };
            }
            else
            {
                component = new Component()
                {
                    LastStatusTimestamp = _timeProvider.Now,
                    Type = message.Type,
                    MaxThreads = message.ParallelThreads,
                    SolvableProblems = message.SolvableProblems.Select(t => new ProblemDefinition { Name = t }).ToList(),
                };
            }
            response.Id = _componentsRepository.Register(component);
            return new IMessage[] { response };
        }

        public ICollection<IMessage> Consume(IMessage message, ConnectionInfo connection = null)
        {
            var status = message as Register;
            if (status == null)
            {
                throw new NotSupportedException("RegisterConsumer consumes Register messages only.\n");
            }

            return Consume(status);
        }
    }
}
