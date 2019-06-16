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
    class Cue : ICue //lanzador de bitmaps para dar ayudas al jugador ante distintos eventos, ej: al inicio usar WASD para moverse
    {
        //puedo usar png con el CustomBitmap!!!
        private ICueLauncher cueLauncher;
        private CustomSprite cue;
        private CustomBitmap bitmap;
        private Drawer2D drawer2D;
        private float duracion;
        private bool terminado = false;
        private bool play_sound = true;

        public Cue(ICueLauncher cueLauncher, string bitmap_path, float relative_scale, TGCVector2 relative_pos, float duracion)
        {//despues agrego mas condiciones para que una cue inicie, ademas de delay
            this.cueLauncher = cueLauncher;
            drawer2D = new Drawer2D();
            cue = new CustomSprite();
            bitmap = new CustomBitmap(VariablesGlobales.mediaDir + bitmap_path, D3DDevice.Instance.Device);
            //@no se xq tiene ese borde azul cuando lo renderiza???
            cue.Bitmap = bitmap;

            //cue.ScalingCenter = new TGCVector2((float)cue.Bitmap.Size.Width / 2f, (float)cue.Bitmap.Size.Height / 2f);
            cue.ScalingCenter = new TGCVector2(0, 0);
            cue.Scaling = CommonHelper.CalculateRelativeScaling(bitmap,relative_scale);
            cue.Position = new TGCVector2(D3DDevice.Instance.Width* relative_pos.X, D3DDevice.Instance.Height* relative_pos.Y);
            this.duracion = duracion;
        }

        public bool IsCurrent()
        {
            return cueLauncher.IsReady();
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
                if (play_sound) {
                    VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.LUKE_OBI_WAN);
                    play_sound = false;
                }
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
