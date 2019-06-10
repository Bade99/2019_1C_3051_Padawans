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

namespace TGC.Group.Model
{
    class XwingEnemigo : IActiveElement
    {
        private TgcScene nave;
        private Xwing target;
        private TemporaryElementManager managerDisparos;
        private TGCVector3 posicion;
        private CoordenadaEsferica coordenadaEsferica;
        private CoordenadaEsferica coordenadaAXwing;
        private float timer = 0;
        //Expresado en segundos
        private readonly float intervaloParaChequearXwing = 2;
        //Escala inicial del mesh
        private readonly TGCVector3 scale = new TGCVector3(.1f, .1f, .1f);
        //Radio de visibilidad de la nave para detectarte. Se aplica para ambas coordenadas, generando un cono de visibilidad
        private readonly float radioAperturaVisibilidad = (float)Math.PI / 4;
        //Distancia maxima a la que el enemigo ve al xwing propio
        private readonly float distanciaLejanaVisibilidad = 1000;
        //Velocidad fija de la nave enemigo
        private float velocidadGeneral = 20;

        public XwingEnemigo(TGCVector3 posicionInicial, Xwing target, float velocidadInicial)
        {
            posicion = posicionInicial;
            velocidadGeneral = velocidadInicial;
            //Por defecto, se encuentra mirando hacia el z+
            this.coordenadaEsferica = new CoordenadaEsferica(FastMath.PI_HALF, FastMath.PI_HALF);
            nave = new TgcSceneLoader().loadSceneFromFile(VariablesGlobales.mediaDir+"XWing\\X-Wing-TgcScene.xml");//@ésta deberia ser nuestra nave, no la enemiga!
            nave.Meshes.ForEach(mesh => {
                mesh.AutoTransformEnable = false;
                mesh.Transform = TGCMatrix.Scaling(scale) * 
                TGCMatrix.RotationYawPitchRoll(coordenadaEsferica.GetRotation().Y, coordenadaEsferica.GetRotation().X, coordenadaEsferica.GetRotation().Z) * 
                TGCMatrix.Translation(posicion);
                //Shader
                if(VariablesGlobales.SHADERS) VariablesGlobales.shaderManager.AgregarMesh(mesh, ShaderManager.MESH_TYPE.SHADOW);
            });
            this.target = target;
            this.managerDisparos = managerDisparos;
            //Calcula inicialmente en que direccion esta el xwing principal
            TGCVector3 vectorDistancia = CommonHelper.SumarVectores(target.GetPosition(), -posicion);
            coordenadaAXwing = new CoordenadaEsferica(vectorDistancia.X, vectorDistancia.Y, vectorDistancia.Z);
        }

        public void Update()
        {
            timer += VariablesGlobales.elapsedTime;
            //Que chequee la posicion de la nave enemiga cada cierto tiempo, por ejemplo 2 segundos
            if (timer > intervaloParaChequearXwing)
            {
                //Lo pongo en otro if porque c# no hace lazy evaluating, es decir, evalua esa funcion tan cara
                // aunque la primer parte haya dado falso
                if (XwingSeEncuentraEnRadioDeVisibilidad())
                {
                    timer = 0;
                    Disparar();

                }
            }
            //Dirigirse en direccion al xwing
            coordenadaEsferica = coordenadaAXwing;
            //Moverse en direccion de la coordenadaEsferica y la velocidad general
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
                "Misil\\misil_xwing_enemigo-TgcScene.xml"));
            VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.DISPARO_MISIL_ENEMIGO);
        }

        public void Render(){nave.RenderAll();}

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
            TGCVector3 vectorDistancia = CommonHelper.SumarVectores(target.GetPosition(), -posicion);
            coordenadaAXwing = new CoordenadaEsferica(vectorDistancia.X, vectorDistancia.Y, vectorDistancia.Z);
            CoordenadaEsferica dif = this.coordenadaEsferica.Diferencia(coordenadaAXwing);
            return dif.acimutal < radioAperturaVisibilidad && dif.polar < radioAperturaVisibilidad
                && vectorDistancia.Length() < distanciaLejanaVisibilidad;
        }
    }
}