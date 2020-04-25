.. _refSubscribing:
Subscribing
-----------

There are two ways to say you want to subscribe to an event or synchronizer.
The difference between these is that an event is a one-way fire-and-forget message that is intended for multiple recipients,
and a synchronized is a two-way message that is intended for one recipient, but can be called from multiple sources.

Direct subscription
===================

You can subscribe using the methods found in ``ICambion``, which are accessible in your ``CambionMessageHandler`` object.

::

    cambion.AddEventHandler<TEvent>(MyEventCallback);
    cambion.AddSynchronizedHandler<TRequest, TResponse>(MySynchronizedCallback);

The method signature for the callbacks will then be as follows

::
    private void MyEventCallback(TEvent data) { }
    private TResponse MySynchronizedCallback(TRequest data) { }

You can also use lambda expressions instead of callback methods

::
    cambion.AddEventHandler<TEvent>(data => { });
    cambion.AddSynchronizedHandler<TRequest, TResponse>(data => { return null; });

Make sure that your synchronized methods/expressions always return data.

Interface subscription
======================

Another way to subscribe is using an interfaces.
To use this approach, the class where you want your subscriptions implement has to have either ``IEventHandler<>`` or ``ISynchronizedHandler<>``.
You can now register all your subscriptions in a single call to ``Register()``.

Example
^^^^^^^

::

    public class MySubscriptionClass : IEventHandler<TEvent>, ISynchronizedHandler<TRequest, TResponse>
    {
        public MySubscriptionClass(ICambion cambion)
        {
            cambion.Register(this);
        }
    
        public void HandleEvent(TEvent data) { }
    
        public TResponse HandleSynchronized(TRequest data)
        {
            return null;
        }
    }
