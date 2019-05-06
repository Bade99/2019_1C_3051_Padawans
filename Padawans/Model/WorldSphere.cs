using System.Collections.Generic;
using TGC.Core.SceneLoader;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    class WorldSphere : SceneElement //ver como randomizar la textura para que no se vea tan tileada
    {
        private TgcSceneLoader loader;
        TgcScene worldsphere; //@tambien puedo usar tgcskybox, habria que crear los png para las texturas de cada cara
        Xwing pivot;

        public WorldSphere(TgcSceneLoader loader, Xwing pivot)
        {
            this.loader = loader;
            this.pivot = pivot;
            worldsphere = loader.loadSceneFromFile("Padawans_media\\XWing\\GeodesicSphere02-TgcScene.xml");//tiene un solo mesh
            worldsphere.Meshes[0].Rotation = new TGCVector3(2,5,1);
        }

        public override void Render()
        {

            var scaler = new TGCVector3(100f, 100f, 100f);

            worldsphere.Meshes[0].Scale = scaler;
            worldsphere.Meshes[0].Render();
        }

        public override void Update()
        {
            //@hacer que se mueva con el xwing
            worldsphere.Meshes[0].Position = pivot.GetPosition();
            worldsphere.Meshes[0].Rotation += new TGCVector3(.00001f,.00001f,.00001f);//creo que agregar elapsedtime no es necesario
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
