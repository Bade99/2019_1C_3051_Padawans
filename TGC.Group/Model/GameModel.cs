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
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        //Mesh de TgcLogo.
        private TgcMesh Xwing;

        private TgcScene scene;

        private bool barrelRoll = false;
        private int barrelRollAvance = 0;

        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;


            //Cargo el unico mesh que tiene la escena.
            Xwing = new TgcSceneLoader().loadSceneFromFile(MediaDir + "XWing\\xwing-TgcScene.xml").Meshes[0];
            //Defino una escala en el modelo logico del mesh que es muy grande.
            Xwing.Scale = new TGCVector3(0.5f, 0.5f, 0.5f);
            Xwing.RotateY(FastMath.PI_HALF);

            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "XWing\\TRENCH_RUN-TgcScene.xml");

            //Suelen utilizarse objetos que manejan el comportamiento de la camara.
            //Lo que en realidad necesitamos gráficamente es una matriz de View.
            //El framework maneja una cámara estática, pero debe ser inicializada.

            //Posición de la camara.
            var cameraPosition = new TGCVector3(0, 100, 125);
            //Quiero que la camara mire hacia el origen (0,0,0).
            var lookAt = TGCVector3.Empty;


            //Configuro donde esta la posicion de la camara y hacia donde mira.
            Camara.SetCamera(cameraPosition, lookAt);
            //Internamente el framework construye la matriz de view con estos dos vectores.
            //Luego en nuestro juego tendremos que crear una cámara que cambie la matriz de view con variables como movimientos o animaciones de escenas.
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            //F para mostrar bounding box
            if (Input.keyPressed(Key.F))
            {
                BoundingBox = !BoundingBox;
            }

            //Movimientos W+A+S+D
            if (Input.keyDown(Key.W))
            {
                Xwing.Position = new TGCVector3(Xwing.Position.X, Xwing.Position.Y, Xwing.Position.Z - 1);
            }
            if (Input.keyDown(Key.S))
            {
                Xwing.Position = new TGCVector3(Xwing.Position.X, Xwing.Position.Y, Xwing.Position.Z + 1);
            }
            if (Input.keyDown(Key.A))
            {
                Xwing.Position = new TGCVector3(Xwing.Position.X + 1, Xwing.Position.Y, Xwing.Position.Z);
            }
            if (Input.keyDown(Key.D))
            {
                Xwing.Position = new TGCVector3(Xwing.Position.X - 1, Xwing.Position.Y, Xwing.Position.Z);
            }

            //BarrelRoll con barra espaciadora
            if (Input.keyPressed(Key.Space))
            {
                barrelRoll = true;
            }
            if (barrelRoll)
            {
                var angulo = barrelRollAvance * FastMath.TWO_PI / 100;
                Xwing.Position = new TGCVector3(Xwing.Position.X + FastMath.Cos(angulo), Xwing.Position.Y + FastMath.Sin(angulo), Xwing.Position.Z);
                Xwing.RotateX(-FastMath.TWO_PI / 100);
                barrelRollAvance++;
                if (barrelRollAvance >= 100)
                {
                    barrelRoll = false;
                    barrelRollAvance = 0;
                }
            }

            //Ruedita para alejar/acercar camara
            if (Input.WheelPos == -1)
            {
                Camara.SetCamera(Camara.Position + new TGCVector3(0, 2, 2), Camara.LookAt);
            }
            if (Input.WheelPos == 1)
            {
                Camara.SetCamera(Camara.Position + new TGCVector3(0, -2, -2), Camara.LookAt);
            }

            //Sleep para que no vaya tan rapido, ajustarlo segun gusto
            Thread.Sleep(10);

            PostUpdate();
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            //Dibuja un texto por pantalla
            DrawText.drawText($"Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("Con la ruedita aleja/acerca la camara [Actual]: " + TGCVector3.PrintVector3(Camara.Position), 0, 30, Color.OrangeRed);
            DrawText.drawText("Posicion Xwing: " + TGCVector3.PrintVector3(Xwing.Position), 0, 40, Color.OrangeRed);


            //Cuando tenemos modelos mesh podemos utilizar un método que hace la matriz de transformación estándar.
            //Es útil cuando tenemos transformaciones simples, pero OJO cuando tenemos transformaciones jerárquicas o complicadas.
            Xwing.UpdateMeshTransform();
            //Render del mesh
            Xwing.Render();
            //scene.RenderAll();

            //Render de BoundingBox, muy útil para debug de colisiones.
            if (BoundingBox)
            {
                Xwing.BoundingBox.Render();
            }

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            //Dispose del mesh.
            Xwing.Dispose();
        }
    }
}