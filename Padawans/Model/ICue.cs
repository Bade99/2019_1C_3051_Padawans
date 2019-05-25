using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    interface ICue
    {
        bool IsCurrent();
        void Update();
        void Render();
        void Dispose();
        bool Terminado();
    }
}
