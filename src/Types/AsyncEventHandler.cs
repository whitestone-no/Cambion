using System;
using System.Threading.Tasks;

namespace Whitestone.Cambion.Types
{
    internal class AsyncEventHandler
    {
        private readonly WeakReference _reference;
        private readonly Delegate _method;

        public AsyncEventHandler(Delegate handler)
        {
            _reference = new WeakReference(handler.Target);

            Type messageType = handler.Method.GetParameters()[0].ParameterType;
            Type delegateType = typeof(Func<,,>).MakeGenericType(handler.Target.GetType(), messageType, typeof(Task));

            _method = Delegate.CreateDelegate(delegateType, handler.Method);
        }

        public bool IsAlive => _reference?.IsAlive == true;

        public async Task<bool> InvokeAsync(object data)
        {
            if (!IsAlive)
            {
                return false;
            }

            if (_reference.Target == null)
            {
                return false;
            }
            
            var t = (Task)_method.DynamicInvoke(_reference.Target, data);
            
            await t.ConfigureAwait(false);

            return t.IsCompleted;
        }
    }
}
