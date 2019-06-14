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

namespace TGC.Group.Model
{
    public class Misil : BulletSceneElement, IActiveElement
    {
        private float tiempoDeVida = 5f;
        private bool terminado = false;
        private static TGCVector3 escalaMisil = new TGCVector3(.2f, .2f, 4f);
       
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
            velocidadGeneral = 1000f;
            meshs = new TgcMesh[1];
            meshs[0] = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + pathScene).Meshes[0];
            meshs[0].AutoTransformEnable = false;
            matrizInicialTransformacion = TGCMatrix.Scaling(escalaMisil) * TGCMatrix.RotationY(FastMath.PI_HALF);

            if (VariablesGlobales.SHADERS)
            {
                VariablesGlobales.shaderManager.AgregarMesh(meshs[0], ShaderManager.MESH_TYPE.SHADOW);
            }
            TGCVector3 sizeEscalado = meshs[0].BoundingBox.calculateSize();
            sizeEscalado.X *= escalaMisil.X;
            sizeEscalado.Y *= escalaMisil.Y;
            sizeEscalado.Z *= escalaMisil.Z;
            switch (origenMisil)
            {
                case OrigenMisil.ENEMIGO:
                    collisionObject = VariablesGlobales.physicsEngine.AgregarMisilEnemigo(sizeEscalado);
                    break;
                case OrigenMisil.XWING:
                    collisionObject = VariablesGlobales.physicsEngine.AgregarMisilXwing(sizeEscalado);
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

        public override void Render()
        {
            if (!terminado)
            {
                meshs[0].Render();
            }
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

        public enum OrigenMisil { ENEMIGO, XWING}
    }
}
