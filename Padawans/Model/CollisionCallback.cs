using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    class NaveMisilCollisionCallback : ContactResultCallback
    {
        public override float AddSingleResult(ManifoldPoint cp, CollisionObjectWrapper colObj0Wrap, int partId0, int index0, CollisionObjectWrapper colObj1Wrap, int partId1, int index1)
        {
            int misilId = FastMath.Max(colObj0Wrap.CollisionObject.UserIndex, colObj1Wrap.CollisionObject.UserIndex);
            if (!VariablesGlobales.physicsEngine.CheckCollisionMisilXwing(misilId))
            {
                VariablesGlobales.physicsEngine.AgregarColisionConXwing(misilId);
                VariablesGlobales.vidas--;
                VariablesGlobales.physicsEngine.EliminarMisilDeLista(misilId);
            }
            return 0;
        }
    }
}
