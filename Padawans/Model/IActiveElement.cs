using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public interface IActiveElement
    {
        void Update(float ElapsedTime);
        bool Terminado();
        void Render();
        void RenderBoundingBox();
        void Dispose();
        void Render(string technique);
    }
}
