using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Sound;

namespace TGC.Group.Model
{
    class SonidoMP3 : ISoundElement
    {
        private string path;
        private TgcMp3Player sonido;
        bool terminado = false;
        float duracion;
        float delay;
        bool infinito = false;
        bool paused = false;

        public SonidoMP3(string path, float duracion, int repeticiones/*-1 para infinito*/,float delay/*delay antes del 1er inicio*/)
        {//@@RECOMIENDO NO USAR MP3, TARDAN MUCHISIMO EN CARGAR, supongo q xq necesita hacer la descompresion y conversion
            this.path = path;
            this.sonido = new TgcMp3Player();
            sonido.FileName = VariablesGlobales.mediaDir + path;

            this.duracion = duracion * repeticiones;
            if (repeticiones < 0) infinito = true;

            try
            {
                sonido.play(true);
            }
            catch (Exception e) { terminado = true; }
        }
        public void Update()
        {
            if (paused) return;
            if (delay > 0f)
            {
                delay -= VariablesGlobales.elapsedTime;
                return;
            }
            if (!infinito) {
                if (!terminado)
                {
                    if (duracion > 0.0f) duracion -= VariablesGlobales.elapsedTime;
                    else terminado = true;
                }

            }
        }
        public bool Terminado()
        {
            return terminado;
        }
        public void Pause()
        {
            sonido.pause();
            paused = true;
        }
        public void Resume()
        {
            sonido.resume();
            paused = false;
        }

        public string GetPath() { return path; }
        public void Terminar() { terminado = true; }
        public void Dispose()
        {
            sonido.closeFile();
        }
    }
}
