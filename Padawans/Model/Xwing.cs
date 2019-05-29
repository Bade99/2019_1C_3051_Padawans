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
        private bool BULLET = false;

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

        private float DISTANCIA_ORIGEN_MISIL_DIRECCION_NAVE = 44;
        private float DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL = 6;
        private float EXTRA_ANGULO_POLAR_CANION_ABAJO = 0.3f;

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

        private float velocidadBullet;

        public Xwing(TgcSceneLoader loader, TGCVector3 posicionInicial)
        {

            //TGCMatrix rotation_matrix = TGCMatrix.RotationX(1); 

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

            if(BULLET) velocidadGeneral = 5f;//Bullet
            else velocidadGeneral = 300f;// minimaVelocidad;

            barrelRoll = false;
            ActualizarCoordenadaEsferica();

            //Rotation y animation
            rotationYAnimation = false;
            rotationYAnimacionAdvance = 0;

            //agrego al physics engine
            body_xwing = VariablesGlobales.physicsEngine.AgregarPersonaje( CommonHelper.MultiplicarVectores(xwing.BoundingBox.calculateSize(),escala),
                                                                           1, posicion,rotation,1,true);


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
            if (BULLET)//movimiento constante
            {
            /*personaje_body.AngularVelocity = rotation.ToBulletVector3();
            body_xwing.ApplyCentralForce(new BulletSharp.Math.Vector3(0, 0, -10) * 5);*/
            }

            if (BULLET)
            {
            //Bullet
            TGCMatrix bullet_transform = new TGCMatrix(body_xwing.InterpolationWorldTransform);
            xwing.Transform = matrizXwingInicial * bullet_transform;//* TGCMatrix.Translation(posicion);
            alaXwing.Transform = matrizXwingInicial * bullet_transform;// * TGCMatrix.Translation(posicion);
            //
            } else
            {
            //
            //Forma normal
            xwing.Transform = matrizXwingInicial * GetRotationMatrix()* TGCMatrix.Translation(posicion);
            alaXwing.Transform = matrizXwingInicial * GetRotationMatrix() * TGCMatrix.Translation(posicion);
            //
            }
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
            ElapsedTime = 0.01f; //Lo hardcodeo hasta que sepamos bien como hacer esto

            //Teclas especiales para moverse rapido y mas facil por el mapa
            TestingInput(input);

            //Movimientos flechas
            if (input.keyDown(Key.A))
            {
                if (BULLET)
                {
                    //body_xwing.AngularVelocity = coordenadaEsferica.GetXYZCoord().ToBulletVector3();
                    //body_xwing.ApplyCentralImpulse(new BulletSharp.Math.Vector3(1,0,0));
                    //body_xwing.ApplyTorque(new BulletSharp.Math.Vector3(1, 0, 0));//piola gira el xwing y parece q puedo usar direcciones (1,0,0) dsps
                    //body_xwing.ApplyTorqueImpulse(new BulletSharp.Math.Vector3(1, 0, 0));creo que no
                    //body_xwing.CheckCollideWith(CollisionObject); ver q onda
                    //body_xwing.CollisionFlags ni idea mirar
                    //body_xwing.CompanionId hmmm
                    //body_xwing.ComputeGyroscopicImpulseImplicitBody(step) servira para saber el giro? devuelve vector3
                    //tiene otras funciones compute tmb
                    //body_xwing.ContactSolverType ???
                    //body_xwing.Friction
                    //body_xwing.ActivationState creo q es al pedo setearlo
                    //body_xwing.GetConstraintRef tiene esta cosa de constraints ver q onda
                    //body_xwing.GetVelocityInLocalPoint ???
                    //body_xwing.GetWorldTransform hmmm
                    //body_xwing.InterpolationAngularVelocity apa parece util
                    //body_xwing.InterpolationLinearVelocity + cosas utiles
                    //body_xwing.InterpolationWorldTransform el q usamos
                    //body_xwing.InvInertiaDiagLocal ??
                    //body_xwing.IsKinematicObject como consigo un kinematic object?
                    //body_xwing.LinearFactor ??
                    //body_xwing.LinearVelocity apa
                    //body_xwing.MotionState hmm
                    //body_xwing.Orientation apa apa (en quaternion)
                    //body_xwing.PredictIntegratedTransform maemia podes hacer predicciones
                    //body_xwing.ProceedToTransform le podes enchufar un transform
                    //body_xwing. hay un custom debug color, no se si lo podemos renderizar o algo
                    //body_xwing.Restitution algo a setear!
                    //body_xwing.RollingFriction algo a setear!
                    //body_xwing.SetContactStiffnessAndDamping algo a setear!
                    //body_xwing.SetDamping algo a setear!
                    //body_xwing.SetIgnoreCollisionCheck podes ignorar colisiones con algo
                    //body_xwing.SetMassProps podes resetear la masa e inercia (tmb hay un ref, no se si te permitir� guardar la inercia?)
                    //body_xwing.SetSleepingThresholds ni idea pero ver setear!
                    //body_xwing.SpinningFriction algo a setear!
                    //body_xwing.TotalForce te dice cuanto es
                    //body_xwing.TotalTorque te dice cuanto es
                    //body_xwing.Translate lo puedo mover hmm (tiene ref tmb)
                    //body_xwing.WorldArrayIndex q he esto ?
                    //body_xwing.WorldTransform q ser� esto ?
                }
                else
                {
                rotation.Add(CommonHelper.ClampRotationY(TGCVector3.Down *ElapsedTime));
                ActualizarCoordenadaEsferica();
                }
            }
            if (input.keyDown(Key.D))
            {
                if (BULLET)
                {
                    //body_xwing.AngularVelocity = coordenadaEsferica.GetXYZCoord().ToBulletVector3();
                    body_xwing.ApplyCentralImpulse(new BulletSharp.Math.Vector3(-1, 0, 0));
                }
                else
                {
                rotation.Add(CommonHelper.ClampRotationY(TGCVector3.Up * ElapsedTime));
                ActualizarCoordenadaEsferica();
                }
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
                    VariablesGlobales.managerElementosTemporales.AgregarElemento(new Misil(CalcularPosicionInicialMisil(), coordenadaEsferica,rotation, "Misil\\misil_xwing-TgcScene.xml", Color.Green));
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
            if (BULLET)
            {

            }
            else
            {
            //Efecto de friccion, aceleracion o velocidad constante
            posicion = CalcularNuevaPosicion(posicion, ElapsedTime);
            ultimaPosicion = posicion + TGCVector3.One;

            }
        }

        private void DownArrow(float ElapsedTime)
        {
            if (coordenadaEsferica.polar < (FastMath.PI - limiteAnguloPolar))
            {
                if (BULLET)
                {
                    //body_xwing.AngularVelocity = coordenadaEsferica.GetXYZCoord().ToBulletVector3();
                body_xwing.ApplyCentralImpulse(new BulletSharp.Math.Vector3(0, -1, 0));
                } else
                {
                rotation.Add(back * ElapsedTime);
                ActualizarCoordenadaEsferica();
                }
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
                if (BULLET)
                {
                    //body_xwing.AngularVelocity = coordenadaEsferica.GetXYZCoord().ToBulletVector3();
                body_xwing.ApplyCentralImpulse(new BulletSharp.Math.Vector3(0, 1, 0));
                } else
                {
                rotation.Add(front * ElapsedTime);
                ActualizarCoordenadaEsferica();
                }
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
            if(BULLET) return new TGCVector3(body_xwing.CenterOfMassPosition);//Bullet
            else return posicion;//forma normal
        }

        public CoordenadaEsferica GetCoordenadaEsferica()
        {
            return coordenadaEsferica;
        }
        /*
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
        */

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
                return CommonHelper.SumarVectores(CommonHelper.SumarVectores(GetPosition(), deltaDireccionNave), deltaOrtogonalNave);
            }
            else
            //Caniones superiores
            {
                CoordenadaEsferica direccionOrtogonal = new CoordenadaEsferica(coordenadaEsferica.acimutal + (FastMath.PI / 2) * signo, FastMath.PI / 2);
                TGCVector3 deltaOrtogonalNave =
                    new TGCVector3(direccionOrtogonal.GetXCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL,
                    direccionOrtogonal.GetYCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL,
                    direccionOrtogonal.GetZCoord() * DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL);
                return CommonHelper.SumarVectores(CommonHelper.SumarVectores(GetPosition(), deltaDireccionNave), deltaOrtogonalNave);
            }
        }
    }
}
