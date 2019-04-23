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
        private float anguloAcimutal;
        private float anguloPolar;

        public Xwing(TgcSceneLoader loader)
        {
            this.loader = loader;
            xwing = loader.loadSceneFromFile("Padawans_media\\XWing\\xwing-TgcScene.xml").Meshes[0];
            xwing.Position = new TGCVector3(0,0,0);
            xwing.Scale = new TGCVector3(0.1f, 0.1f, 0.1f);
            xwing.RotateY(FastMath.PI_HALF);

            velocidadGeneral = minimaVelocidad;
            barrelRoll = false;
            anguloAcimutal = GetAnguloAcimutal(xwing.Rotation.Y);
            anguloPolar = GetAnguloPolar(xwing.Rotation.Z);
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

            //Teclas especiales para moverse rapido y mas facil por el mapa
            TestingInput(input);

            //Movimientos flechas
            if (input.keyDown(Key.LeftArrow))
            {
                xwing.RotateY(-1f*ElapsedTime);
                anguloAcimutal = GetAnguloAcimutal(xwing.Rotation.Y);
            }
            if (input.keyDown(Key.RightArrow))
            {
                xwing.RotateY(1f*ElapsedTime);
                anguloAcimutal = GetAnguloAcimutal(xwing.Rotation.Y);
            }
            if (input.keyDown(Key.UpArrow))
            {
                xwing.RotateZ(1f*ElapsedTime);
                anguloPolar = GetAnguloPolar(xwing.Rotation.Z);
            }
            if (input.keyDown(Key.DownArrow))
            {
                xwing.RotateZ(-1f*ElapsedTime);
                anguloPolar = GetAnguloPolar(xwing.Rotation.Z);
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

        private float GetAnguloPolar(float angulo)
        {
            anguloPolar = (-xwing.Rotation.Z + FastMath.PI / 2) % (2 * FastMath.PI);
            if (anguloPolar > FastMath.PI)
            {
                anguloPolar = (FastMath.PI * 2) - anguloPolar;
            }
            return anguloPolar;
        }

        private float GetAnguloAcimutal(float angulo)
        {
            return (angulo + (FastMath.PI)) % (FastMath.PI * 2);
        }

        private TGCVector3 CalcularAvanceNave(TGCVector3 posicion, float ElapsedTime)
        {
            float x = posicion.X - velocidadGeneral * FastMath.Cos(anguloAcimutal) * FastMath.Sin(anguloPolar) * ElapsedTime;
            float y = posicion.Y + velocidadGeneral * FastMath.Cos(anguloPolar) * ElapsedTime;
            float z = posicion.Z + velocidadGeneral * FastMath.Sin(anguloAcimutal) * FastMath.Sin(anguloPolar) * ElapsedTime;
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
        public float GetPolar()
        {
            return anguloPolar;
        }
        public float GetAcimutal()
        {
            return anguloAcimutal;
        }
    }
}