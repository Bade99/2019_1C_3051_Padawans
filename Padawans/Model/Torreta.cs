using System;
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
using BulletSharp;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model
{
    public class Torreta : IActiveElement, IPostProcess, IShaderObject
    {
        private TgcMesh torreta;
        private TGCVector3 posicion;
        private TGCVector3 rotation;
        private TGCVector3 scaleVector;
        private readonly float tiempoEntreDisparos = 2f;
        private float tiempoDesdeUltimoDisparo = 1.5f;
        private readonly float distanciaMinimaATarget = 600;
        private readonly float distanciaMinimaAVisible = 3000;
        private readonly float factorEscala = .1f;
        private readonly TGCVector3 origenDeDisparos;
        private Xwing target;
        private bool isActive;
        private bool isVisible = false;
        private int vida;
        TGCVector3 deltaPosicionHumoTorreta = new TGCVector3(-5, 12, -5);
        public CollisionObject collisionObject { get; set; }

        private ParticleEmitter particulaHumo;

        TGCVector3 DistanciaAXwing;

        public Torreta(Xwing target, TGCVector3 posicion, TGCVector3 rotacionBase)
        {
            origenDeDisparos = new TGCVector3(0 * factorEscala, 70 * factorEscala, -20 * factorEscala);
            this.target = target;
            this.posicion = posicion;
            this.rotation = rotacionBase;
            torreta = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + "XWing\\torreta-TgcScene.xml").Meshes[0];
            scaleVector = new TGCVector3(factorEscala, factorEscala, factorEscala);
            torreta.AutoTransformEnable = false;
            this.vida = 1;
            if (VariablesGlobales.SHADERS)
            {
                Effect shader = VariablesGlobales.shaderManager.AskForEffect(ShaderManager.MESH_TYPE.SHADOW);
                if (shader != null)
                    torreta.Effect = shader;
                VariablesGlobales.shaderManager.AddObject(this);
            }
            //EMISOR PARTICULAS
            particulaHumo = new ParticleEmitter(VariablesGlobales.mediaDir + "Particulas\\pisada.png", 30);
            particulaHumo.MinSizeParticle = 1.5f;
            particulaHumo.MaxSizeParticle = 2;
            particulaHumo.ParticleTimeToLive = 0.2f;
            particulaHumo.CreationFrecuency = 0.01f;
            particulaHumo.Dispersion = 1;
            particulaHumo.Speed = new TGCVector3(10, 10, 10);
            collisionObject = VariablesGlobales.physicsEngine.AgregarTorreta(this, torreta.BoundingBox.calculateSize());
            Posicionar();
        }

        public void SetTechnique(string technique, ShaderManager.MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case ShaderManager.MESH_TYPE.SHADOW: torreta.Technique = technique; break;
            }
        }

        private void Posicionar()
        {
            TGCMatrix rotationTranslation = TGCMatrix.RotationYawPitchRoll(rotation.Y, 0, 0) * TGCMatrix.Translation(posicion);
            torreta.Transform = TGCMatrix.Scaling(scaleVector) * rotationTranslation;
            collisionObject.CollisionShape.LocalScaling = scaleVector.ToBulletVector3();
            collisionObject.WorldTransform = CommonHelper.TgcToBulletMatrix(rotationTranslation);
            /* Esto permite que rote en su eje, pero lo dejo desactivado porque los misiles quedan mal
            * TGCMatrix.Translation(-FastMath.Cos(rotation.Y) * (tamanioBoundingBox.X * factorEscala), 0,
            -FastMath.Sin(rotation.Y) * (tamanioBoundingBox.Z * factorEscala));
            */
            particulaHumo.Position = CommonHelper.SumarVectores(this.posicion, deltaPosicionHumoTorreta);
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
            DistanciaAXwing = target.GetPosition() - posicion - origenDeDisparos;
            float distanciaLength = DistanciaAXwing.Length();
            isVisible = TGCVector3.Dot(-DistanciaAXwing, target.coordenadaDireccionCartesiana) > 0 &&
                     distanciaLength < distanciaMinimaAVisible;
            tiempoDesdeUltimoDisparo += VariablesGlobales.elapsedTime;
            if (tiempoDesdeUltimoDisparo > tiempoEntreDisparos)
            {
                tiempoDesdeUltimoDisparo = 0f;
                if (this.vida > 0 && distanciaLength < distanciaMinimaATarget)
                {
                    isActive = true;
                    CoordenadaEsferica direccionXwing = new CoordenadaEsferica(DistanciaAXwing.X,
                        DistanciaAXwing.Y, DistanciaAXwing.Z);
                    VariablesGlobales.managerElementosTemporales.AgregarElemento(new Misil(
                        posicion + origenDeDisparos, direccionXwing, direccionXwing.GetRotation()
                        , "Misil\\misil_torreta.xml", Misil.OrigenMisil.ENEMIGO));
                }
                else
                {
                    isActive = false;
                }
            }
        }

        public void Render(ShaderManager.MESH_TYPE tipo){
            if (isVisible)
            {
                switch (tipo)
                {
                    case ShaderManager.MESH_TYPE.SHADOW: torreta.Render();break;
                }
            }
        }

        public void RenderPostProcess(string effect)
        {
            if (this.vida <= 0)
            {
                //IMPORTANTE PARA PERMITIR EL EFECTO DE LA PARTICULA.
                D3DDevice.Instance.ParticlesEnabled = true;
                D3DDevice.Instance.EnableParticles();
                particulaHumo.render(VariablesGlobales.elapsedTime);
            }
        }

        public bool Terminado()
        {
            return false;
        }

        public void DisminuirVida()
        {
            vida--;
        }

        public void Render(){}
    }
}
