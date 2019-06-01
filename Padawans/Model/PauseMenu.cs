using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.Example;
using Microsoft.DirectX.DirectInput;
namespace TGC.Group.Model
{
    class PauseMenu : IMenu //la idea es que el menu de pausa sea semi opaco, con color negro, y se vea atras el juego, no se como hacerlo por ahora
    {
        private CustomSprite fondo;
        private CustomBitmap bitmap;
        private Drawer2D drawer2D;
        private bool isCurrent = false;
        private Key mappedKey;
        private string path = "Sonidos\\main_menu.wav";
        private string soundID = "pause_menu";
        public PauseMenu(Microsoft.DirectX.DirectInput.Key mappedKey)
        {//recibe un key.algo para la key que abre y cierra el menu
            this.mappedKey = mappedKey;
            drawer2D = new Drawer2D();
            fondo = new CustomSprite();
            bitmap = new CustomBitmap(VariablesGlobales.mediaDir + "Bitmaps\\pause_menu.png", D3DDevice.Instance.Device);
            fondo.Bitmap = bitmap;
            CalcularFullScreenScalingAndPosition(fondo);
        }

        public bool CheckStartKey(TgcD3dInput input)
        {
            if (input.keyPressed(mappedKey)) {
                isCurrent = true;
                VariablesGlobales.managerSonido.PauseAll();
                
                VariablesGlobales.managerSonido.AgregarElemento(new Sonido(path, 0, 0, -1, 0,soundID));
                return true;
            } 
            return false;
        }
        public void Update(TgcD3dInput input)
        {
            if (input.keyPressed(mappedKey))
            {
                isCurrent = false;
                VariablesGlobales.managerSonido.RemoveID(soundID);
                VariablesGlobales.managerSonido.ResumeAll();
            }
            if (input.keyPressed(Key.M))
            {
                VariablesGlobales.managerSonido.MuteOrUnMute();
            }
        }
        public void Render()
        {

            VariablesGlobales.postProcess.RenderPostProcess("oscurecer");

            //Terminamos el renderizado de la escena
            drawer2D.BeginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            drawer2D.DrawSprite(fondo);

            //Finalizar el dibujado de Sprites
            drawer2D.EndDrawSprite();

        }
        public void Dispose()
        {
            fondo.Dispose();
        }
        public bool IsCurrent()
        {
            return isCurrent;
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
