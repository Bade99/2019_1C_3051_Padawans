using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    interface ICueLauncher//las cue lo usan para saber cuando lanzarse,podria ser un launcher por tiempo, colision, etc
    {
        bool IsReady();//es como el Update para un CueLauncher
    }
}
