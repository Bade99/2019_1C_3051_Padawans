using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    class CueManager : Manager<ICue>
        //cueManager va a ser basicamente una cola de fifo
    {
        private ICue currentCue;
        private int index=0;//para saber por cual va
        public CueManager(params ICue[] parameters)
        {
            this.elems = new List<ICue>();
            this.currentCue = parameters[0];
            foreach (ICue c in parameters) elems.Add(c);
        }
        public override void AgregarElemento(ICue cue)
        {
            if (currentCue == null) currentCue = cue;
            base.AgregarElemento(cue);
        }
        public void Update()
        {
            if (currentCue != null)
            {
                if (currentCue.IsCurrent()) currentCue.Update();
                else if (currentCue.Terminado())
                {
                    currentCue.Dispose();
                    elems.RemoveAt(index);
                    index++;
                }
            }
        }
        public void Render()
        {
            if (currentCue != null)
            {
                if (currentCue.IsCurrent()) currentCue.Render();
            }
        }
        public void Dispose()
        {
            elems.ForEach(elem => elem.Dispose());
            RemoverTodosLosElementos();
        }
    }
}
