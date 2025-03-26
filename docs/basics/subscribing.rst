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

The method signature for the callbacks will then be as follows:

::

    private void MyEventCallback(TEvent data) { }
    private TResponse MySynchronizedCallback(TRequest data) { }

You can also use lambda expressions instead of callback methods:

::

    cambion.AddEventHandler<TEvent>(data => { });
    cambion.AddSynchronizedHandler<TRequest, TResponse>(data => { return null; });

Make sure that your synchronized methods/expressions always return data.

Interface subscription
======================

Another way to subscribe is using an interfaces.
To use this approach, the class where you want your subscriptions implement has to have either ``IEventHandler<>`` or ``ISynchronizedHandler<>``.
You can now register all your subscriptions in a single call to ``.Register()``.

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
            return new TResponse();
        }
    }

Async Handlers
==============

In addition to the subscription methods described above you have ``Async`` versions of the same methods,
and similarily with the interfaces you have ``IAsyncEventHandler<>`` or ``IAsyncSynchronizedHandler<>``, which can be used with ``.Register()``.

The main difference with these subscriptions are that the callbacks need to return a ``Task`` instead of ``void``.
These can also be marked as ``async`` but should only do si if you actually use ``await`` in your handler.

Examples
^^^^^^^^

::

    public class MyAsyncSubscriptionClass : IAsyncEventHandler<TEvent>, IAsyncSynchronizedHandler<TRequest, TResponse>
    {
        public MyAsyncSubscriptionClass(ICambion cambion)
        {
            cambion.Register(this);

            cambion.AddAsyncEventHandler<TEvent>(MyAsyncEventCallback);
            cambion.AddAsyncSynchronizedHandler<TRequest, TResponse>(MyAsyncSynchronizedCallback);

            cambion.AddAsyncEventHandler<TEvent>(data => { return Task.CompletedTask; });
            cambion.AddAsyncSynchronizedHandler<TRequest, TResponse>(data => { return Task.FromResult((TResponse)null); });

            cambion.AddAsyncEventHandler<TEvent>(async data => { await DoSomethingAsync(); });
            cambion.AddAsyncSynchronizedHandler<TRequest, TResponse>(data =>
            {
                await DoSomethingAsync();
                return null;
            });
        }

        public Task MyAsyncEventCallback(TEvent data)
        {
            return Task.CompletedTask;
        }

        public async Task<TResponse> MyAsyncSynchronizedCallback(TRequest data)
        {
            await DoSomethingAsync();
            return null;
        }

        public async Task HandleEventAsync(TEvent data)
        {
            await DoSomethingElseAsync();
        }

        public Task<TResponse> HandleSyncronizedAsync(TRequest data)
        {
            return Task.FromResult((TResponse)null);
        }
    }
