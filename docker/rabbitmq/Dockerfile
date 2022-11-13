ARG PLUGIN_VERSION=3.11.1

FROM ubuntu:20.04 AS builder

ARG PLUGIN_VERSION

RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y curl

RUN mkdir -p /plugins
COPY rabbitmq_delayed_message_exchange-${PLUGIN_VERSION}.ez /plugins

FROM rabbitmq:3-management

ARG PLUGIN_VERSION

COPY --from=builder --chown=rabbitmq:rabbitmq \
	/plugins/ \
	$RABBITMQ_HOME/plugins/

RUN rabbitmq-plugins enable --offline rabbitmq_delayed_message_exchange
RUN rabbitmq-plugins enable --offline rabbitmq_consistent_hash_exchange