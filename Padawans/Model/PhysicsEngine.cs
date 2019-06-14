using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using BulletSharp.Math;
using TGC.Core.BulletPhysics;
using TGC.Core.Mathematica;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Input;

namespace TGC.Group.Model
{
    public class PhysicsEngine
    {
        protected CollisionWorld collisionWorld;
        protected CollisionDispatcher dispatcher;
        protected DefaultCollisionConfiguration collisionConfiguration;
        protected SequentialImpulseConstraintSolver constraintSolver;
        protected BroadphaseInterface overlappingPairCache;
        private NaveMisilCollisionCallback collisionNaveMisilCallback;
        private Dictionary<int, CollisionObject> listaMisilesEnemigo;
        private Dictionary<int, CollisionObject> listaMisilesXwing;
        private Dictionary<int, Torreta> listaTorretas;
        private List<int> listaIdMisilesQueColisionaronConXwing;
        private int misilIdCount = 20;
        private int torretaIdCount = 1;
        private static readonly int ID_XWING = 1;
        TorretaMisilCollisionCallback torretaMisilCollisionCallback = new TorretaMisilCollisionCallback();

        CollisionObject main_character;

        public PhysicsEngine()
        {
            collisionNaveMisilCallback = new NaveMisilCollisionCallback();
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            collisionWorld = new CollisionWorld(dispatcher, overlappingPairCache, collisionConfiguration);
            listaMisilesEnemigo = new Dictionary<int, CollisionObject>();
            listaMisilesXwing = new Dictionary<int, CollisionObject>();
            listaTorretas = new Dictionary<int, Torreta>();
            listaIdMisilesQueColisionaronConXwing = new List<int>();
            collisionWorld.DebugDrawer = new DebugDrawerTest();
            collisionWorld.DebugDrawer.DebugMode = DebugDrawModes.All;
        }

        public CollisionObject AgregarMisilEnemigo(TGCVector3 size)
        {
            CollisionObject misilActual = AgregarMisil(size);
            if (VariablesGlobales.MODO_DIOS)
            {
                misilActual.SetIgnoreCollisionCheck(main_character, true);
            }
            listaMisilesEnemigo.Add(misilIdCount, misilActual);
            misilIdCount++;
            misilActual.UserIndex = misilIdCount;
            return misilActual;
        }
        public CollisionObject AgregarMisilXwing(TGCVector3 size)
        {
            CollisionObject misilActual = AgregarMisil(size);
            listaMisilesXwing.Add(misilIdCount, misilActual);
            misilIdCount++;
            return misilActual;
        }

        public CollisionObject AgregarTorreta(Torreta torreta, TGCVector3 size)
        {
            CollisionObject torretaCollision = CrearCollisionObject(size);
            listaTorretas.Add(torretaIdCount, torreta);
            torretaCollision.CollisionFlags = torretaCollision.CollisionFlags | CollisionFlags.CustomMaterialCallback;
            torretaCollision.UserIndex = torretaIdCount;
            torretaIdCount++;
            collisionWorld.AddCollisionObject(torretaCollision);
            return torretaCollision;
        }

        private CollisionObject AgregarMisil(TGCVector3 size)
        {
            CollisionObject misil = CrearCollisionObject(size);
            misil.CollisionFlags = misil.CollisionFlags | CollisionFlags.CustomMaterialCallback;
            collisionWorld.AddCollisionObject(misil);
            return misil;
        }

        private void ChequearColisionesXwingConMisiles()
        {
            if (VariablesGlobales.MODO_DIOS) return;
            listaMisilesEnemigo.Values.ToList<CollisionObject>().ForEach((misil) =>
            {
                collisionWorld.ContactPairTest(misil, main_character, new NaveMisilCollisionCallback());
            });
        }
        private void ChequearColisionesTorretasConMisiles()
        {
            listaMisilesXwing.Values.ToList<CollisionObject>().ForEach((misil) =>
            {
                listaTorretas.Values.ToList<Torreta>().ForEach((torreta) =>
                {
                    collisionWorld.ContactPairTest(misil, torreta.CollisionObject, torretaMisilCollisionCallback);
                });
            });
        }

