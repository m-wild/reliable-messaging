using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Api.Middleware;

namespace Api.Features.Communication
{
    public class Create
    {
        public class Command : IRequest<Response>, IIdentifiableRequest
        {
            public Guid RequestId { get; set; }
            public Guid CustomerId { get; set; }
            public string TemplateKey { get; set; }
            public string Payload { get; set; }
        }

        public class Response 
        {
            public Guid CommunicationId { get; set; }
        }

        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly Settings settings;

            public Handler(IOptions<Settings> settings)
            {
                this.settings = settings.Value;
            }


            public Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {



                return Task.Run(() => new Response { CommunicationId = Guid.NewGuid() });
            }
        }

    }
}