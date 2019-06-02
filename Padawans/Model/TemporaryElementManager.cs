using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public class TemporaryElementManager : ActiveElementManager, IPostProcess
    {
        public TemporaryElementManager()  : base()
        {

        }

        public void RenderPostProcess(string effect)
        {
            elems.ForEach(elem => elem.Render());
        }
    }
}
