using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Collision;
using TGC.Core.BoundingVolumes;
using System;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Una clase de ejemplo que grafica pistas para poder tener una referencia de la velocidad de la nave
    /// </summary>
    public class MainRunway : SceneElement
    {
        private TgcSceneLoader loader;
        TgcScene escena_bomba, escena_alrededores, escena_alrededores2,hierro, tubo_rojo_gira, tubo_rojo_derecho;
        List<List<TgcMesh>> main_escena_instancia; //deberia ser lista de lista de lista
        //bloques de construccion
        //TgcScene piso;
        private int n;
        TgcFrustum frustum;
        private bool frustum_culling = true;

        Random random;

        Xwing target;

        /// <summary>
        ///     n representa la cantidad de pistas que va a graficar
        /// </summary>
        public MainRunway(TgcSceneLoader loader, int n, TgcFrustum frustum, Xwing targetTorretas)
        {
            main_escena_instancia = new List<List<TgcMesh>>();
            random = new Random();
            this.loader = loader;
            this.frustum = frustum;
            this.target = targetTorretas;
            escena_bomba = loader.loadSceneFromFile("Padawans_media\\XWing\\TRENCH_RUN-TgcScene.xml");
            escena_alrededores = loader.loadSceneFromFile("Padawans_media\\XWing\\death+star-TgcScene.xml");
            escena_alrededores2 = loader.loadSceneFromFile("Padawans_media\\XWing\\death+star2-TgcScene.xml");
            hierro = loader.loadSceneFromFile("Padawans_media\\XWing\\hierros-TgcScene.xml");
            tubo_rojo_gira = loader.loadSceneFromFile("Padawans_media\\XWing\\pipeline-TgcScene.xml");
            tubo_rojo_derecho = loader.loadSceneFromFile("Padawans_media\\XWing\\tuberia-TgcScene.xml");

            //bloques de construccion
            //piso = loader.loadSceneFromFile("Padawans_media\\XWing\\m1-TgcScene.xml");
            //

            this.n = n;

            Escena_Principal();

        }

        private void PlaceMeshRnd() { }//@funcion a realizar

        private void RenderMeshList (List <TgcMesh> meshes)
        {
            meshes.ForEach(mesh=>{ mesh.Render(); });
        }

        private void AddListListMeshToMain(List<List<TgcMesh>> todas_escenas)
        {
            todas_escenas.ForEach(escena => { main_escena_instancia.Add(escena); });
        }

        private TGCVector3 PlaceSceneLine(TgcScene escena, TGCVector3 posicion, TGCVector3 escalador, 
            int repeticiones, int mesh_pivot,float distancia_extra, TGCVector3 rotacion, bool shader)//agrega la scene al render
        //la rotacion es muy limitada solo queda bien en pi/2 o pi y aun asi solo en ciertos casos, estoy trabajando en un nuevo metodo
        //el mesh pivot es para elegir cual de las meshes es el que va a usar de separador
        {
            List<List<TgcMesh>> todas_escenas = new List<List<TgcMesh>>();//tengo que devolverlos como list de list ya que tgcscene no soporta que le agregue meshes
            
            for (int i = 0; i < repeticiones; i++)
            {
                todas_escenas.Add( new List<TgcMesh>());

                foreach (TgcMesh mesh in escena.Meshes)
                {
                    mesh.Scale = escalador;
                    mesh.Rotation = rotacion;
                    mesh.Position = posicion;
                    todas_escenas[i].Add(mesh.clone(mesh.Name));
                }
                
                posicion = new TGCVector3(todas_escenas[i][mesh_pivot].Position.X,
                                            todas_escenas[i][mesh_pivot].Position.Y,
                                            todas_escenas[i][mesh_pivot].Position.Z -
                                                todas_escenas[i][mesh_pivot].BoundingBox.calculateSize().Z - distancia_extra);//sip de momento solo en z
            }
            //Shader
            if(shader)
                todas_escenas.ForEach(scena => scena.ForEach(mesh => VariablesGlobales.shaderManager.AgregarMesh(mesh, ShaderManager.MESH_TYPE.SHADOW)));
            else 
                todas_escenas.ForEach(scena => scena.ForEach(mesh => VariablesGlobales.shaderManager.AgregarMesh(mesh, ShaderManager.MESH_TYPE.DEFAULT)));

            AddListListMeshToMain(todas_escenas);//@lo agrego acá asi puedo retornar la ultima posicion por si es necesaria, conviene hacer asi??
            return posicion;
        }

        private void CrearTorreta(TGCVector3 pos, TGCVector3 rotation)
        {
            Torreta torreta = new Torreta(target, pos, rotation);
            VariablesGlobales.managerEnemigos.AgregarElemento(torreta);
            VariablesGlobales.postProcess.AgregarElemento(torreta);
            //new TGCVector3(50f, 10f, 0f), new TGCVector3(0, FastMath.PI_HALF, 0))
        }

        private float MyRandom()//retorna float entre 0 y 1 de a .1 incrementos, puede ser positivo o negativo
        {
            float signo = random.Next(1)==1 ? 1f : -1f;
            return ((float)random.Next(10) / 10f)*signo;
        }

        private void PonerTorretas(TGCVector3 posInicial, TGCVector3 posFinal,TGCVector3 rotation,
                                    float mitadAnchoPista,int cantidad)
        {
            float signoZFinal = (float)Math.Sign(posFinal.Z);
            //mejor hacer abs de zinicial - zfinal y usar el signo de zfinal p sumarle a zinicial
            int zLargo =(int)FastMath.Abs(posInicial.Z-posFinal.Z);
            for (int i = 0; i < cantidad; i++)
            {//@@Problema aca con el random, solo sirve pa ints positivos, cambiarlo y obtener el + o - afuera, esta tirando exception
                CrearTorreta(new TGCVector3(posInicial.X+mitadAnchoPista*MyRandom() + 30, posInicial.Y-65,posInicial.Z+ signoZFinal * (float)random.Next(zLargo)),rotation);
            }
            
        }

        private void Escena_Principal()
        {
            var posicion = new TGCVector3(-160f, 0f, -500f);//por el tamaño de los objetos su centro termina cerca de 0,0,0

            TGCVector3 posicion_inicial = new TGCVector3(0,0,0);//variable para contar la distancia recorrida, y asi agregar objetos extra luego
            TGCVector3 posicion_final; //en z es una linea recta hacia posicion_inicial

            var escalador = new TGCVector3(30f, 30f, 30f);
            int mesh_pivot = 0;
            var rotacion = new TGCVector3(0f, 0f, 0f);

            //escenario principal
            //-----

            posicion = PlaceSceneLine(escena_bomba, posicion, escalador, n/2, mesh_pivot, 0,rotacion,true);

            posicion.X += 325f;//necesitamos rotacion sin que modifique posicion
            rotacion = new TGCVector3(0f, FastMath.PI, 0f);
            posicion = PlaceSceneLine(escena_bomba, posicion, escalador, 1, mesh_pivot, 0, rotacion,true);

            posicion.X -= 325f;
            rotacion = new TGCVector3(0f, 0f, 0f);
            posicion = PlaceSceneLine(escena_bomba, posicion, escalador, n / 2, mesh_pivot, 0, rotacion,true);


            posicion = new TGCVector3(-350f, 50f, -500f);
            escalador = new TGCVector3(50f, 50f, 30f);
            mesh_pivot = 1;
            rotacion = new TGCVector3(0,FastMath.TWO_PI, 0f);
            PlaceSceneLine(escena_alrededores, posicion, escalador, n, mesh_pivot, 0,rotacion,false);

            posicion = new TGCVector3(0f, 800f, -500f);
            mesh_pivot = 2;
            rotacion = new TGCVector3(0f, 0f, FastMath.PI);
            posicion_final = PlaceSceneLine(escena_alrededores2, posicion, escalador, n, mesh_pivot, 0,rotacion,false);
            //-----

            //extras
            //-----
            TGCVector3 distancia = new TGCVector3(0,0, -FastMath.Abs(posicion_final.Z));

            posicion = distancia*.1f;
            //posicion = new TGCVector3(0f, 0f, -500f);
            //posicion = posicion_inicial - (distancia * .5f);
            escalador = new TGCVector3(.65f, .3f, 1f);
            mesh_pivot = 0;
            rotacion = new TGCVector3(0f, 0f, 0f);
            PlaceSceneLine(tubo_rojo_gira, posicion, escalador, n, mesh_pivot, 400f, rotacion,true);

            posicion = distancia * .4f + new TGCVector3(30f,0,0);
            escalador = new TGCVector3(.3f, .3f, 5f);
            PlaceSceneLine(tubo_rojo_derecho, posicion, escalador, (int)(n * 1.5f), mesh_pivot, 0,rotacion,true);

            posicion = distancia * .6f + new TGCVector3(-30f, 0, 0);
            PlaceSceneLine(tubo_rojo_derecho, posicion, escalador, (int)(n * 1.5f), mesh_pivot, 0, rotacion,true);

            escalador = new TGCVector3(30f, 50f, 50f);
            mesh_pivot = 0;
            rotacion = new TGCVector3(0f, 0f, 0f);
            //PlaceSceneLine(hierro, posicion, escalador, (int)(n * .8f), mesh_pivot, 0,rotacion);

            //-----

            //agregar al physics engine (por ahora no guardo los rigidbody, no se si sirven para algo)
            main_escena_instancia.ForEach(mesh_list => VariablesGlobales.physicsEngine.AgregarEscenario(mesh_list));
            //
            PonerTorretas(posicion_inicial, posicion_final, new TGCVector3(0,FastMath.PI,0),50f,30);

        }

        public override void Render()
        {
            if (frustum_culling)
            {
                foreach (var listMesh in main_escena_instancia)
                {
                    foreach (var mesh in listMesh)
                    {
                        //Solo mostrar la malla si colisiona contra el Frustum
                        var huboColision = TgcCollisionUtils.classifyFrustumAABB(this.frustum, mesh.BoundingBox);
                        if (huboColision != TgcCollisionUtils.FrustumResult.OUTSIDE)
                        {
                            mesh.Render();
                        }
                    }
                }
            }
            else
            {
                main_escena_instancia.ForEach(escena => { RenderMeshList(escena); });//@@esta bien que renderize cada vez si no hay cambios??
            }
        }

        public override void Update()
        {

        }

        public override void Dispose()
        {
            main_escena_instancia.ForEach(escena => { escena.ForEach(mesh => { mesh.Dispose(); }); });
        }

        public override void RenderBoundingBox()
        {
            main_escena_instancia.ForEach(escena => { escena.ForEach(mesh => { mesh.BoundingBox.Render(); }); });
        }

    }
}