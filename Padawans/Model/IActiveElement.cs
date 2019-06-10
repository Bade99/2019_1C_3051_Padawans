using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public interface IActiveElement
    {
        void Update();
        bool Terminado();
        void Render();
        void RenderBoundingBox();
        void Dispose();
    }
}
