using System;
using Cysharp.Threading.Tasks;

namespace Hermes
{
    internal class RequestHandler<TRequest, TResponse> : RequestHandler
    {
        public Func<TRequest, TResponse> Handler { get; set; }
    }

    internal class AsyncRequestHandler<TRequest, TResponse> : RequestHandler
    {
        public Func<TRequest, UniTask<TResponse>> Handler { get; set; }
    }

    internal class RequestHandler<TRequest> : RequestHandler
    {
        public Action<TRequest> Handler { get; set; }
    }

    internal class AsyncRequestHandler<TRequest> : RequestHandler
    {
        public Func<TRequest, UniTask> Handler { get; set; }
    }

    internal abstract class RequestHandler
    {
        internal Guid Id { get; } = Guid.NewGuid();
    }
}