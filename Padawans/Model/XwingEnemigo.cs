using BulletSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model
{
    public class XwingEnemigo : IActiveElement,IShaderObject
    {
        private TgcScene nave;
        private Xwing target;
        private TGCVector3 posicion;
        private CoordenadaEsferica coordenadaEsferica;
        private CoordenadaEsferica coordenadaAXwing;
        private float timer = 0;
        //Expresado en segundos
        private readonly float intervaloParaChequearXwing = 2;
        //Escala inicial del mesh
        private static readonly float escalar = .1f;
        private readonly TGCVector3 scale = new TGCVector3(escalar, escalar, escalar);
        //Radio de visibilidad de la nave para detectarte. Se aplica para ambas coordenadas, generando un cono de visibilidad
        private readonly float radioAperturaVisibilidad = (float)Math.PI / 4;
        //Distancia maxima a la que el enemigo ve al xwing propio
        private readonly float distanciaLejanaVisibilidad = 1200;
        //Velocidad fija de la nave enemigo
        private float velocidadGeneral;
        private CollisionObject collisionObject;
        private readonly float AlturaMinimaChequeoXwing = 20;
        private bool activado = false;
        private readonly static float DistanciaMinimaPersecucion = 100;
        private TGCVector3 vectorDistancia;

        public XwingEnemigo(TGCVector3 posicionInicial, Xwing target, float velocidadInicial, CoordenadaEsferica direccionInicial)
        {
            posicion = posicionInicial;
            velocidadGeneral = velocidadInicial;
            //Por defecto, se encuentra mirando hacia el z+
            this.coordenadaEsferica = direccionInicial;
            nave = new TgcSceneLoader().loadSceneFromFile(VariablesGlobales.mediaDir+"XWing\\X-Wing-TgcScene.xml");//@ésta deberia ser nuestra nave, no la enemiga!
            nave.Meshes.ForEach(mesh => {
                mesh.AutoTransformEnable = false;
                mesh.Transform = TGCMatrix.Scaling(scale) * 
                TGCMatrix.RotationYawPitchRoll(coordenadaEsferica.GetRotation().Y, coordenadaEsferica.GetRotation().X, coordenadaEsferica.GetRotation().Z) * 
                TGCMatrix.Translation(posicion);
                //Shader
                if (VariablesGlobales.SHADERS)
                {
                    Effect shader = VariablesGlobales.shaderManager.AskForEffect(ShaderManager.MESH_TYPE.SHADOW);
                    if (shader != null)
                        nave.Meshes.ForEach(m => m.Effect = shader);
                    VariablesGlobales.shaderManager.AddObject(this);
                }
            });
            this.target = target;
            //Calcula inicialmente en que direccion esta el xwing principal
            vectorDistancia = CommonHelper.SumarVectores(target.GetPosition(), -posicion);
            coordenadaAXwing = new CoordenadaEsferica(vectorDistancia.X, vectorDistancia.Y, vectorDistancia.Z);
            collisionObject = VariablesGlobales.physicsEngine.AgregarXwingEnemigo(this, CommonHelper.VectorXEscalar(nave.Meshes[0].BoundingBox.calculateSize(), escalar));
        }

        public void Update()
        {
            timer += VariablesGlobales.elapsedTime;
            //Que chequee la posicion de la nave enemiga cada cierto tiempo, por ejemplo 2 segundos
            if (timer > intervaloParaChequearXwing)
            {
                //Lo pongo en otro if porque c# no hace lazy evaluating, es decir, evalua esa funcion tan cara
                // aunque la primer parte haya dado falso
                if (XwingSeEncuentraEnRadioDeVisibilidad() && target.GetPosition().Y > AlturaMinimaChequeoXwing)
                {
                    timer = 0;
                    Disparar();
                    activado = true;
                }
            }
            if (activado)
            {
                Moverse();
            }
            //Dirigirse en direccion al xwing
            coordenadaEsferica = coordenadaAXwing;
        }
        private void Moverse()
        {
            posicion = CommonHelper.MoverPosicionEnDireccionCoordenadaEsferica(posicion, coordenadaEsferica, velocidadGeneral, VariablesGlobales.elapsedTime);
            nave.Meshes.ForEach(mesh => {
                mesh.Transform = TGCMatrix.Scaling(scale) *
                TGCMatrix.RotationYawPitchRoll(coordenadaEsferica.GetRotation().Y, coordenadaEsferica.GetRotation().X, coordenadaEsferica.GetRotation().Z) *
                TGCMatrix.Translation(posicion);
            });
        }
        private void Disparar()
        {
            VariablesGlobales.managerElementosTemporales.AgregarElemento(
                new Misil(posicion, coordenadaAXwing, coordenadaAXwing.GetRotation(),
                "Misil\\misil_xwing_enemigo-TgcScene.xml", Misil.OrigenMisil.ENEMIGO));
            VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.DISPARO_MISIL_ENEMIGO);
        }

        public void Render(){}

        public bool Terminado()
        {
            return false;
        }

        public void RenderBoundingBox()
        {
            nave.Meshes.ForEach(mesh=> { mesh.BoundingBox.Render(); });
        }
        public void Dispose()
        {
            nave.DisposeAll();
        }
 /**
  * Chequea dado el radio de apertura y la direccion en que mira la nave enemigo, si esta viendo al xwing 
  * principal.
  **/
        public bool XwingSeEncuentraEnRadioDeVisibilidad()
        {
            vectorDistancia = CommonHelper.SumarVectores(target.GetPosition(), -posicion);
            if (vectorDistancia.Length() > DistanciaMinimaPersecucion)
            {
                coordenadaAXwing = new CoordenadaEsferica(vectorDistancia.X, vectorDistancia.Y, vectorDistancia.Z);
                CoordenadaEsferica dif = this.coordenadaEsferica.Diferencia(coordenadaAXwing);
                return dif.acimutal < radioAperturaVisibilidad && dif.polar < radioAperturaVisibilidad
                    && vectorDistancia.Length() < distanciaLejanaVisibilidad;
            } else
            {
                return false;
            }
        }

        public void SetTechnique(string technique, ShaderManager.MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case ShaderManager.MESH_TYPE.SHADOW: nave.Meshes.ForEach(m => m.Technique = technique); break;
            }
        }

        public void Render(ShaderManager.MESH_TYPE tipo)
        {
            switch (tipo)
            {
                case ShaderManager.MESH_TYPE.SHADOW:
                    if (vectorDistancia.Length() < 2000)//optimizar
                        nave.RenderAll();
                    break;
            }
        }
    }
}