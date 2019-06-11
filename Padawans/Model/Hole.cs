using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
namespace TGC.Group.Model
{
    class Hole : IPostProcess//para indicar donde tiene que tirar la bomba
    {
        TgcScene hole;
        TGCVector3 pos;
        public Hole(TGCVector3 position)
        {
            pos = position;
            hole = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + "Hole\\hole-TgcScene.xml");
            hole.Meshes.ForEach(m => m.Position = position);
            hole.Meshes.ForEach(m => m.Scale = new TGCVector3(1.5f, 1.5f, 1.5f));
            hole.Meshes.ForEach(m => m.RotateZ(FastMath.PI));
        }
        public void Render()
        {
            if(CheckDist())
                hole.RenderAll();
        }

        public void RenderPostProcess(string effect)
        {
            if(CheckDist())
                if(effect== "bloom")
                    hole.RenderAll();
        }
        public bool CheckDist()
        {
            return pos.Z + 1500 > VariablesGlobales.xwing.GetPosition().Z && VariablesGlobales.xwing.GetPosition().Z > pos.Z - 1500;
        }
    }
}
