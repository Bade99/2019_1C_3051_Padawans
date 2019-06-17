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
        private TgcLine linea;

        public DebugDrawerTest()
        {
            linea = new TgcLine();
        }

        public override void Draw3DText(ref Vector3 location, string textString)
        {
            
        }

        public override void DrawLine(ref Vector3 from, ref Vector3 to, ref Vector3 color)
        {
            linea.PStart = new TGCVector3(from);
            linea.PEnd = new TGCVector3(to);
            linea.Color = Color.Green;
            linea.Enabled = true;
            linea.updateValues();
            linea.Render();

        }

        public override void ReportErrorWarning(string warningString)
        {

        }
    }
}
