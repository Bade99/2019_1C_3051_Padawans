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

namespace TGC.Group.Model
{
    /// <summary>
    ///     Una clase de ejemplo que grafica pistas para poder tener una referencia de la velocidad de la nave
    /// </summary>
    public class MainRunway : SceneElement
    {
        private TgcSceneLoader loader;
        TgcScene escena_bomba, escena_alrededores, escena_alrededores2,hierro, tubo_rojo_gira, tubo_rojo_derecho;
        List<List<TgcMesh>> main_escena_instancia = new List<List<TgcMesh>>();//deberia ser lista de lista de lista
        //bloques de construccion
        //TgcScene piso;
        private int n;
        /// <summary>
        ///     n representa la cantidad de pistas que va a graficar
        /// </summary>
        public MainRunway(TgcSceneLoader loader, int n)
        {
            this.loader = loader;

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

        private TGCVector3 PlaceSceneLineRot(TgcScene escena, TGCVector3 posicion, TGCVector3 escalador, int repeticiones, int mesh_pivot,float distancia_extra,TGCVector3 rotacion)//NO usar
        //no funciona del todo porque las bounding boxes no se actualizan con rotaciones, WHAT
        {
            for (int i = 0; i < repeticiones; i++)
            {
                foreach (TgcMesh mesh in escena.Meshes)
                {
                    //mesh.AutoTransform = false;
                    //mesh.AutoUpdateBoundingBox = false;

                    //mesh. = TGCMatrix.Scaling(escalador.X, escalador.Y, escalador.Z);
                    //mesh.Scale = escalador;
                    mesh.AutoTransformEnable = false;
                    mesh.AutoUpdateBoundingBox = true;
                    //mesh.Transform = TGCMatrix.Scaling(escalador);
                    var centro = new TGCVector3();
                    mesh.GetPosition(centro);
                    mesh.Transform = TGCMatrix.Transformation(centro,TGCQuaternion.Identity/*new TGCQuaternion(escalador.X,escalador.Y,escalador.Z,1f)*/,escalador,centro,new TGCQuaternion(rotacion.X,rotacion.Y,rotacion.Z,1f),posicion);
                    //mesh.Transform = TGCMatrix.RotationYawPitchRoll(rotacion.X, rotacion.Y, rotacion.Z);
                    //mesh.Transform = TGCMatrix.Scaling(escalador) * TGCMatrix.RotationYawPitchRoll(rotacion.Y, rotacion.X, rotacion.Z) * TGCMatrix.Translation(posicion);
                    //mesh.Transform.RotateX(rotacion.X);
                    //mesh.Transform.RotateY(rotacion.Y);
                    //mesh.Transform.RotateZ(rotacion.Z);

                    //mesh.createBoundingBox();
                    mesh.BoundingBox.Render();
                    //mesh.Transform.Transform
                    //mesh.Rotation = rotacion;
                    //mesh.Position = posicion;
                    //mesh.MoveOrientedY

                }
                //var size = new TGCVector3(0,0,0);
                //size = escena.Meshes[mesh_pivot].BoundingBox.calculateSize();
                //posicion = new TGCVector3(escena.Meshes[mesh_pivot].Position.X                                     - size.X * FastMath.Sin(rotacion.Y) + size.X * FastMath.Cos(rotacion.Z),//- size.X * FastMath.Cos(rotacion.X)
                //                          escena.Meshes[mesh_pivot].Position.Y - size.Y * FastMath.Cos(rotacion.X)                                     + size.Y * FastMath.Cos(rotacion.Z),
                //                          escena.Meshes[mesh_pivot].Position.Z - size.Z * FastMath.Cos(rotacion.X) - size.Z * FastMath.Sin(rotacion.Y) - distancia_extra);//- size.Z * FastMath.Sin(rotacion.Z)

                posicion = new TGCVector3(escena.Meshes[mesh_pivot].Position.X,
                                            escena.Meshes[mesh_pivot].Position.Y,
                                            escena.Meshes[mesh_pivot].Position.Z -
                                                escena.Meshes[mesh_pivot].BoundingBox.calculateSize().Z - distancia_extra);

                //foreach(TgcMesh mesh in escena.Meshes)
                //{
                //    //mesh.AutoTransform = false;

                //    //mesh.UpdateMeshTransform();
                //    mesh.createBoundingBox();
                //    //mesh.updateBoundingBox();
                //    mesh.BoundingBox.Render();

                //}

                //var esquinas = new TGCVector3[8];
                //esquinas = escena.Meshes[mesh_pivot].BoundingBox.computeCorners();

                //posicion = esquinas[3];

                escena.RenderAll();

            }
            return posicion;//retorna la proxima posicion
        }

        private void RenderMeshList (List <TgcMesh> meshes)
        {
            meshes.ForEach(mesh=>{ mesh.Render(); });
        }

        private void AddListListMeshToMain(List<List<TgcMesh>> todas_escenas)
        {
            todas_escenas.ForEach(escena => { main_escena_instancia.Add(escena); });
        }

        private TGCVector3 PlaceSceneLine(TgcScene escena, TGCVector3 posicion, TGCVector3 escalador, int repeticiones, int mesh_pivot,float distancia_extra, TGCVector3 rotacion)//agrega la scene al render
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
            AddListListMeshToMain(todas_escenas);//@lo agrego acá asi puedo retornar la ultima posicion por si es necesaria, conviene hacer asi??
            return posicion;
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
            posicion = PlaceSceneLine(escena_bomba, posicion, escalador, n/2, mesh_pivot, 0,rotacion);

            posicion.X += 325f;//necesitamos rotacion sin que modifique posicion
            rotacion = new TGCVector3(0f, FastMath.PI, 0f);
            posicion = PlaceSceneLine(escena_bomba, posicion, escalador, 1, mesh_pivot, 0, rotacion);

            posicion.X -= 325f;
            rotacion = new TGCVector3(0f, 0f, 0f);
            posicion = PlaceSceneLine(escena_bomba, posicion, escalador, n / 2, mesh_pivot, 0, rotacion);


            posicion = new TGCVector3(-350f, 50f, -500f);
            escalador = new TGCVector3(50f, 50f, 30f);
            mesh_pivot = 1;
            rotacion = new TGCVector3(0,FastMath.TWO_PI, 0f);
            PlaceSceneLine(escena_alrededores, posicion, escalador, n, mesh_pivot, 0,rotacion);

            posicion = new TGCVector3(0f, 800f, -500f);
            mesh_pivot = 2;
            rotacion = new TGCVector3(0f, 0f, FastMath.PI);
            posicion_final = PlaceSceneLine(escena_alrededores2, posicion, escalador, n, mesh_pivot, 0,rotacion);
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
            PlaceSceneLine(tubo_rojo_gira, posicion, escalador, n, mesh_pivot, 400f, rotacion);

            posicion = distancia * .4f + new TGCVector3(30f,0,0);
            escalador = new TGCVector3(.3f, .3f, 5f);
            PlaceSceneLine(tubo_rojo_derecho, posicion, escalador, (int)(n * 1.5f), mesh_pivot, 0,rotacion);

            posicion = distancia * .6f + new TGCVector3(-30f, 0, 0);
            PlaceSceneLine(tubo_rojo_derecho, posicion, escalador, (int)(n * 1.5f), mesh_pivot, 0, rotacion);

            escalador = new TGCVector3(30f, 50f, 50f);
            mesh_pivot = 0;
            rotacion = new TGCVector3(0f, 0f, 0f);
            //PlaceSceneLine(hierro, posicion, escalador, (int)(n * .8f), mesh_pivot, 0,rotacion);

            //-----
        }

        public override void Render()
        {
            main_escena_instancia.ForEach(escena => { RenderMeshList(escena); });//@@esta bien que renderize cada vez si no hay cambios??
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