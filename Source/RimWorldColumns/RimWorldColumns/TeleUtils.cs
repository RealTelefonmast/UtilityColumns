using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimWorldColumns
{
    public static class TeleUtils
    {
        public static float AngleWrapped(float angle)
        {
            while (angle > 360)
            {
                angle -= 360;
            }
            while (angle < 0)
            {
                angle += 360;
            }
            return angle == 360 ? 0f : angle;
        }

        public static IEnumerable<IntVec3> SectorCells(IntVec3 center, Map map, float radius, float angle, float rotation, bool useCenter = false)
        {
            int cellCount = GenRadial.NumCellsInRadius(radius);
            int startCell = useCenter ? 0 : 1;
            var angleMin = AngleWrapped(rotation - angle * 0.5f);
            var angleMax = AngleWrapped(angleMin + angle);
            for (int i = startCell; i < cellCount; i++)
            {
                IntVec3 cell = GenRadial.RadialPattern[i] + center;
                float curAngle = (cell.ToVector3Shifted() - center.ToVector3Shifted()).AngleFlat();
                var invert = angleMin > angleMax;
                var flag = invert ? (curAngle >= angleMin || curAngle <= angleMax) : (curAngle >= angleMin && curAngle <= angleMax);
                if (!cell.InBounds(map) || !flag)
                    continue;
                yield return cell;
            }
        }
    }
}
