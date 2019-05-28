using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public class DelayCueLauncher : ICueLauncher//seria bueno no usar delaylaunchers
    {//si queres un delay capaz conviene x ej detectar algo y despues usar delay, sino el delay va a ir bajando desde el inicio del programa
        public float delay;
        public DelayCueLauncher(float delay)
        {
            this.delay = delay;
        }
        public bool IsReady()
        {
            if (delay < 0)//ya toy prediciendo que tener cues x tiempo var a dar problemas, xq no se pueden "lockear" como otras, el contador no para
            {
                return true;
            }
            else
            {
                delay -= VariablesGlobales.elapsedTime;
                return false;
            }
        }
    }
}
