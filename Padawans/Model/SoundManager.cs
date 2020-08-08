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
        private static SoundManager instance;
        private bool ultimoEstadoDeMute = true;

        private SoundManager()
        {
            this.elems = new List<ISoundElement>();
        }

        public static SoundManager GetInstance()
        {
            if (instance == null)
            {
                instance = new SoundManager();
            }
            return instance; 
        }

        public void Update()
        {
            if (this.Terminado() == false)
            {
                elems.RemoveAll(elem => { if (elem.Terminado()) elem.Dispose(); return elem.Terminado() == true; });
                elems.ForEach(elem => { elem.Update(); });
            }
            if (!VariablesGlobales.SOUND && ultimoEstadoDeMute)
            {
                elems.ForEach((x) => x.mutear());
            }
            if (VariablesGlobales.SOUND && !ultimoEstadoDeMute)
            {
                elems.ForEach((x) => x.unmute());
            }
            ultimoEstadoDeMute = VariablesGlobales.SOUND;
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

        protected new void RemoverTodosLosElementos()
        {
            List<ISoundElement> mp3s = this.elems.FindAll((x) => !x.GetID().Equals(""));
            this.elems.Clear();
            this.elems = mp3s;
        }
        public void StopID(SONIDOS sonido)
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
                case SONIDOS.EXPLOSION_FINAL:
                    ID = "explosion_final";
                    break;
            }
            ISoundElement sonidoATerminar = elems.Find(elem => elem.GetID().Equals(ID));
            if (sonidoATerminar != null)
            {
                sonidoATerminar.Stop();
            }
        }
        public void PauseAll()
        {
            elems.FindAll((x) => !x.IsStoppeado())
                .ForEach(elem => elem.Pause());
        }
        public void ResumeAll()
        {
            elems.FindAll((x) => !x.IsStoppeado())
                .ForEach(elem => elem.Resume());
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
                case SONIDOS.EXPLOSION_TORRETA:
                    AgregarElemento(new Sonido("Sonidos\\430058__manimato2__explosion.wav", 0, 1f, 1, 0, ""));
                    break;
                case SONIDOS.DISPARO_MISIL_XWING:
                    AgregarElemento(new Sonido("Sonidos\\XWing_1_disparo.wav", 0, 1f, 1, 0, ""));
                    break;
                case SONIDOS.DISPARO_MISIL_ENEMIGO:
                    AgregarElemento(new Sonido("Sonidos\\TIE_fighter_1_disparo.wav", 0, 1f, 1, 0, ""));
                    break;
                case SONIDOS.BACKGROUND_BATTLE:
                    FindAndPlay("Sonidos\\Background_space_battle_10min.mp3", "background_battle");
                    break;
                case SONIDOS.LUKE_OBI_WAN:
                    AgregarElemento(new Sonido("Sonidos\\obi_wan_luke.wav", 0, 0.5f, 1, 0, ""));
                    break;
                case SONIDOS.PAUSE:
                    FindAndPlay("Sonidos\\main_menu.mp3", "pause_menu");
                    break;
                case SONIDOS.MAIN_MENU:
                    FindAndPlay("Sonidos\\main_menu.mp3", "main_menu");
                    break;
                case SONIDOS.FLYBY_2:
                    FindAndPlay("Sonidos\\XWing_flyby_2.mp3", "flyby_2", true);
                    break;
                case SONIDOS.XWING_ENGINE:
                    FindAndPlay("Sonidos\\XWing_engine.mp3", "xwing_engine");
                    break;
                case SONIDOS.XWING_BOMB:
                    AgregarElemento(new Sonido("Sonidos\\Xwing_bomb_sound.wav", 0, 2, 1, 0, ""));
                    break;
                case SONIDOS.EXPLOSION_FINAL:
                    FindAndPlay("Sonidos\\final_explotion.mp3", "explosion_final");
                    break;
                case SONIDOS.FORCE_THEME:
                    FindAndPlay("Sonidos\\Force_Theme.mp3", "story_menu");
                    break;
                case SONIDOS.DAMAGE:
                    AgregarElemento(new Sonido("Sonidos\\Damage.wav", 0, 5, 1, 0, ""));
                    break;
            }
         }


        public enum SONIDOS
        {
            DISPARO_MISIL_XWING, DISPARO_MISIL_ENEMIGO, BACKGROUND_BATTLE,
            LUKE_OBI_WAN, MAIN_MENU, PAUSE, FLYBY_2, XWING_ENGINE,NO_SOUND,XWING_BOMB,EXPLOSION_FINAL,
            FORCE_THEME,DAMAGE,EXPLOSION_TORRETA
        }

        private void FindAndPlay(string path, string id)
        {
            ISoundElement found = AddOrGetMp3(path, id, false);
            found.Play();
        }

        private void FindAndPlay(string path, string id, bool oneTimePlayed)
        {
            ISoundElement found = AddOrGetMp3(path, id, oneTimePlayed);
            found.Play();
        }

        public ISoundElement AddOrGetMp3(string path, string id, bool oneTimePlayed)
        {
            ISoundElement found = elems.Find((x) => id.Equals(x.GetID()));
            if (found == null)
            {
                ISoundElement newElement = new SonidoWmp(path, id, oneTimePlayed);
                AgregarElemento(newElement);
                return newElement;
            } else
            {
                return found;
            }
        }
    }
}
