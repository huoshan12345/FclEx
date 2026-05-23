# FclEx.Messaging

Messaging helpers for Kafka and RabbitMQ.

## What Is Included

- Kafka consumer startup helpers based on `ConsumerBuilder`.
- Kafka JSON deserializers for `System.Text.Json` and Newtonsoft.Json.
- Kafka logging helpers for errors and delivery results.
- RabbitMQ publisher, consumer, router, and processor base classes.
- RabbitMQ options for connections, exchanges, queues, consumers, publishers, and routers.
- RabbitMQ message conversion and `RoutingMessage<T>` helpers.
- RabbitMQ constants for exchange types, queue arguments, binding arguments, and headers.
- RabbitMQ basic-property extensions for headers, retry counts, and delay metadata.

## Notes

The RabbitMQ processors are asynchronous and disposable. Always dispose publishers, consumers, and routers so channels and connections are closed cleanly.
