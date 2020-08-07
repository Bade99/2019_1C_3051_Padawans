using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMPLib;
namespace TGC.Group.Model
{
    class Wmp : ISoundElement
    {
        private WindowsMediaPlayer myPlayer;
        private string id;
        private bool stoppeado = false;

        public bool IsStoppeado()
        {
            return stoppeado;
        }
        public Wmp(string path, string id)
        {
            this.id = id;
            myPlayer = new WindowsMediaPlayer();
            myPlayer.URL = VariablesGlobales.mediaDir + path;
        }
        public void Dispose()
        {
            return;
        }

        public string GetID()
        {
            return this.id;
        }

        public void Pause()
        {
            myPlayer.controls.pause();
        }

        public void Resume()
        {
            myPlayer.controls.play();
        }

        public bool Terminado()
        {
            return stoppeado;
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
            myPlayer.controls.play();
            stoppeado = false;
        }

        public void Update()
        {
            return;
        }
    }
}
