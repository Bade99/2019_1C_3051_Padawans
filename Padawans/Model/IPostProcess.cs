using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public interface IPostProcess
    {
        void RenderPostProcess(string effect);
        void Render();//el render comun
    }
}
