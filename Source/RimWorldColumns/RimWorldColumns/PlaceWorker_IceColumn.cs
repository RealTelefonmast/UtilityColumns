using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorldColumns
{
    public class PlaceWorker_IceColumn : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Map currentMap = Find.CurrentMap;
            Room room = center.GetRoom(currentMap);
            if (room != null && !room.UsesOutdoorTemperature)
            {
                GenDraw.DrawFieldEdges(room.Cells.ToList(), GenTemperature.ColorRoomCold);
            }
        }
    }
}