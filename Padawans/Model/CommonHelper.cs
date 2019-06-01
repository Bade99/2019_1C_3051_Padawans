using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Una clase de ejemplo que grafica pistas para poder tener una referencia de la velocidad de la nave
    /// </summary>
    public class CommonHelper
    {

        public static TGCVector3 vectorMitad = new TGCVector3(0.5f, 0.5f, 0.5f);
        public static TGCVector3 vectorDecima = new TGCVector3(0.1f, 0.1f, 0.1f);
        public readonly static float PI = (float)FastMath.PI;
        public static float TRES_MEDIO_PI = PI + FastMath.PI_HALF;

        public static TGCVector3 SumarVectores(TGCVector3 vector1, TGCVector3 vector2)
        {
            return new TGCVector3(vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z);
        }

        public static TGCVector3 RestarVectores(TGCVector3 vector1, TGCVector3 restando)
        {
            return new TGCVector3(vector1.X - restando.X, vector1.Y - restando.Y, vector1.Z - restando.Z);
        }

        public static TGCVector3 MultiplicarVectores(TGCVector3 a,TGCVector3 b)
        {
            return new TGCVector3(a.X*b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static TGCVector3 VectorXEscalar(TGCVector3 v, float f)
        {
            return new TGCVector3(v.X * f, v.Y * f, v.Z * f);
        }
        /**
         * Clampea el angulo del primer parametro entre 0 y el valor del segundo parametro
         * */
        public static float ClampPositiveRadians(float radians, float angulo)
        {
            while (radians < 0)
            {
                radians += angulo;
            }
            return radians % (angulo);
        }

        /**
         * Clampea el angulo por parametro entre 0 y 2pi
         * */
        public static float ClampPositiveRadians(float radians)
        {
            return ClampPositiveRadians(radians, FastMath.TWO_PI);
        }

        public static void ClampRotationZ(TgcMesh rotando)
        {
            if (rotando.Rotation.Z > (FastMath.PI * 2))
            {
                rotando.RotateZ(-(FastMath.PI * 2));
            }
            if (rotando.Rotation.Z < 0)
            {
                rotando.RotateZ((FastMath.PI * 2));
            }
        }

        public static TGCVector3 RotateY180(TGCVector3 rotando)
        {
            rotando.Y += (FastMath.PI);
            ClampRotationY(rotando);
            return rotando;
        }

        public static TGCVector3 ClampRotationY(TGCVector3 rotation)
        {
            if (rotation.Y > (FastMath.PI * 2))
            {
                rotation.Y -= (FastMath.PI * 2);
            }
            if (rotation.Y < 0)
            {
                rotation.Y += (FastMath.PI * 2);
            }
            return rotation;
        }

        public static TGCMatrix MatrixXEscalar(TGCMatrix m, float f)
        {
            m.M11 *= f;
            m.M12 *= f;
            m.M13 *= f;
            m.M14 *= f;

            m.M21 *= f;
            m.M22 *= f;
            m.M23 *= f;
            m.M24 *= f;

            m.M31 *= f;
            m.M32 *= f;
            m.M33 *= f;
            m.M34 *= f;

            m.M41 *= f;
            m.M42 *= f;
            m.M43 *= f;
            m.M44 *= f;

            return m;
        } 
        public TGCVector3 DividirPorEscalar(TGCVector3 v, float e)
        {
            return new TGCVector3(v.X / e, v.Y / e, v.Z / e);
        }
        public TGCVector3 Normalize(TGCVector3 v)
        {
            float magnitude=FastMath.Sqrt(FastMath.Pow2(v.X) + FastMath.Pow2(v.Y) + FastMath.Pow2(v.Z));
            return DividirPorEscalar(v,magnitude);
        }
        public static float GetArgumento(float a, float b)
        {
            if (a != 0)
            {
                int cuadrante = Cuadrante(a, b);
                float angulo = FastMath.Atan(b / a);
                if (cuadrante == 2 || cuadrante == 3)
                {
                    angulo += CommonHelper.PI;
                }
                if (cuadrante == 4)
                {
                    angulo += FastMath.TWO_PI;
                }
                return angulo;
            }
            else
            {
                return (b >= 0) ? FastMath.PI_HALF : TRES_MEDIO_PI;
            }
        }
        public static int Cuadrante(double a, double b)
        {
            if (a >= 0)
            {
                if (b >= 0)
                {
                    return 1;
                }
                else
                {
                    return 4;
                }
            }
            else
            {
                if (b >= 0)
                {
                    return 2;
                }
                else
                {
                    return 3;
                }
            }
        }
        public static TGCVector3 MoverPosicionEnDireccionCoordenadaEsferica(TGCVector3 posicion, 
            CoordenadaEsferica coordenadaEsferica, float speed, float timeLapse)
        {
            float x = posicion.X + speed * coordenadaEsferica.GetXCoord() * timeLapse;
            float y = posicion.Y + speed * coordenadaEsferica.GetYCoord() * timeLapse;
            float z = posicion.Z + speed * coordenadaEsferica.GetZCoord() * timeLapse;
            return new TGCVector3(x, y, z);
        }
    }
}