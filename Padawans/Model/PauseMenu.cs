using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.Example;

namespace TGC.Group.Model
{
    class PauseMenu : IMenu //la idea es que el menu de pausa sea semi opaco, con color negro, y se vea atras el juego, no se como hacerlo por ahora
    {
        private CustomSprite fondo;
        private CustomBitmap bitmap;
        private Drawer2D drawer2D;
        private bool isCurrent = false;
        private Microsoft.DirectX.DirectInput.Key mappedKey;
        private string path = "Sonidos\\main_menu.wav";

        //Shader
        private Surface depthStencil; // Depth-stencil buffer
        private Effect effect;
        private Surface pOldRT;
        private Surface pOldDS;
        private Texture renderTarget2D;
        private VertexBuffer screenQuadVB;
        //

        public PauseMenu(Microsoft.DirectX.DirectInput.Key mappedKey)
        {//recibe un key.algo para la key que abre y cierra el menu
            this.mappedKey = mappedKey;
            drawer2D = new Drawer2D();
            fondo = new CustomSprite();
            bitmap = new CustomBitmap(VariablesGlobales.mediaDir + "Bitmaps\\pause_menu.png", D3DDevice.Instance.Device);
            fondo.Bitmap = bitmap;
            //fondo.ScalingCenter = new TGCVector2( D3DDevice.Instance.Width / 2, D3DDevice.Instance.Height / 2);
            fondo.Scaling = new TGCVector2(1,.6f);
            var tamanio_textura = fondo.Bitmap.Size;
            fondo.Position = new TGCVector2(FastMath.Max(D3DDevice.Instance.Width / 2 - tamanio_textura.Width / 2, 0), FastMath.Max(D3DDevice.Instance.Height / 2 - tamanio_textura.Height / 2, 0));
            IniciarShader();
        }
        public void IniciarShader()
        {
            CustomVertex.PositionTextured[] screenQuadVertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            //vertex buffer de los triangulos
            screenQuadVB = new VertexBuffer(typeof(CustomVertex.PositionTextured), 4, D3DDevice.Instance.Device, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            screenQuadVB.SetData(screenQuadVertices, 0, LockFlags.None);

            //Creamos un Render Targer sobre el cual se va a dibujar la pantalla
            renderTarget2D = new Texture(D3DDevice.Instance.Device, D3DDevice.Instance.Device.PresentationParameters.BackBufferWidth,
                D3DDevice.Instance.Device.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            //Creamos un DepthStencil que debe ser compatible con nuestra definicion de renderTarget2D.
            depthStencil = D3DDevice.Instance.Device.CreateDepthStencilSurface(D3DDevice.Instance.Device.PresentationParameters.BackBufferWidth,
                    D3DDevice.Instance.Device.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);

            //Cargar shader con efectos de Post-Procesado
            effect = TGCShaders.Instance.LoadEffect(VariablesGlobales.shadersDir + "PostProcess.fx");

            //Configurar Technique dentro del shader
            effect.Technique = "OscurecerTechnique";

        }
        public bool CheckStartKey(TgcD3dInput input)
        {
            if (input.keyPressed(mappedKey)) {
                isCurrent = true;
                VariablesGlobales.managerSonido.PauseAll();
                //este agregar es el problema!!!!
                VariablesGlobales.managerSonido.AgregarElemento(new Sonido(path, 0, 0, -1, 0));
                return true;
            } 
            return false;
        }
        public void Update(TgcD3dInput input)
        {
            if (input.keyPressed(mappedKey))
            {
                isCurrent = false;
                VariablesGlobales.managerSonido.Remove(path);
                VariablesGlobales.managerSonido.ResumeAll();
            }
        }
        public void Render()
        {
            //Shader
            TexturesManager.Instance.clearAll();

            //Cargamos el Render Targer al cual se va a dibujar la escena 3D. Antes nos guardamos el surface original
            //En vez de dibujar a la pantalla, dibujamos a un buffer auxiliar, nuestro Render Target.
            pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);
            pOldDS = D3DDevice.Instance.Device.DepthStencilSurface;
            var pSurf = renderTarget2D.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, pSurf);
            D3DDevice.Instance.Device.DepthStencilSurface = depthStencil;
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Dibujamos la escena comun, pero en vez de a la pantalla al Render Target
            drawSceneToRenderTarget(D3DDevice.Instance.Device);

            //Liberar memoria de surface de Render Target
            pSurf.Dispose();
            //
            //Ahora volvemos a restaurar el Render Target original (osea dibujar a la pantalla)
            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT);
            D3DDevice.Instance.Device.DepthStencilSurface = pOldDS;

            //Luego tomamos lo dibujado antes y lo combinamos con una textura con efecto de alarma
            drawPostProcess(D3DDevice.Instance.Device);

        }
        public void Dispose()
        {
            fondo.Dispose();
            effect.Dispose();
            screenQuadVB.Dispose();
            renderTarget2D.Dispose();
            depthStencil.Dispose();
        }
        public bool IsCurrent()
        {
            return isCurrent;
        }


        /// <summary>
        ///     Dibujamos toda la escena pero en vez de a la pantalla, la dibujamos al Render Target que se cargo antes.
        ///     Es como si dibujaramos a una textura auxiliar, que luego podemos utilizar.
        /// </summary>
        private void drawSceneToRenderTarget(Device d3dDevice)
        {
            //Arrancamos el renderizado. Esto lo tenemos que hacer nosotros a mano porque estamos en modo CustomRenderEnabled = true
            //@d3dDevice.BeginScene();

            //Dibujamos todos los meshes del escenario
            VariablesGlobales.gameModel.RenderizarTodo();

            //Terminamos manualmente el renderizado de esta escena. Esto manda todo a dibujar al GPU al Render Target que cargamos antes
            //@d3dDevice.EndScene();
        }

        /// <summary>
        ///     Se toma todo lo dibujado antes, que se guardo en una textura, y se le aplica un shader para distorsionar la imagen
        /// </summary>
        private void drawPostProcess(Device d3dDevice)
        {
            //Arrancamos la escena
            //@d3dDevice.BeginScene();

            //Cargamos para renderizar el unico modelo que tenemos, un Quad que ocupa toda la pantalla, con la textura de todo lo dibujado antes
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);

            //Cargamos parametros en el shader de Post-Procesado
            effect.SetValue("render_target2D", renderTarget2D);
            effect.SetValue("scaleFactor", .5f);

            //Limiamos la pantalla y ejecutamos el render del shader
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            //Terminamos el renderizado de la escena
            drawer2D.BeginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            drawer2D.DrawSprite(fondo);

            //Finalizar el dibujado de Sprites
            drawer2D.EndDrawSprite();

            //@d3dDevice.EndScene();
            //@d3dDevice.Present();
        }
    }
}
