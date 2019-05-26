using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using TGC.Core.BulletPhysics;
using TGC.Core.Mathematica;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    class PhysicsEngine //solo se usa para agregar meshes al world, e indicar atributos -> te devuelve el rigid body a usar, 
                        //cada entidad debe guardar su rigidbody para operar con él
    {
        protected DiscreteDynamicsWorld dynamicsWorld;
        protected CollisionDispatcher dispatcher;
        protected DefaultCollisionConfiguration collisionConfiguration;
        protected SequentialImpulseConstraintSolver constraintSolver;
        protected BroadphaseInterface overlappingPairCache;

        //Si algo tiene masa 0 es estatico.

        public PhysicsEngine()
        {
            //escenario = new List<TgcMesh>();
            //base para bullet
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            overlappingPairCache = new DbvtBroadphase(); //AxisSweep3(new BsVector3(-5000f, -5000f, -5000f), new BsVector3(5000f, 5000f, 5000f), 8192);
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, constraintSolver, collisionConfiguration);
            dynamicsWorld.Gravity = new TGCVector3(0, 0/*gravedad*/, 0).ToBulletVector3();
            //
            
        }
        public List<RigidBody> AgregarEscenarios(List<TgcMesh> meshesEscenario)
        {
            List<RigidBody> bodies = new List<RigidBody>();
            meshesEscenario.ForEach(mesh =>
            {
                var body = BulletRigidBodyFactory.Instance.CreateRigidBodyFromTgcMesh(mesh);
                //atribs a recibir por param
                body.Friction = 0.5f;
                //
                dynamicsWorld.AddRigidBody(body);
                bodies.Add(body);
            });
            return bodies;
        }
        public RigidBody AgregarPersonajePrincipal(TgcMesh personaje_ppal)//solo recibo uno de los dos meshes del xwing, dsps vemos si necesito el otro
        {
            //new RigidBodyConstructionInfo(10, new DefaultMotionState(), shape...);
            var personaje = BulletRigidBodyFactory.Instance.CreateRigidBodyFromTgcMesh(personaje_ppal);
            //atributos q luego recibirá la funcion
            personaje.SetDamping(0.1f, 0f);
            personaje.Restitution = 0.1f;
            personaje.Friction = 1;
            //
            dynamicsWorld.AddRigidBody(personaje);
            return personaje;
        }

        public void Update()
        {
            dynamicsWorld.StepSimulation(VariablesGlobales.elapsedTime,1);
        }
        public void Render()
        {

        }
        public void Dispose()
        {
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
        }
    }
}