        public void DisminuirVidaTorreta(int torretaId)
        {
            listaTorretas[torretaId].DisminuirVida();
        }

        public void EliminarMisilDeLista(int misilId)
        {
            listaMisilesEnemigo.Remove(misilId);
        }

        public void AgregarColisionConXwing(int misilId)
        {
            listaIdMisilesQueColisionaronConXwing.Add(misilId);
        }
        /**
         * Metodo que elimina un misilId de la lista, para que cada colision con el xwing se contabilice
         * solo una vez
         * */
        public bool CheckCollisionMisilXwing(int misilId)
        {
            return listaIdMisilesQueColisionaronConXwing.Contains(misilId);
        }

        public List<RigidBody> AgregarEscenario(List<TgcMesh> meshesEscenario)
        {
            List<RigidBody> bodies = new List<RigidBody>();
            meshesEscenario.ForEach(mesh =>
            {
                /**
                var body = CreateRigidBodyFromTgcMesh(mesh);
                body.Friction = 0.5f;
                
                dynamicsWorld.AddRigidBody(body);
                bodies.Add(body);
    */
            });
            return bodies;
        }
        public CollisionObject AgregarPersonaje(TGCVector3 size)
        {
            main_character = CrearCollisionObject(size);
            main_character.CollisionFlags = main_character.CollisionFlags | CollisionFlags.CustomMaterialCallback;
            main_character.UserIndex = 1;
            collisionWorld.AddCollisionObject(main_character);
            return main_character;
        }
        public void EliminarObjeto(CollisionObject objeto)
        {
            collisionWorld.RemoveCollisionObject(objeto);
            objeto.Dispose();
        }

        public void Update()
        {
            //           collisionWorld.DebugDrawWorld();
            //           ChequearColisionesXwingConMisiles();
            //           ChequearColisionesTorretasConMisiles();
            collisionWorld.PerformDiscreteCollisionDetection();
            int numManifolds = collisionWorld.Dispatcher.NumManifolds;
            for (int i = 0; i < numManifolds; i++)
            {
                PersistentManifold contactManifold = collisionWorld.Dispatcher.GetManifoldByIndexInternal(i);
                CollisionObject obA = contactManifold.Body0;
                CollisionObject obB = contactManifold.Body1;
                contactManifold.RefreshContactPoints(obA.WorldTransform, obB.WorldTransform);
                int numContacts = contactManifold.NumContacts;
                for (int j = 0; j < numContacts; j++)
                {
                    //Get the contact information
                    ManifoldPoint pt = contactManifold.GetContactPoint(j);
                    double ptdist = pt.Distance;
                    if (Math.Abs(ptdist) < 3 && ( (obA.UserIndex == 1 && obB.UserIndex != 1)))
                    {
                        VariablesGlobales.vidas--;
                    }
                }
            }
        }
        
        public void Render(TgcD3dInput input)
        {

        }
        
        public void Dispose()
        {
            collisionWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
        }
        private CollisionObject CreateCollisionObjectFromTgcMesh(TgcMesh mesh)
        {
            var vertexCoords = mesh.getVertexPositions();

            TriangleMesh triangleMesh = new TriangleMesh();
            for (int i = 0; i < vertexCoords.Length; i = i + 3)
            {
                triangleMesh.AddTriangle(vertexCoords[i].ToBulletVector3(), vertexCoords[i + 1].ToBulletVector3(), vertexCoords[i + 2].ToBulletVector3());
            }

            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(0, 0, 0).ToBsMatrix;
            DefaultMotionState motionState = new DefaultMotionState(transformationMatrix);

            var bulletShape = new BvhTriangleMeshShape(triangleMesh, false);
            var collisionObject = new CollisionObject();
            collisionObject.CollisionShape = bulletShape;

            return collisionObject;
        }
        private CollisionObject CrearCollisionObject(TGCVector3 size)
        {
            CollisionObject collisionObject = new CollisionObject();
            BoxShape box2DShape = new BoxShape(CommonHelper.VectorXEscalar(size, 0.5f).ToBulletVector3());
            collisionObject.CollisionShape = box2DShape;
            return collisionObject;
        }
    }
}
