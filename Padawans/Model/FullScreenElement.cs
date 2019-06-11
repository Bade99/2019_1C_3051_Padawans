using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    class FullScreenElement
    {
        private CustomSprite fondo;
        private CustomBitmap bitmap;
        private Drawer2D drawer2D;
        float duracion;

        /// <summary>
        /// Crea un elemento que se ajusta al tamaño de la pantalla, la duracion es del render, NO del sonido
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="sonido"></param>
        /// <param name="duracion"></param>
        public FullScreenElement(string dir,SoundManager.SONIDOS sonido,float duracion)
        {
            this.duracion = duracion;
            drawer2D = new Drawer2D();
            fondo = new CustomSprite();
            bitmap = new CustomBitmap(VariablesGlobales.mediaDir + dir, D3DDevice.Instance.Device);
            fondo.Bitmap = bitmap;
            CalcularFullScreenScalingAndPosition(fondo);
            VariablesGlobales.managerSonido.ReproducirSonido(sonido);
        }
        public void Render()
        {
            if (duracion > 0)
            {
                duracion -= VariablesGlobales.elapsedTime;

                drawer2D.BeginDrawSprite();

                //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
                drawer2D.DrawSprite(fondo);

                //Finalizar el dibujado de Sprites
                drawer2D.EndDrawSprite();
            }
        }

        public void CalcularFullScreenScalingAndPosition(CustomSprite fondo)
        {//para 1280x720 lo transforma en 2048x1024 (no tiene mucha precision)
            var tamanio_textura = fondo.Bitmap.Size;
            fondo.ScalingCenter = new TGCVector2(0, 0);
            if (D3DDevice.Instance.AspectRatio < 2.1f)//16:9 o parecido
            {
                fondo.Position = new TGCVector2(0, 0);

                float width = (float)D3DDevice.Instance.Width / (float)tamanio_textura.Width;
                float height = (float)D3DDevice.Instance.Height / (float)tamanio_textura.Height;
                fondo.Scaling = new TGCVector2(width, height);
            }
            else //21:9 o parecido
            {
                fondo.Position = new TGCVector2(((float)D3DDevice.Instance.Width - ((float)D3DDevice.Instance.Width * 16f / 21.6f)) / 2, 0);
                float width = ((float)D3DDevice.Instance.Width * 16f / 21.6f) / (float)tamanio_textura.Width;
                float height = (float)D3DDevice.Instance.Height / (float)tamanio_textura.Height;
                fondo.Scaling = new TGCVector2(width, height);
            }
        }
    }
}
