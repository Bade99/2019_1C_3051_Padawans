using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public interface ISoundElement
    {
        void Update();
        bool Terminado();
        void Terminar();
        string GetPath();
        void Dispose();
        void Pause();
        void Resume();
    }
}
