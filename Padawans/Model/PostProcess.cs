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
using TGC.Core.Mathematica;
using Microsoft.DirectX;

namespace TGC.Group.Model
{
    public class PostProcess
    //@@no calculamos z-buffer x lo q hay ciertas cosas q se renderizan cuando no deberian
    //cuando comienza todo lo anterior a postProcess ya debe estar en pantalla, cuando termina deja el render abierto para otras cosas, osea NO llama a postRender()
    //@supongo q para cada efecto lo q haces es cortar el render del anterior, guardarlo en un temp, hacer el tuyo, combinarlos
    {
        private List<IPostProcess> PostProcessElements;
        private GameModel gameModel;
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
        string compilationErrors;

        //Shader
        private ShaderManager shaderManager;

        private readonly float far_plane = 1500f;
        private readonly float near_plane = 2f;
        private readonly int SHADOWMAP_SIZE = 1024;
        private Effect shader;

        private TGCVector3 lightDir; // direccion de la luz actual
        private TGCVector3 lightPos; // posicion de la luz actual (la que estoy analizando)
        private TGCMatrix lightView; // matriz de view del light
        private TGCMatrix shadowProj; // Projection matrix for shadow map

        private Texture shadow_map; // Texture to which the shadow map is rendered
        private Surface shadow_depth; // Depth-stencil buffer for rendering to shadow map

        //

        public PostProcess(GameModel gamemodel,ShaderManager shaderManager)
        {
            PostProcessElements = new List<IPostProcess>();
            this.gameModel = gamemodel;
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

            //Shaders
            if(VariablesGlobales.SHADERS)
            {
                IniciarShaders(ref shader);
            }
            shaderManager.Effect(shader);//seteo el effect q se va a usar para todos los meshes
            this.shaderManager = shaderManager;
            //
        }

        private void IniciarShaders(ref Effect shader)
        {
            shader = Effect.FromFile(d3dDevice, VariablesGlobales.shadersDir + "Shader.fx", null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (shader == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }

            VariablesGlobales.shader = shader;

            //--------------------------------------------------------------------------------------
            // Creo el shadowmap.
            // Format.R32F
            // Format.X8R8G8B8
            shadow_map = new Texture(d3dDevice, SHADOWMAP_SIZE, SHADOWMAP_SIZE, 1, Usage.RenderTarget, Format.R32F, Pool.Default);

            // tengo que crear un stencilbuffer para el shadowmap manualmente
            // para asegurarme que tenga la el mismo tamano que el shadowmap, y que no tenga
            // multisample, etc etc.
            shadow_depth = d3dDevice.CreateDepthStencilSurface(SHADOWMAP_SIZE, SHADOWMAP_SIZE, DepthFormat.D24S8, MultiSampleType.None, 0, true);
            // por ultimo necesito una matriz de proyeccion para el shadowmap, ya
            // que voy a dibujar desde el pto de vista de la luz.
            // El angulo tiene que ser mayor a 45 para que la sombra no falle en los extremos del cono de luz
            // de hecho, un valor mayor a 90 todavia es mejor, porque hasta con 90 grados es muy dificil
            // lograr que los objetos del borde generen sombras
            shadowProj = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(80), D3DDevice.Instance.AspectRatio,
                                                    50, 5000);
            /*
            d3dDevice.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f),
                                             D3DDevice.Instance.AspectRatio, D3DDevice.Instance.ZNearPlaneDistance,
                                             D3DDevice.Instance.ZFarPlaneDistance).ToMatrix();//near_plane, far_plane).ToMatrix();
            //@@@@@CUIDADO TOY CAMBIANDO TODO EL VIEW ACA
            */
            lightPos = new TGCVector3(0, 100, 50);
            lightDir = new TGCVector3(0, -1, 1);
            lightDir.Normalize();
        }

        //@@@

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
            oscurecer = Effect.FromFile(d3dDevice, VariablesGlobales.shadersDir + "PostProcess.fx", null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (oscurecer == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Cargar shader con efectos de Post-Procesado
            //oscurecer = TGCShaders.Instance.LoadEffect(VariablesGlobales.shadersDir + "PostProcess.fx");

            //Configurar Technique dentro del shader
            oscurecer.Technique = "OscurecerTechnique";
        }

        private void IniciarBloom(ref Effect bloom)
        {
            bloom = Effect.FromFile(d3dDevice, VariablesGlobales.shadersDir + "GaussianBlur.fx", null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (bloom == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //bloom = TGCShaders.Instance.LoadEffect(VariablesGlobales.shadersDir + "GaussianBlur.fx");//de momento voy a usar el blur comun ese
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

        public void PPCheckInicial(string[] efectos)
        {
            if (VariablesGlobales.POSTPROCESS == false)
                throw new Exception("La funcion post procesado no puede ser llamada con POSTPROCESS off");
            if (!hasBaseRender)
                throw new Exception("No existe target para el post procesado");
            if (efectos.Length == 0)
                throw new Exception("No hay efectos en el pedido de post procesado");
        }

        /// <summary>
        /// Usado para renderizar efectos de post procesado, se puede usar en cualquier momento por ahora
        /// </summary>
        public void RenderPostProcess(params string[] efectos)
        {
            PPCheckInicial(efectos);

            foreach(string efecto in efectos)
            {

                switch (efecto)//@HACERLO ENUM
                {
                    case "bloom":
                        ProcesarBloom(bloom);//@@@@cada uno toma un base y lo usa para crear un finished
                        hasFinishedRender = true; //@cada metodo procesar deberia devolver un valor indicando si se pudo ejecutar, en tal caso hay un nuevo render
                        break;
                }
                if (hasFinishedRender) PasarFinishedABase();
            }
            //Terminas teniendo el render terminado en base
            //d3dDevice.BeginScene();//para dejar el render abierto para mas edicion
        }

        /// <summary>
        /// La diferencia es q como todavia tas renderizando otras cosas te dejo abierto el render
        /// </summary>
        /// <param name="efectos"></param>
        public void RenderMenuPostProcess(params string[] efectos)
        {
            PPCheckInicial(efectos);

            d3dDevice.EndScene();

            foreach (string efecto in efectos)
            {

                switch (efecto)//@HACERLO ENUM
                {
                    case "oscurecer":
                        ProcesarMenuOscurecer(oscurecer);
                        hasFinishedRender = true;
                        break;
                }
                if (hasFinishedRender) PasarFinishedABase();
            }
            d3dDevice.BeginScene();
        }

        private void PasarFinishedABase()
        {
            var pSurf = base_render.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);//@@@no puedo usar el finished_render????
            d3dDevice.DepthStencilSurface = base_depth;//@@sigo sin saber q hacer con el depth
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(0, 0, 0), 1.0f, 0);
            /*
            if(VariablesGlobales.time<0)
            TextureLoader.Save(VariablesGlobales.shadersDir + "test.bmp", ImageFileFormat.Bmp, base_render);
            else VariablesGlobales.time-=VariablesGlobales.elapsedTime; 
            */
            d3dDevice.BeginScene();
            //@@poner un efecto especifico para hacer esto
            bloom.Technique = "Copy";
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);
            bloom.SetValue("g_RenderTarget", finished_render);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            bloom.Begin(FX.None);
            bloom.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            bloom.EndPass();
            bloom.End();

            d3dDevice.EndScene();
            //
            //base_render = finished_render;//@@@@@esto debe estar mal escrito
            //base_depth = finished_depth;
            hasFinishedRender = false;
        }

        public void RenderToScreen()
        {
            d3dDevice.SetRenderTarget(0, screen_render);//@@@no puedo usar el finished_render????
            d3dDevice.DepthStencilSurface = screen_depth_stencil;
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(0, 0, 0), 1.0f, 0);
            /*
            if(VariablesGlobales.time<0)
            TextureLoader.Save(VariablesGlobales.shadersDir + "test.bmp", ImageFileFormat.Bmp, base_render);
            else VariablesGlobales.time-=VariablesGlobales.elapsedTime; 
            */
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
        public void DoEndgameRender()
        {
            if (VariablesGlobales.endgameManager.Fin())
            {
                d3dDevice.BeginScene();
                gameModel.RenderizarFinJuego();
                d3dDevice.EndScene();
            }
        }

        public void DoMenuRender()
        {
            d3dDevice.BeginScene();
            gameModel.RenderizarMenus();
            d3dDevice.EndScene();
        }

        public void DoExtrasRender()
        {
            d3dDevice.BeginScene();
            gameModel.RenderizarExtras();
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


            if (VariablesGlobales.SHADERS)
            {
                RenderShaders();

                var pSurf = base_render.GetSurfaceLevel(0);
                d3dDevice.SetRenderTarget(0, pSurf);
                d3dDevice.DepthStencilSurface = base_depth;
                //d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(0, 0, 0), 1.0f, 0);

                d3dDevice.BeginScene();
                // dibujo la escena pp dicha

                shaderManager.RenderMesh(ShaderManager.MESH_TYPE.DEFAULT);
                shaderManager.SetTechnique("RenderScene", ShaderManager.MESH_TYPE.SHADOW);
                shaderManager.RenderMesh(ShaderManager.MESH_TYPE.SHADOW);

                d3dDevice.EndScene();

                pSurf.Dispose();
            }
            else
            {
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
                gameModel.RenderizarMeshes();
                //Terminamos manualmente el renderizado de esta escena. Esto manda todo a dibujar al GPU al Render Target que cargamos antes
                d3dDevice.EndScene();

                //Liberar memoria de surface de Render Target
                pSurf.Dispose();
                //
                }

        }

        private void RenderShaders()// x ahora solo shadowmap
        {
            // Calculo la matriz de view de la luz

            if (VariablesGlobales.DameLuz)
            {
                TGCVector3 light_pos = VariablesGlobales.xwing.GetPosition();
                TGCVector3 light_dir = VariablesGlobales.xwing.GetCoordenadaEsferica().GetXYZCoord();
                shader.SetValue("g_vLightPos", new Vector4(light_pos.X, light_pos.Y, light_pos.Z , 1));
                shader.SetValue("g_vLightDir", new Vector4(light_dir.X, light_dir.Y, light_dir.Z, 1));
                lightView = TGCMatrix.LookAtLH(light_pos, light_pos + light_dir, new TGCVector3(0, 0, 1));
            }
            else
            {
                lightPos.Y = VariablesGlobales.xwing.GetPosition().Y+50;
                lightPos.Z = VariablesGlobales.xwing.GetPosition().Z;
                shader.SetValue("g_vLightPos", new Vector4(lightPos.X, lightPos.Y, lightPos.Z, 1));

                lightDir =  VariablesGlobales.xwing.GetPosition() - lightPos;
                lightDir.Normalize();
                shader.SetValue("g_vLightDir", new Vector4(lightDir.X, lightDir.Y, lightDir.Z, 1));
                lightView = TGCMatrix.LookAtLH(lightPos, lightPos+lightDir, new TGCVector3(0, 0, 1));
            }
            //@@@@xq está el up vector en la z??
            
            //@@@@@@@@Probar solo renderizar en el shadow al xwing, y dsps poner al piso como render scene

            // inicializacion standard:
            shader.SetValue("g_mProjLight", shadowProj.ToMatrix());
            shader.SetValue("g_mViewLightProj", (lightView * shadowProj).ToMatrix());

            // Primero genero el shadow map, para ello dibujo desde el pto de vista de luz
            // a una textura, con el VS y PS que generan un mapa de profundidades.
            var pShadowSurf = shadow_map.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, pShadowSurf);
            D3DDevice.Instance.Device.DepthStencilSurface = shadow_depth;
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            // Hago el render de la escena pp dicha
            shader.SetValue("g_txShadow", shadow_map);//deja la textura seteada pa q se use dsps en el render normal

            D3DDevice.Instance.Device.BeginScene();

            //necesito un metodo q todos puedan pasar su mesh y q el postprocess les ponga la technique
            shaderManager.SetTechnique("RenderShadow", ShaderManager.MESH_TYPE.SHADOW);
            shaderManager.RenderMesh(ShaderManager.MESH_TYPE.SHADOW);

            // Termino
            D3DDevice.Instance.Device.EndScene();

            //TextureLoader.Save(variablesGlobales.shadersDir+"shadowmap.bmp", ImageFileFormat.Bmp, shadow_map);

        }

        /// <summary>
        /// Todo efecto debe renderizar a  este target la nueva img procesada
        /// </summary>
        private void SetearTargetAFinished()//@no se q onda con el depth, de momento lo seteo tmb
        {
            d3dDevice.SetRenderTarget(0, finished_render.GetSurfaceLevel(0));
            d3dDevice.DepthStencilSurface = finished_depth;//@@ver xq uso este
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(0, 0, 0), 1.0f, 0);//@@creo q esto no va 
        }

        private void ProcesarMenuOscurecer(Effect oscurecer)
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
            d3dDevice.DepthStencilSurface = base_depth;//@@me parece q aca está cagando el depth base!!
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

            //al final re-seteamos-el render target a nuestro finished

            SetearTargetAFinished();//@@@@ACA ESTÁ EL PROBLEMA como hago base=finished se estan pisando

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

            /*
            if (VariablesGlobales.time < 0)
            {
                TextureLoader.Save(VariablesGlobales.shadersDir + "base_after_combo.bmp", ImageFileFormat.Bmp, base_render);
                TextureLoader.Save(VariablesGlobales.shadersDir + "gauss1.bmp", ImageFileFormat.Bmp, gaussian_aux);
                TextureLoader.Save(VariablesGlobales.shadersDir + "finished.bmp", ImageFileFormat.Bmp, finished_render);
            }
            else VariablesGlobales.time -= VariablesGlobales.elapsedTime;
            */

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
            shadow_map.Dispose();
            shadow_depth.Dispose();
        }
    }
}
