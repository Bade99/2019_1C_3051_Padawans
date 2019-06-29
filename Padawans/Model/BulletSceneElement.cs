using BulletSharp;
using BulletSharp.Math;
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
        protected CollisionObject collisionObject;
        protected TgcMesh[] meshs;
        protected TGCVector3 rotation;
        protected TGCVector3 position;
        protected TGCMatrix matrizInicialTransformacion = TGCMatrix.Identity;
        protected TGCVector3 scaleVector;
        protected CoordenadaEsferica coordenadaEsferica;
        public TGCVector3 coordenadaDireccionCartesiana;
        protected float velocidadGeneral;

        public void UpdateBullet()
        {
            coordenadaDireccionCartesiana = coordenadaEsferica.GetXYZCoord();
            position.X = position.X + velocidadGeneral * coordenadaDireccionCartesiana.X * VariablesGlobales.elapsedTime;
            position.Y = position.Y + velocidadGeneral * coordenadaDireccionCartesiana.Y * VariablesGlobales.elapsedTime;
            position.Z = position.Z + velocidadGeneral * coordenadaDireccionCartesiana.Z * VariablesGlobales.elapsedTime;
            TGCMatrix rotationTranslation = matrizInicialTransformacion * GetRotationMatrix() * TGCMatrix.Translation(position);
            for (int a=0;a<meshs.Length;a++)
            {
                meshs[a].Transform = TGCMatrix.Scaling(scaleVector) * rotationTranslation;
            }
            collisionObject.CollisionShape.LocalScaling = scaleVector.ToBulletVector3();
            collisionObject.WorldTransform = CommonHelper.TgcToBulletMatrix(rotationTranslation);
        }
        protected TGCMatrix GetRotationMatrix()
        {
            return
                TGCMatrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        }
    }
}
