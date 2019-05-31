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
    ///     Contiene el angulo acimutal y polar para representar las coordenadas esfericas
    /// </summary>
    ///

    public class CoordenadaEsferica
    {
        public float acimutal { get; set; }
        public float polar { get; set; }

        public CoordenadaEsferica(TGCVector3 rotation)
        {
            polar = CommonHelper.ClampPositiveRadians(-rotation.Z + (FastMath.PI_HALF));
            acimutal = CommonHelper.ClampPositiveRadians(-rotation.Y);
        }
        /**
         * Constructor de coordenada esferica utilizando coordenadas cartesianas.
         * No hacer clamp del angulo, ya que lo uso para medir distancias de angulos y me conviene que queden
         * en negativo
         * */
        public CoordenadaEsferica(float x, float y, float z)
        {
            if (x != 0)
            {
                acimutal = FastMath.Atan(z / x);
            } else
            {
                acimutal = FastMath.PI_HALF;
            }
            if (y!=0)
            {
                polar = FastMath.Atan(FastMath.Pow((x*x+z*z), 0.5f) / y);
            } else
            {
                polar = FastMath.PI_HALF;
            }
        }

        public CoordenadaEsferica(float acimutal, float polar)
        {
            //El polar va entre 0 y pi
            this.polar = CommonHelper.ClampPositiveRadians(polar, FastMath.PI);
            //El acimutal va entre 0 y 2pi
            this.acimutal = CommonHelper.ClampPositiveRadians(acimutal);
        }
        public TGCVector3 GetXYZCoord()
        {
            return new TGCVector3(GetXCoord(), GetYCoord(), GetZCoord());
        }

        public float GetXCoord()
        {
            return FastMath.Cos(acimutal) * FastMath.Sin(polar);
        }

        public float GetYCoord()
        {
            return FastMath.Cos(polar);
        }

        public float GetZCoord()
        {
            return FastMath.Sin(acimutal) * FastMath.Sin(polar);
        }
    }
}