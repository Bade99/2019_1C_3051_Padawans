using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;


namespace TGC.Group.Model
{
    /// <summary>
    ///     La nave principal que utiliza el jugador
    /// </summary>
    public class Xwing : SceneElement, InteractiveElement
    {
        private TgcSceneLoader loader;
        TgcMesh xwing;
        private readonly float minimaVelocidad = 25f;
        private float velocidadGeneral;
        private readonly float velocidadEjes = 30f;
        private readonly float aceleracion = 80f;//@necesitamos saber el elapsed time para poder tener esto bien seteado, preguntar de donde lo sacamos
        private readonly float friccion = 10f;
        private readonly float maximaVelocidadZ = 300;
        private bool barrelRoll;
        private int barrelRollAvance;
        private bool rotationYAnimation;
        private float rotationYAnimacionAdvance;
        private readonly float progressUnityRotationAdvance = FastMath.PI / 60;
        private CoordenadaEsferica coordenadaEsferica;

        public Xwing(TgcSceneLoader loader)
        {
            this.loader = loader;
            xwing = loader.loadSceneFromFile("Padawans_media\\XWing\\xwing-TgcScene.xml").Meshes[0];
            xwing.Position = new TGCVector3(0,0,0);
            xwing.Scale = new TGCVector3(0.1f, 0.1f, 0.1f);
            xwing.RotateY(FastMath.PI_HALF);

            velocidadGeneral = minimaVelocidad;
            barrelRoll = false;
            ActualizarCoordenadaEsferica();

            //Rotation y animation
            rotationYAnimation = false;
            rotationYAnimacionAdvance = 0;
        }

        public override void Render()
        {
            xwing.Render();
        }

        public override void RenderBoundingBox()
        {
            xwing.BoundingBox.Render();
        }

        public override void Update()
        {

        }

        private void TestingInput(TgcD3dInput input)
        {
            //Movimientos Y+G+H+J para moverse rapido por el mapa
            if (input.keyDown(Key.Y) && !rotationYAnimation)
            {
                xwing.Position = new TGCVector3(xwing.Position.X, xwing.Position.Y + 10, xwing.Position.Z);
            }
            if (input.keyDown(Key.H))
            {
                xwing.Position = new TGCVector3(xwing.Position.X, xwing.Position.Y - 10, xwing.Position.Z);
            }
            if (input.keyDown(Key.G))
            {
                xwing.Position = new TGCVector3(xwing.Position.X + 10, xwing.Position.Y, xwing.Position.Z);
            }
            if (input.keyDown(Key.J))
            {
                xwing.Position = new TGCVector3(xwing.Position.X - 10, xwing.Position.Y, xwing.Position.Z);
            }
            if (input.keyDown(Key.V))//ir para adelante
            {
                velocidadGeneral += aceleracion;
            }
            if (input.keyDown(Key.C))//ir para atras
            {
                velocidadGeneral = minimaVelocidad;
                xwing.Position = new TGCVector3(xwing.Position.X, xwing.Position.Y, xwing.Position.Z + 10);
            }

        }


