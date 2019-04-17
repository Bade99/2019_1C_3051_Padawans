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
        private float velocidadZ = 0;
        private readonly float aceleracion = 0.5f;
        private readonly float friccion = 0.2f;
        private readonly float maximaVelocidadZ = 20;
        private bool barrelRoll = false;
        private int barrelRollAvance = 0;

        public Xwing(TgcSceneLoader loader)
        {
            this.loader = loader;
            xwing = loader.loadSceneFromFile("Padawans_media\\XWing\\xwing-TgcScene.xml").Meshes[0];
            xwing.Scale = new TGCVector3(0.5f, 0.5f, 0.5f);
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

        public void UpdateInput(TgcD3dInput input)
        {
            //Movimientos W+A+S+D
            if (input.keyDown(Key.W))
            {
                xwing.Position = new TGCVector3(xwing.Position.X, xwing.Position.Y + 1, xwing.Position.Z);
            }
            if (input.keyDown(Key.S))
            {
                xwing.Position = new TGCVector3(xwing.Position.X, xwing.Position.Y - 1, xwing.Position.Z);
            }
            if (input.keyDown(Key.A))
            {
                xwing.Position = new TGCVector3(xwing.Position.X + 1, xwing.Position.Y, xwing.Position.Z);
            }
            if (input.keyDown(Key.D))
            {
                xwing.Position = new TGCVector3(xwing.Position.X - 1, xwing.Position.Y, xwing.Position.Z);
            }
            //Movimientos flechas
            if (input.keyDown(Key.LeftArrow))
            {
                xwing.RotateY(-0.1f);
            }
            if (input.keyDown(Key.RightArrow))
            {
                xwing.RotateY(0.1f);
            }
            if (input.keyDown(Key.UpArrow))
            {
                xwing.RotateZ(-0.1f);
            }
            if (input.keyDown(Key.DownArrow))
            {
                xwing.RotateZ(0.1f);
            }
            //Acelerar
            if (input.keyDown(Key.LeftShift) && velocidadZ < maximaVelocidadZ)
            {
                velocidadZ += aceleracion;
            }
            //Frenar
            if (input.keyDown(Key.LeftControl))
            {
                velocidadZ -= aceleracion;
            }
            //Permite que la nave se detenga totalmente
            if (velocidadZ > 0)
            {
                velocidadZ -= friccion;
            }
            else
            {
                velocidadZ = 0;
            }

            //BarrelRoll con barra espaciadora
            if (input.keyPressed(Key.Space))
            {
                barrelRoll = true;
            }
            if (barrelRoll)
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

            //Efecto de friccion
            xwing.Position = new TGCVector3(xwing.Position.X, xwing.Position.Y, xwing.Position.Z - velocidadZ);
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