using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using BulletSharp.Math;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    class Debug_Draw_Bullet : DebugDraw
    {
        public override DebugDrawModes DebugMode { get; set; }
        private TgcLine linea;

        public Debug_Draw_Bullet()
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
            linea.Color = Color.Red;
            linea.Enabled = true;
            linea.updateValues();
            linea.Render();
        }

        public override void ReportErrorWarning(string warningString)
        {
            throw new Exception(warningString);
        }
    }
}
