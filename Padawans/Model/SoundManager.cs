using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Example;

namespace TGC.Group.Model
{
    public class SoundManager : Manager<ISoundElement>
    {
        public SoundManager()
        {
            this.elems = new List<ISoundElement>();
        }

        public void Update()
        {
            if (this.Terminado() == false)
            {
                elems.RemoveAll(elem => { if (elem.Terminado()) elem.Dispose(); return elem.Terminado() == true; });
                elems.ForEach(elem => { elem.Update(); });
            }
        }
        public void MuteOrUnMute()
        {
            VariablesGlobales.SOUND = !VariablesGlobales.SOUND;
        }
        public void Dispose()
        {
            elems.ForEach(elem => { elem.Dispose(); });
            RemoverTodosLosElementos();
        }
        public void Remove(string path)
        {
            var sonidoATerminar = elems.Find(elem =>  elem.GetPath()==path );
            if (sonidoATerminar != null) sonidoATerminar.Terminar();
        }
        public void PauseAll()
        {
            elems.ForEach(elem=>elem.Pause());
        }
        public void ResumeAll()
        {
            elems.ForEach(elem => elem.Resume());
        }
        public void PauseID(string ID)
        {
            elems.ForEach(elem => { if (elem.GetID() == ID) elem.Pause(); });
        }
        public void ResumeID(string ID)
        {
            elems.ForEach(elem=> { if (elem.GetID() == ID) elem.Resume(); });
        }
        public void ReproducirSonido(SONIDOS sonido)
        {
            switch(sonido)
            {
                case SONIDOS.DISPARO_MISIL:
                    AgregarElemento(new Sonido("Sonidos\\XWing_1_disparo.wav", 0, 1f, 1, 0,""));
                    break;
            }
        }

        public enum SONIDOS
        {
            DISPARO_MISIL
        }
    }
}
