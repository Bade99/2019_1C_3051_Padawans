using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.Threading;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using BulletSharp;

namespace TGC.Group.Model
{
    /// <summary>
    ///     La nave principal que utiliza el jugador
    /// </summary>
    public class Xwing : SceneElement, InteractiveElement
    {
        private TgcSceneLoader loader;
        TgcMesh xwing,alaXwing;
        private TemporaryElementManager managerDisparos;
        public RigidBody body_xwing;

        //Constantes
        private readonly float minimaVelocidad = 10f;
        private readonly float velocidadEjes = 10f;
        private readonly float aceleracion = 80;
        //private readonly float aceleracion = 1;//Bullet
        private readonly float friccion = 10f;
        private readonly float maximaVelocidad = 300;
        private readonly float limiteAnguloPolar=0.1f;
        private readonly float progressUnityRotationAdvance = FastMath.PI / 60;

        private TGCVector3 left = new TGCVector3(-1, 0, 0);
        private TGCVector3 right = new TGCVector3(1, 0, 0);
        private TGCVector3 front = new TGCVector3(0, 0, 1);
        private TGCVector3 back = new TGCVector3(0, 0, -1);

        private float DISTANCIA_ORIGEN_MISIL_DIRECCION_NAVE = 84;
        private float DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL = 6;
        private float EXTRA_ANGULO_POLAR_CANION_ABAJO = 0.3f;
        private float EXTRA_DISTANCIA_CANION_ABAJO = 0.5f;

        private float velocidadGeneral;
        private bool barrelRoll;
        private int barrelRollAvance=0;
        private bool rotationYAnimation;
        private float rotationYAnimacionAdvance;
        private CoordenadaEsferica coordenadaEsferica;
        private bool swapPolarKeys = false;
        private TGCVector3 ultimaPosicion;
        private float rotacionBarrelRoll;
        private float tiempoEntreDisparos=.5f;
        private float tiempoDesdeUltimoDisparo = .5f;
        //matrices de transformaciones
        private TGCMatrix matrizXwingInicial;

        //propiedades de la nave
        private TGCVector3 posicion;
        public TGCVector3 rotation;
        private TGCVector3 escala;
        private float escalar = .1f;

        public Xwing(TgcSceneLoader loader,TemporaryElementManager managerElementosTemporales, TGCVector3 posicionInicial)
        {

            this.managerDisparos = managerElementosTemporales;
            this.loader = loader;
            xwing = loader.loadSceneFromFile(VariablesGlobales.mediaDir +"XWing\\xwing-TgcScene.xml").Meshes[0];
            alaXwing = loader.loadSceneFromFile(VariablesGlobales.mediaDir +"XWing\\xwing-TgcScene.xml").Meshes[1];
            //Posicion, rotacion y escala inicial
            escala = new TGCVector3(escalar,escalar,escalar);//(0.1f, 0.1f, 0.1f);
            matrizXwingInicial = TGCMatrix.Scaling(escala);
            posicion = posicionInicial;
            rotation = new TGCVector3(0, FastMath.PI_HALF, -FastMath.QUARTER_PI*.8f);

            xwing.AutoTransformEnable = false;
            alaXwing.AutoTransformEnable = false;

            velocidadGeneral = 300f;// minimaVelocidad;
            //velocidadGeneral = 5f;//Bullet
            barrelRoll = false;
            ActualizarCoordenadaEsferica();

            //Rotation y animation
            rotationYAnimation = false;
            rotationYAnimacionAdvance = 0;

            //agrego al physics engine
            body_xwing = VariablesGlobales.physicsEngine.AgregarPersonaje( CommonHelper.MultiplicarVectores(xwing.BoundingBox.calculateSize(),escala),
                                                                           1, posicion,rotation,.5f,.1f,.1f,.5f,true);
            //
            VariablesGlobales.managerSonido.AgregarElemento(new Sonido("Sonidos\\XWing_flyby_2.wav", -600, 8, 1,0));
            VariablesGlobales.managerSonido.AgregarElemento(new Sonido("Sonidos\\XWing_engine.wav",-600,1,-1,0));
        }

        public override void Render()
        {
            xwing.Render();
            alaXwing.Render();
        }

        public override void RenderBoundingBox()
        {
            xwing.BoundingBox.Render();
            alaXwing.BoundingBox.Render();
        }

        public override void Update()
        {
            //Bullet
            TGCMatrix bullet_transform = new TGCMatrix(body_xwing.InterpolationWorldTransform);
            xwing.Transform = matrizXwingInicial * bullet_transform;//* TGCMatrix.Translation(posicion);
            alaXwing.Transform = matrizXwingInicial * bullet_transform;// * TGCMatrix.Translation(posicion);
            //
            //Forma normal
            xwing.Transform = matrizXwingInicial * GetRotationMatrix()* TGCMatrix.Translation(posicion);
            alaXwing.Transform = matrizXwingInicial * GetRotationMatrix() * TGCMatrix.Translation(posicion);
            //
        }

        private void TestingInput(TgcD3dInput input)
        {
            if (input.keyDown(Key.V))
            {
                velocidadGeneral += aceleracion;
            }
            if (input.keyDown(Key.C))
            {
                velocidadGeneral = minimaVelocidad;
            }

        }

