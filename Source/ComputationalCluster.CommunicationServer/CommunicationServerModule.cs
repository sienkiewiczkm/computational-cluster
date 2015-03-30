﻿using Autofac;
using ComputationalCluster.Communication;
using ComputationalCluster.Communication.Messages;
using ComputationalCluster.CommunicationServer.Consumers;
using ComputationalCluster.CommunicationServer.Models;
using ComputationalCluster.CommunicationServer.Queueing;
using ComputationalCluster.CommunicationServer.Repositories;
using ComputationalCluster.Dependencies;
using ComputationalCluster.NetModule;
using ComputationalCluster.PluginManager;
using System.Data.Entity;

namespace ComputationalCluster.CommunicationServer
{
    public class CommunicationServerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CommunicationServerService>().AsSelf();

            builder.RegisterModule<CommonModule>();

            builder.RegisterType<RegisterConsumer>().As<IMessageConsumer<Register>>();
            builder.RegisterType<StatusConsumer>().As<IMessageConsumer<Status>>();
            builder.RegisterType<SolveRequestConsumer>().As<IMessageConsumer<SolveRequest>>();
            builder.RegisterType<SolutionsConsumer>().As<IMessageConsumer<Solutions>>();
            builder.RegisterType<SolvePartialProblemsConsumer>().As<IMessageConsumer<SolvePartialProblems>>();
            builder.RegisterType<SolutionRequestConsumer>().As<IMessageConsumer<SolutionRequest>>();

            //builder.RegisterType<ServerDbContext>().As<DbContext>().AsSelf().InstancePerDependency();
            //builder.RegisterType<ProblemsRepository>().As<RepositoryBase<Problem>>().As<IRepository<Problem>>().AsSelf();

            builder.RegisterType<ProblemsInMemoryRepository>().As<IProblemsRepository>().As<IQueuableTasksRepository<Problem>>().SingleInstance();
            builder.RegisterType<ComponentsInMemoryRepository>().As<IComponentsRepository>().SingleInstance();
            builder.RegisterType<ProblemDefinitionsInMemoryRepository>().As<IProblemDefinitionsRepository>().SingleInstance();
            builder.RegisterType<PartialProblemsInMemoryRepository>().As<IQueuableTasksRepository<OrderedPartialProblem>>().As<IPartialProblemsRepository>().SingleInstance();
            builder.RegisterType<TaskQueue<OrderedPartialProblem>>().AsSelf().SingleInstance();
            builder.RegisterType<TaskQueue<Problem>>().AsSelf().SingleInstance();

            builder.RegisterType<MessageReceiver>().AsImplementedInterfaces().AsSelf();
            builder.RegisterType<MessageTranslator>().AsImplementedInterfaces().AsSelf();

        }
    }
}