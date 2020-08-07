using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Sound;

namespace TGC.Group.Model
{
    class Sonido : ISoundElement//WAV desde 8 a 32 bit
    {
        private string path;
        private TgcStaticSound sonido;
        bool terminado = false;
        float duracion;
        float delay;
        bool infinito = false;
        bool paused = false;
        int volumen;
        string ID;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">La direccion sin mediaDir</param>
        /// <param name="volumen">Va entre 0(max) y -10000(min) en centenas de decibel</param>
        /// <param name="duracion"></param>
        /// <param name="repeticiones"></param>
        /// <param name="delay">Tiempo hasta que empiece el sonido</param>
        /// <param name="ID">Un nombre por si necesitas encontrar ese sonido, usar "" para no-name</param>
        public Sonido(string path,int volumen,float duracion,int repeticiones/*-1 = infinito*/,float delay,string ID/*usar "" pa indicar no ID*/)//volumen es atenuacion en hundredths of a decibel -> entre 0 y -10000 https://docs.microsoft.com/en-us/previous-versions/ms817348(v=msdn.10)
        {
            this.path = path;
            this.volumen = volumen;
            this.sonido = new TgcStaticSound();
            if (this.volumen > 0) this.volumen = 0;
            sonido.loadSound(VariablesGlobales.mediaDir + this.path,this.volumen, VariablesGlobales.soundDevice);
            this.ID = ID;
            this.duracion = duracion*repeticiones;
            if (repeticiones < 0) infinito = true;
            //this.repeticiones = repeticiones;
        }
        public bool IsStoppeado()
        {
            return false;
        }
        public void Update()
        {
            if (paused) return;
            CheckMute();
            if (delay > 0f)
            {
                delay -= VariablesGlobales.elapsedTime;
                return;
            }
            if (infinito) sonido.play(true);
            else if (!terminado)
            {
                if (duracion > 0.0f)
                {
                    duracion -= VariablesGlobales.elapsedTime;

                    try
                    {
                        sonido.play(true);
                    }
                    catch (System.Exception e)
                    {
                        terminado = true;
                    }
                }
                else
                {
                    terminado = true;
                    sonido.stop();
                }
            }
        }
        private void CheckMute()
        {
            if (VariablesGlobales.SOUND) sonido.SoundBuffer.Volume = volumen;
            else sonido.SoundBuffer.Volume = -10000;
        }
        public string GetID()
        {
            return ID;
        }
        public void Dispose()
        {
            sonido.dispose();
        }

        public void Pause()
        {
            sonido.stop();
            paused = true;
        }
        public void Resume()
        {
            sonido.play(true);
            paused = false;
        }

        public void Terminar() { terminado = true; }

        public bool Terminado()
        {
            return terminado;
        }

        public void Play()
        {
            return;
        }

        public void Stop()
        {
            return;
        }
    }
}
