using System;

namespace Api.Middleware
{
    public class Event
    {
        public Guid EventId { get; set; }

        public Guid RequestId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? SentAt { get; set; }

        public string Key { get; set; }

        public string Payload { get; set; }
    }
}