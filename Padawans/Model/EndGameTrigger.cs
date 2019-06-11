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
        float duracion =5;
        float timer = 3;
        Cue obi_festeja;
        FullScreenElement congrats;
        bool soundAdded=false;

        public EndGameTrigger(TGCVector3 position, TGCVector3 size)
        {
            targets = new List<ICueLauncher>();
            this.position = position;
            this.size = size;
            obi_festeja = new Cue(null, "Bitmaps\\Game_Won.png", VariablesGlobales.cues_relative_scale, VariablesGlobales.cues_relative_position, duracion);
            congrats = new FullScreenElement("Bitmaps\\Congrats.png", SoundManager.SONIDOS.NO_SOUND, duracion);
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
                    duracion -= VariablesGlobales.elapsedTime;
                }
                else timer -= VariablesGlobales.elapsedTime;
            }
        }
        public bool GameFinished()
        {
            return gameFinished;
        }

        public bool Terminado()
        {
            return duracion < 0;
        }

        public void Dispose()
        {
            targets.Clear();
        }
        public void RenderWon()//@@agregar postprocesado q se oscurezca la pantalla
        {
            if (!soundAdded)
            {
                VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.EXPLOSION_FINAL);
                soundAdded = true;
            }
            obi_festeja.Update();
            obi_festeja.Render();
            congrats.Render();
        }
    }
}
