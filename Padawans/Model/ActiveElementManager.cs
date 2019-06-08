using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public abstract class ActiveElementManager : Manager<IActiveElement>
    {
        public ActiveElementManager()
        {
            elems = new List<IActiveElement>();
        }
        public void Update(float ElapsedTime)
        {
            if (this.Terminado() == false)
            {
                elems.RemoveAll(elem => { if (elem.Terminado()) elem.Dispose(); return elem.Terminado() == true; });
                elems.ForEach(elem => { elem.Update(ElapsedTime); });
            }
        }
        public void Render(string technique)
        {
            if (this.Terminado() == false)
            {
                elems.ForEach(elem => { elem.Render(technique); });
            }
        }
        public void Render()
        {
            if (this.Terminado() == false)
            {
                elems.ForEach(elem => { elem.Render(); });
            }
        }
        public void RenderBoundingBox()
        {
            if (this.Terminado() == false)
            {
                elems.ForEach(elem => { elem.RenderBoundingBox(); });
            }
        }
        public void Dispose()
        {
            elems.ForEach(elem => { elem.Dispose(); });
        }
    }
}

