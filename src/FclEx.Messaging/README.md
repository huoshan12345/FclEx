# FclEx.Messaging

Kafka and RabbitMQ helpers for FclEx.

## What Is Included

- Consumer, publisher, router, and processor abstractions.
- Message conversion helpers.
- RabbitMQ routing, queue, consumer, publisher, and processor options.
- Kafka helper types and option models.
- Retry metadata and logging helpers for messaging workflows.

## Usage Notes

- This package includes both RabbitMQ and Kafka dependencies.
- Keep broker-specific configuration in the appropriate option objects.
- Messaging tests may require local broker services.
