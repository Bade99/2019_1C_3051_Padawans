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
using TGC.Group.Form;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample , IRenderizer
    {
        private GameForm gameForm;
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
        private Hole hole;
        private HUD hud;
        private BackgroundScene backgroundSceneLeft, backgroundSceneRight, backgroundSceneFront;

        string iddqd="";

        public GameModel(string mediaDir, string shadersDir,GameForm gameForm) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
            this.gameForm = gameForm;
        }
        
        public override void Init()
        {
            VariablesGlobales.vidas = 8;
            VariablesGlobales.mediaDir = this.MediaDir;
            VariablesGlobales.shadersDir = this.ShadersDir;
            VariablesGlobales.soundDevice = DirectSound.DsDevice;
            VariablesGlobales.loader = new TgcSceneLoader();

            hud = new HUD();
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

            managerSonido = SoundManager.GetInstance();
            VariablesGlobales.managerSonido = managerSonido;


            //VariablesGlobales.elapsedTime debe ser actualizado por tanto va a Update()

            managerElementosTemporales = new TemporaryElementManager();
            VariablesGlobales.managerElementosTemporales = managerElementosTemporales;

            xwing = new Xwing(VariablesGlobales.loader, new TGCVector3(0, 500, 1000));
            VariablesGlobales.xwing = xwing;

            VariablesGlobales.miniMap.agregarTarget(xwing);

            managerEnemigos = new EnemyManager();
            VariablesGlobales.managerEnemigos = managerEnemigos;
            ColocarXwingEnemigos();
            pistaReferencia = new MainRunway(VariablesGlobales.loader, 5, this.Frustum, xwing);

            worldSphere = new WorldSphere(VariablesGlobales.loader, xwing);
            followingCamera = new FollowingCamera(xwing);

            VariablesGlobales.camara = followingCamera;

            boundingBoxHelper = new BoundingBoxHelper(new SceneElement[]{ xwing, pistaReferencia, worldSphere },new ActiveElementManager[] { managerElementosTemporales });
            cues = new CueManager(new Cue(new DelayCueLauncher(3),"Bitmaps\\WASD.png", VariablesGlobales.cues_relative_scale, VariablesGlobales.cues_relative_position,3),
                                  new Cue(new DelayCueLauncher(3),"Bitmaps\\Pause.png", VariablesGlobales.cues_relative_scale, VariablesGlobales.cues_relative_position, 3),
                                  new Cue(new DelayCueLauncher(3),"Bitmaps\\Left_Mouse.png", VariablesGlobales.cues_relative_scale, VariablesGlobales.cues_relative_position, 3)
                                  );
            dynamicCueManager = new DynamicCueManager(new Cue(new PositionAABBCueLauncher(xwing,new TGCVector3(0,-30,-13200),new TGCVector3(100,100,100)),
                                                            "Bitmaps\\G_Bomb.png", VariablesGlobales.cues_relative_scale, VariablesGlobales.cues_relative_position, 5));
            postProcess.AgregarElemento(xwing);
            postProcess.AgregarElemento(managerElementosTemporales);
            hole = new Hole(new TGCVector3(0, -40, -13875));
            postProcess.AgregarElemento(hole);
            VariablesGlobales.miniMap.agregarObjetoMeta(hole);

            endGameManager = new EndgameManager(this,new EndGameTrigger(new TGCVector3(0,-50,-13900),new TGCVector3(100,100,100)),
                                    new LostGameTrigger(xwing,new TGCVector3(0,-30,-14000)));
            VariablesGlobales.endgameManager = endGameManager;

            managerMenu = new MenuManager(new StartMenu(Key.Return), new PauseMenu(Key.Escape));//tiene q ir ultimo pa parar el resto de sonidos
            VariablesGlobales.managerMenu = managerMenu;

            backgroundSceneLeft = new BackgroundScene(new TGCVector3(500, 100, 1000), new TGCVector3(200, 200, -15000), 100);
            backgroundSceneRight = new BackgroundScene(new TGCVector3(-600, 100, 1000), new TGCVector3(200, 200, -15000), 100);
            backgroundSceneFront = new BackgroundScene(new TGCVector3(500, 100, -15000), new TGCVector3(-1000, 200, -200), 100);
        }
        private void ColocarXwingEnemigos()
        {
            //Inicial mirando hacia adelante cerca de la entrada
            XwingEnemigo enemigoInicial1 = new XwingEnemigo(
                new TGCVector3(200f, 100, 500f),
                xwing, 80, new CoordenadaEsferica(FastMath.PI_HALF, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigoInicial1);
            VariablesGlobales.miniMap.agregarObjeto(enemigoInicial1);
            //Segundo mirando para atras un poco mas adentro
            XwingEnemigo enemigoInicial2 = new XwingEnemigo(
                new TGCVector3(0, 50, -200f),
                xwing, 80, new CoordenadaEsferica(FastMath.PI_HALF * 3, FastMath.PI_HALF*0.8f));
            managerEnemigos.AgregarElemento(enemigoInicial2);
            VariablesGlobales.miniMap.agregarObjeto(enemigoInicial2);
            //Dos esperandote mas atras
            XwingEnemigo enemigo3 = new XwingEnemigo(
                new TGCVector3(400f, 100, -2300f), 
                xwing, 100, new CoordenadaEsferica(3 * FastMath.QUARTER_PI, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigo3);
            VariablesGlobales.miniMap.agregarObjeto(enemigo3);
            XwingEnemigo enemigo4 = new XwingEnemigo(
                new TGCVector3(-400f, 100, -3785),
                xwing, 100, new CoordenadaEsferica(FastMath.QUARTER_PI, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigo4);
            VariablesGlobales.miniMap.agregarObjeto(enemigo4);
            //Cuatro en linea de frente
            XwingEnemigo enemigoLinea1 = new XwingEnemigo(
                new TGCVector3(-200, 130, -4354),
                xwing, 150, new CoordenadaEsferica(FastMath.PI_HALF, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigoLinea1);
            VariablesGlobales.miniMap.agregarObjeto(enemigoLinea1);
            XwingEnemigo enemigoLinea2 = new XwingEnemigo(
                new TGCVector3(-50, 80, -5783),
                xwing, 150, new CoordenadaEsferica(FastMath.PI_HALF, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigoLinea2);
            VariablesGlobales.miniMap.agregarObjeto(enemigoLinea2);
            XwingEnemigo enemigoLinea3 = new XwingEnemigo(
                new TGCVector3(-50, 80, -6432),
                xwing, 150, new CoordenadaEsferica(FastMath.PI_HALF, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigoLinea3);
            VariablesGlobales.miniMap.agregarObjeto(enemigoLinea3);
            XwingEnemigo enemigoLinea4 = new XwingEnemigo(
                new TGCVector3(-200, 130, -7655),
                xwing, 150, new CoordenadaEsferica(FastMath.PI_HALF, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigoLinea4);
            VariablesGlobales.miniMap.agregarObjeto(enemigoLinea4);
            //Los seis grosos
            XwingEnemigo enemigoGroso1 = new XwingEnemigo(
                new TGCVector3(-50, 100, -8845),
                xwing, 210, new CoordenadaEsferica(FastMath.QUARTER_PI, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigoGroso1);
            VariablesGlobales.miniMap.agregarObjeto(enemigoGroso1);
            XwingEnemigo enemigoGroso2 = new XwingEnemigo(
                new TGCVector3(50, 100, -9312),
                xwing, 210, new CoordenadaEsferica(2 * FastMath.QUARTER_PI, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigoGroso2);
            VariablesGlobales.miniMap.agregarObjeto(enemigoGroso2);
            XwingEnemigo enemigoGroso3 = new XwingEnemigo(
                new TGCVector3(-100, 120, -10564),
                xwing, 210, new CoordenadaEsferica(3 * FastMath.QUARTER_PI, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigoGroso3);
            VariablesGlobales.miniMap.agregarObjeto(enemigoGroso3);
            XwingEnemigo enemigoGroso4 = new XwingEnemigo(
                new TGCVector3(100, 120, -11245),
                xwing, 210, new CoordenadaEsferica(5 * FastMath.QUARTER_PI, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigoGroso4);
            VariablesGlobales.miniMap.agregarObjeto(enemigoGroso4);
            XwingEnemigo enemigoGroso5 = new XwingEnemigo(
                new TGCVector3(-50, 80, -12486),
                xwing, 210, new CoordenadaEsferica(6 * FastMath.QUARTER_PI, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigoGroso5);
            VariablesGlobales.miniMap.agregarObjeto(enemigoGroso5);
            XwingEnemigo enemigoGroso6 = new XwingEnemigo(
                new TGCVector3(50, 80, -13651),
                xwing, 210, new CoordenadaEsferica(7 * FastMath.QUARTER_PI, FastMath.PI_HALF));
            managerEnemigos.AgregarElemento(enemigoGroso6);
            VariablesGlobales.miniMap.agregarObjeto(enemigoGroso6);
        }


        public override void Update()
        {
            VariablesGlobales.elapsedTime = ElapsedTime;
            PreUpdate();
            GodMode();
            MiniMap();
            DebugMode();
            Mute();
            managerMenu.Update(Input);
            managerSonido.Update();
            if (!managerMenu.IsCurrent()) { //si no estoy en un menu ->
                contarTiempo();
                worldSphere.Update();
                xwing.UpdateInput(Input,ElapsedTime);
                xwing.Update();
                cues.Update();
                hud.UpdateMiniMap();
                dynamicCueManager.Update();
                followingCamera.Update(Camara,Input,ElapsedTime);
                managerElementosTemporales.Update();
                managerEnemigos.Update();
                boundingBoxHelper.UpdateInput(Input, ElapsedTime);
                endGameManager.Update();
                physicsEngine.Update();
                backgroundSceneLeft.Update();
                backgroundSceneRight.Update();
                backgroundSceneFront.Update();
                VariablesGlobales.Shader_DEAD_time+=VariablesGlobales.elapsedTime;

            }

            PostUpdate();
        }
        public void RenderizarMenus()
        {
            managerMenu.Render();//ahora mismo estamos haciendo doble render en el menu, dsps lo arreglo
        }

        private void contarTiempo()
        {
            if (!VariablesGlobales.endgameManager.Fin())
            {
                VariablesGlobales.tiempoTotal += ElapsedTime;
            }
        }

        public void RenderizarExtras()//renderizar estas cosas luego de los shaders
        {
            physicsEngine.Render(Input);
            hud.Render();
            cues.Render();
            dynamicCueManager.Render();
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
            //Por ahora no le encontr� un uso
        }
        public void NormalPreRender()
        {
            PreRender();
        }
        public void VariablesEnPantalla()
        {
            DrawText.drawText("Tiempo Total: " + VariablesGlobales.tiempoTotal, 0, 20, Color.Yellow);

            /*
            DrawText.drawText("Con la ruedita aleja/acerca la camara [Actual]: " + TGCVector3.PrintVector3(Camara.Position), 0, 10, Color.White);
            DrawText.drawText("GodMode (IDDQD): " + iddqd + " : " + VariablesGlobales.MODO_DIOS, 0, 20, Color.White);

            DrawText.drawText($"Con la tecla F se dibuja el bounding box.", 0, 30, Color.OrangeRed);
            DrawText.drawText("Posicion Xwing: " + TGCVector3.PrintVector3(xwing.GetPosition()), 0, 40, Color.OrangeRed);
            DrawText.drawText("Velocidad Xwing: " + xwing.GetVelocidadGeneral(), 0, 50, Color.OrangeRed);
            DrawText.drawText("La nave dispara con click izquierdo ", 0, 60, Color.White);
            DrawText.drawText("Elementos temporales: " + managerElementosTemporales.CantidadElementos(), 0, 70, Color.White);
            DrawText.drawText("Sonidos: " + managerSonido.CantidadElementos(), 0, 80, Color.White);
            DrawText.drawText("En un menu: " + managerMenu.IsCurrent(), 0, 90, Color.White);
            DrawText.drawText("Cam distance: " + followingCamera.fixedDistanceCamera, 0, 100, Color.White);
            DrawText.drawText("xwing coord esf: " + TGCVector3.PrintVector3(xwing.GetCoordenadaEsferica().GetXYZCoord()), 0, 110, Color.White);
            DrawText.drawText("Vidas " + VariablesGlobales.vidas, 0, 120, Color.White);
            */
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

        public void GameEnded()
        {
            VariablesGlobales.tiempoTotal = 0;
            gameForm.RestartGame();
        }

        private void DebugMode()
        {
            if (Input.keyPressed(Key.U))
            {
                VariablesGlobales.debugMode = !VariablesGlobales.debugMode;
            }
        }
        private void MiniMap()
        {
            if (Input.keyPressed(Key.I))
            {
                VariablesGlobales.mostrarMiniMapa = !VariablesGlobales.mostrarMiniMapa;
            }
        }
        private void Mute()
        {
            if (Input.keyPressed(Key.M))
            {
                VariablesGlobales.managerSonido.MuteOrUnMute();
            }
        }
        private void GodMode()
        {
            if (Input.keyPressed(Key.I)) iddqd = "i";
            else if (Input.keyPressed(Key.D)&&iddqd.Length>0)
            {
                switch (iddqd.Length)
                {
                    case 1:iddqd += 'd';
                        break;
                    case 2:if (iddqd[1] == 'd') iddqd += 'd'; else iddqd = "";
                        break;
                    case 4:
                        VariablesGlobales.MODO_DIOS = !VariablesGlobales.MODO_DIOS;
                        iddqd = "";
                        break;
                    default:iddqd = "";
                        break;
                }
            }
            else if (Input.keyPressed(Key.Q))
            {
                if (iddqd.Length == 3 && iddqd == "idd")
                    iddqd += 'q';
                else iddqd = "";
            }
        }

        public void RenderizarMeshes()
        {
            
        }

        /*solo para saber qu� hacen
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
