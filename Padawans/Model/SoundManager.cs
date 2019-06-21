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
        public void RemoveID(SONIDOS sonido)
        {
            string ID = "";
            switch (sonido)
            {
                case SONIDOS.PAUSE:
                    ID = "pause_menu";
                    break;
                case SONIDOS.MAIN_MENU:
                    ID = "main_menu";
                    break;
                case SONIDOS.FORCE_THEME:
                    ID = "story_menu";
                    break;
            }
            ISoundElement sonidoATerminar = elems.Find(elem => elem.GetID().Equals(ID));
            if (sonidoATerminar != null)
            {
                sonidoATerminar.Terminar();
            }
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
                case SONIDOS.DISPARO_MISIL_XWING:
                    AgregarElemento(new Sonido("Sonidos\\XWing_1_disparo.wav", 1, 1f, 1, 0, ""));
                    break;
                case SONIDOS.DISPARO_MISIL_ENEMIGO:
                    AgregarElemento(new Sonido("Sonidos\\TIE_fighter_1_disparo.wav", -900, 1f, 1, 0, ""));
                    break;
                case SONIDOS.BACKGROUND_BATTLE:
                    AgregarElemento(new Sonido("Sonidos\\Background_space_battle_10min.wav", -1000, 0, -1, 0, ""));
                    break;
                case SONIDOS.LUKE_OBI_WAN:
                    AgregarElemento(new Sonido("Sonidos\\obi_wan_luke.wav", -300, 0.5f, 1, 0, ""));
                    break;
                case SONIDOS.PAUSE:
                    AgregarElemento(new Sonido("Sonidos\\main_menu.wav", 0, 0, -1, 0, "pause_menu"));
                    break;
                case SONIDOS.MAIN_MENU:
                    AgregarElemento(new Sonido("Sonidos\\main_menu.wav", 0, 0, -1, 0, "main_menu"));
                    break;
                case SONIDOS.FLYBY_2:
                    AgregarElemento(new Sonido("Sonidos\\XWing_flyby_2.wav", -900, 8, 1, 0, ""));
                    break;
                case SONIDOS.XWING_ENGINE:
                    AgregarElemento(new Sonido("Sonidos\\XWing_engine.wav", -600, 1, -1, 0, ""));
                    break;
                case SONIDOS.XWING_BOMB:
                    AgregarElemento(new Sonido("Sonidos\\Xwing_bomb_sound.wav", 0, 2, 1, 0, ""));
                    break;
                case SONIDOS.EXPLOSION_FINAL:
                    AgregarElemento(new Sonido("Sonidos\\final_explotion.wav", 0, 20, 1, 0, ""));
                    break;
                case SONIDOS.FORCE_THEME:
                    AgregarElemento(new Sonido("Sonidos\\Force_Theme.wav", 0, 0, -1, 0, "story_menu"));
                    break;
            }
         }


        public enum SONIDOS
        {
            DISPARO_MISIL_XWING, DISPARO_MISIL_ENEMIGO, BACKGROUND_BATTLE,
            LUKE_OBI_WAN, MAIN_MENU, PAUSE, FLYBY_2, XWING_ENGINE,NO_SOUND,XWING_BOMB,EXPLOSION_FINAL,
            FORCE_THEME
        }
    }
}
