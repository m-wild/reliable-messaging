using System;

namespace Api.Middleware
{
    public class Request
    {
        public Request(IIdentifiableRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.RequestId == default(Guid)) throw new ArgumentNullException();

            RequestId = request.RequestId;
            CreatedAt = DateTime.Now;
        }

        public Request(Guid requestId, DateTime createdAt)
        {
            RequestId = requestId;
            CreatedAt = createdAt;
        }

        public Guid RequestId { get; }

        public DateTime CreatedAt { get; }
    }

}