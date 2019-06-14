using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Group.Model
{
    class DebugDrawerTest : DebugDraw
    {
        public override DebugDrawModes DebugMode { get; set; }

        public override void Draw3DText(ref Vector3 location, string textString)
        {
            
        }

        public override void DrawLine(ref Vector3 from, ref Vector3 to, ref Vector3 color)
        {
            //Fran, podes hacer que este metodo dibuje una linea de from a to?

        }

        public override void ReportErrorWarning(string warningString)
        {

        }
    }
}
