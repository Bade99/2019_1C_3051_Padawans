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
            polar = CommonHelper.ClampPositiveRadians(-rotation.Z + (FastMath.PI_HALF), FastMath.PI);
            acimutal = CommonHelper.ClampPositiveRadians(-rotation.Y);
        }
        public CoordenadaEsferica(float x, float y, float z)
        {
            acimutal = CommonHelper.GetArgumento(x, z);
            if (y!=0)
            {
                polar = FastMath.Atan(FastMath.Pow((x*x+z*z), 0.5f) / y);
            } else
            {
                polar = FastMath.PI_HALF;
            }
            polar = CommonHelper.ClampPositiveRadians(polar, FastMath.PI);
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

        public CoordenadaEsferica Diferencia(CoordenadaEsferica coordenadaEsferica)
        {
            float deltaAcimutal = FastMath.Abs(this.acimutal - coordenadaEsferica.acimutal);
            float deltaPolar = FastMath.Abs(this.polar - coordenadaEsferica.polar);
            return new CoordenadaEsferica(FastMath.Min(deltaAcimutal, FastMath.TWO_PI - deltaAcimutal),
                FastMath.Min(deltaPolar, FastMath.TWO_PI - deltaPolar));
        }
    }
}