using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;

namespace TGC.Group.Model
{
    public class Misil : ITemporaryElement
    {
        private TGCBox misil;//queria usar cilindro pero no tiene las tapas, WTF
        private TGCVector3 escala;
        private TGCVector3 rotacion;
        private float velocidadZ;
        private float tiempoDeVida=10f;
        private bool terminado = false;

        public Misil(TGCVector3 posicionXwing,TGCVector3 offset)
        {
            misil = new TGCBox();
            escala = new TGCVector3(5f, 5f, 5f);//podria recibir estos valores como parametro tmb
            rotacion = new TGCVector3(0,0,0);
            velocidadZ = 5f;

            //misil.AutoTransform = true;
            //misil.AutoTransformEnabled = false;
            //misil.setTexture
            //misil.UseTexture
            //misil.TopRadius = 3f;
            //misil.BottomRadius = 3f;
            //misil.updateValues
            misil.Color = Color.Red;

            //offset = new TGCVector3(5,1,.5f); //indica desde que ala dispara

            misil.Size = new TGCVector3(.1f, .1f, 10);
            misil.Position = posicionXwing+offset;
            misil.Rotation = rotacion;
            //misil.Scale = escala;


            misil.updateValues();
            //misil.Transform = TGCMatrix.Scaling(misil.Scale) * TGCMatrix.RotationYawPitchRoll(misil.Rotation.Y, misil.Rotation.X, misil.Rotation.Z) * TGCMatrix.Translation(misil.Position);

        }

        public void Update(float ElapsedTime)
        {
            tiempoDeVida -= ElapsedTime;
            if (tiempoDeVida < 0f)
            {
                terminado = true;
            }
            else {
                //misil.updateValues();
                //misil.Transform = TGCMatrix.Translation(FastMath.Cos(rotacion.X), FastMath.Sin(rotacion.Y), velocidadZ);
                //misil.Scale += escala;
                misil.Position += new TGCVector3(0, FastMath.Sin(rotacion.Y), -velocidadZ);
                misil.updateValues();

            }
        }

        public bool Terminado()
        {
            return terminado;
        }

        public void Render()
        {
            if (!terminado)
            {
                //misil.Transform = TGCMatrix.Scaling(misil.Scale);
                misil.Transform = TGCMatrix.RotationYawPitchRoll(misil.Rotation.Y, misil.Rotation.X, misil.Rotation.Z);
                misil.Transform *= TGCMatrix.Translation(misil.Position);
                misil.Render();
            }
        }

        public void RenderBoundingBox()
        {
            if (!terminado)
            {
                misil.BoundingBox.Render();
            }
        }

        public void Dispose()
        {
            misil.Dispose();
        }

    }
}
