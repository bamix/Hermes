using System;
using Cysharp.Threading.Tasks;

namespace Hermes
{
    public interface IMediator : IDisposable
    {
        EventSubscription<TEvent> On<TEvent>(Action<TEvent> handler) where TEvent : IEvent;

        void Notify<TEvent>(TEvent payload) where TEvent : IEvent;

        void Notify<TEvent>(Func<TEvent> payload) where TEvent : IEvent;

        TResponse Send<TRequest, TResponse>(TRequest request = default) where TRequest : IRequest<TResponse>;

        void Send<TRequest>(TRequest request = default) where TRequest : IRequest;

        UniTask<TResponse> SendAsync<TRequest, TResponse>(TRequest request = default) where TRequest : IAsyncRequest<TResponse>;

        UniTask SendAsync<TRequest>(TRequest request = default) where TRequest : IAsyncRequest;

        void Handle<TRequest, TResponse>(Func<TRequest, TResponse> handler) where TRequest : IRequest<TResponse>;

        void HandleAsync<TRequest, TResponse>(Func<TRequest, UniTask<TResponse>> handler) where TRequest : IAsyncRequest<TResponse>;

        void Handle<TRequest>(Action<TRequest> handler) where TRequest : IRequest;

        void HandleAsync<TRequest>(Func<TRequest, UniTask> handler) where TRequest : IAsyncRequest;
    }
}