using System;

namespace Hermes
{
    public class HandlerSubscription
    {
        internal HandlerSubscription(Guid id)
        {
            Id = id;
        }

        internal Guid Id { get; }
    }
}
