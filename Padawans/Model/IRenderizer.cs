using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    interface IRenderizer
    {
        void NormalPreRender();//Renderer no se encarga de iniciar el render
        void CustomPreRender();
        void RenderizarMenus();
        void RenderizarMeshes(string technique);
        void NormalPostRender();//Renderer no se encarga de terminar el render ni mostrar en pantalla
        void CustomPostRender();
    }
}
