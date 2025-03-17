using System;
using System.Threading;

namespace Whitestone.Cambion.Types
{
    internal class SynchronizedHandler
    {
        private readonly WeakReference _reference;
        private readonly Delegate _method;

        public SynchronizedHandler(Delegate handler)
        {
            _reference = new WeakReference(handler.Target);

            Type messageType = handler.Method.GetParameters()[0].ParameterType;
            Type delegateType = typeof(Func<,,>).MakeGenericType(handler.Target.GetType(), messageType, handler.Method.ReturnType);

            _method = Delegate.CreateDelegate(delegateType, handler.Method);
        }

        public bool IsAlive => _reference != null && _reference.IsAlive;

        public object Invoke(object data)
        {
            if (!IsAlive)
                return null;

            if (_reference.Target != null)
            {
                return _method.DynamicInvoke(_reference.Target, data);
            }

            return null;
        }
    }

    internal class SynchronizedHandlerKey
    {
        public Type RequestType { get; }
        public Type ResponseType { get; }

        public SynchronizedHandlerKey(Type requestType, Type responseType)
        {
            RequestType = requestType;
            ResponseType = responseType;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var eb = (SynchronizedHandlerKey)obj;

            return eb.RequestType == RequestType &&
                   eb.ResponseType == ResponseType;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;

                // Multiply by a big prime number
                hash = hash * 486187739 + RequestType.GetHashCode();
                hash = hash * 486187739 + ResponseType.GetHashCode();

                return hash;
            }
        }
    }

    internal class SynchronizedDataPackage
    {
        public object Data { get; set; }
        public ManualResetEvent ResetEvent { get; set; }

        public SynchronizedDataPackage(ManualResetEvent mre)
        {
            ResetEvent = mre;
        }
    }
}