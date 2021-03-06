﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    class CueManager : Manager<ICue>
        //@@cueManager va a ser basicamente una cola de fifo, capaz conviene que no sea asi ahora que tenemos cue launchers
    {
        private ICue currentCue;
        private bool isCurrent;
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
                if (currentCue.IsCurrent()) {
                    currentCue.Update();
                    isCurrent = true;
                } 
                if (currentCue.Terminado())
                {
                    currentCue.Dispose();
                    //elems.RemoveAt(0);
                    elems.Remove(currentCue);
                    isCurrent = false;
                    try
                    {
                    currentCue = elems.ElementAt(0);
                    }
                    catch { currentCue = null; }
                }
            }
        }
        public void Render()
        {
            if (currentCue != null)
            {
                if (isCurrent) currentCue.Render();
            }
        }
        public void Dispose()
        {
            elems.ForEach(elem => elem.Dispose());
            RemoverTodosLosElementos();
        }
    }
}
