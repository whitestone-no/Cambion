.. _refInstantiation:
Exception Handling
------------------

If an exception occurs inside an event handler or synchronized handler, and this is not handled by the handler itself,
there exists an ``EventHandler`` you can subscribe to to be notified of these exceptions so that you can take appropriate
action.
Simply inject Cambion into the class where you want to handle these exceptions, and subscribe to the ``UnhandledException`` event:

::

    public class YourExceptionHandler
	{
	    public YourExceptionHandler(ICambion cambion)
		{
		    cambion.UnhandledException += HandleException;
		}
		
		private void HandleException(object sender, System.IO.ErrorEventArgs e)
        {
            // Handle exceptions here using e.GetException() to get the offending exception
        }
	}
