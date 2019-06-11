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
using System.Collections.Generic;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample , IRenderizer
    {
        private Renderer renderer;
        private ShaderManager shaderManager;
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
        private PostProcess postProcess;
        private DynamicCueManager dynamicCueManager;
        private EndgameManager endGameManager;

        //public TGCBox caja;

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }
        
        public override void Init()
        {
            VariablesGlobales.mediaDir = this.MediaDir;
            VariablesGlobales.shadersDir = this.ShadersDir;
            VariablesGlobales.soundDevice = DirectSound.DsDevice;
            VariablesGlobales.loader = new TgcSceneLoader();

            //var d3dDevice = D3DDevice.Instance.Device;

            VariablesGlobales.cues_relative_position = new TGCVector2(.05f, .5f);
            VariablesGlobales.cues_relative_scale = .3f;

            //Shaders & Post-processing @@ ver como hacer pa q estos se carguen primero!!!!
            shaderManager = new ShaderManager();
            VariablesGlobales.shaderManager = shaderManager;//@@@@@TERMINAR: PONERSELO A TODOS LOS MESHES
            postProcess = new PostProcess(this,shaderManager);
            VariablesGlobales.postProcess = postProcess;
            renderer = new Renderer(this,postProcess);
            /*
            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(D3DDevice.Instance.FieldOfView, D3DDevice.Instance.AspectRatio,
                    D3DDevice.Instance.ZNearPlaneDistance, D3DDevice.Instance.ZFarPlaneDistance).ToMatrix();
            */
            physicsEngine = new PhysicsEngine();
            VariablesGlobales.physicsEngine = physicsEngine;

            managerSonido = new SoundManager();
            VariablesGlobales.managerSonido = managerSonido;

            managerMenu = new MenuManager(new StartMenu(Key.Return),new PauseMenu(Key.Escape));

            //VariablesGlobales.elapsedTime debe ser actualizado por tanto va a Update()

            managerElementosTemporales = new TemporaryElementManager();
            VariablesGlobales.managerElementosTemporales = managerElementosTemporales;

            xwing = new Xwing(VariablesGlobales.loader, new TGCVector3(0, 1000f, 1000));
            VariablesGlobales.xwing = xwing;

            managerEnemigos = new EnemyManager();
            VariablesGlobales.managerEnemigos = managerEnemigos;
            managerEnemigos.AgregarElemento(new XwingEnemigo(new TGCVector3(200f, 600f, 500f), xwing, 20));

            pistaReferencia = new MainRunway(VariablesGlobales.loader, 5, this.Frustum, xwing);

            worldSphere = new WorldSphere(VariablesGlobales.loader, xwing);
            followingCamera = new FollowingCamera(xwing);
            boundingBoxHelper = new BoundingBoxHelper(new SceneElement[]{ xwing, pistaReferencia, worldSphere },new ActiveElementManager[] { managerElementosTemporales });
            cues = new CueManager(new Cue(new DelayCueLauncher(3),"Bitmaps\\WASD.png", VariablesGlobales.cues_relative_scale, VariablesGlobales.cues_relative_position,3),
                                  new Cue(new DelayCueLauncher(3),"Bitmaps\\Pause.png", VariablesGlobales.cues_relative_scale, VariablesGlobales.cues_relative_position, 3),
                                  new Cue(new DelayCueLauncher(3),"Bitmaps\\Left_Mouse.png", VariablesGlobales.cues_relative_scale, VariablesGlobales.cues_relative_position, 3)
                                  );
            dynamicCueManager = new DynamicCueManager(new Cue(new PositionAABBCueLauncher(xwing,new TGCVector3(0,-30,-13200),new TGCVector3(100,100,100)),
                                                            "Bitmaps\\G_Bomb.png", VariablesGlobales.cues_relative_scale, VariablesGlobales.cues_relative_position, 5));
            /*
            caja = new TGCBox();
            caja.Position = new TGCVector3(0, -40, -13900);
            caja.Size = new TGCVector3(100, 100, 100);
            */
            postProcess.AgregarElemento(xwing);
            postProcess.AgregarElemento(managerElementosTemporales);

            endGameManager = new EndgameManager(new EndGameTrigger(new TGCVector3(0,-50,-13900),new TGCVector3(100,100,100)),
                                    new LostGameTrigger(xwing,new TGCVector3(0,-30,-14000)));
            VariablesGlobales.endgameManager = endGameManager;

            managerSonido.ReproducirSonido(SoundManager.SONIDOS.BACKGROUND_BATTLE);
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
                worldSphere.Update();
                xwing.UpdateInput(Input,ElapsedTime);
                xwing.Update();
                cues.Update();
                dynamicCueManager.Update();
                followingCamera.Update(Camara,Input,ElapsedTime);
                managerElementosTemporales.Update();
                managerEnemigos.Update();
                boundingBoxHelper.UpdateInput(Input, ElapsedTime);
                endGameManager.Update();
            }
            //Thread.Sleep(1);//@mientras mas chico el numero mas ganas en performance, tmb podemos sacar esto y listo

            PostUpdate();
        }
        public void RenderizarMenus()
        {
            managerMenu.Render();//ahora mismo estamos haciendo doble render en el menu, dsps lo arreglo
        }
        public void RenderizarMeshes()
        {
            physicsEngine.Render(Input);

            worldSphere.Render();
            xwing.Render();
            pistaReferencia.Render();
            managerElementosTemporales.Render();
            managerEnemigos.Render();
        }

        public void RenderizarExtras()//renderizar estas cosas luego de los shaders@@@@
        {
            cues.Render();
            dynamicCueManager.Render();
            //caja.BoundingBox.Render();
            boundingBoxHelper.RenderBoundingBoxes();
        }

        public void RenderizarFinJuego()//va a necesitar al menos un shader de oscurecer
        {
            endGameManager.Render();
        }

        public override void Render()
        {
            renderer.Render();
        }
        public void CustomPreRender()
        {
            //Por ahora no le encontré un uso
        }
        public void NormalPreRender()
        {
            PreRender();
        }        
        public void VariablesEnPantalla()
        {
            DrawText.drawText($"Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("Con la ruedita aleja/acerca la camara [Actual]: " + TGCVector3.PrintVector3(Camara.Position), 0, 30, Color.OrangeRed);
            DrawText.drawText("Posicion Xwing: " + TGCVector3.PrintVector3(xwing.GetPosition()), 0, 40, Color.OrangeRed);
            DrawText.drawText("Velocidad Xwing: " + xwing.GetVelocidadGeneral(), 0, 50, Color.OrangeRed);
            DrawText.drawText("La nave dispara con click izquierdo ", 0, 60, Color.White);
            DrawText.drawText("Elementos temporales: " + managerElementosTemporales.CantidadElementos(), 0, 70, Color.White);
            DrawText.drawText("Sonidos: " + managerSonido.CantidadElementos(), 0, 80, Color.White);
            DrawText.drawText("En un menu: " + managerMenu.IsCurrent(), 0, 90, Color.White);
            TGCVector3 pos_body = new TGCVector3(xwing.body_xwing.CenterOfMassPosition);
            DrawText.drawText("Pos body:" + "x=" + pos_body.X + "y=" + pos_body.Y + "z=" + pos_body.Z, 0, 100, Color.White);
            DrawText.drawText("Cam distance: " + followingCamera.fixedDistanceCamera, 0, 110, Color.White);
            TGCVector3 esf_coord = xwing.GetCoordenadaEsferica().GetXYZCoord();
            DrawText.drawText("xwing coord esf: " + esf_coord.X + " , " + esf_coord.Y + " , " + esf_coord.Z, 0, 120, Color.White);
            DrawText.drawText("xwing body orientacion: " + xwing.body_xwing.Orientation.Axis.X + " , " + xwing.body_xwing.Orientation.Axis.Y + " , " + xwing.body_xwing.Orientation.Axis.Z, 0, 130, Color.White);
            DrawText.drawText("xwing body linear vel: " + xwing.body_xwing.LinearVelocity.X + " , " + xwing.body_xwing.LinearVelocity.Y + " , " + xwing.body_xwing.LinearVelocity.Z, 0, 140, Color.White);
            DrawText.drawText("xwing body angular vel: " + xwing.body_xwing.AngularVelocity.X + " , " + xwing.body_xwing.AngularVelocity.Y + " , " + xwing.body_xwing.AngularVelocity.Z, 0, 150, Color.White);
        }
        public void NormalPostRender()
        {
            PostRender();
        }
        public void CustomPostRender()
        {
            VariablesEnPantalla();
            RenderAxis();
            RenderFPS();
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
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, BackgroundColor, 1.0f, 0);
            D3DDevice.Instance.Device.BeginScene();
            ClearTextures();
        }

        protected virtual void PostRender()
        {
            RenderAxis();
            RenderFPS();
            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
        }

        */
    }
}
