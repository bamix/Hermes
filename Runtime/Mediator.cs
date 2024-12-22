using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Hermes
{
    // TODO remove handlers on dispose
    public class Mediator : IMediator
    {
        private List<EventSubscription> subscriptions;
        private List<HandlerSubscription> handlerSubscriptions;
        private readonly MediatorGlobal global;

        private List<EventSubscription> Subscriptions => this.subscriptions ??= new List<EventSubscription>();
        private List<HandlerSubscription> HandlerSubscriptions => this.handlerSubscriptions ??= new List<HandlerSubscription>();
        public Mediator(MediatorGlobal global)
        {
            this.global = global;
        }

        public EventSubscription<TEvent> On<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            var subscription = new EventSubscription<TEvent>(handler, this);
            this.global.Subscribe(subscription);
            Subscriptions.Add(subscription);
            return subscription;
        }

        internal void Off(EventSubscription subscription)
        {
            var isRemoved = Subscriptions.Remove(subscription);
            if (isRemoved)
            {
                this.global.Unsubscribe(subscription);
            }
        }

        public void Notify<TEvent>(TEvent payload) where TEvent : IEvent
        {
            this.global.Notify(() => payload);
        }

        public void Notify<TEvent>(Func<TEvent> payload) where TEvent : IEvent
        {
            this.global.Notify(payload);
        }

        public TResponse Send<TRequest, TResponse>(TRequest request) where TRequest : IRequest<TResponse>
        {
            return this.global.Send<TRequest, TResponse>(request);
        }

        public void Send<TRequest>(TRequest request) where TRequest : IRequest
        {
            this.global.Send(request);
        }

        public UniTask<TResponse> SendAsync<TRequest, TResponse>(TRequest request) where TRequest : IAsyncRequest<TResponse>
        {
            return this.global.SendAsync<TRequest, TResponse>(request);
        }

        public UniTask SendAsync<TRequest>(TRequest request) where TRequest : IAsyncRequest
        {
            return this.global.SendAsync(request);
        }

        public UniTask<TResponse> RequestAsync<TRequest, TResponse>(TRequest request) where TRequest : IAsyncRequest<TResponse>
        {
            return this.global.SendAsync<TRequest, TResponse>(request);
        }

        public void Handle<TRequest, TResponse>(Func<TRequest, TResponse> handler) where TRequest : IRequest<TResponse>
        {
            HandlerSubscriptions.Add(this.global.Handle(handler));
        }

        public void HandleAsync<TRequest, TResponse>(Func<TRequest, UniTask<TResponse>> handler) where TRequest : IAsyncRequest<TResponse>
        {
            HandlerSubscriptions.Add(this.global.HandleAsync(handler));
        }

        public void Handle<TRequest>(Action<TRequest> handler) where TRequest : IRequest
        {
            HandlerSubscriptions.Add(this.global.Handle(handler));
        }

        public void HandleAsync<TRequest>(Func<TRequest, UniTask> handler) where TRequest : IAsyncRequest
        {
            HandlerSubscriptions.Add(this.global.HandleAsync(handler));
        }

        public void Dispose()
        {
            if (this.subscriptions != null)
            {
                foreach (var subscription in Subscriptions)
                {
                    this.global.Unsubscribe(subscription);
                }
                Subscriptions.Clear();
            }

            if (this.handlerSubscriptions != null)
            {
                foreach (var subscription in HandlerSubscriptions)
                {
                    this.global.Unsubscribe(subscription);
                }
                HandlerSubscriptions.Clear();
            }
        }
    }
}