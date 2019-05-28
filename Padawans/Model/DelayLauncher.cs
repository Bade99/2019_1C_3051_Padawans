using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public class DelayLauncher : ICueLauncher//seria bueno no usar delaylaunchers
    {
        public float delay;
        public DelayLauncher(float delay)
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
