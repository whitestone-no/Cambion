using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion
{
    internal class EventHandler
    {
        readonly WeakReference _reference;
        readonly Dictionary<Type, MethodInfo> _supportedHandlers = new Dictionary<Type, MethodInfo>();

        public EventHandler(object handler)
        {
            _reference = new WeakReference(handler);

            IEnumerable<Type> interfaces = handler.GetType().GetInterfaces()
                .Where(x => typeof(IEventHandler).IsAssignableFrom(x) && x.IsGenericType);

            foreach (var @interface in interfaces)
            {
                Type type = @interface.GetGenericArguments()[0];
                MethodInfo method = @interface.GetMethod("Handle", new Type[] { type });

                if (method != null)
                {
                    _supportedHandlers[type] = method;
                }
            }
        }

        public bool Matches(object instance)
        {
            return _reference.Target == instance;
        }

        public bool Handle(Type dataType, object data)
        {
            var target = _reference.Target;
            if (target == null)
            {
                return false;
            }

            foreach (var pair in _supportedHandlers)
            {
                if (pair.Key.IsAssignableFrom(dataType))
                {
                    var result = pair.Value.Invoke(target, new[] { data });
                }
            }

            return true;
        }
    }
}
