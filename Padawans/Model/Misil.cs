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
        //Como el misil nunca cambia la trayectoria, guardo las coordenadas cartesianas de la coordenada
        //esferica para no calcular tantos senos y cosenos
        private float tiempoDeVida = 10f;
        private readonly float velocidadGeneral = 1000f;
        private bool terminado = false;
       
        /**
         * Posicion nave: Posicion inicial del misil
         * coordenadaEsfericaP: Direccion y sentido del misil
         * Rotacion nave: Constante con el que misil se rota inicialmente para estar alineado a la trayectoria
         * pathscene: Path donde esta ubicado el mesh
         * */
        public Misil(TGCVector3 posicionInicial, CoordenadaEsferica coordenadaEsferica, TGCVector3 rotacionInicial, string pathScene)
        {
            this.rotation = rotacionInicial;
            meshs = new TgcMesh[1];
            meshs[0] = VariablesGlobales.loader.loadSceneFromFile(VariablesGlobales.mediaDir + pathScene).Meshes[0];
            meshs[0].AutoTransformEnable = false;
            matrizInicialTransformacion = TGCMatrix.Scaling(new TGCVector3(.2f, .2f, 4f)) * TGCMatrix.RotationY(FastMath.PI_HALF);
            bulletVelocity = CommonHelper.VectorXEscalar(coordenadaEsferica.GetXYZCoord(), velocidadGeneral);
            //Shader
            if (VariablesGlobales.SHADERS)
            {
                VariablesGlobales.shaderManager.AgregarMesh(meshs[0], ShaderManager.MESH_TYPE.SHADOW);
            }
            body = VariablesGlobales.physicsEngine.AgregarMisilEnemigo(meshs[0].BoundingBox.calculateSize(), 
                1, posicionInicial, TGCVector3.Empty);
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
            VariablesGlobales.physicsEngine.EliminarObjeto(body);
        }
    }
}
