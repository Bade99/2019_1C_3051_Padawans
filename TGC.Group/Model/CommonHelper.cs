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
       public static TGCVector3 SumarVectores(TGCVector3 vector1, TGCVector3 vector2)
        {
            vector1.X += vector2.X;
            vector1.Y += vector2.Y;
            vector1.Z += vector2.Z;
            return vector1;
        }

        public static float ClampPositiveRadians(float radians)
        {
            if (radians<0)
            {
                return ClampPositiveRadians(radians + (FastMath.PI * 2));
            }
            return radians % (FastMath.PI * 2);
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

        public static void RotateY180(TgcMesh rotando)
        {
            rotando.RotateY(FastMath.PI);
            ClampRotationY(rotando);
        }

        public static void ClampRotationY(TgcMesh rotando)
        {
            if (rotando.Rotation.Y > (FastMath.PI * 2))
            {
                rotando.RotateY(-(FastMath.PI * 2));
            }
            if (rotando.Rotation.Y < 0)
            {
                rotando.RotateY((FastMath.PI * 2));
            }
        }
    }
}