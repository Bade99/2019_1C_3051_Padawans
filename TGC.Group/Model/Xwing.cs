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
        private static float minimaVelocidadZ = 25f;
        private float velocidadZ = minimaVelocidadZ;
        private float velocidadEjes = 30f;
        private readonly float aceleracion = 80f;//@necesitamos saber el elapsed time para poder tener esto bien seteado, preguntar de donde lo sacamos
        private readonly float friccion = 10f;
        private readonly float maximaVelocidadZ = 300;
        private bool barrelRoll = false;
        private int barrelRollAvance = 0;

        public Xwing(TgcSceneLoader loader)
        {
            this.loader = loader;
            xwing = loader.loadSceneFromFile("Padawans_media\\XWing\\xwing-TgcScene.xml").Meshes[0];
            xwing.Position = new TGCVector3(0,0,0);
            xwing.Scale = new TGCVector3(0.1f, 0.1f, 0.1f);
            xwing.RotateY(FastMath.PI_HALF);
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
            if (input.keyDown(Key.Y))
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
                velocidadZ += aceleracion;
            }
            if (input.keyDown(Key.C))//ir para atras
            {
                velocidadZ = minimaVelocidadZ;
                xwing.Position = new TGCVector3(xwing.Position.X, xwing.Position.Y, xwing.Position.Z + 10);
            }

        }


        public void UpdateInput(TgcD3dInput input,float ElapsedTime) //@@@no se está teniendo en cuenta el ElapsedTime!!
        {//@la nave deberia tener rotacion, en vez de movimiento de solo un eje, ver tgc examples
            //Movimientos W+A+S+D
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

            //Teclas especiales para moverse rapido y mas facil por el mapa
            TestingInput(input);

            //Movimientos flechas
            if (input.keyDown(Key.LeftArrow))
            {
                xwing.RotateY(-1f*ElapsedTime);
            }
            if (input.keyDown(Key.RightArrow))
            {
                xwing.RotateY(1f*ElapsedTime);
            }
            if (input.keyDown(Key.UpArrow))
            {
                xwing.RotateZ(-1f*ElapsedTime);
            }
            if (input.keyDown(Key.DownArrow))
            {
                xwing.RotateZ(1f*ElapsedTime);
            }
            //Acelerar
            if (input.keyDown(Key.LeftShift) && velocidadZ < maximaVelocidadZ)
            {
                velocidadZ += (aceleracion*ElapsedTime);
            }
            //Frenar
            if (input.keyDown(Key.LeftControl))
            {
                velocidadZ -= (aceleracion*ElapsedTime / 2);
            }
            //Permite que la nave se detenga totalmente
            if (velocidadZ > minimaVelocidadZ)
            {
                velocidadZ -= (friccion*ElapsedTime);
            }
            else
            {
                velocidadZ = minimaVelocidadZ;
            }

            //BarrelRoll con barra espaciadora
            if (input.keyPressed(Key.Space))
            {
                barrelRoll = true;
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
            xwing.Position = new TGCVector3(xwing.Position.X, xwing.Position.Y, xwing.Position.Z - velocidadZ*ElapsedTime);//@@todos los ejes deberian usar funciones similares, para evitar ese salto que aparenta dar la nave
        }

        public override void Dispose()
        {
            xwing.Dispose();
        }

        public float GetVelocidadZ()
        {
            return velocidadZ;
        }

        public TGCVector3 GetPosition()
        {
            return xwing.Position;
        }
    }
}