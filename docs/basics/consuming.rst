Consuming
---------

Once you have all your subscriptions set up you can call the callback methods using the functions provided in ``ICambion``.

Events
======

Events are fire-and-forget, and there may be multiple recipients.

Publish an event using the following functionality:

::

    cambion.PublishEvent(new TEvent());

All the examples regarding events described in :ref:`Subscribing<refSubscribing>` use the ``TEvent`` type during subscription,
so this call to ``PublishEvent()`` will cause the callbacks for all those subscriptions to be called.

.. note:: Note that you do not need to specify the type as a generic, even though the method signature has it. This will be handled by the compiler.

Synchronized
============

Synchronized are two-way, and there should be only one recipient.

Publish a synchronized using the following functionality:

::

    TResponse response = cambion.CallSynchronizedHandler<TRequest, TResponse>(new TRequest());

All the examples regarding synchronized described above use the `TRequest` and `TResponse` types during subscription, so this call to `CallSynchronizedHandler` will cause the callbacks for all those subscriptions to be called.

> Note that you are here required to specify the types as generics for the method signature.


A note about synchronized messages
==================================

Because the distributed transports will use multiple instances of Cambion, a synchronized handler can be set up for the same message signature in multiple instances.
Because Cambion sends the synchronized message out to all handlers, there is no way of telling which one will reply, and the reply will be handled by the first instance that is able to send a reply.

Therefore, make sure that no synchronized handler signatures are shared between all your instances.