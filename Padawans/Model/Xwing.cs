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
using BulletSharp.Math;

namespace TGC.Group.Model
{
    /// <summary>
    ///     La nave principal que utiliza el jugador
    /// </summary>
    public class Xwing : SceneElement, InteractiveElement, IPostProcess
    {
        private TgcSceneLoader loader;
        TgcMesh xwing,alaXwing;
        private TemporaryElementManager managerDisparos;
        public RigidBody body_xwing;


        //Post processing
        TgcMesh[] bloom;
        //

        //Constantes
        private readonly float minimaVelocidad = 7f;
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

        private readonly float DISTANCIA_ORIGEN_MISIL_DIRECCION_NAVE = 44;
        private readonly float DISTANCIA_ORIGEN_MISIL_DIRECCION_ORTOGONAL = 6;
        private readonly float EXTRA_ANGULO_POLAR_CANION_ABAJO = 0.3f;

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

        //private readonly float velocidad_giro_bullet = 5f;
        private float tiempo =5;

        public Xwing(TgcSceneLoader loader, TGCVector3 posicionInicial)
        {

            //TGCMatrix rotation_matrix = TGCMatrix.RotationX(1); 

            this.loader = loader;
            xwing = loader.loadSceneFromFile(VariablesGlobales.mediaDir +"XWing\\xwing-TgcScene.xml").Meshes[0];
            alaXwing = loader.loadSceneFromFile(VariablesGlobales.mediaDir +"XWing\\xwing-TgcScene.xml").Meshes[1];

            //Shader
            if (VariablesGlobales.SHADERS)
            {
                xwing.Effect = VariablesGlobales.shader;
                alaXwing.Effect = VariablesGlobales.shader;
            }

            //Posicion, rotacion y escala inicial
            escala = new TGCVector3(escalar,escalar,escalar);//(0.1f, 0.1f, 0.1f);
            matrizXwingInicial = TGCMatrix.Scaling(escala);
            posicion = posicionInicial;
            rotation = new TGCVector3(0, FastMath.PI_HALF, -FastMath.QUARTER_PI*.8f);

            xwing.AutoTransformEnable = false;
            alaXwing.AutoTransformEnable = false;

            if(VariablesGlobales.BULLET) velocidadGeneral = 5f;//Bullet
            else velocidadGeneral = 300f;// minimaVelocidad;

            barrelRoll = false;

            //Rotation y animation
            rotationYAnimation = false;
            rotationYAnimacionAdvance = 0;

            //agrego al physics engine
            body_xwing = VariablesGlobales.physicsEngine.AgregarPersonaje( CommonHelper.MultiplicarVectores(xwing.BoundingBox.calculateSize(),escala),
                                                                           1, posicion,rotation,.5f,true);
            //@Params para modificar
            //body_xwing.Friction = 1;
            //body_xwing.Restitution=.1f;
            //body_xwing.SetDamping(.5f,.5f);
            //body_xwing.SpinningFriction=1;

            ActualizarCoordenadaEsferica();

            bloom = new TgcMesh[2];
            bloom[0] = loader.loadSceneFromFile(VariablesGlobales.mediaDir + "Postprocess\\Bloom\\Main_Xwing\\main_xwing.xml").Meshes[0];
            bloom[1] = loader.loadSceneFromFile(VariablesGlobales.mediaDir + "Postprocess\\Bloom\\Main_Xwing\\main_xwing.xml").Meshes[1];
            bloom[0].AutoTransformEnable = false; bloom[1].AutoTransformEnable = false;
            //

            VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.FLYBY_2);
            VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.XWING_ENGINE);
        }

        public void Render(string technique)
        {
            xwing.Technique = technique;
            xwing.Render();
            alaXwing.Technique = technique;
            alaXwing.Render();
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

        public void RenderPostProcess(string effect)//renderiza el xwing q solo tiene las partes que se necesitan para ese efecto
        {
            if (effect == "bloom")
            {
                if (VariablesGlobales.BULLET)
                {
                    //Bullet
                    TGCMatrix bullet_transform = new TGCMatrix(body_xwing.InterpolationWorldTransform);
                    bloom[0].Transform = matrizXwingInicial * bullet_transform;
                    bloom[1].Transform = matrizXwingInicial * bullet_transform;
                    //
                }
                else
                {
                    //
                    //Forma normal
                    bloom[0].Transform = matrizXwingInicial * GetRotationMatrix() * TGCMatrix.Translation(posicion);
                    bloom[1].Transform = matrizXwingInicial * GetRotationMatrix() * TGCMatrix.Translation(posicion);
                    //
                }
                bloom[0].Render();
                bloom[1].Render();
            }
        }

        public override void Update()
        {
            if (VariablesGlobales.BULLET)
            {
            //Bullet
            TGCMatrix bullet_transform = new TGCMatrix(body_xwing.InterpolationWorldTransform);
            xwing.Transform = matrizXwingInicial * bullet_transform;
            alaXwing.Transform = matrizXwingInicial * bullet_transform;
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

        public void UpdateInput(TgcD3dInput input, float ElapsedTime)
        {
            if (VariablesGlobales.BULLET) UpdateInputBullet(input, ElapsedTime);
            else UpdateInputManual(input, ElapsedTime);
        }

        public void UpdateInputBullet(TgcD3dInput input, float ElapsedTime)//no uso elapsedtime x ahora, creo q ni hace falta
        {
            body_xwing.ApplyCentralForce(coordenadaEsferica.GetXYZCoord().ToBulletVector3()* velocidadGeneral *ElapsedTime);

            //Movimientos flechas
            if (input.keyDown(Key.A))
            {
                body_xwing.ApplyTorque(TGCVector3.Down.ToBulletVector3());
            }
            if (input.keyDown(Key.D))
            {

                body_xwing.ApplyTorque(TGCVector3.Up.ToBulletVector3());

                //body_xwing.AngularVelocity = coordenadaEsferica.GetXYZCoord().ToBulletVector3();
                //body_xwing.ApplyCentralImpulse(new BulletSharp.Math.Vector3(-1, 0, 0));
            }
            if (input.keyDown(Key.W) && !rotationYAnimation)
            {
                if (!swapPolarKeys)
                {
                    UpArrowBullet(ElapsedTime);
                }
                else
                {
                    DownArrowBullet(ElapsedTime);
                }
            }
            if (input.keyDown(Key.S) && !rotationYAnimation)
            {
                if (!swapPolarKeys)
                {
                    DownArrowBullet(ElapsedTime);
                }
                else
                {
                    UpArrowBullet(ElapsedTime);
                }
            }

            AcelerarYFrenar(input, ElapsedTime);
            Disparar(input, ElapsedTime);
            ActualizarCoordenadaEsferica();
        }

        private void DownArrowBullet(float ElapsedTime)
        {
            if (coordenadaEsferica.polar < (FastMath.PI - limiteAnguloPolar))
            {
                var down_arrow = new TGCVector3(-1, 0, 0);

                body_xwing.ApplyTorque(down_arrow.ToBulletVector3());
            }
            else
            {
                rotationYAnimation = true;
                swapPolarKeys = !swapPolarKeys;
            }
        }

        private void UpArrowBullet(float ElapsedTime)
        {
            if (coordenadaEsferica.polar > limiteAnguloPolar)
            {
                var up_arrow = new TGCVector3(1, 0, 0);

                body_xwing.ApplyTorque(up_arrow.ToBulletVector3());
            }
            else
            {
                rotationYAnimation = true;
                swapPolarKeys = !swapPolarKeys;
            }
        }

        public void UpdateInputManual(TgcD3dInput input,float ElapsedTime)
        {
            ElapsedTime = 0.01f; //Lo hardcodeo hasta que sepamos bien como hacer esto

            //Teclas especiales para moverse rapido y mas facil por el mapa
            TestingInput(input);

            //Movimientos flechas
            if (input.keyDown(Key.A))
            {
                rotation.Add(CommonHelper.ClampRotationY(TGCVector3.Down *ElapsedTime));
                ActualizarCoordenadaEsferica();
            }
            if (input.keyDown(Key.D))
            {
                rotation.Add(CommonHelper.ClampRotationY(TGCVector3.Up * ElapsedTime));
                ActualizarCoordenadaEsferica();
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
            AcelerarYFrenar(input, ElapsedTime);
            Disparar(input, ElapsedTime);

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
            posicion = CommonHelper.MoverPosicionEnDireccionCoordenadaEsferica(posicion, coordenadaEsferica, 
                velocidadGeneral, ElapsedTime);
            ultimaPosicion = posicion + TGCVector3.One;
        }

        private void Disparar(TgcD3dInput input, float ElapsedTime)
        {
            tiempoDesdeUltimoDisparo += ElapsedTime;
            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                if (tiempoDesdeUltimoDisparo > tiempoEntreDisparos)
                {
                    tiempoDesdeUltimoDisparo = 0f;
                    VariablesGlobales.managerElementosTemporales.AgregarElemento(new Misil(CalcularPosicionInicialMisil(), coordenadaEsferica, rotation, "Misil\\misil_xwing-TgcScene.xml"));
                    VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.DISPARO_MISIL_XWING);
                }
            }
        }

        private void AcelerarYFrenar(TgcD3dInput input, float ElapsedTime)
        {
            //Acelerar
            if (input.keyDown(Key.LeftShift) && velocidadGeneral < maximaVelocidad)
            {
                velocidadGeneral += (aceleracion * ElapsedTime);
            }
            //Frenar
            if (input.keyDown(Key.LeftControl))
            {
                velocidadGeneral -= (aceleracion * ElapsedTime / 2);
            }
            //Permite que la nave se detenga paulatinamente con la friccion
            if (velocidadGeneral > minimaVelocidad)
            {
                velocidadGeneral -= (friccion * ElapsedTime);
            }
            else
            {
                velocidadGeneral = minimaVelocidad;
            }
        }

        private void DownArrow(float ElapsedTime)
        {
            if (coordenadaEsferica.polar < (FastMath.PI - limiteAnguloPolar))
            {
                rotation.Add(back * ElapsedTime);
                ActualizarCoordenadaEsferica();
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
            if (VariablesGlobales.BULLET)
            {
                body_xwing.InterpolationWorldTransform.Decompose(out _, out Quaternion rotation, out _);
                TGCVector3 rotationEuler = CommonHelper.QuaternionToEuler(rotation);
                coordenadaEsferica = new CoordenadaEsferica(rotationEuler);
            }
            else
            {
                coordenadaEsferica = new CoordenadaEsferica(rotation);
            }
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
            if (VariablesGlobales.BULLET)
            {
                return new TGCVector3(body_xwing.CenterOfMassPosition);
            }
            else
            {
                return posicion;
            }
        }

        public CoordenadaEsferica GetCoordenadaEsferica()
        {
            return coordenadaEsferica;
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
