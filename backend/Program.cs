using System;
using System.Text;
using System.Transactions;
using Dapper;
using Newtonsoft.Json;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Backend
{
    public class Settings
    {
        public string DbConnectionString { get; set; } = "Host=localhost; Database=reliableapi; Enlist=true;";
        public string MqHostname { get; set; } = "localhost";
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var p = new Program();
            try
            {
                p.Run();
                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
            }
            finally
            {
                p.Dispose();
            }
        }

        private readonly Settings settings = new Settings();

        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;

        public void Run()
        {
            var factory = new ConnectionFactory() { HostName = settings.MqHostname };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            consumer = new EventingBasicConsumer(channel);
            consumer.Received += Handle;

            channel.BasicConsume(
                queue: "communication.created",
                autoAck: false,
                consumer: consumer);
        }

        public void Handle(object sender, BasicDeliverEventArgs ea) 
        {
            // 1. parse the message
            var body = Encoding.UTF8.GetString(ea.Body);
            var createdEvent = JsonConvert.DeserializeObject<CommunicationCreatedEvent>(body);

            // 2. check if we have processed this message already
            BasicEvent existing = null;
            using (var conn = new NpgsqlConnection(settings.DbConnectionString))
            {
                existing = conn.QueryFirstOrDefault(
                    "SELECT * FROM Backend.Processed WHERE EventId = @EventId",
                    new { createdEvent.EventId });
            }

            // 3. process the event only if we havent processed it before
            if (existing == null)
            {
                using (var tx = new TransactionScope(
                    TransactionScopeOption.RequiresNew, 
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                    TransactionScopeAsyncFlowOption.Enabled))
                using (var conn = new NpgsqlConnection(settings.DbConnectionString))
                {

                    Console.WriteLine($"Process event {createdEvent.EventId}");
                    

                    conn.Execute(
                        "INSERT INTO Backend.Processed (EventId) VALUES (@EventId)",
                        new { createdEvent.EventId });
                    
                    tx.Complete();
                }
            }

            var consumer = sender as EventingBasicConsumer;
            consumer.Model.BasicAck(ea.DeliveryTag, multiple: false);
        }

        public void Dispose()
        {
            if (channel != null) { channel.Dispose(); }
            if (connection != null) { connection.Dispose(); }
        }
    }

    public class BasicEvent
    {
        public Guid EventId { get; set; }
    }

    public class CommunicationCreatedEvent
    {
        public Guid EventId { get; set; }
        public Guid CommunicationId { get; set; }
    }

}
