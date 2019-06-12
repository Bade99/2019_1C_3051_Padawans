﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using System.Drawing;
using TGC.Core.Input;
using TGC.Core.Particle;
using TGC.Core.Direct3D;

namespace TGC.Group.Model
{
    class Torreta : IActiveElement
    {
        TgcMesh torreta;
        private TGCVector3 posicion;
        private TGCVector3 rotation;
        private TGCMatrix matrizInicial;
        private readonly float tiempoEntreDisparos = 1.5f;
        private float tiempoDesdeUltimoDisparo = 1.5f;
        private readonly float distanciaMinimaATarget = 300f;
        private readonly float factorEscala = .1f;
        private readonly TGCVector3 origenDeDisparos;
        private TGCVector3 tamanioBoundingBox;
        private TGCVector3 rotacionBase;
        private Xwing target;
        private bool isActive;
        private int vida;

        private ParticleEmitter particulaHumo;

        TGCVector3 DistanciaAXwing;

        public Torreta(Xwing target, TGCVector3 posicion, TGCVector3 rotacionBase)
        {
            origenDeDisparos = new TGCVector3(0 * factorEscala, 70 * factorEscala, -20 * factorEscala);
            this.target = target;
            this.posicion = posicion;
            this.rotacionBase = rotacionBase;
            this.rotation = rotacionBase;
            torreta = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + "XWing\\torreta-TgcScene.xml").Meshes[0];
            tamanioBoundingBox = torreta.BoundingBox.calculateSize();
            matrizInicial = TGCMatrix.Scaling(factorEscala, factorEscala, factorEscala);
            torreta.AutoTransformEnable = false;
            Posicionar();
            this.vida = 3;
            if (VariablesGlobales.SHADERS) VariablesGlobales.shaderManager.AgregarMesh(torreta, ShaderManager.MESH_TYPE.SHADOW);
            //EMISOR PARTICULAS
            particulaHumo = new ParticleEmitter(VariablesGlobales.mediaDir + "Particulas\\pisada.png", 10);
            particulaHumo.Position = this.posicion;
            particulaHumo.MinSizeParticle = 4;
            particulaHumo.MaxSizeParticle = 6;
            particulaHumo.ParticleTimeToLive = 1;
            particulaHumo.CreationFrecuency = 1;
            particulaHumo.Dispersion = 100;
            particulaHumo.Speed = new TGCVector3(30, 30, 30);
        }

        public void Posicionar()
        {
            torreta.Transform = matrizInicial * TGCMatrix.RotationYawPitchRoll(rotation.Y, 0, 0) * TGCMatrix.Translation(posicion);
            /* Esto permite que rote en su eje, pero lo dejo desactivado porque los misiles quedan mal
            * TGCMatrix.Translation(-FastMath.Cos(rotation.Y) * (tamanioBoundingBox.X * factorEscala), 0,
            -FastMath.Sin(rotation.Y) * (tamanioBoundingBox.Z * factorEscala));
            */
            particulaHumo.Position = this.posicion;
        }

        public void RenderBoundingBox()
        {
            torreta.BoundingBox.Render();
        }

        public void Dispose()
        {
            torreta.Dispose();
            particulaHumo.dispose();
        }

        public void Update()
        {
            tiempoDesdeUltimoDisparo += VariablesGlobales.elapsedTime;
            if (tiempoDesdeUltimoDisparo > tiempoEntreDisparos)
            {
                tiempoDesdeUltimoDisparo = 0f;

                DistanciaAXwing = target.GetPosition() - posicion; //+ origenDeDisparos;

                if (this.vida > 0 && DistanciaAXwing.Length() < distanciaMinimaATarget)
                {
                    isActive = true;
                    CoordenadaEsferica direccionXwing = new CoordenadaEsferica(DistanciaAXwing.X,
                        DistanciaAXwing.Y, DistanciaAXwing.Z);
                    //Posicionar();
                    VariablesGlobales.managerElementosTemporales.AgregarElemento(new Misil(
                        posicion+ torreta.BoundingBox.calculateSize()*factorEscala,
                        direccionXwing, direccionXwing.GetRotation(), "Misil\\misil_torreta.xml"));
                }
                else isActive = false;
            }
        }

        public void Render(){
            if (isActive)
            {
                torreta.Render();
                if (this.vida <= 0)
                {
                    //IMPORTANTE PARA PERMITIR EL EFECTO DE LA PARTICULA.
                    D3DDevice.Instance.ParticlesEnabled = true;
                    D3DDevice.Instance.EnableParticles();
                    particulaHumo.render(VariablesGlobales.elapsedTime);
                }
            }
        }

        public bool Terminado()
        {
            return false;
        }
    }
}
