using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public class TemporaryElementManager : ITemporaryElement
    {
        private List<ITemporaryElement> elementos;
        public TemporaryElementManager()
        {
            elementos = new List<ITemporaryElement>();
        }
        public void AgregarElemento(ITemporaryElement nuevoElemento)
        {
            elementos.Add(nuevoElemento);
        }
        public void Update(float ElapsedTime)
        {
            if (this.Terminado() == false)
            {
                elementos.RemoveAll(elem => { return elem.Terminado() == true; });
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
            if (this.Terminado() == false)
            {
                elementos.ForEach(elem => { elem.Dispose(); });
            }
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
            return elementos.Count==0;
        }
    }
}
