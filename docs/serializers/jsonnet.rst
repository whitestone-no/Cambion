JSON.NET
--------

The default serializer that comes preinstalled with Cambion is called ``JsonNet`` and uses Newtonsoft.Json to serialize the data for the transport.
This does not require any specific configuration, and is used if no other serializer is specified.

::

    ICambion cambion = new CambionConfiguration()
        .Serializer.UseJsonNet()
	    .Create();
