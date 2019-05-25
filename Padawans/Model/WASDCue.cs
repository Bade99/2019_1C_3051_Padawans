using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    class WASDCue : ICue //lanzador de bitmaps para dar ayudas al jugador ante distintos eventos, ej: al inicio usar WASD para moverse
    {
        //puedo usar png con el CustomBitmap!!!
        private CustomSprite cue;
        private CustomBitmap bitmap;
        private Drawer2D drawer2D;
        private float delay=1;
        private float duracion=3;
        private bool terminado = false;
        private bool play_sound = true;

        public WASDCue()
        {
            drawer2D = new Drawer2D();
            cue = new CustomSprite();
            bitmap = new CustomBitmap(VariablesGlobales.mediaDir + "Bitmaps\\WASD.png", D3DDevice.Instance.Device);
            //@no se xq tiene ese borde azul cuando lo renderiza???
            cue.Bitmap = bitmap;
            cue.Scaling = new TGCVector2(.7f,.7f);
            cue.Position = new TGCVector2(D3DDevice.Instance.Width*.1f,D3DDevice.Instance.Height*.7f);
            
        }
        public bool IsCurrent()
        {
            if (delay < 0)
            {
                return true;
            }
            else {
                delay -= VariablesGlobales.elapsedTime;
                return false;
            }
        }
        public void Update()
        {
            if (duracion > 0)
            {
                
                duracion -= VariablesGlobales.elapsedTime;
                //si lo logro, ir atenuando el sprite cuando va a terminar y al reves para el inicio
            }
        }
        public void Render()
        {
            if (duracion > 0)
            {
                if (play_sound) { VariablesGlobales.managerSonido.AgregarElemento(new Sonido("Sonidos\\obi_wan_luke.wav", 0, .5f, 1, 0)); play_sound = false; }
                drawer2D.BeginDrawSprite();
                drawer2D.DrawSprite(cue);
                drawer2D.EndDrawSprite();
            }
            else terminado = true;
        }
        public void Dispose()
        {
            cue.Dispose();
        }
        public bool Terminado()
        {
            return terminado;
        }
    }
}
