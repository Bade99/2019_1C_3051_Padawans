using System.Collections.Generic;
using TGC.Core.SceneLoader;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    class WorldSphere : SceneElement 
    {
        private TgcSceneLoader loader;
        TgcScene worldsphere; //@tambien puedo usar tgcskybox, habria que crear los png para las texturas de cada cara
        Xwing pivot;
        //private TGCBooleanModifier alphaModifier;

        public WorldSphere(TgcSceneLoader loader, Xwing pivot)
        {
            this.loader = loader;
            this.pivot = pivot;
            worldsphere = loader.loadSceneFromFile("Padawans_media\\XWing\\GeodesicSphere02-TgcScene.xml");
        }

        public override void Render()
        {

            var scaler = new TGCVector3(100f, 100f, 100f);

            foreach (TgcMesh mesh in worldsphere.Meshes)
            {
                    mesh.Scale = scaler;
                    //mesh.Position = pivot.GetPosition();
            }
            worldsphere.RenderAll();
        }

        public override void Update()
        {
            //@hacer que se mueva con el xwing
            foreach (TgcMesh mesh in worldsphere.Meshes)
            {
                mesh.Position = pivot.GetPosition();
            }
        }

        public override void Dispose()
        {
            worldsphere.DisposeAll();
        }

        public override void RenderBoundingBox()
        {
            worldsphere.BoundingBox.Render();
        }

    }
}
