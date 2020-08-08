using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMPLib;
namespace TGC.Group.Model
{
    class SonidoWmp : ISoundElement
    {
        private WindowsMediaPlayer myPlayer;
        private string id;
        private bool stoppeado;
        private bool oneTimePlayed;
        private int normalVolume = 50;

        public bool IsStoppeado()
        {
            return stoppeado;
        }
        public SonidoWmp(string path, string id, bool oneTimePlayed)
        {
            this.id = id;
            this.stoppeado = false;
            this.oneTimePlayed = oneTimePlayed;
            myPlayer = new WindowsMediaPlayer();
            myPlayer.settings.volume = normalVolume;
            myPlayer.URL = VariablesGlobales.mediaDir + path;
        }

        public SonidoWmp(string path, string id) : this(path, id, false)
        {
        }
        public void Dispose()
        {
            return;
        }

        public string GetID()
        {
            return this.id;
        }

        public void mutear()
        {
            myPlayer.settings.volume = 0;
        }
        public void unmute()
        {
            myPlayer.settings.volume = normalVolume;
        }

        public void Pause()
        {
            if (oneTimePlayed)
            {
                Stop();
            } else
            {
                myPlayer.controls.pause();
            }
        }

        public void Resume()
        {
            myPlayer.controls.play();
        }

        public bool Terminado()
        {
            return false;
        }

        public void Terminar()
        {
            return;
        }

        public void Stop()
        {
            myPlayer.controls.pause();
            stoppeado = true;
        }
        public void Play()
        {
            myPlayer.controls.currentPosition = 0;
            myPlayer.controls.play();
            stoppeado = false;
        }

        public void Update()
        {
            return;
        }
    }
}