        public void UpdateInput(TgcD3dInput input,float ElapsedTime)
        {
            //ElapsedTime = 0.01f; //Lo hardcodeo hasta que sepamos bien como hacer esto

            //Teclas especiales para moverse rapido y mas facil por el mapa
            TestingInput(input);

            //Movimientos flechas
            if (input.keyDown(Key.A))
            {
                rotation.Add(CommonHelper.ClampRotationY(TGCVector3.Down *ElapsedTime));
                ActualizarCoordenadaEsferica();
                body_xwing.ActivationState = ActivationState.ActiveTag;
                body_xwing.AngularVelocity = coordenadaEsferica.GetXYZCoord().ToBulletVector3();
                body_xwing.ApplyCentralImpulse(velocidadGeneral * coordenadaEsferica.GetXYZCoord().ToBulletVector3());
            }
            if (input.keyDown(Key.D))
            {
                rotation.Add(CommonHelper.ClampRotationY(TGCVector3.Up * ElapsedTime));
                ActualizarCoordenadaEsferica();
                body_xwing.ActivationState = ActivationState.ActiveTag;
                body_xwing.AngularVelocity = coordenadaEsferica.GetXYZCoord().ToBulletVector3();
                body_xwing.ApplyCentralImpulse(velocidadGeneral * coordenadaEsferica.GetXYZCoord().ToBulletVector3());
            }
            if (input.keyDown(Key.W) && !rotationYAnimation)
            {
                if (!swapPolarKeys)
                {
                    UpArrow(ElapsedTime);
                }
                else
                {
                    DownArrow(ElapsedTime);
                }
            }
            if (input.keyDown(Key.S) && !rotationYAnimation)
            {
                if (!swapPolarKeys)
                {
                    DownArrow(ElapsedTime);
                } else
                {
                    UpArrow(ElapsedTime);
                }
            }
            //Acelerar
            if (input.keyDown(Key.LeftShift) && velocidadGeneral < maximaVelocidad)
            {
                velocidadGeneral += (aceleracion * ElapsedTime);
            }
            //Frenar
            if (input.keyDown(Key.LeftControl))
            {
                velocidadGeneral -= (aceleracion *ElapsedTime / 2);
            }
            //Permite que la nave se detenga paulatinamente con la friccion
            if (velocidadGeneral > minimaVelocidad)
            {
                velocidadGeneral -= (friccion*ElapsedTime);
            }
            else
            {
                velocidadGeneral = minimaVelocidad;
            }

            //Disparar
            tiempoDesdeUltimoDisparo += ElapsedTime;
            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                if (tiempoDesdeUltimoDisparo > tiempoEntreDisparos) {
                    tiempoDesdeUltimoDisparo = 0f;
                    managerDisparos.AgregarElemento(new Misil(CalcularPosicionInicialMisil(), coordenadaEsferica,rotation, "Misil\\misil_xwing-TgcScene.xml"));
                    VariablesGlobales.managerSonido.AgregarElemento(new Sonido("Sonidos\\XWing_1_disparo.wav", 1,1f,1,0));
                }
            }

            //BarrelRoll con barra espaciadora
            if (input.keyPressed(Key.Space))
            {
                barrelRoll = true;
            }
            //Rotation y animation
            if (rotationYAnimation)
            {
                rotationYAnimacionAdvance += progressUnityRotationAdvance;
                rotation.Add(TGCVector3.Up * progressUnityRotationAdvance);
                if (rotationYAnimacionAdvance >= FastMath.PI)
                {
                    rotationYAnimacionAdvance = 0;
                    rotationYAnimation = false;
                }
                ActualizarCoordenadaEsferica();
            }
            if (barrelRoll)//Sigue andando mal :D
            {
                var angulo = barrelRollAvance * FastMath.TWO_PI / 100;
                
                if (barrelRollAvance == 0)
                {
                    if (ultimaPosicion.X - posicion.X >= 0) rotacionBarrelRoll = -FastMath.TWO_PI / 100;
                    else rotacionBarrelRoll = FastMath.TWO_PI / 100;
                }
                
                rotation.Add(left * rotacionBarrelRoll);

                barrelRollAvance++;
                if (barrelRollAvance >= 100)
                {
                    barrelRoll = false;
                    barrelRollAvance = 0;
                }
            }

            //Efecto de friccion, aceleracion o velocidad constante
            posicion = CalcularNuevaPosicion(posicion, ElapsedTime);
            ultimaPosicion = posicion + TGCVector3.One;
        }

        private void DownArrow(float ElapsedTime)
        {
            if (coordenadaEsferica.polar < (FastMath.PI - limiteAnguloPolar))
            {
                rotation.Add(back * ElapsedTime);
                ActualizarCoordenadaEsferica();
                body_xwing.ActivationState = ActivationState.ActiveTag;
                body_xwing.AngularVelocity = coordenadaEsferica.GetXYZCoord().ToBulletVector3();
                body_xwing.ApplyCentralImpulse(velocidadGeneral * coordenadaEsferica.GetXYZCoord().ToBulletVector3());
            }
            else
            {
                rotationYAnimation = true;
                swapPolarKeys = !swapPolarKeys;
            }
        }

