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
        private TgcCylinder misil;//queria usar cilindro pero no tiene las tapas, wtf
        private TGCVector3 escala;
        private TGCVector3 rotacion;
        private float velocidadZ;
        private float tiempoDeVida=10f;
        private bool terminado = false;

        public Misil(TGCVector3 posicionXwing)
        {
            misil = new TgcCylinder(TGCVector3.Empty, 1, 1);
            escala = new TGCVector3(5f, 5f, 5f);//podria recibir estos valores como parametro tmb
            rotacion = new TGCVector3(0,FastMath.PI_HALF ,FastMath.PI_HALF);
            velocidadZ = 5f;

            //misil.AutoTransform = true;
            //misil.AutoTransformEnabled = false;
            //misil.setTexture
            //misil.UseTexture
            //misil.TopRadius = 3f;
            //misil.BottomRadius = 3f;
            //misil.updateValues
            misil.Color = Color.Red;

            misil.Position = posicionXwing;
            misil.Rotation = rotacion;
            misil.Scale = escala;


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
                misil.Position += new TGCVector3(0, FastMath.Sin(rotacion.Y-FastMath.PI_HALF), -velocidadZ);
                misil.Transform = TGCMatrix.Scaling(misil.Scale);
                misil.Transform *= TGCMatrix.RotationYawPitchRoll(misil.Rotation.Y, misil.Rotation.X, misil.Rotation.Z);
                misil.Transform *= TGCMatrix.Translation( misil.Position);
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
                misil.Render();
            }
        }

        public void RenderBoundingBox()
        {
            if (!terminado)
            {
                misil.BoundingCylinder.Render();
            }
        }

        public void Dispose()
        {
            misil.Dispose();
        }

    }
}
