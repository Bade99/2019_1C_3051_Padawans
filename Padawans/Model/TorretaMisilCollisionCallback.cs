using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    class TorretaMisilCollisionCallback : ContactResultCallback
    {
        public override float AddSingleResult(ManifoldPoint cp, CollisionObjectWrapper colObj0Wrap, int partId0, int index0, CollisionObjectWrapper colObj1Wrap, int partId1, int index1)
        {
            int torretaId = colObj1Wrap.CollisionObject.UserIndex;
            VariablesGlobales.physicsEngine.DisminuirVidaTorreta(torretaId);
            return 0;
        }
    }
}
