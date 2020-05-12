RabbitMQ
--------

One of the transports available for Cambion uses `RabbitMQ <https://www.rabbitmq.com/>`_ to send data between instances of Cambion.
The RabbitMQ transport requires a dedicated RabbitMQ instance either hosted on a dedicated server or in the cloud. This documentation
will not cover how to set up RabbitMQ as there are detailed instructions on their website.

Installation
============

As with other transports, the ``RabbitMQ`` transport for Cambion is also available on `NuGet <https://www.nuget.org/packages/Whitestone.Cambion.Transport.RabbitMQ/>`_ so you can install it using the NuGet Package Manager Console:

::

    Install-Package Whitestone.Cambion.Transport.RabbitMQ

Usage
=====

The RabbitMQ transport can be set up using one of the two provided extension methods. The easiest of these uses the standard single line URI:

::

    ICambion cambion = new CambionConfiguration()
        .Transport.UseRabbitMQ("amqp://username:password@hostname/vhost")
        .Create();

When you need more control over the RabbitMQ setup you should use the extension method that overrides the default configuration:

::

    ICambion cambion = new CambionConfiguration()
        .Transport.UseRabbitMQ(conf => {
			conf.Connection.Hostname = "hostname";
            conf.Connection.Username = "username";
            conf.Connection.Password = "password";
            conf.Connection.VirtualHost = "vhost";
		})
        .Create();

Additional settings
===================

Cambion uses a pub/sub pattern, and will create a durable "fanout" exchange with auto-delete enabled by default. This exchange is named ``cambion.fanout``.

It will also create a durable and exclusive queue with auto-delete enabled, named ``cambion-<Guid>``, and connect this to the previously created exchange.
Seeing as it has a ``Guid`` in its name the name will be unique.

The exchange/queue names and types can be configured using the extension method that overrides the default configuration.
The following is a list of all possible configuration parameters:

+----------------+----------------+----------+-------------------------------------+--------------------+
| Group          | Parameter name | Required | Description                         | Default value      |
+================+================+==========+=====================================+====================+
| **Connection** | Hostname       | Yes      | The hostname of the RabbitMQ server | *null*             |
|                +----------------+----------+-------------------------------------+--------------------+
|                | Username       | Yes      | The username needed to connect      | *null*             |
|                +----------------+----------+-------------------------------------+--------------------+
|                | Password       | Yes      | The password needed to connect      | *null*             |
|                +----------------+----------+-------------------------------------+--------------------+
|                | VirtualHost    | No       | The vhost on the RabbitMQ server    | "/"                |
|                +----------------+----------+-------------------------------------+--------------------+
|                | Port           | No       | The port that RabbitMQ listens on   | 5672               |
+----------------+----------------+----------+-------------------------------------+--------------------+
| **Exchange**   | Name           | No       | The name of the exchange            | "cambion.fanout"   |
|                +----------------+----------+-------------------------------------+--------------------+
|                | Type           | No       | Exchange type                       | "fanout"           |
|                +----------------+----------+-------------------------------------+--------------------+
|                | Durable        | No       | Creates a durable exchange          | true               |
|                +----------------+----------+-------------------------------------+--------------------+
|                | AutoDelete     | No       | Exchange will be deleted when       | true               |
|                |                |          | no longer in use                    |                    |
+----------------+----------------+----------+-------------------------------------+--------------------+
| **Queue**      | Name           | No       | The name of the queue               | "cambion-<Guid>"   |
|                +----------------+----------+-------------------------------------+--------------------+
|                | Durable        | No       | Creates a durable queue             | true               |
|                +----------------+----------+-------------------------------------+--------------------+
|                | Exclusive      | No       | Creates an exclusive queue          | true               |
|                +----------------+----------+-------------------------------------+--------------------+
|                | AutoDelete     | No       | Queue will be deleted when          | true               |
|                |                |          | no longer in use                    |                    |
+----------------+----------------+----------+-------------------------------------+--------------------+

.. note:: The ``Exchange.Type`` must be one of RabbitMQ's `supported types <https://www.rabbitmq.com/tutorials/amqp-concepts.html>`_: ``fanout``, ``direct``, ``headers`` or ``topic``. These are subject to change.

.. note:: Cambion uses a pub/sub pattern. RabbitMQ recommends using a ``fanout`` exchange for this pattern, so it is recommended to leave the ``Exchange.Type`` setting to its default value.

For more detailed descriptions of these settings, refer the `RabbitMQ documentation <https://www.rabbitmq.com/documentation.html>`_.


To use a non-durable exchange named "Cambion", with a non-exclusive and non-durable queue named "CambionQueue", you can use the following configuration:

::

    ICambion cambion = new CambionConfiguration()
        .Transport.UseRabbitMQ(conf => {
			conf.Connection.Hostname = "hostname";
            conf.Connection.Username = "username";
            conf.Connection.Password = "password";
            conf.Exchange.Name = "Cambion";
			conf.Exchange.Durable = false;
			conf.Queue.Name = "CambionQueue";
			conf.Queue.Exclusive = false;
			conf.Queue.Durable = false;
		})
        .Create();
