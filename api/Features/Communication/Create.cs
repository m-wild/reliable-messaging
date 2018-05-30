using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Api.Middleware;
using Dapper;
using System.Transactions;
using Newtonsoft.Json;
using System.Text;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;

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
                Response response = null;

                using (var tx = new TransactionScope(
                    TransactionScopeOption.RequiresNew, 
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                    TransactionScopeAsyncFlowOption.Enabled))
                using (var conn = new SqlConnection(settings.DbConnectionString))
                {
                    // 1. Check if message has been processed already
                    var existing = await conn.QueryFirstOrDefaultAsync<Middleware.Request>(
                        "SELECT * FROM Api.Request WHERE RequestId = @RequestId",
                        new { request.RequestId });
                    
                    if (existing == null)
                    {
                        // 2. Save request
                        await conn.ExecuteAsync(
                            "INSERT INTO Api.Request (RequestId, CreatedAt) VALUES (@RequestId, @CreatedAt)",
                            new Middleware.Request(request));


                        // 3. Process request
                        response = await conn.QuerySingleAsync<Response>(
                            "INSERT INTO Api.Communication (CustomerId, TemplateKey, Payload) " +
                            "OUTPUT Inserted.CommunicationId " +
                            "VALUES (@CustomerId, @TemplateKey, @Payload)",
                            new { request.CustomerId, request.TemplateKey, request.Payload });
                            
                        // 4. Save events to DB
                        var createdEvent = new Create.Event
                        {
                            EventId = Guid.NewGuid(),
                            CommunicationId = response.CommunicationId,
                        };

                        var payload = JsonConvert.SerializeObject(createdEvent);
                        await conn.ExecuteAsync(
                            "INSERT INTO Api.Event (EventId, RequestId, [Key], Payload) VALUES (@EventId, @RequestId, @Key, @Payload)",
                            new { createdEvent.EventId, request.RequestId, createdEvent.Key, payload });
                    }

                    // 5. Commit transaction 1
                    tx.Complete();
                }

                // 6. Get all unsent events for the request
                IEnumerable<Middleware.Event> events;
                using (var conn = new SqlConnection(settings.DbConnectionString))
                    events = await conn.QueryAsync<Middleware.Event>(
                        "SELECT * FROM Api.Event WHERE RequestId = @RequestId AND SentAt IS NULL",
                        new { request.RequestId });

                if (events.Any())
                {
                    // 7. Send all unsent events
                    var factory = new ConnectionFactory() { HostName = settings.MqHostname };
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        foreach (var e in events)
                        {
                            var body = Encoding.UTF8.GetBytes(e.Payload);
                            channel.BasicPublish(
                                exchange: "amq.direct",
                                routingKey: e.Key,
                                body: body);

                            // 8. Mark each event as sent
                            using (var conn = new SqlConnection(settings.DbConnectionString))
                                await conn.ExecuteAsync(
                                    "UPDATE Api.Event SET SentAt = CURRENT_TIMESTAMP WHERE EventId = @EventId",
                                    new { e.EventId });
                        }
                    }
                }

                // 9. Done!
                return response;
            }
        }

        public class Event
        {
            public Guid EventId { get; set; }
            public Guid CommunicationId { get; set; }
            public string Key { get; } = "communication.created";
        }

    }
}