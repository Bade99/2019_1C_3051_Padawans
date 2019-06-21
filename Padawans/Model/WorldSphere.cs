using System.Collections.Generic;
using TGC.Core.SceneLoader;
using TGC.Core.Mathematica;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model
{
    class WorldSphere : SceneElement,IShaderObject //probé con skybox y no me gustó el resultado
    {
        private TgcSceneLoader loader;
        TgcMesh worldsphere;
        Xwing pivot;

        public WorldSphere(TgcSceneLoader loader, Xwing pivot)
        {
            this.loader = loader;
            this.pivot = pivot;
            worldsphere = loader.loadSceneFromFile("Padawans_media\\XWing\\GeodesicSphere02-TgcScene.xml").Meshes[0];//tiene un solo mesh
            worldsphere.Rotation = new TGCVector3(2,5,1);
            worldsphere.Scale = new TGCVector3(100f, 100f, 100f);
            if (VariablesGlobales.SHADERS)
            {
                Effect shader = VariablesGlobales.shaderManager.AskForEffect(ShaderManager.MESH_TYPE.DEFAULT);
                if (shader != null)
                    worldsphere.Effect = shader;
                VariablesGlobales.shaderManager.AddObject(this);
            }
        }

        public override void Render(){}

        public override void Update()
        {
            //@hacer que se mueva con el xwing
            worldsphere.Position = pivot.GetPosition();
            worldsphere.Rotation += new TGCVector3(.00001f,.00001f,.00001f);//creo que agregar elapsedtime no es necesario
        }

        public override void Dispose()
        {
            worldsphere.Dispose();
        }

        public override void RenderBoundingBox()
        {
            worldsphere.BoundingBox.Render();
        }

        public void SetTechnique(string technique, ShaderManager.MESH_TYPE tipo){}

        public void Render(ShaderManager.MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case ShaderManager.MESH_TYPE.DEFAULT: worldsphere.Render(); break;
            }
        }
    }
}
