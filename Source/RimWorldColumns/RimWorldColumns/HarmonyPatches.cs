using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace RimWorldColumns
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        public static RWCSettingsDef settingsDef;
        public static readonly List<ThingDef> extraColumns = new List<ThingDef>();

        static HarmonyPatches()
        {
            Log.Message("[Utility Columns] - Patching...");
            //Load..
            settingsDef = UCDefOf.ColumnSettings;
            extraColumns.Add(UCDefOf.Column);
            extraColumns.AddRange(settingsDef.columnsForRoomRequirement);

            var harmony = new Harmony("nephlite.orbitaltradecolumn");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            VEPatch();
            //LongEventHandler.QueueLongEvent(new Action(VEPatch), "LibraryStartup", false, null);
        }

        private static void VEPatch()
        {
            // Patch VE - RoyaltyPatches
            if (ModLister.GetActiveModWithIdentifier("OskarPotocki.VanillaExpanded.RoyaltyPatches") == null) return;
            foreach (var d in DefDatabase<RoyalTitleDef>.AllDefs)
            {
                if(d == null) continue;
                if (d.bedroomRequirements != null)
                {
                    foreach (var c in d.bedroomRequirements)
                    {
                        if (c is RoomRequirement_ThingAnyOf rr)
                            AddColumns(ref rr);
                    }
                }
                if (d.throneRoomRequirements != null)
                {
                    foreach (var c in d.throneRoomRequirements)
                    {
                        if (c is RoomRequirement_ThingAnyOf rr)
                            AddColumns(ref rr);
                    }

                }
            }
            
            /* 
            if (patched)
                Log.Message("[Utility Columns] Successfully patched Vanially Expanded - Royaltys Patch");
            else
                Log.Error("[Utility Columns] Failed to patch Vanially Expanded - Royaltys Patch");
            */
        }

        private static void AddColumns(ref RoomRequirement_ThingAnyOf rr)
        {
            if (rr.things.Contains(UCDefOf.Column))
            {
                rr.things.AddRange(extraColumns);
            }
        }
    }

    [HarmonyPatch(typeof(RoomRequirement_ThingCount), "Count")]
    static class Patch_RoomRequirement_ThingCount_Count
    {
        static void Postfix(RoomRequirement_ThingCount __instance, ref int __result, Room r)
        {
            if (__instance.thingDef != UCDefOf.Column) return;

            __result += HarmonyPatches.extraColumns.Sum(r.ThingCount);
        }
    }
}