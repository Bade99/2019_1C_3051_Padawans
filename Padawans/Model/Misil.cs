using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Sound;
using BulletSharp;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model
{
    public class Misil : BulletSceneElement, IActiveElement,IShaderObject
    {
        private float tiempoDeVida = 2;
        private bool terminado = false;
       
        /**
         * Posicion nave: Posicion inicial del misil
         * coordenadaEsfericaP: Direccion y sentido del misil
         * Rotacion nave: Constante con el que misil se rota inicialmente para estar alineado a la trayectoria
         * pathscene: Path donde esta ubicado el mesh
         * */
        public Misil(TGCVector3 posicionInicial, CoordenadaEsferica coordenadaEsferica, TGCVector3 rotacionInicial, string pathScene, OrigenMisil origenMisil)
        {
            this.rotation = rotacionInicial;
            this.coordenadaEsferica = coordenadaEsferica;
            this.position = posicionInicial;
            this.scaleVector = new TGCVector3(.2f, .2f, 4f);
            velocidadGeneral = 1000;
            meshs = new TgcMesh[1];
            meshs[0] = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + pathScene).Meshes[0];
            meshs[0].AutoTransformEnable = false;
            matrizInicialTransformacion = TGCMatrix.RotationY(FastMath.PI_HALF);

            if (VariablesGlobales.SHADERS)
            {
                Effect shader = VariablesGlobales.shaderManager.AskForEffect(ShaderManager.MESH_TYPE.SHADOW);
                if (shader != null)
                    meshs[0].Effect = shader;
                VariablesGlobales.shaderManager.AddObject(this);
            }
            switch (origenMisil)
            {
                case OrigenMisil.ENEMIGO:
                    collisionObject = VariablesGlobales.physicsEngine.AgregarMisilEnemigo(meshs[0].BoundingBox.calculateSize());
                    break;
                case OrigenMisil.XWING:
                    collisionObject = VariablesGlobales.physicsEngine.AgregarMisilXwing(meshs[0].BoundingBox.calculateSize());
                    break;
            }
        }


        public override void Update()
        {

            tiempoDeVida -= VariablesGlobales.elapsedTime;
            
            if (tiempoDeVida < 0f)
            {
                terminado = true;
            }
            else {
                UpdateBullet();
            }

        }

        public bool Terminado()
        {
            return terminado;
        }

        public override void Render(){
            if (!terminado)
                meshs[0].Render();
        }

        public override void RenderBoundingBox()
        {
            if (!terminado)
            {
                meshs[0].BoundingBox.Render();
            }
        }

        public override void Dispose()
        {
            meshs[0].Dispose();
            VariablesGlobales.physicsEngine.EliminarObjeto(collisionObject);
        }

        public void SetTechnique(string technique, ShaderManager.MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case ShaderManager.MESH_TYPE.SHADOW: meshs[0].Technique = technique; break;
            }
        }

        public void Render(ShaderManager.MESH_TYPE tipo)
        {
            if (!terminado)
            {
                switch (tipo)
                {
                    case ShaderManager.MESH_TYPE.SHADOW: meshs[0].Render();break;
                }
            }
        }

        public enum OrigenMisil { ENEMIGO, XWING}
    }
}
