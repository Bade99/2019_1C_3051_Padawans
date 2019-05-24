using System;
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

namespace TGC.Group.Model
{
    class XwingEnemigo : IActiveElement
    {
        private TgcScene nave;
        private TGCVector3 velocidad;
        private Xwing target;
        private TemporaryElementManager managerDisparos;
        private float tiempoEntreDisparos = .5f;
        private float tiempoDesdeUltimoDisparo = .5f;

        private float distanciaATarget;
        private bool flyby = false;

        public XwingEnemigo(TGCVector3 posicionInicial,Xwing target,TemporaryElementManager managerDisparos)
        {
            velocidad = new TGCVector3(0,0,70f);
            nave = new TgcSceneLoader().loadSceneFromFile(VariablesGlobales.mediaDir+"XWing\\X-Wing-TgcScene.xml");//@ésta deberia ser nuestra nave, no la enemiga!
            nave.Meshes.ForEach(mesh => { mesh.Position = posicionInicial; });
            nave.Meshes.ForEach(mesh => { mesh.RotateY(-FastMath.PI_HALF); });
            nave.Meshes.ForEach(mesh => { mesh.Scale= new TGCVector3(0.1f, 0.1f, 0.1f); });
            this.target = target;
            this.managerDisparos = managerDisparos; ;
        }

        public void Update(float elapsedTime)
        {
            distanciaATarget = DistanciaATarget();

            if (distanciaATarget < 1000f)
            {

                nave.Meshes.ForEach(mesh => { mesh.Position += velocidad * elapsedTime; });

                if (!flyby && distanciaATarget < 100f)
                {
                    flyby = true;
                    VariablesGlobales.managerSonido.AgregarElemento(new Sonido("Sonidos\\TIE_fighter_flyby_1.wav", 0, 6f, 1,0));
                }
                if (distanciaATarget < 100f)
                {
                    //Disparar
                    tiempoDesdeUltimoDisparo += elapsedTime;
                    if (tiempoDesdeUltimoDisparo > tiempoEntreDisparos)
                    {
                        tiempoDesdeUltimoDisparo = 0f;
                        //@corregir el angulo de disparo
                        managerDisparos.AgregarElemento(new Misil(this.nave.Meshes[0].Position + this.CalcularOffsetUnAla(), new CoordenadaEsferica(new TGCVector3(0, -FastMath.PI_HALF, 0)), new TGCVector3(0, -FastMath.PI_HALF, 0), "\\Misil\\misil_xwing_enemigo-TgcScene.xml"));
                        VariablesGlobales.managerSonido.AgregarElemento(new Sonido("Sonidos\\TIE_fighter_1_disparo.wav", 0, 1f, 1,0));
                    }
                }
            }
        }

        public void Render()
        {
            nave.Meshes.ForEach(mesh => { mesh.Transform = TGCMatrix.Translation(mesh.Position); });
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
        public float DistanciaATarget()//@en vez de hacer sqrt deberiamos poner el otro valor al cuadrado
        {
            return FastMath.Sqrt(FastMath.Pow2(this.nave.Meshes[0].Position.X-target.GetPosition().X)+ FastMath.Pow2(this.nave.Meshes[0].Position.Y - target.GetPosition().Y) + FastMath.Pow2(this.nave.Meshes[0].Position.Z - target.GetPosition().Z));
        }

        private TGCVector3 CalcularOffsetUnAla()
        {
            Random rnd = new Random();
            int rndLargo = (rnd.Next(1, 3) == 1) ? 1 : -1;
            int rndAncho = (rnd.Next(1, 3) == 1) ? 1 : -1;
            var largoOffset = rndLargo * this.nave.Meshes[1].BoundingBox.calculateSize().Z * .5f;
            var anchoOffset = rndAncho * this.nave.Meshes[1].BoundingBox.calculateSize().Y * .5f;
            var distancia = -this.nave.Meshes[1].BoundingBox.calculateSize().X * 1.5f;
            return new TGCVector3(largoOffset, anchoOffset, distancia);
        }

    }
}
