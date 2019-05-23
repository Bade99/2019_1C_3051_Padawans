using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Sound;


namespace TGC.Group.Model
{
    class Sonido : ISoundElement//voy a tratar de que se pueda usar tanto para wav como mp3
    {
        private TgcStaticSound sonido;
        bool terminado = false;
        float duracion;
        //int repeticiones;
        bool infinito = false;

        public Sonido(string path,int volumen,float duracion,int repeticiones/*-1 para infinito*/)//@el volumen NO lo uso porque de momento crashea
        {
            this.sonido = new TgcStaticSound();
            sonido.loadSound(VariablesGlobales.mediaDir + path, VariablesGlobales.soundDevice);

            this.duracion = duracion*repeticiones;
            if (repeticiones == -1) infinito = true;
            //this.repeticiones = repeticiones;
        }
        public void Update()
        {
            if (infinito) sonido.play(true);
            else if (!terminado)
            {
                if (duracion > 0.0f)
                {
                    duracion -= VariablesGlobales.elapsedTime;

                    try //necesario por las dudas para los mp3
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

        public bool Terminado()
        {
            return terminado;
        }

    }
}
