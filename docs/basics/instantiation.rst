Instantiation
-------------

::

    ICambion cambion = new CambionConfiguration().Create();

	
First you create a configuration object, which you then use to create an instance of `ICambion`.
The previous example will initialize Cambion with a default Transport and Serializer.

.. note:: The Cambion instance should be a singleton so that the same instance is shared among all usages throughout the code.
