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
        private TemporaryElementManager managerElementosTemporales;
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
            managerElementosTemporales = new TemporaryElementManager();
            xwing = new Xwing(loader,managerElementosTemporales);
            worldSphere = new WorldSphere(loader, xwing);
            followingCamera = new FollowingCamera(xwing);
            boundingBoxHelper = new BoundingBoxHelper(xwing, pistaReferencia, worldSphere);
        }
        public override void Update()
        {
            PreUpdate();
            followingCamera.Update(Camara,Input,ElapsedTime);
            worldSphere.Update();
            xwing.UpdateInput(Input,ElapsedTime);
            managerElementosTemporales.Update(ElapsedTime);
            boundingBoxHelper.UpdateInput(Input, ElapsedTime);
            Thread.Sleep(1);//@mientras mas chico el numero mas ganas en performance, tmb podemos sacar esto y listo
            PostUpdate();
        }
        public override void Render()
        {
            PreRender();

            DrawText.drawText($"Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("Con la ruedita aleja/acerca la camara [Actual]: " + TGCVector3.PrintVector3(Camara.Position), 0, 30, Color.OrangeRed);
            DrawText.drawText("Posicion Xwing: " + TGCVector3.PrintVector3(xwing.GetPosition()), 0, 40, Color.OrangeRed);
            DrawText.drawText("Velocidad Xwing: " + xwing.GetVelocidadGeneral(), 0, 50, Color.OrangeRed);
            DrawText.drawText("Polar: " + xwing.GetPolar(), 0, 60, Color.OrangeRed);
            DrawText.drawText("Acimutal: " + xwing.GetAcimutal(), 0, 70, Color.OrangeRed);
            DrawText.drawText("RotationZ: " + xwing.GetRotation().Z, 0, 80, Color.OrangeRed);
            DrawText.drawText("RotationY: " + xwing.GetRotation().Y, 0, 90, Color.OrangeRed);

            xwing.Render();
            pistaReferencia.Render();
            worldSphere.Render();
            managerElementosTemporales.Render();
            managerElementosTemporales.RenderBoundingBox();
            boundingBoxHelper.RenderBoundingBoxes();

            PostRender();
        }

        public override void Dispose()
        {
            xwing.Dispose();
            pistaReferencia.Dispose();
            worldSphere.Dispose();
            managerElementosTemporales.Dispose();
        }
    }
}