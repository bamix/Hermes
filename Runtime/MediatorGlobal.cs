using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Hermes
{
    public class MediatorGlobal
    {
        private readonly ConcurrentDictionary<Type, EventSubscriptionCollection> eventSubscriptions = new();
        private readonly ConcurrentDictionary<Type, RequestHandler> requestHandlers = new();

        internal void Subscribe(EventSubscription subscription)
        {
            var collection = this.eventSubscriptions.GetOrAdd(subscription.EventType, _ => new EventSubscriptionCollection());
            collection.Add(subscription);
        }

        internal void Notify<TEvent>(Func<TEvent> payloadFunc) where TEvent : IEvent
        {
            if (this.eventSubscriptions.TryGetValue(typeof(TEvent), out var collection))
            {
                collection.Notify(payloadFunc());
            }
        }

        internal void Unsubscribe(EventSubscription subscription)
        {
            if (this.eventSubscriptions.TryGetValue(subscription.EventType, out var collection))
            {
                collection.Remove(subscription);
            }
        }

        internal void Unsubscribe(HandlerSubscription subscription)
        {
            var handler = this.requestHandlers.FirstOrDefault(h => h.Value.Id == subscription.Id);
            this.requestHandlers.Remove(handler.Key, out var _);
        }

        internal TResponse Send<TRequest, TResponse>(TRequest request) where TRequest : IRequest<TResponse>
        {
            if (this.requestHandlers.TryGetValue(typeof(TRequest), out var requestHandler))
            {
                return ((RequestHandler<TRequest, TResponse>)requestHandler).Handler(request);
            }

            throw new Exception($"Ho handler specified for {typeof(TRequest).Name}");
        }

        internal void Send<TRequest>(TRequest request) where TRequest : IRequest
        {
            if (this.requestHandlers.TryGetValue(typeof(TRequest), out var requestHandler))
            {
                ((RequestHandler<TRequest>)requestHandler).Handler(request);
                return;
            }

            throw new Exception($"Ho handler specified for {typeof(TRequest).Name}");
        }

        internal async UniTask<TResponse> SendAsync<TRequest, TResponse>(TRequest request) where TRequest : IAsyncRequest<TResponse>
        {
            if (this.requestHandlers.TryGetValue(typeof(TRequest), out var requestHandler))
            {
                return await ((AsyncRequestHandler<TRequest, TResponse>)requestHandler).Handler(request);
            }

            throw new Exception($"Ho handler specified for {typeof(TRequest).Name}");
        }

        internal async UniTask SendAsync<TRequest>(TRequest request) where TRequest : IAsyncRequest
        {
            if (this.requestHandlers.TryGetValue(typeof(TRequest), out var requestHandler))
            {
                await ((AsyncRequestHandler<TRequest>)requestHandler).Handler(request);
                return;
            }

            throw new Exception($"Ho handler specified for {typeof(TRequest).Name}");
        }

        internal HandlerSubscription Handle<TRequest, TResponse>(Func<TRequest, TResponse> handler) where TRequest : IRequest<TResponse>
        {
            var requestHandler = new RequestHandler<TRequest, TResponse>
            {
                Handler = handler
            };
            var result = this.requestHandlers.TryAdd(typeof(TRequest), requestHandler);

            if (!result)
            {
                throw new Exception($"Handler already exists for {typeof(TRequest).Name}");
            }

            return new HandlerSubscription(requestHandler.Id);
        }

        internal HandlerSubscription Handle<TRequest>(Action<TRequest> handler) where TRequest : IRequest
        {
            var requestHandler = new RequestHandler<TRequest>
            {
                Handler = handler
            };
            var result = this.requestHandlers.TryAdd(typeof(TRequest), requestHandler);

            if (!result)
            {
                throw new Exception($"Handler already exists for {typeof(TRequest).Name}");
            }

            return new HandlerSubscription(requestHandler.Id);
        }

        internal HandlerSubscription HandleAsync<TRequest, TResponse>(Func<TRequest, UniTask<TResponse>> handler) where TRequest : IAsyncRequest<TResponse>
        {
            var requestHandler = new AsyncRequestHandler<TRequest, TResponse>
            {
                Handler = handler
            };
            var result = this.requestHandlers.TryAdd(typeof(TRequest), requestHandler);

            if (!result)
            {
                throw new Exception($"Handler already exists for {typeof(TRequest).Name}");
            }

            return new HandlerSubscription(requestHandler.Id);
        }

        internal HandlerSubscription HandleAsync<TRequest>(Func<TRequest, UniTask> handler) where TRequest : IAsyncRequest
        {
            var requestHandler = new AsyncRequestHandler<TRequest>
            {
                Handler = handler
            };
            var result = this.requestHandlers.TryAdd(typeof(TRequest), requestHandler);

            if (!result)
            {
                throw new Exception($"Handler already exists for {typeof(TRequest).Name}");
            }

            return new HandlerSubscription(requestHandler.Id);
        }
    }

    internal class EventSubscriptionCollection
    {
        private readonly List<EventSubscription> subscriptions = new();

        public void Add(EventSubscription subscription)
        {
            this.subscriptions.Add(subscription);
        }

        public void Remove(EventSubscription subscription)
        {
            this.subscriptions.Remove(subscription);
        }

        public void Notify<TEvent>(TEvent payload)
        {
            for (var i = 0; i < this.subscriptions.Count; i++)
            {
                var eventSubscription = (EventSubscription<TEvent>)this.subscriptions[i];
                eventSubscription.Execute(payload);
                // In case the subscription was removed during the execution
                if (this.subscriptions.Count <= i || !ReferenceEquals(this.subscriptions[i], eventSubscription))
                {
                    i--;
                }
            }
        }
    }
}