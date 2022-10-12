using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimWorldColumns
{
    public class DamageWorker_Claymore : DamageWorker_AddInjury
    {
        public override IEnumerable<IntVec3> ExplosionCellsToHit(IntVec3 center, Map map, float radius, IntVec3? needLOSToCell1 = null, IntVec3? needLOSToCell2 = null, FloatRange? affectedAngle = null)
        {
            return base.ExplosionCellsToHit(center, map, radius, needLOSToCell1, needLOSToCell2);
        }
    }
}
