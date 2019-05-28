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
        private MenuManager managerMenu;
        private CueManager cues;
        private PhysicsEngine physicsEngine;

        private TGCVector2 cues_relative_posicion;

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
            cues_relative_posicion = new TGCVector2(.05f, .5f);
            /*
            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(D3DDevice.Instance.FieldOfView, D3DDevice.Instance.AspectRatio,
                    D3DDevice.Instance.ZNearPlaneDistance, D3DDevice.Instance.ZFarPlaneDistance).ToMatrix();
            */
            physicsEngine = new PhysicsEngine();
            VariablesGlobales.physicsEngine = physicsEngine;

            managerSonido = new SoundManager();
            VariablesGlobales.gameModel = this;
            VariablesGlobales.mediaDir = this.MediaDir;
            VariablesGlobales.shadersDir = this.ShadersDir;
            VariablesGlobales.loader = new TgcSceneLoader();
            VariablesGlobales.soundDevice = DirectSound.DsDevice;
            //VariablesGlobales.elapsedTime debe ser actualizado por tanto va a Update()
            VariablesGlobales.managerSonido = managerSonido;
            pistaReferencia = new MainRunway(loader, 5, this.Frustum);

            managerElementosTemporales = new TemporaryElementManager();
            xwing = new Xwing(loader,managerElementosTemporales, new TGCVector3(0, 1000f, 2000));
            managerEnemigos = new EnemyManager();
            managerEnemigos.AgregarElemento(new XwingEnemigo(new TGCVector3(0f, 600f, -3000f), xwing,managerElementosTemporales));
            worldSphere = new WorldSphere(loader, xwing);
            followingCamera = new FollowingCamera(xwing);
            boundingBoxHelper = new BoundingBoxHelper(new SceneElement[]{ xwing, pistaReferencia, worldSphere },new ActiveElementManager[] { managerElementosTemporales });
            cues = new CueManager(new Cue(new DelayCueLauncher(3),"Bitmaps\\WASD.png", .3f, cues_relative_posicion,3, "Sonidos\\obi_wan_luke.wav",.5f,0),
                                  new Cue(new DelayCueLauncher(3),"Bitmaps\\Pause.png", .3f, cues_relative_posicion,3, "Sonidos\\obi_wan_luke.wav",.5f,0),
                                  new Cue(new DelayCueLauncher(3),"Bitmaps\\Left_Mouse.png",.3f,cues_relative_posicion,3, "Sonidos\\obi_wan_luke.wav",.5f,0)
                                  );
            managerSonido.AgregarElemento(new Sonido("Sonidos\\Background_space_battle_10min.wav",-1800,0,-1,0));//sonido batalla de fondo
            managerMenu = new MenuManager(new StartMenu(Key.Return),new PauseMenu(Key.Escape));
        }
        public override void Update()
        {
            //seguir menu inicio
            PreUpdate();
            managerMenu.Update(Input);
            managerSonido.Update();
            if (!managerMenu.IsCurrent()) { //si no estoy en un menu ->
                VariablesGlobales.elapsedTime = ElapsedTime;
                physicsEngine.Update();
                cues.Update();
                worldSphere.Update();
                xwing.UpdateInput(Input,ElapsedTime);
                xwing.Update();
                followingCamera.Update(Camara,Input,ElapsedTime);
                managerElementosTemporales.Update(ElapsedTime);
                managerEnemigos.Update(ElapsedTime);
                boundingBoxHelper.UpdateInput(Input, ElapsedTime);
            }
            Thread.Sleep(1);//@mientras mas chico el numero mas ganas en performance, tmb podemos sacar esto y listo

            PostUpdate();
        }
        public void RenderizarTodo()
        {
            xwing.Render();
            pistaReferencia.Render();
            worldSphere.Render();
            managerElementosTemporales.Render();
            boundingBoxHelper.RenderBoundingBoxes();
            managerEnemigos.Render();
            cues.Render();
        }

        public override void Render()
        {
            PreRender();

            if (managerMenu.IsCurrent()) managerMenu.Render();
            else
            {
                RenderizarTodo();
            }

            //this.Frustum.render();

            CustomPostRender();
        }
        public void CustomPostRender()
        {
            DrawText.drawText($"Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("Con la ruedita aleja/acerca la camara [Actual]: " + TGCVector3.PrintVector3(Camara.Position), 0, 30, Color.OrangeRed);
            DrawText.drawText("Posicion Xwing: " + TGCVector3.PrintVector3(xwing.GetPosition()), 0, 40, Color.OrangeRed);
            DrawText.drawText("Velocidad Xwing: " + xwing.GetVelocidadGeneral(), 0, 50, Color.OrangeRed);
            DrawText.drawText("La nave dispara con click izquierdo ", 0, 60, Color.White);
            DrawText.drawText("Elementos temporales: " + managerElementosTemporales.CantidadElementos(), 0, 70, Color.White);
            DrawText.drawText("Sonidos: " + managerSonido.CantidadElementos(), 0, 80, Color.White);
            DrawText.drawText("En un menu: " + managerMenu.IsCurrent(), 0, 90, Color.White);
            TGCVector3 pos_body = new TGCVector3( xwing.body_xwing.CenterOfMassPosition );
            DrawText.drawText("Pos body:"+"x="+pos_body.X + "y=" + pos_body.Y + "z=" + pos_body.Z, 0, 100, Color.White);
            DrawText.drawText("Cam distance: "+followingCamera.fixedDistanceCamera , 0, 110, Color.White);
            PostRender();
        }
        public override void Dispose()
        {
            managerMenu.Dispose();
            xwing.Dispose();
            pistaReferencia.Dispose();
            worldSphere.Dispose();
            managerElementosTemporales.Dispose();
            managerEnemigos.Dispose();
            managerSonido.Dispose();
            cues.Dispose();
            physicsEngine.Dispose();
        }


        /*solo para saber qué hacen
        protected virtual void PreRender()
        {
            BeginRenderScene();
            ClearTextures(); //TODO no se si falta algo mas previo.
        }

        protected virtual void PostRender()
        {
            RenderAxis();
            RenderFPS();
            EndRenderScene();
        }

        protected void BeginRenderScene()
        {
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, BackgroundColor, 1.0f, 0);
            BeginScene();
        }

        public void BeginScene()
        {
            D3DDevice.Instance.Device.BeginScene();
        }

        protected void EndRenderScene()
        {
            EndScene();
            D3DDevice.Instance.Device.Present();
        }

        private static void EndScene()
        {
            D3DDevice.Instance.Device.EndScene();
        }
        */
    }
}
