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
            polar = CommonHelper.ClampPositiveRadians(-rotation.Z + (FastMath.PI / 2));
            acimutal = CommonHelper.ClampPositiveRadians(-rotation.Y);
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