using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    public abstract class BulletSceneElement : SceneElement
    {
        protected RigidBody body;
        protected TGCVector3 bulletVelocity;
        protected TgcMesh[] meshs;
        protected TGCVector3 rotation;
        protected TGCMatrix matrizInicialTransformacion;

        public void UpdateBullet()
        {
            body.LinearVelocity = bulletVelocity.ToBulletVector3();
            TGCMatrix bullet_transform = new TGCMatrix(body.InterpolationWorldTransform);
            for (int a=0;a<meshs.Length;a++)
            {
                meshs[a].Transform = matrizInicialTransformacion * GetRotationMatrix() * bullet_transform;
            }
        }
        protected TGCMatrix GetRotationMatrix()
        {
            return
                TGCMatrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        }
    }
}
