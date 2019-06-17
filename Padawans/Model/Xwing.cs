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
        private readonly float minimaVelocidad = 30;
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

        private ESTADO_BARREL estadoBarrel = ESTADO_BARREL.NADA;
        private readonly float tiempoPermanenciaMedioBarrelRoll = 2;
        private float medioBarrelRollTimer = 0;
        private bool rotationYAnimation;
        private float rotationYAnimacionAdvance;
        private float barrelRollAdvance;
        private float tiempoEntreDisparos=.5f;
        private float tiempoDesdeUltimoDisparo = .5f;
        private float tiempoDesdeUltimaBomba = 5f;
        private float tiempoEntreBombas = 5f;

        //propiedades de la nave
        private float escalar = .1f;
        TGCVector3 escala;

        //Ingreso modo Dios
        private INGRESO_MODO_DIOS ingresoModoDios = INGRESO_MODO_DIOS.NADA;
        private float timer;
        private float limiteTimer = 5;
        private float toleranciaDeRepeticion = 0.08f;

        public Xwing(TgcSceneLoader loader, TGCVector3 posicionInicial)
        {
            this.loader = loader;
            xwing = loader.loadSceneFromFile(VariablesGlobales.mediaDir +"XWing\\xwing-TgcScene.xml").Meshes[0];
            alaXwing = loader.loadSceneFromFile(VariablesGlobales.mediaDir +"XWing\\xwing-TgcScene.xml").Meshes[1];

            escala = new TGCVector3(escalar, escalar, escalar);
            matrizInicialTransformacion = TGCMatrix.Scaling(escala);
            rotation = new TGCVector3(0, FastMath.PI_HALF, -FastMath.QUARTER_PI*.8f);
            position = posicionInicial;

            barrelRollAdvance = 0;
            rotationYAnimation = false;
            rotationYAnimacionAdvance = 0;
            velocidadGeneral = 5f;

            xwing.AutoTransformEnable = false;
            alaXwing.AutoTransformEnable = false;

            if (VariablesGlobales.SHADERS)
            {
                VariablesGlobales.shaderManager.AgregarMesh(xwing, ShaderManager.MESH_TYPE.SHADOW);
                VariablesGlobales.shaderManager.AgregarMesh(alaXwing, ShaderManager.MESH_TYPE.SHADOW);
            }
            TGCVector3 sizeEscalado = CommonHelper.VectorXEscalar(xwing.BoundingBox.calculateSize(), escalar);
            collisionObject = VariablesGlobales.physicsEngine.AgregarPersonaje(sizeEscalado);
            ActualizarCoordenadaEsferica();

            bloom = new TgcMesh[2];
            bloom[0] = loader.loadSceneFromFile(VariablesGlobales.mediaDir + "Postprocess\\Bloom\\Main_Xwing\\main_xwing.xml").Meshes[0];
            bloom[1] = loader.loadSceneFromFile(VariablesGlobales.mediaDir + "Postprocess\\Bloom\\Main_Xwing\\main_xwing.xml").Meshes[1];
            bloom[0].AutoTransformEnable = false;
            bloom[1].AutoTransformEnable = false;

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

        public void RenderPostProcess(string effect)
        {
            if (effect == "bloom")
            {
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
            //ElapsedTime = 0.01f; //Lo hardcodeo hasta que sepamos bien como hacer esto
            TestingInput(input);
            MovimientoFlechas(input, ElapsedTime);
            AcelerarYFrenar(input, ElapsedTime);
            Disparar(input, ElapsedTime);
            BarrelRoll(input, ElapsedTime);
            MedioBarrelRoll(input, ElapsedTime);
            RotationYAnimation();
            EfectoFriccion(ElapsedTime);
        }
        private void RotationYAnimation()
        {
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
        }
        private void BarrelRoll(TgcD3dInput input, float ElapsedTime)
        {
            if (input.keyPressed(Key.Space) && ESTADO_BARREL.NADA.Equals(estadoBarrel))
            {
                estadoBarrel = ESTADO_BARREL.BARRELROLL;
            }
            if (ESTADO_BARREL.BARRELROLL.Equals(estadoBarrel))
            {
                barrelRollAdvance += ElapsedTime * 4;
                matrizInicialTransformacion = matrizInicialTransformacion = TGCMatrix.Scaling(escala)
                    * TGCMatrix.RotationYawPitchRoll(0, barrelRollAdvance, 0);
                if (barrelRollAdvance >= FastMath.TWO_PI)
                {
                    barrelRollAdvance = 0;
                    estadoBarrel = ESTADO_BARREL.NADA;
                }
            }
        }
        private void MedioBarrelRoll(TgcD3dInput input, float ElapsedTime)
        {
            if (input.keyPressed(Key.B) && ESTADO_BARREL.NADA.Equals(estadoBarrel))
            {
                estadoBarrel = ESTADO_BARREL.MEDIO_BARRELROLL;
                medioBarrelRollTimer = 0;
            }
            if (ESTADO_BARREL.MEDIO_BARRELROLL.Equals(estadoBarrel))
            {
                barrelRollAdvance += ElapsedTime * 4;
                matrizInicialTransformacion = matrizInicialTransformacion = TGCMatrix.Scaling(escala)
                    * TGCMatrix.RotationYawPitchRoll(0, barrelRollAdvance, 0);
                if (barrelRollAdvance >= FastMath.PI_HALF)
                {
                    estadoBarrel = ESTADO_BARREL.ESPERA_MEDIO_BARRELROLL;
                }
            }
            if (ESTADO_BARREL.ESPERA_MEDIO_BARRELROLL.Equals(estadoBarrel))
            {
                medioBarrelRollTimer += ElapsedTime;
                if (medioBarrelRollTimer >= tiempoPermanenciaMedioBarrelRoll)
                {
                    estadoBarrel = ESTADO_BARREL.VOLVIENDO_MEDIO_BARRELROLL;
                }
            }
            if (ESTADO_BARREL.VOLVIENDO_MEDIO_BARRELROLL.Equals(estadoBarrel))
            {
                barrelRollAdvance -= ElapsedTime * 4;
                if (barrelRollAdvance <= 0)
                {
                    barrelRollAdvance = 0;
                    estadoBarrel = ESTADO_BARREL.NADA;
                }
                matrizInicialTransformacion = matrizInicialTransformacion = TGCMatrix.Scaling(escala)
                    * TGCMatrix.RotationYawPitchRoll(0, barrelRollAdvance, 0);
            }

        }
        private void MovimientoFlechas(TgcD3dInput input, float ElapsedTime)
        {
            if (input.keyDown(Key.A))
            {
                rotation.Add(TGCVector3.Down * ElapsedTime);
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
        }

        private void Disparar(TgcD3dInput input, float ElapsedTime)
        {
            tiempoDesdeUltimoDisparo += ElapsedTime;
            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                if (tiempoDesdeUltimoDisparo > tiempoEntreDisparos)
                {
                    tiempoDesdeUltimoDisparo = 0f;
                    VariablesGlobales.managerElementosTemporales.AgregarElemento(new Misil(CalcularPosicionInicialMisil(), coordenadaEsferica, rotation, "Misil\\misil_xwing-TgcScene.xml", Misil.OrigenMisil.XWING));
                    VariablesGlobales.managerSonido.ReproducirSonido(SoundManager.SONIDOS.DISPARO_MISIL_XWING);
                }
            }

            tiempoDesdeUltimaBomba += ElapsedTime;
            if (tiempoDesdeUltimaBomba > tiempoEntreBombas) VariablesGlobales.SumarBomba();

            if (input.keyPressed(Key.G))
            {
                if (tiempoDesdeUltimaBomba > tiempoEntreBombas)
                {
                    VariablesGlobales.RestarBomba();
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
            if (input.keyDown(Key.LeftShift) && velocidadGeneral < maximaVelocidad)
            {
                velocidadGeneral += (aceleracion * ElapsedTime);
            }
            if (input.keyDown(Key.LeftControl))
            {
                velocidadGeneral -= (aceleracion * ElapsedTime / 2);
            }
        }

        private void EfectoFriccion(float ElapsedTime)
        {
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
            return position;
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

        private enum INGRESO_MODO_DIOS { NADA, I, D_1, D_2, Q, D_3}
        private enum ESTADO_BARREL { NADA, BARRELROLL, MEDIO_BARRELROLL, ESPERA_MEDIO_BARRELROLL, VOLVIENDO_MEDIO_BARRELROLL}
    }
}
