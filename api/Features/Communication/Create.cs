using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Api.Middleware;
using Npgsql;
using Dapper;
using System.Transactions;

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


            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                
                using (var tx = new TransactionScope())
                using (var conn = new NpgsqlConnection(settings.DbConnectionString))
                {
                    var existing = await conn.QueryFirstOrDefaultAsync<Middleware.Request>(
                        "SELECT * FROM request WHERE request_id = @requestId",
                        new { requestId = request.RequestId });
                    
                    if (existing != null)
                        throw new ErrorException("message_alread_processed");
                    
                    await conn.ExecuteAsync(
                        "INSERT INTO request (request_id, created_at) VALUES (@requestId, @createdAt)",
                        new Middleware.Request(request));


                    
                    var response = conn.QuerySingleAsync<Response>(
                        "INSERT INTO communication (customer_id, template_key, payload) " +
                        "VALUES (@customerId, @templateKey, @payload) RETURNING communication_id",
                        new { request.CustomerId, request.TemplateKey, request.Payload });
                        

                    



                        
                }




                return new Response { CommunicationId = Guid.NewGuid() };
            }
        }

    }
}