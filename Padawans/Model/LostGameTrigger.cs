using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    public class LostGameTrigger
    {
        PositionAABBCueLauncher juegoTerminado;
        bool fin = false;
        Cue obi_triste;
        public LostGameTrigger(ITarget target,TGCVector3 position)
        {
            juegoTerminado = new PositionAABBCueLauncher(target, position, new TGCVector3(1000,1000,20));
            obi_triste = new Cue(null, "Bitmaps\\Game_Lost.png", VariablesGlobales.cues_relative_scale, VariablesGlobales.cues_relative_position, 5);
        }
        public void Update()
        {
            if(!fin) if (juegoTerminado.IsReady()) fin = true;
        }
        public void Render()
        {
            if (fin)
            {
                RenderLost();
            }
        }
        public bool GameFinished()
        {
            return fin;
        }
        public void RenderLost()//@@agregar postprocesado q se oscurezca la pantalla
        {
            obi_triste.Update();
            obi_triste.Render();
        }
    }
}
