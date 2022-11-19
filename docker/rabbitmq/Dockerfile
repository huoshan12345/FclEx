FROM rabbitmq:3-management

COPY *.ez $RABBITMQ_HOME/plugins/

RUN rabbitmq-plugins enable --offline rabbitmq_delayed_message_exchange
RUN rabbitmq-plugins enable --offline rabbitmq_consistent_hash_exchange