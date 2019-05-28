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
        private CustomSprite cue;
        private CustomBitmap bitmap;
        private Drawer2D drawer2D;
        private float delay;
        private float duracion;
        private bool terminado = false;
        private bool play_sound = true;
        private string sonido_path;
        private float sonido_duracion;
        private int sonido_volumen;

        public Cue(string bitmap_path,float relative_scale,TGCVector2 relative_pos,float delay,float duracion,string sonido_path,float sonido_duracion,int sonido_volumen)
        {//despues agrego mas condiciones para que una cue inicie, ademas de delay
            drawer2D = new Drawer2D();
            cue = new CustomSprite();
            bitmap = new CustomBitmap(VariablesGlobales.mediaDir + bitmap_path, D3DDevice.Instance.Device);
            //@no se xq tiene ese borde azul cuando lo renderiza???
            cue.Bitmap = bitmap;

            //cue.ScalingCenter = new TGCVector2((float)cue.Bitmap.Size.Width / 2f, (float)cue.Bitmap.Size.Height / 2f);
            cue.ScalingCenter = new TGCVector2(0, 0);
            cue.Scaling = CalculeRelativeScaling(bitmap,relative_scale);
            cue.Position = new TGCVector2(D3DDevice.Instance.Width* relative_pos.X, D3DDevice.Instance.Height* relative_pos.Y);
            this.delay = delay;
            this.duracion = duracion;
            this.sonido_path = sonido_path;
            this.sonido_duracion = sonido_duracion;
            this.sonido_volumen = sonido_volumen;
        }
        public TGCVector2 CalculeRelativeScaling(CustomBitmap bitmap,float scale)
        {//hace el calculo sobre width
            float screen_occupation = D3DDevice.Instance.Width * scale;
            //float screen_ratio = D3DDevice.Instance.Width / bitmap.Width;
            float escala_real = screen_occupation/ bitmap.Width;
            return new TGCVector2(escala_real, escala_real); 
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
                if (play_sound) { VariablesGlobales.managerSonido.AgregarElemento(new Sonido(sonido_path, sonido_volumen, sonido_duracion, 1, 0)); play_sound = false; }
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
