using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    public class EndgameManager
    {
        EndGameTrigger endGameTrigger;
        LostGameTrigger juegoTerminadoTrigger;
        float timerGameLost = 5f;
        bool juegoGanado=false, juegoPerdido=false;
        GameModel currentGame;
        /// <summary>
        /// Recibe el xwing para saber cuando llegó al final de la pista, 
        /// y tmb le podes ir agregando las bombas para que chequee q ganaste el juego
        /// </summary>
        public EndgameManager(GameModel currentGame,EndGameTrigger endGameTrigger, LostGameTrigger juegoTerminadoTrigger)
        {
            this.currentGame = currentGame;
            this.endGameTrigger = endGameTrigger;
            this.juegoTerminadoTrigger = juegoTerminadoTrigger;
        }
        public void Update()
        {
            if (!juegoTerminadoTrigger.GameFinished())
                juegoTerminadoTrigger.Update();
            else
            {
                timerGameLost -= VariablesGlobales.elapsedTime;
                if (timerGameLost < 0) {
                    juegoPerdido = true;
                    return;//asi no sigue chequeando si ganó
                }
            }

            if(!endGameTrigger.GameFinished())
                endGameTrigger.Update();
            else
                juegoGanado = true;

        }
        public void Render()
        {
            if (endGameTrigger.Terminado() || juegoTerminadoTrigger.Terminado()) currentGame.GameEnded();

            if (juegoGanado) endGameTrigger.Render();
            else if (juegoPerdido) juegoTerminadoTrigger.Render();
        }
        public bool Fin()
        {
            return juegoGanado || juegoPerdido;
        }
        public void AgregarBomba(ITarget target)
        {
            endGameTrigger.AgregarTarget(target);
        }
        public void Dispose()
        {
            endGameTrigger.Dispose();
        }
    }
}
