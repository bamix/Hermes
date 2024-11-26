using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Hermes
{
    // TODO remove handlers on dispose
    public class Mediator : IMediator
    {
        private readonly List<EventSubscription> subscriptions = new();
        private readonly MediatorGlobal global;

        public Mediator(MediatorGlobal global)
        {
            this.global = global;
        }

        public EventSubscription<TEvent> On<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            var subscription = new EventSubscription<TEvent>(handler, this);
            this.global.Subscribe(subscription);
            this.subscriptions.Add(subscription);
            return subscription;
        }

        internal void Off(EventSubscription subscription)
        {
            var isRemoved = this.subscriptions.Remove(subscription);
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
            this.global.Handle(handler);
        }

        public void HandleAsync<TRequest, TResponse>(Func<TRequest, UniTask<TResponse>> handler) where TRequest : IAsyncRequest<TResponse>
        {
            this.global.HandleAsync(handler);
        }

        public void Handle<TRequest>(Action<TRequest> handler) where TRequest : IRequest
        {
            this.global.Handle(handler);
        }

        public void HandleAsync<TRequest>(Func<TRequest, UniTask> handler) where TRequest : IAsyncRequest
        {
            this.global.HandleAsync(handler);
        }

        public void Dispose()
        {
            foreach (var subscription in this.subscriptions)
            {
                this.global.Unsubscribe(subscription);
            }
            this.subscriptions.Clear();
        }
    }
}