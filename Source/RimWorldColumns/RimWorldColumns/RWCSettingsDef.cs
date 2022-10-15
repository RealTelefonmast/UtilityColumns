using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimWorldColumns
{
    public class RWCSettingsDef : Def
    {
        public List<ThingDef> columnsForRoomRequirement;
        public EffecterDef repairEffecter;
        public float repairColumnRange = 7.9f;
        public int repairThingCount = 1;
        public int repairHPAmount = 1;
        public float repairIntervalSeconds = 1.2f;

        public int RepairIntervalTicks => repairIntervalSeconds.SecondsToTicks();
    }
}
