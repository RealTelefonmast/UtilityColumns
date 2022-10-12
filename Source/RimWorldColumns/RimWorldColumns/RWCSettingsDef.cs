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
        public float repairColumnRange = 7.9f;
    }
}
