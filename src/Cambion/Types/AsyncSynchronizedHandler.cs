using System;
using System.Threading.Tasks;

namespace Whitestone.Cambion.Types
{
    internal class AsyncSynchronizedHandler
    {
        private readonly WeakReference _reference;
        private readonly Delegate _method;

        public AsyncSynchronizedHandler(Delegate handler)
        {
            _reference = new WeakReference(handler.Target);

            Type messageType = handler.Method.GetParameters()[0].ParameterType;
            Type delegateType = typeof(Func<,,>).MakeGenericType(handler.Target.GetType(), messageType, handler.Method.ReturnType);

            _method = Delegate.CreateDelegate(delegateType, handler.Method);
        }

        public bool IsAlive => _reference?.IsAlive == true;

        public async Task<object> InvokeAsync(object data)
        {
            if (!IsAlive)
            {
                return null;
            }

            if (_reference.Target == null)
            {
                return null;
            }

            var t = (Task)_method.DynamicInvoke(_reference.Target, data);
            await t.ConfigureAwait(false);

            return t.IsCompleted ? ((dynamic)t).Result : null;
        }
    }

    internal class AsyncSynchronizedHandlerKey
    {
        public Type RequestType { get; }
        public Type ResponseType { get; }

        public AsyncSynchronizedHandlerKey(Type requestType, Type responseType)
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

            AsyncSynchronizedHandlerKey eb = (AsyncSynchronizedHandlerKey)obj;

            if (eb.RequestType == RequestType &&
                eb.ResponseType == ResponseType)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;

                // Multiply by a big prime number
                hash = hash * 486187739 + RequestType.GetHashCode();
                hash = hash * 486187739 + ResponseType.GetHashCode();

                return hash;
            }
        }
    }
}