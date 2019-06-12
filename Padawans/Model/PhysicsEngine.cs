﻿using System;
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
        protected CollisionWorld collisionWorld;
        protected CollisionDispatcher dispatcher;
        protected DefaultCollisionConfiguration collisionConfiguration;
        protected SequentialImpulseConstraintSolver constraintSolver;
        protected BroadphaseInterface overlappingPairCache;

        RigidBody main_character;
        //Si algo tiene masa 0 es estatico.
        RigidBody misilActual;

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
            collisionWorld = new CollisionWorld(dispatcher, overlappingPairCache, collisionConfiguration);
            //
            dynamicsWorld.DebugDrawer = new Debug_Draw_Bullet();
            dynamicsWorld.DebugDrawer.DebugMode = DebugDrawModes.DrawWireframe;
        }

        public RigidBody AgregarMisilEnemigo(TGCVector3 size, float mass, TGCVector3 position, TGCVector3 rotation)
        {
            misilActual = AgregarObjeto(size, mass, position, rotation);
            return misilActual;
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

        private RigidBody AgregarObjeto(TGCVector3 size, float mass, TGCVector3 position, TGCVector3 rotation)
        {
            var personaje_body = BulletRigidBodyFactory.Instance.CreateBox(
                            size,
                            mass,
                            position,
                            rotation.Y, rotation.X, rotation.Z,
                            .5f, true);
            dynamicsWorld.AddRigidBody(personaje_body);
            personaje_body.SetCustomDebugColor(new BulletSharp.Math.Vector3(1, 0, 0));
            personaje_body.ActivationState = ActivationState.DisableDeactivation;
            return personaje_body;
        }

        public RigidBody AgregarPersonaje(TGCVector3 size,float mass,TGCVector3 position,TGCVector3 rotation)
            //solo pido los atributos basicos para el constructor, el resto lo debe definir el dueño del personaje
        {
            //new RigidBodyConstructionInfo(10, new DefaultMotionState(), shape...);
            main_character = AgregarObjeto(size, mass, position, rotation);
            main_character.CollisionFlags = main_character.CollisionFlags | CollisionFlags.CustomMaterialCallback;
            return main_character;
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
