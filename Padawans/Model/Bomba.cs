﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Geometry;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model
{
    public class Bomba : IActiveElement, ITarget,IShaderObject//bomba q si cae dentro del target termina el juego
    {
        TgcMesh bomba;
        private TGCVector3 escala;
        private TGCVector3 posicion;
        //Como el misil nunca cambia la trayectoria, guardo las coordenadas cartesianas de la coordenada
        //esferica para no calcular tantos senos y cosenos
        private TGCVector3 coordEsferica;
        private float tiempoDeVida = 10f;
        private readonly float velocidadGeneral = 500f;
        private bool terminado = false;
        private TGCVector3 gravedad = new TGCVector3(0, -.1f, 0);

        public Bomba(TGCVector3 position, CoordenadaEsferica coordenadaEsferica)
        {
            bomba = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + "Bomb\\bomb-TgcScene.xml").Meshes[0];
            bomba.AutoTransformEnable = false;

            this.posicion = position;
            this.coordEsferica = coordenadaEsferica.GetXYZCoord();
            escala = new TGCVector3(.05f, .05f,.05f);
            VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.XWING_BOMB);
            //@toy en seria duda de esto
            if (VariablesGlobales.SHADERS)
            {
                Effect shader = VariablesGlobales.shaderManager.AskForEffect(ShaderManager.MESH_TYPE.SHADOW);
                if(shader!=null)
                    bomba.Effect = shader;
                VariablesGlobales.shaderManager.AddObject(this);
            }
        }

        public void SetTechnique(string technique,ShaderManager.MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case ShaderManager.MESH_TYPE.SHADOW:bomba.Technique = technique;break;
            }
        }

        public void Update()
        {
            tiempoDeVida -= VariablesGlobales.elapsedTime;

            if (tiempoDeVida < 0f)
            {
                terminado = true;
            }
            else
            {
                TGCVector3 delta = coordEsferica * velocidadGeneral * VariablesGlobales.elapsedTime+gravedad;
                gravedad += new TGCVector3(0, -.1f, 0);
                posicion = posicion + delta;

                bomba.Transform = TGCMatrix.Scaling(escala) * TGCMatrix.Translation(posicion);
            }
        }

        public bool Terminado()
        {
            return terminado;
        }

        public void Render(ShaderManager.MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case ShaderManager.MESH_TYPE.SHADOW:
                    if (!terminado)
                        bomba.Render();
                    break;
            }
        }

        public void Render() {
            if (!terminado)
                bomba.Render();
        }

        public void RenderBoundingBox()
        {
            if (!terminado)
            {
                bomba.BoundingBox.Render();
            }
        }

        public void Dispose()
        {
            bomba.Dispose();
        }

        public TGCVector3 GetPosition()
        {
            return posicion;
        }
    }
}
