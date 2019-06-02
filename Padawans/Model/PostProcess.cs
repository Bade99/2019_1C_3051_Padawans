using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Shaders;
using TGC.Core.Direct3D;
using TGC.Core.Textures;
using System.Drawing;

namespace TGC.Group.Model
{
    public class PostProcess
    //cuando comienza todo lo anterior a postProcess ya debe estar en pantalla, cuando termina deja el render abierto para otras cosas, osea NO llama a postRender()
    //@supongo q para cada efecto lo q haces es cortar el render del anterior, guardarlo en un temp, hacer el tuyo, combinarlos
    {
        private List<IPostProcess> PostProcessElements;
        Device d3dDevice;

        //General
        private Surface old_render_target;
        private Surface old_depth_stencil;
        private VertexBuffer screenQuadVB;
        private Texture base_render;
        private Surface base_depth_stencil;
        //

        //Oscurecer
        private Effect oscurecer;
        //

        //Bloom
        private Effect bloom;
        private Texture glow_mask;
        private Surface glow_depth_stencil;
        private Texture downscaling;
        private Texture gaussian_aux;
        //


        public PostProcess()
        {
            PostProcessElements = new List<IPostProcess>();

            d3dDevice = D3DDevice.Instance.Device;

            IniciarGenerales();

            //Oscurecer
            IniciarOscurecer(ref oscurecer);
            //

            //Bloom
            IniciarBloom(ref bloom);
            //
        }

        private void IniciarGenerales()
        {
            CustomVertex.PositionTextured[] screenQuadVertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            //vertex buffer de los triangulos
            screenQuadVB = new VertexBuffer(typeof(CustomVertex.PositionTextured), 4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            screenQuadVB.SetData(screenQuadVertices, 0, LockFlags.None);

            //Creamos un Render Targer sobre el cual se va a dibujar la pantalla
            base_render = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth,
                d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            //Creamos un base_depth_stencil que debe ser compatible con nuestra definicion de base_render.
            base_depth_stencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                    d3dDevice.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);
        }

        private void IniciarOscurecer(ref Effect oscurecer)
        {

            //Cargar shader con efectos de Post-Procesado
            oscurecer = TGCShaders.Instance.LoadEffect(VariablesGlobales.shadersDir + "PostProcess.fx");

            //Configurar Technique dentro del shader
            oscurecer.Technique = "OscurecerTechnique";
        }

        private void IniciarBloom(ref Effect bloom)
        {
            bloom = TGCShaders.Instance.LoadEffect(VariablesGlobales.shadersDir + "GaussianBlur.fx");//de momento voy a usar el blur comun ese
            //bloom.Technique = "BlurTechnique";

            glow_mask = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth,
                    d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            glow_depth_stencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                        d3dDevice.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);

            downscaling = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth / 4,
                d3dDevice.PresentationParameters.BackBufferHeight / 4, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);

