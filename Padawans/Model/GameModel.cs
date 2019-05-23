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
        private EnemyManager managerEnemigos;
        private SoundManager managerSonido;
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
            managerSonido = new SoundManager();
            
            VariablesGlobales.mediaDir = this.MediaDir;
            VariablesGlobales.loader = new TgcSceneLoader();
            VariablesGlobales.soundDevice = DirectSound.DsDevice;
            //VariablesGlobales.elapsedTime debe ser actualizado por tanto va a Update()
            VariablesGlobales.managerSonido = managerSonido;

            pistaReferencia = new MainRunway(loader, 5, this.Frustum);
            managerElementosTemporales = new TemporaryElementManager();
            xwing = new Xwing(loader,managerElementosTemporales);
            managerEnemigos = new EnemyManager();
            managerEnemigos.AgregarElemento(new XwingEnemigo(new TGCVector3(0f, 10f, -300f), xwing,managerElementosTemporales));
            worldSphere = new WorldSphere(loader, xwing);
            followingCamera = new FollowingCamera(xwing);
            boundingBoxHelper = new BoundingBoxHelper(new SceneElement[]{ xwing, pistaReferencia, worldSphere },new ActiveElementManager[] { managerElementosTemporales });
        }
        public override void Update()
        {
            PreUpdate();
            VariablesGlobales.elapsedTime = ElapsedTime;
            worldSphere.Update();
            xwing.UpdateInput(Input,ElapsedTime);
            xwing.Update();
            followingCamera.Update(Camara,Input,ElapsedTime);
            managerElementosTemporales.Update(ElapsedTime);
            managerEnemigos.Update(ElapsedTime);
            managerSonido.Update();
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
            DrawText.drawText("La nave dispara con click izquierdo ", 0, 60, Color.White);
            DrawText.drawText("Elementos temporales: " + managerElementosTemporales.CantidadElementos(), 0, 70, Color.White);
            DrawText.drawText("Sonidos: " + managerSonido.CantidadElementos(), 0, 80, Color.White);


            xwing.Render();
            pistaReferencia.Render();
            worldSphere.Render();
            managerElementosTemporales.Render();
            boundingBoxHelper.RenderBoundingBoxes();
            managerEnemigos.Render();

            //this.Frustum.render();

            PostRender();
        }

        public override void Dispose()
        {
            xwing.Dispose();
            pistaReferencia.Dispose();
            worldSphere.Dispose();
            managerElementosTemporales.Dispose();
            managerEnemigos.Dispose();
            managerSonido.Dispose();
        }
    }
}
