using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public abstract class ActiveElementManager
    {
        private List<IActiveElement> elementos;
        public ActiveElementManager()
        {
            elementos = new List<IActiveElement>();
        }
        public void AgregarElemento(IActiveElement nuevoElemento)
        {
            elementos.Add(nuevoElemento);
        }
        public void Update(float ElapsedTime)
        {
            if (this.Terminado() == false)
            {
                elementos.RemoveAll(elem => { if (elem.Terminado()) elem.Dispose(); return elem.Terminado() == true; });
                elementos.ForEach(elem => { elem.Update(ElapsedTime); });
            }
        }
        public void Render()
        {
            if (this.Terminado() == false)
            {
                elementos.ForEach(elem => { elem.Render(); });
            }
        }
        public void RenderBoundingBox()
        {
            if (this.Terminado() == false)
            {
                elementos.ForEach(elem => { elem.RenderBoundingBox(); });
            }
        }
        public void Dispose()
        {
            elementos.ForEach(elem => { elem.Dispose(); });
        }
        public void RemoverTodosLosElementos()
        {
            if (this.Terminado() == false)
            {
                elementos.Clear();
            }
        }
        public bool Terminado()
        {
            return elementos.Count == 0;
        }
        public int CantidadElementos()
        {
            if (this.Terminado() == true) return 0;
            return elementos.Count();
        }
    }
}

