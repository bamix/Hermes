using System;

namespace Hermes
{
    public abstract class EventSubscription : IEquatable<EventSubscription>
    {
        private readonly Guid id = Guid.NewGuid();

        internal abstract Type EventType { get; }

        public abstract void Unsubscribe();

        public bool Equals(EventSubscription other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.id.Equals(other.id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((EventSubscription)obj);
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }
    }


    public class EventSubscription<TEvent> : EventSubscription
    {
        private readonly Action<TEvent> handler;

        private readonly Mediator mediator;

        internal EventSubscription(Action<TEvent> handler, Mediator mediator)
        {
            this.handler = handler;
            this.mediator = mediator;
        }

        internal override Type EventType => typeof(TEvent);

        public override void Unsubscribe()
        {
            this.mediator.Off(this);
        }

        internal void Execute(TEvent payload)
        {
            this.handler(payload);
        }
    }
}