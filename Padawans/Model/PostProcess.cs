﻿using System;
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
        private Device d3dDevice;

        private bool hasBaseRender=false;
        private bool hasFinishedRender = false;

        private Surface screen_render;
        private Surface screen_depth_stencil;

        //General
        private VertexBuffer screenQuadVB;
        private Texture base_render;
        private Surface base_depth;
        private Texture finished_render;
        private Surface finished_depth;
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

            screen_render = d3dDevice.GetRenderTarget(0);
            screen_depth_stencil = d3dDevice.DepthStencilSurface;

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
            base_depth = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                    d3dDevice.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);

            finished_render = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth,
                d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            finished_depth = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
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
            if (!hasBaseRender)
               throw new Exception("No existe target para el postprocesado");

            foreach(string efecto in efectos)
            {

                switch (efecto)
                {
                    case "bloom":
                        ProcesarBloom(bloom);//@@@@cada uno toma un base y lo usa para crear un finished
                        hasFinishedRender = true; //@cada metodo procesar deberia devolver un valor indicando si se pudo ejecutar, en tal caso hay un nuevo render
                        break;
                    case "oscurecer":
                        ProcesarOscurecer(oscurecer);
                        hasFinishedRender = true;
                        break;
                }
                if (hasFinishedRender) PasarFinishedABase();
            }
            //Terminas teniendo el render terminado en base
            d3dDevice.BeginScene();//para dejar el render abierto para mas edicion
        }

        private void PasarFinishedABase()
        {
            base_render = finished_render;
            base_depth = finished_depth;
            hasFinishedRender = false;
        }

        public void RenderToScreen()
        {
            d3dDevice.SetRenderTarget(0, screen_render);
            d3dDevice.DepthStencilSurface = screen_depth_stencil;
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(0, 0, 0), 1.0f, 0);

            d3dDevice.BeginScene();
            //@@poner un efecto especifico para hacer esto
            bloom.Technique = "Copy";
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);
            bloom.SetValue("g_RenderTarget", base_render);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            bloom.Begin(FX.None);
            bloom.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            bloom.EndPass();
            bloom.End();

            d3dDevice.EndScene();
        }

        public void DoBaseRender()
        {
            RenderBase();
            hasBaseRender = true;
        }

        public void ClearBaseRender()
        {
            hasBaseRender = false;
        }

        public void ClearFinishedRender()
        {
            hasFinishedRender = false;
        }

        private void RenderBase()
        {
            TexturesManager.Instance.clearAll();

            //Cargamos el Render Targer al cual se va a dibujar la escena 3D. Antes nos guardamos el surface original
            //En vez de dibujar a la pantalla, dibujamos a un buffer auxiliar, nuestro Render Target.

            var pSurf = base_render.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);
            d3dDevice.DepthStencilSurface = base_depth;
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
        }
        /// <summary>
        /// Todo efecto debe renderizar a  este target la nueva img procesada
        /// </summary>
        private void SetearTargetAFinished()//@no se q onda con el depth, de momento lo seteo tmb
        {
            d3dDevice.SetRenderTarget(0, finished_render.GetSurfaceLevel(0));
            d3dDevice.DepthStencilSurface = finished_depth;//@@ver xq uso este
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(0, 0, 0), 1.0f, 0);
        }

        private void ProcesarOscurecer(Effect oscurecer)
        {
            SetearTargetAFinished();

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

        private void ProcesarBloom(Effect bloom)
        {
            var pSurf = glow_mask.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);
            d3dDevice.DepthStencilSurface = base_depth;//@@ver xq uso este
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

            //d3dDevice.DepthStencilSurface = old_depth_stencil;//@@@@@@@@@@@@@@@@@@@@@@@@@hacen esto aca pa q cosas no se tapen?

            // Pasadas de blur
            for (int i = 0; i < 4/*cant_pasadas*/; i++)//@por ahora hardcodeo la cant de pasadas
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
            //TextureLoader.Save(VariablesGlobales.shadersDir + "base.bmp", ImageFileFormat.Bmp, base_render);
            //TextureLoader.Save(VariablesGlobales.shadersDir + "blur.bmp", ImageFileFormat.Bmp, gaussian_aux);


            //al final re-seteamos-el render target, dibujo a la pantalla

            SetearTargetAFinished();

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
            
            //@Problemas:
            //-el z no se está tomando en cuenta y ta dibujando arriba de otras cosas
            //-no anda en menus
        }

        public void Dispose()
        {
            oscurecer.Dispose();
            bloom.Dispose();
            screenQuadVB.Dispose();
            base_render.Dispose();
            base_depth.Dispose();
            finished_render.Dispose();
            finished_depth.Dispose();
        }
    }
}