using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    interface IRenderizer
    {
        void CustomPreRender();
        void RenderizarMenus();
        void RenderizarMeshes();
        void CustomPostRender();
    }
}
