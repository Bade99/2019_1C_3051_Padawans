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
        TgcScene scene, escena_sin_paredes;
        private int n;
        /// <summary>
        ///     n representa la cantidad de pistas que va a graficar
        /// </summary>
        public PistasReferencia(TgcSceneLoader loader, int n)
        {
            this.loader = loader;
            scene = loader.loadSceneFromFile("Padawans_media\\XWing\\TRENCH_RUN-TgcScene.xml");
            escena_sin_paredes = loader.loadSceneFromFile("Padawans_media\\XWing\\death+star-TgcScene.xml");
            this.n = n;
        }

        private void PlaceMeshLine(TgcScene escena,TGCVector3 posicion, TGCVector3 escalador,int repeticiones,int mesh_pivot)
        {
            for (int i = 0; i < repeticiones; i++)
            {
                foreach (TgcMesh mesh in escena.Meshes)
                {
                    mesh.Position = posicion;
                    mesh.Scale = escalador;
                }

                posicion = new TGCVector3(  escena.Meshes[mesh_pivot].Position.X,
                                            escena.Meshes[mesh_pivot].Position.Y,
                                            escena.Meshes[mesh_pivot].Position.Z -
                                                escena.Meshes[mesh_pivot].BoundingBox.calculateSize().Z);
                
                escena.RenderAll();
            }
        }

        public override void Render()
        {

            var posicion = new TGCVector3(0f, 0f, 0f);
            var escalador = new TGCVector3(30f, 30f, 30f);
            int mesh_pivot = 0;
            PlaceMeshLine(scene, posicion,escalador,n,mesh_pivot);

            posicion = new TGCVector3(-50f, 0f, 0f);
            mesh_pivot = 1;
            PlaceMeshLine(escena_sin_paredes, posicion, escalador, n, mesh_pivot);

        }

        public override void Update()
        {

        }

        public override void Dispose()
        {
            scene.DisposeAll();
            escena_sin_paredes.DisposeAll();
        }

        public override void RenderBoundingBox()
        {
            scene.BoundingBox.Render();
            escena_sin_paredes.BoundingBox.Render();
        }
    }
}