using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using BulletSharp;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model
{
    public class Obstaculo : IActiveElement, IPostProcess, IShaderObject
    {
        private TgcMesh obstaculo;
        private TGCVector3 posicion;
        private TGCVector3 rotation;
        private TGCVector3 scaleVector;
        private readonly float factorEscala = .1f;
        private bool isActive;
        private int vida;
        public CollisionObject collisionObject { get; set; }

        public Obstaculo(TGCVector3 posicion, TGCVector3 rotacionBase)
        {
            this.posicion = posicion;
            this.rotation = rotacionBase;
            obstaculo = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + "Roca\\Roca-TgcScene.xml").Meshes[0];
            scaleVector = new TGCVector3(factorEscala, factorEscala, factorEscala);
            this.vida = 1;
            isActive = true;

            if (VariablesGlobales.SHADERS)
            {
                Effect shader = VariablesGlobales.shaderManager.AskForEffect(ShaderManager.MESH_TYPE.SHADOW);
                if (shader != null)
                    obstaculo.Effect = shader;
                VariablesGlobales.shaderManager.AddObject(this);
            }

            
            Posicionar();
            
        }

        public void SetTechnique(string technique, ShaderManager.MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case ShaderManager.MESH_TYPE.SHADOW: obstaculo.Technique = technique; break;
            }
        }

        private void Posicionar()
        {
            TGCMatrix rotationTranslation = TGCMatrix.RotationYawPitchRoll(rotation.Y, 0, 0) * TGCMatrix.Translation(posicion);
            obstaculo.Transform = TGCMatrix.Scaling(scaleVector) * rotationTranslation;
            collisionObject = VariablesGlobales.physicsEngine.AgregarObstaculo(this, obstaculo.BoundingBox.calculateSize());
            collisionObject.CollisionShape.LocalScaling = scaleVector.ToBulletVector3();
            collisionObject.WorldTransform = CommonHelper.TgcToBulletMatrix(rotationTranslation);

        }

        public void RenderBoundingBox()
        {
            obstaculo.BoundingBox.Render();
        }

        public void Dispose()
        {
            obstaculo.Dispose();
            //particulaHumo.dispose();
        }

        public bool Terminado()
        {
            return false;
        }

        public void Destruir()
        {
            vida--;
        }

        public void Render(ShaderManager.MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case ShaderManager.MESH_TYPE.SHADOW: obstaculo.Render(); break;
            }
        }

        public void RenderPostProcess(string effect)
        {
            if (this.vida <= 0)
            {
                //IMPORTANTE PARA PERMITIR EL EFECTO DE LA PARTICULA.
                /*D3DDevice.Instance.ParticlesEnabled = true;
                D3DDevice.Instance.EnableParticles();
                particulaHumo.render(VariablesGlobales.elapsedTime);*/
            }
        }

        public void Update() { }

        public void Render() { }
    }
}
