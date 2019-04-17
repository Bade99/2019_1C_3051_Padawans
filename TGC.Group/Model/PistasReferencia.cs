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
    ///     Una clase de ejemplo que grafica pistas para poder tener una referencia de la velocidad de la nave
    /// </summary>
    public class PistasReferencia : SceneElement
    {
        private TgcSceneLoader loader;
        TgcScene scene;
        private int n;
        /// <summary>
        ///     n representa la cantidad de pistas que va a graficar
        /// </summary>
        public PistasReferencia(TgcSceneLoader loader, int n)
        {
            this.loader = loader;
            scene = loader.loadSceneFromFile("Padawans_media\\XWing\\TRENCH_RUN-TgcScene.xml");
            this.n = n;
        }

        public override void Render()
        {
            TGCVector3 posicionIncremental = new TGCVector3(0,0,0);
            for (int i=0;i<n;i++)
            {
                posicionIncremental.Z = - i * 100;
                foreach (TgcMesh mesh in scene.Meshes)
                {
                    mesh.Position = posicionIncremental;
                }
                scene.RenderAll();
            }
        }

        public override void Update()
        {

        }

        public override void Dispose()
        {
            scene.DisposeAll();
        }

        public override void RenderBoundingBox()
        {
            scene.BoundingBox.Render();
        }
    }
}