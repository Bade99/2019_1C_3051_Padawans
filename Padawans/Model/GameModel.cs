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
        private PostProcess postProcess;
        private TGCVector2 cues_relative_posicion;

        private float time = 5;

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
            managerElementosTemporales = new TemporaryElementManager();
            VariablesGlobales.managerElementosTemporales = managerElementosTemporales;
            pistaReferencia = new MainRunway(loader, 5, this.Frustum);

            xwing = new Xwing(loader, new TGCVector3(0, 1000f, 2000));

            VariablesGlobales.xwing = xwing;

            managerEnemigos = new EnemyManager();
            managerEnemigos.AgregarElemento(new XwingEnemigo(new TGCVector3(200f, 600f, 500f), xwing, 20));
            worldSphere = new WorldSphere(loader, xwing);
            followingCamera = new FollowingCamera(xwing);
            boundingBoxHelper = new BoundingBoxHelper(new SceneElement[]{ xwing, pistaReferencia, worldSphere },new ActiveElementManager[] { managerElementosTemporales });
            cues = new CueManager(new Cue(new DelayCueLauncher(3),"Bitmaps\\WASD.png", .3f, cues_relative_posicion,3),
                                  new Cue(new DelayCueLauncher(3),"Bitmaps\\Pause.png", .3f, cues_relative_posicion,3),
                                  new Cue(new DelayCueLauncher(3),"Bitmaps\\Left_Mouse.png",.3f,cues_relative_posicion,3)
                                  );
            managerSonido.ReproducirSonido(SoundManager.SONIDOS.BACKGROUND_BATTLE);
            managerMenu = new MenuManager(new StartMenu(Key.Return),new PauseMenu(Key.Escape));

            postProcess = new PostProcess();
            VariablesGlobales.postProcess = postProcess;
            postProcess.AgregarElemento(xwing);
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
                pistaReferencia.UpdateTime(ElapsedTime);
                boundingBoxHelper.UpdateInput(Input, ElapsedTime);
            }
            Thread.Sleep(1);//@mientras mas chico el numero mas ganas en performance, tmb podemos sacar esto y listo

            PostUpdate();
        }
        public void RenderizarMenus()
        {
            managerMenu.Render();//ahora mismo estamos haciendo doble render en el menu, dsps lo arreglo
        }
        public void RenderizarMeshes()
        {
            physicsEngine.Render(Input);
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
            //RenderizarMeshes();
            if (VariablesGlobales.POSTPROCESS)//mi idea era q el postprocess pueda obtener todo ya renderizado, pero de momento tengo q re-renderizar todo again antes de poder usarlo
            {
                //if (time < 0f)
                //{
                postProcess.RenderPostProcess("bloom");
                //}
                //else time -= VariablesGlobales.elapsedTime;
            }
            RenderizarMenus();
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
            TGCVector3 esf_coord = xwing.GetCoordenadaEsferica().GetXYZCoord();
            DrawText.drawText("xwing coord esf: " + esf_coord.X +" , "+ esf_coord.Y + " , " + esf_coord.Z, 0, 120, Color.White);
            DrawText.drawText( "xwing body orientacion: "+ xwing.body_xwing.Orientation.Axis.X + " , " + xwing.body_xwing.Orientation.Axis.Y +" , "+ xwing.body_xwing.Orientation.Axis.Z, 0, 130, Color.White);
            DrawText.drawText("xwing body linear vel: " + xwing.body_xwing.LinearVelocity.X + " , " + xwing.body_xwing.LinearVelocity.Y + " , " + xwing.body_xwing.LinearVelocity.Z, 0, 140, Color.White);
            DrawText.drawText("xwing body angular vel: " + xwing.body_xwing.AngularVelocity.X + " , " + xwing.body_xwing.AngularVelocity.Y + " , " + xwing.body_xwing.AngularVelocity.Z, 0, 150, Color.White);
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
