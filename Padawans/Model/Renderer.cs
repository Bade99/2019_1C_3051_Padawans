using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;

namespace TGC.Group.Model
{
    class Renderer//el renderer va a hablar con el postprocess para definir q renderizar
    {
        private IRenderizer gameModel;
        private PostProcess postProcess;

        public Renderer(IRenderizer gamemodel,PostProcess postprocess)
        {
            this.gameModel = gamemodel;
            this.postProcess = postprocess;
        }

        private void CustomPostRender()
        {
            D3DDevice.Instance.Device.BeginScene();
            gameModel.CustomPostRender();
            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
        }

        public void Render()
        {
            if (VariablesGlobales.POSTPROCESS)//mi idea era q el postprocess pueda obtener todo ya renderizado, pero de momento tengo q re-renderizar todo again antes de poder usarlo
            {
                //gameModel.CustomPreRender();//por si quiere hacer algo antes q empiece a renderizar

                postProcess.DoBaseRender();

                postProcess.RenderPostProcess("bloom");
                postProcess.DoMenuRender();

                postProcess.RenderToScreen();

                postProcess.ClearBaseRender();
                postProcess.ClearFinishedRender();

                CustomPostRender();
            }
            else
            {
                gameModel.NormalPreRender();
                gameModel.RenderizarMeshes(null);
                gameModel.RenderizarMenus();
                gameModel.NormalPostRender();

            }
        }
    }
}
