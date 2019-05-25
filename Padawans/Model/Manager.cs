using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public abstract class Manager<T>
    {
        public List<T> elems;
        
        public virtual void AgregarElemento(T nuevoElemento)
        {
            elems.Add(nuevoElemento);
        }
        protected void RemoverTodosLosElementos()
        {
            elems.Clear();
        }
        public bool Terminado()
        {
            return elems.Count == 0;
        }
        public int CantidadElementos()
        {
            if (this.Terminado() == true) return 0;
            return elems.Count();
        }
    }
}
