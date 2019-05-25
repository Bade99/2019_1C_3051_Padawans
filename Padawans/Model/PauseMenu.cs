using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    class PauseMenu : IMenu //la idea es que el menu de pausa sea semi opaco, con color negro, y se vea atras el juego, no se como hacerlo por ahora
    {
        private CustomSprite fondo;
        private CustomBitmap bitmap;
        private Drawer2D drawer2D;
        private bool isCurrent = false;
        private Microsoft.DirectX.DirectInput.Key mappedKey;
        private string path = "Sonidos\\main_menu.wav";


        public PauseMenu(Microsoft.DirectX.DirectInput.Key mappedKey)
        {//recibe un key.algo para la key que abre y cierra el menu
            this.mappedKey = mappedKey;
            drawer2D = new Drawer2D();
            fondo = new CustomSprite();
            bitmap = new CustomBitmap(VariablesGlobales.mediaDir + "Bitmaps\\pause_menu.png", D3DDevice.Instance.Device);
            fondo.Bitmap = bitmap;
            //fondo.ScalingCenter = new TGCVector2( D3DDevice.Instance.Width / 2, D3DDevice.Instance.Height / 2);
            fondo.Scaling = new TGCVector2(1,.6f);
            var tamanio_textura = fondo.Bitmap.Size;
            fondo.Position = new TGCVector2(FastMath.Max(D3DDevice.Instance.Width / 2 - tamanio_textura.Width / 2, 0), FastMath.Max(D3DDevice.Instance.Height / 2 - tamanio_textura.Height / 2, 0));
        }
        public bool CheckStartKey(TgcD3dInput input)
        {
            if (input.keyPressed(mappedKey)) {
                isCurrent = true;
                VariablesGlobales.managerSonido.PauseAll();
                //este agregar es el problema!!!!
                VariablesGlobales.managerSonido.AgregarElemento(new Sonido(path, 0, 0, -1, 0));
                return true;
            } 
            return false;
        }
        public void Update(TgcD3dInput input)
        {
            if (input.keyPressed(mappedKey))
            {
                isCurrent = false;
                VariablesGlobales.managerSonido.Remove(path);
                VariablesGlobales.managerSonido.ResumeAll();
            }
        }
        public void Render()
        {
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
    }
}
