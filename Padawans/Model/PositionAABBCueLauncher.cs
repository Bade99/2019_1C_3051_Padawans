using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    class PositionAABBCueLauncher : ICueLauncher//con el cueManager actual no sirve tanto xq no pregunta a todos los cueLauncher a la vez, pero estoy en eso
    {
        ITarget target;
        TGCVector3 pos_min, pos_max;
        bool launched = false;

        public PositionAABBCueLauncher(ITarget target,TGCVector3 trigger_position, TGCVector3 trigger_size)//crea una caja
        {
            this.target = target;
            this.pos_min = trigger_position - CommonHelper.DividirPorEscalar(trigger_size,2f);
            this.pos_max = trigger_position + CommonHelper.DividirPorEscalar(trigger_size, 2f);
        }
        public bool IsReady()
        {
            if (!launched) launched = PointInAABB();
            return launched;
        }

        bool PointInAABB()
        {

            if (target.GetPosition().X < pos_min.X || target.GetPosition().X > pos_max.X) return false;
            if (target.GetPosition().Y < pos_min.Y || target.GetPosition().Y > pos_max.Y) return false;
            if (target.GetPosition().Z < pos_min.Z || target.GetPosition().Z > pos_max.Z) return false;
            return true;
            /*
            if (target_pos.X > pos_min.X && target_pos.X < pos_max.X &&
            target_pos.Y > pos_min.Y && target_pos.Y < pos_max.Y &&
            target_pos.Z > pos_min.Z && target_pos.Z < pos_max.Z)
            {
                return true;
            }
            return false;
            */
        }
    }
}
