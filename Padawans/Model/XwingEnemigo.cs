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
        private TemporaryElementManager managerDisparos;
        private TGCVector3 scale = new TGCVector3(.1f, .1f, .1f);
        private TGCVector3 posicion;

        public XwingEnemigo(TGCVector3 posicionInicial,Xwing target, TemporaryElementManager managerDisparos)
        {
            posicion = posicionInicial;
            velocidad = new TGCVector3(0,0,70f);
            nave = new TgcSceneLoader().loadSceneFromFile(VariablesGlobales.mediaDir+"XWing\\X-Wing-TgcScene.xml");//@ésta deberia ser nuestra nave, no la enemiga!
            nave.Meshes.ForEach(mesh => {
                mesh.AutoTransformEnable = false;
                mesh.Transform = TGCMatrix.Scaling(scale) * TGCMatrix.RotationY(-FastMath.PI_HALF) * TGCMatrix.Translation(posicion);
            });
            this.target = target;
            this.managerDisparos = managerDisparos;
        }

        public void Update(float elapsedTime)
        {
            elapsedTime = 0.01f;
        }

        public void Render()
        {
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
