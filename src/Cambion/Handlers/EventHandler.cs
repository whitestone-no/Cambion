using System;

namespace Whitestone.Cambion.Handlers
{
    public class EventHandler
    {
        private readonly WeakReference _reference;
        private readonly Delegate _method;

        public EventHandler(Delegate handler)
        {
            _reference = new WeakReference(handler.Target);

            Type messageType = handler.Method.GetParameters()[0].ParameterType;
            Type delegateType = typeof(Action<,>).MakeGenericType(handler.Target.GetType(), messageType);

            _method = Delegate.CreateDelegate(delegateType, handler.Method);
        }

        public bool IsAlive => _reference != null && _reference.IsAlive;


        public bool Invoke(object data)
        {
            if (!IsAlive)
                return false;

            if (_reference.Target != null)
            {
                _method.DynamicInvoke(_reference.Target, data);
            }

            return true;
        }
    }
}
