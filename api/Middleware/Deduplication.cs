using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;

namespace Api.Middleware
{
    // public class Deduplication<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IIdentifiableRequest
    // {
    //     private readonly Settings settings;

    //     public Deduplication(IOptions<Settings> settings)
    //     {
    //         this.settings = settings.Value;
    //     }

    //     public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    //     {
            




    //         throw new NotImplementedException();
    //     }
    // }

    public interface IIdentifiableRequest
    {
        Guid RequestId { get; }
    }
}