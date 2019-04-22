using Microsoft.DirectX.DirectInput;
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
    public class GameModel : TgcExample
    {
        private Xwing xwing;
        private MainRunway pistaReferencia;
        private WorldSphere worldSphere;
        private FollowingCamera followingCamera;
        private BoundingBoxHelper boundingBoxHelper;

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        public override void Init()
        {
            var d3dDevice = D3DDevice.Instance.Device;
            var loader = new TgcSceneLoader();
            pistaReferencia = new MainRunway(loader, 5);
            xwing = new Xwing(loader);
            worldSphere = new WorldSphere(loader, xwing);
            followingCamera = new FollowingCamera(xwing);
            boundingBoxHelper = new BoundingBoxHelper(xwing, pistaReferencia);
        }
        public override void Update()
        {
            PreUpdate();
            followingCamera.Update(Camara,Input);
            worldSphere.Update();
            xwing.UpdateInput(Input,ElapsedTime);
            boundingBoxHelper.UpdateInput(Input, ElapsedTime);

            ////Ruedita para alejar/acercar camara
            //if (Input.WheelPos == -1)
            //{
            //    Camara.SetCamera(Camara.Position + new TGCVector3(0, 2, 2), Camara.LookAt);
            //}
            //if (Input.WheelPos == 1)
            //{
            //    Camara.SetCamera(Camara.Position + new TGCVector3(0, -2, -2), Camara.LookAt);
            //}

            //Sleep para que no vaya tan rapido, ajustarlo segun gusto
            Thread.Sleep(1);//@mientras mas chico el numero mas ganas en performance, tmb podemos sacar esto y listo
            PostUpdate();
        }
        public override void Render()
        {
            PreRender();

            DrawText.drawText($"Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("Con la ruedita aleja/acerca la camara [Actual]: " + TGCVector3.PrintVector3(Camara.Position), 0, 30, Color.OrangeRed);
            DrawText.drawText("Posicion Xwing: " + TGCVector3.PrintVector3(xwing.GetPosition()), 0, 40, Color.OrangeRed);
            DrawText.drawText("Velocidad Xwing: " + xwing.GetVelocidadZ(), 0, 50, Color.OrangeRed);

            xwing.Render();
            pistaReferencia.Render();
            worldSphere.Render();
            boundingBoxHelper.RenderBoundingBoxes();

            PostRender();
        }

        public override void Dispose()
        {

            xwing.Dispose();
            pistaReferencia.Dispose();
            worldSphere.Dispose();
        }
    }
}