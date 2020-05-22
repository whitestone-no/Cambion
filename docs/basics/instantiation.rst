.. _refInstantiation:
Instantiation
-------------

::

    ICambion cambion = new CambionConfiguration().Create();

First you create a configuration object, which you then use to create an instance of `ICambion`.
The previous example will initialize Cambion with a default Transport and Serializer.

.. note:: The Cambion instance should be a singleton so that the same instance is shared among all usages throughout the code.

Disposing
=========	

Cambion implements ``IDisposable`` and the Disposable pattern. Calling ``Dispose()`` on the Cambion object ensures that any
external connections established by transports are properly closed and handled.

Because Cambion is supposed to be running for the lifetime of the application you should never use Cambion in a ``using``
statement. Therefore you have to manually make sure that Cambion is properly disposed when your application terminates by
calling the ``Dispose()`` method.