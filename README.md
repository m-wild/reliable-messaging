## Reliable Messaging
Projects for testing reliable messaging between distributed systems.

* dotnet
* RabbitMQ
* postgresql

#### References:
* https://vimeo.com/111998645

#### Running:
Prerequisites
* postgresql
* rabbitmq
* dotnet

First set up the database
```sh
createdb reliableapi
psql -d reliableapi -f api/api.sql
```

Set up the queues
```sh
# might already be in PATH...
alias rabbitmqadmin=/usr/local/Cellar/rabbitmq/3.7.4/sbin/rabbitmqadmin

rabbitmqadmin declare queue name=communication.created
rabbitmqadmin declare binding source=amq.direct destination=communication.created routing_key=communication.created
```

Build and run each app
```sh
# term 1
cd api
dotnet restore
dotnet build
dotnet run

# term 2
cd backend
dotnet restore
dotnet build
dotnet run
```

### Making requests

```sh
curl -X POST \
  http://localhost:5000/api/communications \
  -H 'Content-Type: application/json' \
  -d '{
    "requestId": "9BE63422-A012-4850-AA9B-7121EBCDF47A",
    "customerId": "42B19D6E-3B17-4824-B9A5-1760F8BC351F",
    "templateKey": "invoice.ready",
    "payload": "{ \"totalOwingIncludingGst\": 250.23 }"
    }'
```