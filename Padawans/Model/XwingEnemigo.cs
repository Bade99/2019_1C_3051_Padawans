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
        private TGCVector3 velocidad;
        private Xwing target;
        private TemporaryElementManager managerDisparos;
        private TGCVector3 posicion;
        private CoordenadaEsferica coordenadaEsferica;
        private TGCVector3 rotation;
        private CoordenadaEsferica coordenadaAXwing;
        private float timer = 0;

        private readonly float intervaloParaChequearXwing = 2;
        private readonly TGCVector3 scale = new TGCVector3(.1f, .1f, .1f);
        private readonly float radioAperturaVisibilidad = (float)Math.PI / 4;

        public XwingEnemigo(TGCVector3 posicionInicial, Xwing target)
        {
            posicion = posicionInicial;
            velocidad = new TGCVector3(0,0,70f);
            this.rotation = new TGCVector3(0, FastMath.PI_HALF * 3, 0);
            this.coordenadaEsferica = new CoordenadaEsferica(rotation);
            nave = new TgcSceneLoader().loadSceneFromFile(VariablesGlobales.mediaDir+"XWing\\X-Wing-TgcScene.xml");//@ésta deberia ser nuestra nave, no la enemiga!
            nave.Meshes.ForEach(mesh => {
                mesh.AutoTransformEnable = false;
                mesh.Transform = TGCMatrix.Scaling(scale) * TGCMatrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) * TGCMatrix.Translation(posicion);
            });
            this.target = target;
            this.managerDisparos = managerDisparos;
            //por defecto, mira hacia el z+ y paralelo al plano ZX
        }

        public void Update(float elapsedTime)
        {
            timer += elapsedTime;
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
        }
        private void Disparar()
        {
            VariablesGlobales.managerElementosTemporales.AgregarElemento(
                new Misil(posicion, coordenadaAXwing, rotation,
                "Misil\\misil_xwing-TgcScene.xml", Color.FromArgb(255, 0, 0, 255)));
            VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.DISPARO_MISIL);
        }
        public void Render()
        {
            nave.RenderAll();
        }

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
        //Chequea que la nave enemiga este viendo la nave principal chequeando si esta en su radio de apertura
        public bool XwingSeEncuentraEnRadioDeVisibilidad()
        {
            TGCVector3 vectorDistancia = CommonHelper.SumarVectores(target.GetPosition(), -posicion);
            coordenadaAXwing = new CoordenadaEsferica(vectorDistancia.X, vectorDistancia.Y, vectorDistancia.Z);
            CoordenadaEsferica dif = this.coordenadaEsferica.Diferencia(coordenadaAXwing);
            return dif.acimutal < radioAperturaVisibilidad && dif.polar < radioAperturaVisibilidad;
        }
    }
}