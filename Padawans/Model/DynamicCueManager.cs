using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    class DynamicCueManager : Manager<ICue>//para cues q necesitan chequear cosas todo el tiempo
    {
        public DynamicCueManager(params ICue[] parameters)
        {
            this.elems = new List<ICue>();
            foreach (ICue c in parameters) elems.Add(c);
        }
        public override void AgregarElemento(ICue cue)
        {
            base.AgregarElemento(cue);
        }
        public void Update()
        {
            elems.ForEach(cue => { if (cue.IsCurrent()) cue.Update(); });
            elems.ForEach(cue=> { if (cue.Terminado()) { cue.Dispose(); } });
            elems.RemoveAll(cue => cue.Terminado());
        }
        public void Render()
        {
            elems.ForEach(cue=> { if (cue.IsCurrent()) cue.Render(); });
        }
        public void Dispose()
        {
            elems.ForEach(elem => elem.Dispose());
            RemoverTodosLosElementos();
        }
    }
}
