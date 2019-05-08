using System;
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
    class XwingEnemigo : IActiveElement
    {
        private TgcScene nave;
        private TGCVector3 velocidad;
        private Xwing target;

        public XwingEnemigo(TGCVector3 posicionInicial,Xwing target)
        {
            velocidad = new TGCVector3(0,0,10f);
            nave = new TgcSceneLoader().loadSceneFromFile("Padawans_media\\XWing\\X-Wing-TgcScene.xml");
            nave.Meshes.ForEach(mesh => { mesh.Position = posicionInicial; });
            nave.Meshes.ForEach(mesh => { mesh.RotateY(-FastMath.PI_HALF); });
            nave.Meshes.ForEach(mesh => { mesh.Scale= new TGCVector3(0.1f, 0.1f, 0.1f); });
            this.target = target;
        }

        public void Update(float elapsedTime)
        {
            nave.Meshes.ForEach(mesh => { mesh.Position += velocidad*elapsedTime; });
        }

        public void Render()
        {
            nave.Meshes.ForEach(mesh => { mesh.Transform = TGCMatrix.Translation(mesh.Position); });
            nave.RenderAll();
        }

        public bool Terminado()
        {
            return false;
        }

        public void RenderBoundingBox()
        {
            nave.Meshes.ForEach(mesh=> { mesh.BoundingBox.Render(); });
        }

        public void Dispose()
        {
            nave.DisposeAll();
        }
    }
}
