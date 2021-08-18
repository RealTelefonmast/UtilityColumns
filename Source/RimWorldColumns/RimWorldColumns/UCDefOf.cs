using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimWorldColumns
{
    [DefOf]
    public static class UCDefOf
    {
        public static RWCSettingsDef ColumnSettings;

        public static ThingDef LightColumnMod;
        public static ThingDef DarkColumnMod;
        public static ThingDef OrbitalTradeColumnMod;
        public static ThingDef SunColumnMod;
        public static ThingDef IceColumnMod;
        public static ThingDef DetColumnMod;
        public static ThingDef FlameColumnMod;
        public static ThingDef GraveColumnMod;

        // Base game column def
        public static ThingDef Column;
    }
}
