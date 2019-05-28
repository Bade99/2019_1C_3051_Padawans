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
    public class PhysicsEngine //solo se usa para agregar meshes al world, e indicar atributos -> te devuelve el rigid body a usar, 
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
        public List<RigidBody> AgregarEscenario(List<TgcMesh> meshesEscenario)
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
        public RigidBody AgregarPersonaje(TGCVector3 size,float mass,TGCVector3 position,TGCVector3 rotation,float friction, float linDamping,float angDamping,float restitution,bool inertia)//solo recibo uno de los dos meshes del xwing, dsps vemos si necesito el otro
        {
            //new RigidBodyConstructionInfo(10, new DefaultMotionState(), shape...);
            var personaje_body = BulletRigidBodyFactory.Instance.CreateBox(
                            size, 
                            mass,
                            position,
                            rotation.Y, rotation.X, rotation.Z,
                            friction,
                            inertia);
            //+ atributos
            personaje_body.SetDamping(linDamping,angDamping);
            personaje_body.Restitution = restitution;
            //
            dynamicsWorld.AddRigidBody(personaje_body);
            personaje_body.ActivationState = ActivationState.ActiveTag;
            personaje_body.AngularVelocity = rotation.ToBulletVector3();
            //personaje_body.ApplyCentralForce(new BulletSharp.Math.Vector3(0, 0, -10)*5);
            return personaje_body;
        }

        public void Update()
        {
            dynamicsWorld.StepSimulation(VariablesGlobales.elapsedTime,10);
        }
        /*
        public void Render()
        {

        }
        */
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
