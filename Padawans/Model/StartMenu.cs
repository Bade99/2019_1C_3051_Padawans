using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Input;

namespace TGC.Group.Model
{
    class StartMenu : IMenu //@queda calcular el escalado de la textura
    {
        private CustomSprite fondo;
        private CustomBitmap bitmap;
        private Drawer2D drawer2D;
        private bool isCurrent= true;
        private Microsoft.DirectX.DirectInput.Key mappedKey;

        public StartMenu(Microsoft.DirectX.DirectInput.Key mappedKey) {//recibe un key.algo para la key que abre y cierra el menu
            this.mappedKey = mappedKey;
            drawer2D = new Drawer2D();
            fondo = new CustomSprite();
            bitmap = new CustomBitmap(VariablesGlobales.mediaDir + "Bitmaps\\startmenu.bmp", D3DDevice.Instance.Device);
            fondo.Bitmap = bitmap;
            var tamanio_textura = fondo.Bitmap.Size;
            fondo.Position = new TGCVector2(FastMath.Max(D3DDevice.Instance.Width / 2 - tamanio_textura.Width / 2, 0), FastMath.Max(D3DDevice.Instance.Height / 2 - tamanio_textura.Height / 2, 0));
            VariablesGlobales.managerSonido.AgregarElemento(new SonidoMP3("Sonidos\\main_menu.mp3",0,-1,0));
        }
        public bool CheckStartKey(TgcD3dInput input)
        {
            return false;
        }

        public void Update(TgcD3dInput input)
        {
            if (input.keyPressed(mappedKey)) {
                isCurrent = false;
                VariablesGlobales.managerSonido.Remove("Sonidos\\main_menu.mp3");
            }
        }
        public void Render() {
            drawer2D.BeginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            drawer2D.DrawSprite(fondo);
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