        public void UpdateInput(TgcD3dInput input,float ElapsedTime) //@@@no se está teniendo en cuenta el ElapsedTime!!
        {//@la nave deberia tener rotacion, en vez de movimiento de solo un eje, ver tgc examples
            //Movimientos W+A+S+D
            ElapsedTime = 0.01f; //Lo hardcodeo hasta que sepamos bien como hacer esto
            if (input.keyDown(Key.W))
            {
                xwing.Position = new TGCVector3(xwing.Position.X, xwing.Position.Y + velocidadEjes * ElapsedTime, xwing.Position.Z);
            }
            if (input.keyDown(Key.S))
            {
                xwing.Position = new TGCVector3(xwing.Position.X, xwing.Position.Y - velocidadEjes * ElapsedTime, xwing.Position.Z);
            }
            if (input.keyDown(Key.A))
            {
                xwing.Position = new TGCVector3(xwing.Position.X + velocidadEjes * ElapsedTime, xwing.Position.Y, xwing.Position.Z);
            }
            if (input.keyDown(Key.D))
            {
                xwing.Position = new TGCVector3(xwing.Position.X - velocidadEjes * ElapsedTime, xwing.Position.Y, xwing.Position.Z);
            }
            if (input.keyDown(Key.R))
            {
                xwing.RotateY(FastMath.PI);
            }

            //Teclas especiales para moverse rapido y mas facil por el mapa
            TestingInput(input);

            //Movimientos flechas
            if (input.keyDown(Key.LeftArrow))
            {
                xwing.RotateY(-1f*ElapsedTime);
                CommonHelper.ClampRotationY(xwing);
                ActualizarCoordenadaEsferica();
            }
            if (input.keyDown(Key.RightArrow))
            {
                xwing.RotateY(1f*ElapsedTime);
                CommonHelper.ClampRotationY(xwing);
                ActualizarCoordenadaEsferica();
            }
            if (input.keyDown(Key.UpArrow) && !rotationYAnimation)
            {
                xwing.RotateZ(1f*ElapsedTime);
                ActualizarCoordenadaEsferica();

            }
            if (input.keyDown(Key.DownArrow) && !rotationYAnimation)
            {
                xwing.RotateZ(-1f*ElapsedTime);
                ActualizarCoordenadaEsferica();
            }
            //Acelerar
            if (input.keyDown(Key.LeftShift) && velocidadGeneral < maximaVelocidadZ)
            {
                velocidadGeneral += (aceleracion*ElapsedTime);
            }
            //Frenar
            if (input.keyDown(Key.LeftControl))
            {
                velocidadGeneral -= (aceleracion*ElapsedTime / 2);
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

            //BarrelRoll con barra espaciadora
            if (input.keyPressed(Key.Space))
            {
                barrelRoll = true;
            }
            //Rotation y animation
            if (rotationYAnimation)
            {
                rotationYAnimacionAdvance += progressUnityRotationAdvance;
                xwing.RotateY(progressUnityRotationAdvance);
                if (rotationYAnimacionAdvance >= FastMath.PI)
                {
                    rotationYAnimacionAdvance = 0;
                    rotationYAnimation = false;
                }
                ActualizarCoordenadaEsferica();
            }
            if (barrelRoll)//@la nave debe girar para el lado que está yendo
            {
                var angulo = barrelRollAvance * FastMath.TWO_PI / 100;
                xwing.Position = new TGCVector3(xwing.Position.X + FastMath.Cos(angulo), xwing.Position.Y + FastMath.Sin(angulo), xwing.Position.Z);
                xwing.RotateX(-FastMath.TWO_PI / 100);
                barrelRollAvance++;
                if (barrelRollAvance >= 100)
                {
                    barrelRoll = false;
                    barrelRollAvance = 0;
                }
            }

            //Efecto de aceleracion
            xwing.Position = CalcularAvanceNave(xwing.Position, ElapsedTime);//@@todos los ejes deberian usar funciones similares, para evitar ese salto que aparenta dar la nave
        }

        private void ActualizarCoordenadaEsferica()
        {
            coordenadaEsferica = new CoordenadaEsferica(xwing.Rotation);
        }

        private TGCVector3 CalcularAvanceNave(TGCVector3 posicion, float ElapsedTime)
        {
            float x = posicion.X + velocidadGeneral * FastMath.Cos(coordenadaEsferica.acimutal) * FastMath.Sin(coordenadaEsferica.polar) * ElapsedTime;
            float y = posicion.Y + velocidadGeneral * FastMath.Cos(coordenadaEsferica.polar) * ElapsedTime;
            float z = posicion.Z + velocidadGeneral * FastMath.Sin(coordenadaEsferica.acimutal) * FastMath.Sin(coordenadaEsferica.polar) * ElapsedTime;
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

        public TGCVector3 GetPosition()
        {
            return xwing.Position;
        }
        public TGCVector3 GetRotation()
        {
            return xwing.Rotation;
        }
        public float GetPolar()
        {
            return coordenadaEsferica.polar;
        }
        public float GetAcimutal()
        {
            return coordenadaEsferica.acimutal;
        }
    }
}