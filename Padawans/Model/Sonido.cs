using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Sound;


namespace TGC.Group.Model
{
    class Sonido : ISoundElement//WAV
    {
        private string path;
        private TgcStaticSound sonido;
        bool terminado = false;
        float duracion;
        float delay;
        bool infinito = false;

        public Sonido(string path,int volumen,float duracion,int repeticiones/*-1 = infinito*/,float delay)//volumen es atenuacion en hundredths of a decibel -> entre 0 y -10000 https://docs.microsoft.com/en-us/previous-versions/ms817348(v=msdn.10)
        {
            this.path = path;
            this.sonido = new TgcStaticSound();
            if (volumen > 0) volumen = 0; 
            sonido.loadSound(VariablesGlobales.mediaDir + path,volumen, VariablesGlobales.soundDevice);

            this.duracion = duracion*repeticiones;
            if (repeticiones < 0) infinito = true;
            //this.repeticiones = repeticiones;
        }
        public void Update()
        {
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
        public void Dispose()
        {
            sonido.dispose();
        }

        public string GetPath() { return path; }

        public void Terminar() { terminado = true; }

        public bool Terminado()
        {
            return terminado;
        }

    }
}
