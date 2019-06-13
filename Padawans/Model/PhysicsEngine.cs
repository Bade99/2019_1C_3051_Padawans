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
        protected DiscreteDynamicsWorld dynamicsWorld;
        protected CollisionWorld collisionWorld;
        protected CollisionDispatcher dispatcher;
        protected DefaultCollisionConfiguration collisionConfiguration;
        protected SequentialImpulseConstraintSolver constraintSolver;
        protected BroadphaseInterface overlappingPairCache;
        private NaveMisilCollisionCallback collisionNaveMisilCallback;
        private Dictionary<int, RigidBody> listaMisiles;
        private List<int> listaIdMisilesQueColisionaronConXwing;
        private int misilIdCount = 20;
        private static readonly int ID_XWING = 1;

        RigidBody main_character;

        public PhysicsEngine()
        {
            collisionNaveMisilCallback = new NaveMisilCollisionCallback();
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, 0/*gravedad*/, 0).ToBulletVector3();
            collisionWorld = new CollisionWorld(dispatcher, overlappingPairCache, collisionConfiguration);
            //
            dynamicsWorld.DebugDrawer = new Debug_Draw_Bullet();
            dynamicsWorld.DebugDrawer.DebugMode = DebugDrawModes.DrawWireframe;
            listaMisiles = new Dictionary<int, RigidBody>();
            listaIdMisilesQueColisionaronConXwing = new List<int>();
        }

        public RigidBody AgregarMisilEnemigo(TGCVector3 size, float mass, TGCVector3 position, TGCVector3 rotation)
        {
            RigidBody misilActual = AgregarObjeto(size, mass, position, rotation);
            misilActual.UserIndex = misilIdCount;
            misilActual.CollisionFlags =  CollisionFlags.CustomMaterialCallback;
            listaMisiles.Add(misilIdCount, misilActual);
            misilIdCount++;
            return misilActual;
        }

        public void ChequearColisionesXwingConMisiles()
        {
            listaMisiles.Values.ToList<RigidBody>().ForEach((misil) =>
            {
                dynamicsWorld.ContactPairTest(misil, main_character, collisionNaveMisilCallback);
            });
        }

        public void EliminarMisilDeLista(int misilId)
        {
            listaMisiles.Remove(misilId);
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
                var body = CreateRigidBodyFromTgcMesh(mesh);
                body.Friction = 0.5f;
                //
                dynamicsWorld.AddRigidBody(body);
                bodies.Add(body);
            });
            return bodies;
        }

        private RigidBody AgregarObjeto(TGCVector3 size, float mass, TGCVector3 position, TGCVector3 rotation)
        {
            var objeto = BulletRigidBodyFactory.Instance.CreateBox(
                            size,
                            mass,
                            position,
                            rotation.Y, rotation.X, rotation.Z,
                            .5f, true);
            dynamicsWorld.AddRigidBody(objeto);
            objeto.SetCustomDebugColor(new BulletSharp.Math.Vector3(1, 0, 0));
            objeto.ActivationState = ActivationState.DisableDeactivation;
            return objeto;
        }

        public RigidBody AgregarPersonaje(TGCVector3 size,float mass,TGCVector3 position,TGCVector3 rotation)
        {
            main_character = AgregarObjeto(size, mass, position, rotation);
            main_character.CollisionFlags = CollisionFlags.CustomMaterialCallback;
            main_character.UserIndex = 1;
            return main_character;
        }
        public void EliminarObjeto(RigidBody body)
        {
            dynamicsWorld.RemoveRigidBody(body);
            body.Dispose();
        }

        public void Update()
        {
            dynamicsWorld.StepSimulation(1f/60f,5);
        }
        
        public void Render(TgcD3dInput input)
        {
            if (input.keyPressed(Microsoft.DirectX.DirectInput.Key.U))
            {
            dynamicsWorld.DebugDrawWorld();
            }
        }
        
        public void Dispose()
        {
            dynamicsWorld.Dispose();
            collisionWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
        }
        private RigidBody CreateRigidBodyFromTgcMesh(TgcMesh mesh)
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
            var boxLocalInertia = bulletShape.CalculateLocalInertia(0);

            var bodyInfo = new RigidBodyConstructionInfo(0, motionState, bulletShape, boxLocalInertia);
            var rigidBody = new RigidBody(bodyInfo);
            rigidBody.Friction = 0.4f;
            rigidBody.RollingFriction = 1;
            rigidBody.Restitution = 1f;

            return rigidBody;
        }
    }
}
