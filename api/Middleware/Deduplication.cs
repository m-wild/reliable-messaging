using System;
using MediatR;

namespace Api.Middleware
{
    public class Deduplication<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {


    }

    public interface IIdentifiableRequest
    {
        Guid RequestId { get; }
    }
}