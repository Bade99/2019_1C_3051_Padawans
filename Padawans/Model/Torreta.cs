using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using System.Drawing;

namespace TGC.Group.Model
{
    class Torreta : SceneElement
    {
        private TgcSceneLoader loader;
        TgcMesh torreta;
        private TGCVector3 posicion;
        public TGCVector3 rotation;
        private TGCMatrix matrizInicial;
        private float tiempoEntreDisparos = 1.5f;
        private float tiempoDesdeUltimoDisparo = 1.5f;

        public Torreta(TgcSceneLoader loader)
        {
            this.loader = loader;
            torreta = loader.loadSceneFromFile(VariablesGlobales.mediaDir + "XWing\\torreta-TgcScene.xml").Meshes[0];

            matrizInicial = TGCMatrix.Scaling(0.2f, 0.2f, 0.2f);
            torreta.AutoTransformEnable = false;

            
            //torreta.setColor(Color.Red);
        }

        public void Posicionar(TGCVector3 posicion, float anguloRotacion)
        {
            this.posicion = posicion;
            torreta.Transform = matrizInicial * TGCMatrix.RotationY(anguloRotacion) * TGCMatrix.Translation(posicion);
        }

        public void UpdateDisparar(float ElapsedTime)
        {
            //Falta agregar la condición de que dispare solo cuando la nave se encuentre a X distancia.
            tiempoDesdeUltimoDisparo += ElapsedTime;
            if (tiempoDesdeUltimoDisparo > tiempoEntreDisparos)
            {
                tiempoDesdeUltimoDisparo = 0f;
                VariablesGlobales.managerElementosTemporales.AgregarElemento(new Misil(posicion, new CoordenadaEsferica(rotation), rotation, "Misil\\misil_torreta.xml"));
            }

        }

        public override void Render()
        {
            torreta.Render();
        }

        public override void RenderBoundingBox()
        {
            torreta.BoundingBox.Render();
        }

        public override void Update()
        {

        }

        public override void Dispose()
        {
            torreta.Dispose();
        }
    }
}
