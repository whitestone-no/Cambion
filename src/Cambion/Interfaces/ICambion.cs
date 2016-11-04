using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whitestone.Cambion.Interfaces
{
    public interface ICambion
    {
        void Initialize(Action<IMessageHandlerInitializer> initializer);
        void Reinitialize(Action<IMessageHandlerInitializer> initializer);
        void Register(object handler);
    }
}
