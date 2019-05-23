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
        
        public void AgregarElemento(T nuevoElemento)
        {
            elems.Add(nuevoElemento);
        }
        public void RemoverTodosLosElementos()
        {
            if (this.Terminado() == false)
            {
                elems.Clear();
            }
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
