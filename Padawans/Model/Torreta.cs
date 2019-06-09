using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using System.Drawing;
using TGC.Core.Input;

namespace TGC.Group.Model
{
    class Torreta : IActiveElement
    {
        private TgcSceneLoader loader;
        TgcMesh torreta;
        private TGCVector3 posicion;
        private TGCVector3 rotation;
        private TGCMatrix matrizInicial;
        private readonly float tiempoEntreDisparos = 1.5f;
        private float tiempoDesdeUltimoDisparo = 1.5f;
        private readonly float distanciaMinimaATarget = 1000;
        private readonly TGCVector3 origenDeDisparos = new TGCVector3(0, 70, -20);
        private TGCVector3 tamanioBoundingBox;
        private readonly float factorEscala = 0.5f;
        private TGCVector3 rotacionBase;
        private Xwing target;

        public Torreta(TgcSceneLoader loader, Xwing target, TGCVector3 posicion, TGCVector3 rotacionBase)
        {
            this.loader = loader;
            this.target = target;
            this.posicion = posicion;
            this.rotacionBase = rotacionBase;
            this.rotation = rotacionBase;
            torreta = loader.loadSceneFromFile(VariablesGlobales.mediaDir + "XWing\\torreta-TgcScene.xml").Meshes[0];
            tamanioBoundingBox = torreta.BoundingBox.calculateSize();
            matrizInicial = TGCMatrix.Scaling(factorEscala, factorEscala, factorEscala);
            torreta.AutoTransformEnable = false;
            Posicionar();
            if(VariablesGlobales.SHADERS) torreta.Effect = VariablesGlobales.shader;
        }

        public void Posicionar()
        {
            torreta.Transform = matrizInicial * TGCMatrix.RotationYawPitchRoll(rotation.Y, 0, 0) * TGCMatrix.Translation(posicion);     
                /* Esto permite que rote en su eje, pero lo dejo desactivado porque los misiles quedan mal
                * TGCMatrix.Translation(-FastMath.Cos(rotation.Y) * (tamanioBoundingBox.X * factorEscala), 0,
                -FastMath.Sin(rotation.Y) * (tamanioBoundingBox.Z * factorEscala));
                */
        }

        public void Render(string technique)
        {
            torreta.Technique = technique;
            torreta.Render();
        }

        public void RenderBoundingBox()
        {
            torreta.BoundingBox.Render();
        }

        public void Dispose()
        {
            torreta.Dispose();
        }

        public void Update(float elapsedTime)
        {
            tiempoDesdeUltimoDisparo += elapsedTime;
            if (tiempoDesdeUltimoDisparo > tiempoEntreDisparos)
            {
                tiempoDesdeUltimoDisparo = 0f;
                TGCVector3 vectorDistanciaAXwing = 
                    CommonHelper.SumarVectores(target.GetPosition(), -(CommonHelper.SumarVectores(posicion, origenDeDisparos)));
                if (vectorDistanciaAXwing.Length() < distanciaMinimaATarget)
                {
                    CoordenadaEsferica direccionXwing = new CoordenadaEsferica(vectorDistanciaAXwing.X, 
                        vectorDistanciaAXwing.Y, vectorDistanciaAXwing.Z);
                    Posicionar();
                    VariablesGlobales.managerElementosTemporales.AgregarElemento(new Misil(
                        CommonHelper.SumarVectores(posicion, origenDeDisparos), 
                        direccionXwing, direccionXwing.GetRotation(), "Misil\\misil_torreta.xml"));
                }
            }
        }

        public void Render(){torreta.Render();}

        public bool Terminado()
        {
            return false;
        }
    }
}
