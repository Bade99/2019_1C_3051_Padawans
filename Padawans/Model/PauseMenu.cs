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
        //Main window
        private CustomSprite fondo;
        private CustomBitmap bitmap_fondo;
        //
        private struct Rect
        {
            public TGCVector2 top_right, bottom_left, bottom_right;
        }
        private Rect main_rect;

        //Sound icon
        private CustomSprite sound;
        private CustomBitmap bitmap_sound;
        private CustomBitmap bitmap_sound_off;
        //
        private Drawer2D drawer2D;
        private bool isCurrent = false;
        private Key mappedKey;
        public PauseMenu(Microsoft.DirectX.DirectInput.Key mappedKey)
        {//recibe un key.algo para la key que abre y cierra el menu
            this.mappedKey = mappedKey;
            drawer2D = new Drawer2D();
            fondo = new CustomSprite();
            bitmap_fondo = new CustomBitmap(VariablesGlobales.mediaDir + "Bitmaps\\pause_menu.png", D3DDevice.Instance.Device);
            fondo.Bitmap = bitmap_fondo;
            CalcularFullScreenScalingAndPosition(fondo);
            sound = new CustomSprite();
            bitmap_sound = new CustomBitmap(VariablesGlobales.mediaDir + "Bitmaps\\sound_icon.png", D3DDevice.Instance.Device);
            bitmap_sound_off = new CustomBitmap(VariablesGlobales.mediaDir + "Bitmaps\\sound_off_icon.png", D3DDevice.Instance.Device);
            CalcularChildScalingAndPosition(sound,bitmap_sound/*lo uso como base, total el de sound on y off son iguales*/
                ,.90f,.90f,.05f,.1f,main_rect);//90% de x 90% de y , .5% del tamaño de la pantalla
        }//@ ver si hay alguna forma de no tener q pasar los dos scale, usar aspect_ratio del rect capaz

        public bool CheckStartKey(TgcD3dInput input)
        {
            if (input.keyPressed(mappedKey)) {
                isCurrent = true;
                VariablesGlobales.managerSonido.PauseAll();
                VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.PAUSE);
                return true;
            } 
            return false;
        }
        public void Update(TgcD3dInput input)
        {
            if (input.keyPressed(mappedKey))
            {
                isCurrent = false;
                VariablesGlobales.managerSonido.RemoveID(SoundManager.SONIDOS.PAUSE);
                VariablesGlobales.managerSonido.ResumeAll();
            }
            if (input.keyPressed(Key.M))
            {
                VariablesGlobales.managerSonido.MuteOrUnMute();
            }
        }
        public void Render()
        {

            VariablesGlobales.postProcess.RenderMenuPostProcess("oscurecer");

            //Terminamos el renderizado de la escena
            drawer2D.BeginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            drawer2D.DrawSprite(fondo);
            
            //Sound icon
            if (VariablesGlobales.SOUND) sound.Bitmap = bitmap_sound;
            else sound.Bitmap = bitmap_sound_off;
            drawer2D.DrawSprite(sound);
            //

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
        private void CalcularFullScreenScalingAndPosition(CustomSprite main)
        {//para 1280x720 lo transforma en 2048x1024 (no tiene mucha precision)
            var tamanio_textura = main.Bitmap.Size;
            main.ScalingCenter = new TGCVector2(0, 0);
            if (D3DDevice.Instance.AspectRatio < 2.1f)//16:9 o parecido
            {
                main.Position = new TGCVector2(0, 0);

                float width = (float)D3DDevice.Instance.Width / (float)tamanio_textura.Width;
                float height = (float)D3DDevice.Instance.Height / (float)tamanio_textura.Height;
                main.Scaling = new TGCVector2(width, height);
            }
            else //21:9 o parecido
            {
                main.Position = new TGCVector2(((float)D3DDevice.Instance.Width - ((float)D3DDevice.Instance.Width * 16f / 21.6f)) / 2, 0);
                float width = ((float)D3DDevice.Instance.Width * 16f / 21.6f) / (float)tamanio_textura.Width;
                float height = (float)D3DDevice.Instance.Height / (float)tamanio_textura.Height;
                main.Scaling = new TGCVector2(width, height);
            }
            var tr_x = main.Position.X + tamanio_textura.Width * main.Scaling.X;
            var tr_y = main.Position.Y;
            main_rect.top_right = new TGCVector2(tr_x,tr_y);
            var bl_x = main.Position.X;
            var bl_y = main.Position.Y + tamanio_textura.Height * main.Scaling.Y;
            main_rect.bottom_left = new TGCVector2(bl_x,bl_y);

            var br_x = tr_x;
            var br_y = bl_y;
            main_rect.bottom_right = new TGCVector2(br_x,br_y);
            //@asignar main_rect not so easy to do
        }
        private void CalcularChildScalingAndPosition(CustomSprite child,CustomBitmap bitmap_base,float relX,float relY,float relScaleX, float relScaleY, Rect main)
        {
            float posX = main.bottom_right.X - main.bottom_left.X;
            float posY = main.bottom_right.Y - main.top_right.Y;
            child.Position = new TGCVector2(posX * relX, posY * relY);
            child.Scaling = CalculeRelativeScaling(bitmap_base, posX,posY,relScaleX,relScaleY);//@capaz usar un scale para cada eje?
        }
        private TGCVector2 CalculeRelativeScaling(CustomBitmap bitmap,float width,float height, float scale_x,float scale_y)
        {//hace el calculo sobre width
            float screen_occupation_x = width * scale_x;
            float screen_occupation_y = height * scale_y;
            //float screen_ratio = D3DDevice.Instance.Width / bitmap.Width;
            float escala_real_x = screen_occupation_x / bitmap.Width;
            float escala_real_y = screen_occupation_y / bitmap.Height;
            return new TGCVector2(escala_real_x, escala_real_y);
        }
    }
}
