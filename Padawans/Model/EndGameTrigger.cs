using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    public class EndGameTrigger
    {
        List<ICueLauncher> targets;
        TGCVector3 position, size;
        bool gameFinished;
        float timer = 3;
        Cue obi_festeja;

        public EndGameTrigger(TGCVector3 position, TGCVector3 size)
        {
            targets = new List<ICueLauncher>();
            this.position = position;
            this.size = size;
            obi_festeja = new Cue(null, "Bitmaps\\Game_Won.png", VariablesGlobales.cues_relative_scale, VariablesGlobales.cues_relative_position, 5);
        }

        public void AgregarTarget(ITarget target)
        {
            targets.Add(new PositionAABBCueLauncher(target,position,size));
        }

        public void Update()
        {
            if (!gameFinished)
            {
                for(int i = 0; i < targets.Count; i++)
                {
                    try
                    {
                        if (targets[i].IsReady())
                        {
                            gameFinished = true;
                            targets.Clear();
                        }
                    }
                    catch
                    {
                        targets.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        public void Render()
        {
            if (gameFinished)
            {
                if (timer < 0)
                {
                    RenderWon();
                }
                else timer -= VariablesGlobales.elapsedTime;
                //@@obi festejando
            }
        }
        public bool GameFinished()
        {
            return gameFinished;
        }

        public void Dispose()
        {
            targets.Clear();
        }
        public void RenderWon()//@@agregar postprocesado q se oscurezca la pantalla
        {
            obi_festeja.Update();
            obi_festeja.Render();
        }
    }
}
