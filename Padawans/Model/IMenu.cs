using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Input;

namespace TGC.Group.Model
{
    interface IMenu
    {
        bool CheckStartKey(TgcD3dInput input);
        void Update(TgcD3dInput input);
        void Render();
        void Dispose();
        bool IsCurrent();
    }
}
