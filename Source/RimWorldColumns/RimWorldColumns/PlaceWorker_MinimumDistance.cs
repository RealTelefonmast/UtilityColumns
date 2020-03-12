using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimWorldColumns
{
    public class PlaceWorker_MinimumDistance : PlaceWorker
    {
        public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
        {
            base.PostPlace(map, def, loc, rot);
            var radialCells = GenRadial.RadialCellsAround(loc, def.GetModExtension<ColumnExtension>()?.radius ?? 0, false);
            if (radialCells.Any(c => c.InBounds(map) && c.GetThingList(map).Any(t => t is Building && t.Faction == Faction.OfPlayer)))
                Messages.Message("RWC_TooClose".Translate(), MessageTypeDefOf.CautionInput, false);
        }
    }
}
