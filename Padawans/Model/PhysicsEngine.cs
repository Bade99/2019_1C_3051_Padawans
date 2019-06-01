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
{//@@@Hay que agregar el rigid body y dsps agregarlo como collision object e indicar con q colisiona!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public class PhysicsEngine //solo se usa para agregar meshes al world, e indicar atributos -> te devuelve el rigid body a usar, 
                        //cada entidad debe guardar su rigidbody para operar con él
    {
        protected DiscreteDynamicsWorld dynamicsWorld;
        protected CollisionDispatcher dispatcher;
        protected DefaultCollisionConfiguration collisionConfiguration;
        protected SequentialImpulseConstraintSolver constraintSolver;
        protected BroadphaseInterface overlappingPairCache;

        RigidBody main_character;
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
            dynamicsWorld.DebugDrawer = new Debug_Draw_Bullet();
            dynamicsWorld.DebugDrawer.DebugMode = DebugDrawModes.DrawWireframe;
        }

        public RigidBody AgregarCamara(TGCVector3 position)//@ o con constraint o con child shape
        {
            var camara_body = BulletRigidBodyFactory.Instance.CreateBall(1, 0, position);
            //camara_body.CollisionFlags |= CollisionFlags.KinematicObject;
            //camara_body.CollisionFlags |= CollisionFlags.NoContactResponse;// quiero q la cam no tenga colision
            dynamicsWorld.AddRigidBody(camara_body);
            //camara_body.ActivationState = ActivationState.DisableDeactivation;

            //var cam_main_character_constraint = new Point2PointConstraint(camara_body, main_character, camara_body.CenterOfMassPosition, main_character.CenterOfMassPosition);
            //@quiza el pivot de la camara deberia ser tmb el character?
            //dynamicsWorld.AddConstraint(cam_main_character_constraint, true);//deshabilité colision entre cam y personaje
            /*
            var slider = new SliderConstraint(main_character, camara_body, Matrix.Identity, Matrix.Identity, true)
            {
                LowerLinearLimit = -15.0f,
                UpperLinearLimit = -5.0f,
                //LowerLinearLimit = -10.0f,
                //UpperLinearLimit = -10.0f,
                LowerAngularLimit = -(float)Math.PI / 3.0f,
                UpperAngularLimit = (float)Math.PI / 3.0f,
                DebugDrawSize = 5.0f
            };
            dynamicsWorld.AddConstraint(slider, true);
            */
            return camara_body;
        }

        public List<RigidBody> AgregarEscenario(List<TgcMesh> meshesEscenario)
        {
            List<RigidBody> bodies = new List<RigidBody>();
            meshesEscenario.ForEach(mesh =>
            {
                var body = CreateRigidBodyFromTgcMesh(mesh);
                //atribs a recibir por param
                body.Friction = 0.5f;
                //
                dynamicsWorld.AddRigidBody(body);
                bodies.Add(body);
            });
            return bodies;
        }
        public RigidBody AgregarPersonaje(TGCVector3 size,float mass,TGCVector3 position,TGCVector3 rotation,float friction,bool inertia)
            //solo pido los atributos basicos para el constructor, el resto lo debe definir el dueño del personaje
        {
            //new RigidBodyConstructionInfo(10, new DefaultMotionState(), shape...);
            var personaje_body = BulletRigidBodyFactory.Instance.CreateBox(
                            size, 
                            mass,
                            position,
                            rotation.Y, rotation.X, rotation.Z,
                            friction,
                            inertia);
            dynamicsWorld.AddRigidBody(personaje_body);
            personaje_body.SetCustomDebugColor(new BulletSharp.Math.Vector3(1, 0, 0));
            personaje_body.ActivationState = ActivationState.DisableDeactivation;

            main_character = personaje_body;//me guardo el personaje ppal

            return personaje_body;
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
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
        }

        //Robado de tgc asi lo puedo modificar
        /// <summary>
        /// Se crea uncuerpo rigido a partir de un TgcMesh, pero no tiene masa por lo que va a ser estatico.
        /// </summary>
        /// <param name="mesh">TgcMesh</param>
        /// <returns>Cuerpo rigido de un Mesh</returns>
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
