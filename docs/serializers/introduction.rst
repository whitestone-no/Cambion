Introduction
------------

Serializers are used by Cambion to serialize the data being sent over the transport.
As with Transports, Cambion can easily be configured to use any serializer, and you can even implement your own custom serializer.

.. note:: You must set up a serializer in addition to a transport, otherwise the local loopback will be used and you won't be able to send messages across the network.