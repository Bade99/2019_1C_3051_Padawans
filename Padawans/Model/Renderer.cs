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

        private float time = 5;

        public Renderer(IRenderizer gamemodel,PostProcess postprocess)
        {
            this.gameModel = gamemodel;
            this.postProcess = postprocess;


        }

        public void Render()
        {
            if (VariablesGlobales.POSTPROCESS)//mi idea era q el postprocess pueda obtener todo ya renderizado, pero de momento tengo q re-renderizar todo again antes de poder usarlo
            {
                //if (time < 0f)
                //{
                postProcess.DoBaseRender();
                postProcess.RenderPostProcess("bloom");
                //postProcess.DoMenuRender();
                postProcess.RenderToScreen();
                postProcess.ClearBaseRender();
                postProcess.ClearFinishedRender();
                //parche de momento
                D3DDevice.Instance.Device.BeginScene();
                gameModel.CustomPostRender();
                //}
                //else time -= VariablesGlobales.elapsedTime;
            }
            else
            {
                gameModel.CustomPreRender();
                gameModel.RenderizarMeshes();
                gameModel.RenderizarMenus();
                gameModel.CustomPostRender();

            }
        }
    }
}
