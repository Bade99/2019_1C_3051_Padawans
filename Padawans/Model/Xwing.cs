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
    public class Xwing : BulletSceneElement, InteractiveElement, IPostProcess,ITarget
    {
        private TgcSceneLoader loader;
        TgcMesh xwing,alaXwing;

        //Post processing
        TgcMesh[] bloom;
        //

        //Constantes
        private readonly float minimaVelocidad = 7f;
        private readonly float aceleracion = 80;
        private readonly float friccion = 10f;
        private readonly float maximaVelocidad = 300;
        private readonly float limiteAnguloPolar=0.1f;
        private readonly float progressUnityRotationAdvance = FastMath.PI / 10;

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
        private TGCVector3 ultimaPosicion;
        private float rotacionBarrelRoll;
        private float tiempoEntreDisparos=.5f;
        private float tiempoDesdeUltimoDisparo = .5f;
        private float tiempoDesdeUltimaBomba = 5f;
        private float tiempoEntreBombas = 5f;
        //matrices de transformaciones
        private TGCMatrix matrizXwingInicial;

        //propiedades de la nave
        private float escalar = .1f;

        public Xwing(TgcSceneLoader loader, TGCVector3 posicionInicial)
        {
            this.loader = loader;
            xwing = loader.loadSceneFromFile(VariablesGlobales.mediaDir +"XWing\\xwing-TgcScene.xml").Meshes[0];
            alaXwing = loader.loadSceneFromFile(VariablesGlobales.mediaDir +"XWing\\xwing-TgcScene.xml").Meshes[1];
            //Shader
            if (VariablesGlobales.SHADERS)
            {
                VariablesGlobales.shaderManager.AgregarMesh(xwing, ShaderManager.MESH_TYPE.SHADOW);
                VariablesGlobales.shaderManager.AgregarMesh(alaXwing, ShaderManager.MESH_TYPE.SHADOW);
            }

            //Posicion, rotacion y escala inicial
            TGCVector3 escala = new TGCVector3(escalar, escalar, escalar);
            matrizInicialTransformacion = TGCMatrix.Scaling(escala);
            rotation = new TGCVector3(0, FastMath.PI_HALF, -FastMath.QUARTER_PI*.8f);

            xwing.AutoTransformEnable = false;
            alaXwing.AutoTransformEnable = false;

            if (VariablesGlobales.BULLET)
            {
                velocidadGeneral = 5f;
            }
            else
            {
                velocidadGeneral = 300f;
            }

            barrelRoll = false;

            //Rotation y animation
            rotationYAnimation = false;
            rotationYAnimacionAdvance = 0;

            //agrego al physics engine
            body = VariablesGlobales.physicsEngine.AgregarPersonaje( CommonHelper.MultiplicarVectores(xwing.BoundingBox.calculateSize(),escala),
                                                                           0.1f, posicionInicial, TGCVector3.Empty);
            //@Params para modificar
            //body_xwing.Friction = 1;
            //body_xwing.Restitution=.1f;
            //body_xwing.SetDamping(.5f,.5f);
            //body_xwing.SpinningFriction=1;

            ActualizarCoordenadaEsferica();

            bloom = new TgcMesh[2];
            bloom[0] = loader.loadSceneFromFile(VariablesGlobales.mediaDir + "Postprocess\\Bloom\\Main_Xwing\\main_xwing.xml").Meshes[0];
            bloom[1] = loader.loadSceneFromFile(VariablesGlobales.mediaDir + "Postprocess\\Bloom\\Main_Xwing\\main_xwing.xml").Meshes[1];
            bloom[0].AutoTransformEnable = false;
            bloom[1].AutoTransformEnable = false;

            //Bullet meshs
            meshs = new TgcMesh[4];
            meshs[0] = xwing;
            meshs[1] = alaXwing;
            meshs[2] = bloom[0];
            meshs[3] = bloom[1];

            VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.FLYBY_2);
            VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.XWING_ENGINE);
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
                TGCMatrix bullet_transform = new TGCMatrix(body.InterpolationWorldTransform);
                bloom[0].Render();
                bloom[1].Render();
            }
        }

        public override void Update()
        {
            UpdateBullet();
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
            UpdateInputManual(input, ElapsedTime);
        }

        public void UpdateInputManual(TgcD3dInput input,float ElapsedTime)
        {
            //ElapsedTime = 0.01f; //Lo hardcodeo hasta que sepamos bien como hacer esto

            //Teclas especiales para moverse rapido y mas facil por el mapa
            TestingInput(input);
            if (input.keyDown(Key.X))
            {
                rotation.X += ElapsedTime;
            }
            if (input.keyDown(Key.Y))
            {
                rotation.Y += ElapsedTime;
            }
            if (input.keyDown(Key.Z))
            {
                rotation.Z += ElapsedTime;
            }
            //Movimientos flechas
            if (input.keyDown(Key.A))
            {
                rotation.Add(TGCVector3.Down *ElapsedTime);
                ActualizarCoordenadaEsferica();
            }
            if (input.keyDown(Key.D))
            {
                rotation.Add(TGCVector3.Up * ElapsedTime);
                ActualizarCoordenadaEsferica();
            }
            if (input.keyDown(Key.W) && !rotationYAnimation)
            {
                UpArrow(ElapsedTime);
            }
            if (input.keyDown(Key.S) && !rotationYAnimation)
            {
                DownArrow(ElapsedTime);
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

            bulletVelocity = CommonHelper.VectorXEscalar(coordenadaEsferica.GetXYZCoord(), velocidadGeneral);
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
            tiempoDesdeUltimaBomba += ElapsedTime;
            if (input.keyPressed(Key.G))
            {
                if (tiempoDesdeUltimaBomba > tiempoEntreBombas)
                {
                    tiempoDesdeUltimaBomba = 0f;
                    var bomba = new Bomba(this.GetPosition(),coordenadaEsferica);
                    VariablesGlobales.endgameManager.AgregarBomba(bomba);
                    VariablesGlobales.managerElementosTemporales.AgregarElemento(bomba);
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

            }
        }

        private void ActualizarCoordenadaEsferica()
        {
            coordenadaEsferica = new CoordenadaEsferica(this.rotation);
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
            return new TGCVector3(body.CenterOfMassPosition);
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
