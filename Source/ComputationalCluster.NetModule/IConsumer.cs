﻿using System.Collections.Generic;

namespace ComputationalCluster.NetModule
{
    public interface IMessageConsumer
    {
        ICollection<IMessage> Consume(IMessage message);
    }

    public interface IMessageConsumer<TMessage> : IMessageConsumer
        where TMessage : IMessage
    {
        ICollection<IMessage> Consume(TMessage message);
    }
}