        private void UpArrow(float ElapsedTime)
        {
            if (coordenadaEsferica.polar > limiteAnguloPolar)
            {
                rotation.Add(front * ElapsedTime);
                ActualizarCoordenadaEsferica();
                body_xwing.ActivationState = ActivationState.ActiveTag;
                body_xwing.AngularVelocity = coordenadaEsferica.GetXYZCoord().ToBulletVector3();
                body_xwing.ApplyCentralImpulse(velocidadGeneral * coordenadaEsferica.GetXYZCoord().ToBulletVector3());
            }
            else
            {
                rotationYAnimation = true;
                swapPolarKeys = !swapPolarKeys;
            }
        }

        public TGCMatrix GetRotationMatrix()
        {
            return
                TGCMatrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        }

        private void ActualizarCoordenadaEsferica()
        {
            coordenadaEsferica = new CoordenadaEsferica(rotation);
        }

        private TGCVector3 CalcularNuevaPosicion(TGCVector3 posicion, float ElapsedTime)
        {
            float x = posicion.X + velocidadGeneral * coordenadaEsferica.GetXCoord() * ElapsedTime;
            float y = posicion.Y + velocidadGeneral * coordenadaEsferica.GetYCoord() * ElapsedTime;
            float z = posicion.Z + velocidadGeneral * coordenadaEsferica.GetZCoord() * ElapsedTime;
            return new TGCVector3(x, y, z);
        }

        public override void Dispose()
        {
            xwing.Dispose();
        }

        public float GetVelocidadGeneral()
        {
            return velocidadGeneral;
        }

        public float GetVelocidadMaxima()
        {
            return maximaVelocidad;
        }

        public Boolean MaxSpeed()
        {
            return FastMath.Abs(velocidadGeneral - maximaVelocidad) < 20; 
        }

        public TGCVector3 GetPosition()
        {
            //return new TGCVector3(body_xwing.CenterOfMassPosition);//Bullet
            return posicion;//forma normal
        }

        public CoordenadaEsferica GetCoordenadaEsferica()
        {
            return coordenadaEsferica;
        }
        private TGCVector3 CalcularOffsetUnAla()
        {
            Random rnd = new Random();
            int rndLargo = (rnd.Next(1, 3)==1) ? 1 : -1;
            int rndAncho = (rnd.Next(1, 3)==1) ? 1 : -1;
            var largoOffset = rndLargo * this.alaXwing.BoundingBox.calculateSize().Z * .5f;
            var anchoOffset = rndAncho * this.alaXwing.BoundingBox.calculateSize().Y * .5f;
            var distancia = -this.alaXwing.BoundingBox.calculateSize().X * 1.5f;
            return new TGCVector3(largoOffset, anchoOffset, distancia);
        }

        /**
 * Devuelve la posicion inicial desde donde sale el misil en funcion de la posicion y la rotacion de la nave.
 * */
        private TGCVector3 CalcularPosicionInicialMisil()
        {
            //Random de las 4 opciones posibles de caniones
            Random random = new Random();
            int canion = (int)Math.Round(random.NextDouble() * 4);
            int signo = 0;
            bool canionInferior = false;
            //Caniones izquierdos o derechos
            if (canion == 0 || canion == 2)
            {
                signo = 1;
            }
            else
            {
                signo = -1;
            }
            //Caniones inferiores o superiores
            if (canion == 2 || canion == 3)
            {
                canionInferior = true;
            }
            else
            {
                canionInferior = false;
            }
            //Delta en la direccion nave (para que no parezca que sale de atras)
            TGCVector3 deltaDireccionNave = new TGCVector3(coordenadaEsferica.GetXYZCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_NAVE);
            //Delta en la direccion ortogonal a la direccion de la nave (para que salga desde alguna de las alas)
            //Caniones inferiores
            if (canionInferior)
            {
                CoordenadaEsferica direccionOrtogonal = new CoordenadaEsferica(coordenadaEsferica.acimutal + (FastMath.PI / 2) * signo, FastMath.PI / 2 + EXTRA_ANGULO_POLAR_CANION_ABAJO);
                TGCVector3 deltaOrtogonalNave =
                    new TGCVector3(direccionOrtogonal.GetXYZCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL);
                return CommonHelper.SumarVectores(CommonHelper.SumarVectores(posicion, deltaDireccionNave), deltaOrtogonalNave);
            }
            else
            //Caniones superiores
            {
                CoordenadaEsferica direccionOrtogonal = new CoordenadaEsferica(coordenadaEsferica.acimutal + (FastMath.PI / 2) * signo, FastMath.PI / 2);
                TGCVector3 deltaOrtogonalNave =
                    new TGCVector3(direccionOrtogonal.GetXCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL,
                    direccionOrtogonal.GetYCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL,
                    direccionOrtogonal.GetZCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL);
                return CommonHelper.SumarVectores(CommonHelper.SumarVectores(posicion, deltaDireccionNave), deltaOrtogonalNave);
            }
        }
    }
}