            gaussian_aux = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth / 4,
                d3dDevice.PresentationParameters.BackBufferHeight / 4, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);

        }

        public void AgregarElemento(IPostProcess elem)
        {
            PostProcessElements.Add(elem);
        }

        /// <summary>
        /// Usado para renderizar efectos de post procesado, se puede usar en cualquier momento por ahora
        /// </summary>
        public void RenderPostProcess(params string[] efectos)
        {
            CloseRender();
            RenderBase();
            foreach(string efecto in efectos)
            {
                switch (efecto)
                {
                    case "bloom":
                        ProcesarBloom(bloom);
                        break;
                    case "oscurecer":
                        ProcesarOscurecer(oscurecer);
                        break;
                }
            }
            d3dDevice.BeginScene();//para dejar el render abierto para mas edicion
        }

        private void CloseRender()
        {
            try
            {
                d3dDevice.EndScene();
            }
            catch
            {
                return;
            }
        }

        private void RenderBase()
        {
            TexturesManager.Instance.clearAll();

            //Cargamos el Render Targer al cual se va a dibujar la escena 3D. Antes nos guardamos el surface original
            //En vez de dibujar a la pantalla, dibujamos a un buffer auxiliar, nuestro Render Target.
            old_render_target = d3dDevice.GetRenderTarget(0);
            old_depth_stencil = d3dDevice.DepthStencilSurface;
            var pSurf = base_render.GetSurfaceLevel(0);
            //pSurf = old_render_target;
            d3dDevice.SetRenderTarget(0, pSurf);
            d3dDevice.DepthStencilSurface = base_depth_stencil;
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(0,0,0), 1.0f, 0);

            //Dibujamos la escena comun, pero en vez de a la pantalla al Render Target
            //Arrancamos el renderizado. Esto lo tenemos que hacer nosotros a mano porque estamos en modo CustomRenderEnabled = true

            d3dDevice.BeginScene();

            //Dibujamos todos los meshes del escenario
            VariablesGlobales.gameModel.RenderizarMeshes();

            //Terminamos manualmente el renderizado de esta escena. Esto manda todo a dibujar al GPU al Render Target que cargamos antes
            d3dDevice.EndScene();

            //Liberar memoria de surface de Render Target
            pSurf.Dispose();
            //
            //Ahora volvemos a restaurar el Render Target original (osea dibujar a la pantalla)
            d3dDevice.SetRenderTarget(0, old_render_target);
            d3dDevice.DepthStencilSurface = old_depth_stencil;
        }

        private void ProcesarOscurecer(Effect oscurecer)
        {
            //Luego tomamos lo dibujado antes y lo combinamos con una textura
            //Arrancamos la escena
            d3dDevice.BeginScene();

            //Le decimos que va a recibir los datos desde este quad
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);
            //Cargamos parametros en el shader de Post-Procesado
            oscurecer.SetValue("render_target2D", base_render);//le pasamos al shader la textura con lo ya renderizado
            oscurecer.SetValue("scaleFactor", .5f);

            //Limiamos la pantalla y ejecutamos el render del shader
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            oscurecer.Begin(FX.None);
            oscurecer.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            oscurecer.EndPass();
            oscurecer.End();

            d3dDevice.EndScene();

            //@@@@@@@En algun lado la estoy cagando, xq se quedan lockeados los shaders 
        }

        private void ProcesarBloom(Effect bloom)//NO me sirve blur, @probar downsampling 
        {

            Surface old_render_target = d3dDevice.GetRenderTarget(0);
            Surface old_depth_stencil = d3dDevice.DepthStencilSurface;

            var pSurf = glow_mask.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);
            d3dDevice.DepthStencilSurface = base_depth_stencil;
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(0,0,0), 1.0f, 0);


            d3dDevice.BeginScene();

            PostProcessElements.ForEach(elem => elem.RenderPostProcess("bloom"));

            d3dDevice.EndScene();

            //Si quisieramos ver que se dibujo, podemos guardar el resultado a una textura en un archivo para debugear su resultado (ojo, es lento)
            
            //TextureLoader.Save(VariablesGlobales.shadersDir + "glow_map.bmp", ImageFileFormat.Bmp, glow_mask);

            pSurf.Dispose();

            //Hago downscale x4 de la img
            pSurf = downscaling.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);

            bloom.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            bloom.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            d3dDevice.BeginScene();
            bloom.Technique = "DownFilter4";
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);
            bloom.SetValue("g_RenderTarget", glow_mask);

            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            bloom.Begin(FX.None);
            bloom.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            bloom.EndPass();
            bloom.End();

            pSurf.Dispose();

            d3dDevice.EndScene();

            d3dDevice.DepthStencilSurface = old_depth_stencil;//hacen esto aca pa q cosas no se tapen?

            // Pasadas de blur
            for (int i = 0; i < 5/*cant_pasadas*/; i++)//@por ahora hardcodeo la cant de pasadas
            {
                // Gaussian blur Horizontal
                // -----------------------------------------------------
                pSurf = gaussian_aux.GetSurfaceLevel(0);//arranca con el auxiliar
                d3dDevice.SetRenderTarget(0, pSurf);
                // dibujo el quad pp dicho :

                d3dDevice.BeginScene();
                bloom.Technique = "GaussianBlurSeparable";
                d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
                d3dDevice.SetStreamSource(0, screenQuadVB, 0);
                bloom.SetValue("g_RenderTarget", downscaling);//le mando el original

                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                bloom.Begin(FX.None);
                bloom.BeginPass(0);
                d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                bloom.EndPass();
                bloom.End();
                pSurf.Dispose();

                d3dDevice.EndScene();

                pSurf = downscaling.GetSurfaceLevel(0);//uso el original
                d3dDevice.SetRenderTarget(0, pSurf);
                pSurf.Dispose();//xq hacen el dispose aca????????

                //  Gaussian blur Vertical
                // -----------------------------------------------------

                d3dDevice.BeginScene();
                bloom.Technique = "GaussianBlurSeparable";
                d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
                d3dDevice.SetStreamSource(0, screenQuadVB, 0);
                bloom.SetValue("g_RenderTarget", gaussian_aux);//le mando el auxiliar

                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                bloom.Begin(FX.None);
                bloom.BeginPass(1);
                d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                bloom.EndPass();
                bloom.End();

                d3dDevice.EndScene();
                //@no hacen psurf dispose????
            }

            //al final re-seteamos-el render target, dibujo a la pantalla
            d3dDevice.SetRenderTarget(0, old_render_target);

            d3dDevice.BeginScene();

            bloom.Technique = "BasePlusGlow";
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);
            bloom.SetValue("g_RenderTarget", base_render);
            bloom.SetValue("g_GlowMap", gaussian_aux);//@no entiendo xq usa el auxiliar en vez del downscaling
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            bloom.Begin(FX.None);
            bloom.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            bloom.EndPass();
            bloom.End();

            d3dDevice.EndScene();

            /*Blur viejo
            d3dDevice.BeginScene();

            
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);
            
            bloom.SetValue("render_target2D", glow_mask);
            bloom.SetValue("blur_intensity", 100f);

            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            bloom.Begin(FX.None);
            bloom.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            bloom.EndPass();
            bloom.End();

            d3dDevice.EndScene();
            */

            //Test
            //TextureLoader.Save(VariablesGlobales.shadersDir + "glow_applied.bmp", ImageFileFormat.Bmp, glow_applied);
            //cSurf.Dispose();
            //

            //ahora falta combinar esto con el base(de momento el base no es modificado, capaz dsps conviene pa concatenar effects)

        }

        public void Dispose()
        {
            oscurecer.Dispose();
            bloom.Dispose();
            screenQuadVB.Dispose();
            base_render.Dispose();
            base_depth_stencil.Dispose();
        }
    }
}
