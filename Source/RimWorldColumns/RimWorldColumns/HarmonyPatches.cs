using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RimWorldColumns
{
    [StaticConstructorOnStartup]
    class Main
    {
        public static Pawn ChoicesForPawn = null;

        static Main()
        {
            var harmony = new Harmony("nephlite.orbitaltradecolumn");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            LongEventHandler.QueueLongEvent(new Action(Init), "LibraryStartup", false, null);
        }

        private static void Init()
        {
            // Vanilla Royalty Support
            if (ModLister.GetActiveModWithIdentifier("OskarPotocki.VanillaExpanded.RoyaltyPatches") != null)
            {
                bool patched = false;
                foreach (var d in DefDatabase<RoyalTitleDef>.AllDefs)
                {
                    if (d.bedroomRequirements != null)
                    {
                        foreach (var c in d?.bedroomRequirements)
                            if (c is RoomRequirement_ThingAnyOf rr && AddColumns(rr))
                                patched = true;
                    }
                    if (d.throneRoomRequirements != null)
                    {
                        foreach (var c in d?.throneRoomRequirements)
                            if (c is RoomRequirement_ThingAnyOf rr && AddColumns(rr))
                                patched = true;

                    }
                }
                if (patched)
                    Log.Message("[Utility Columns] Successfully patched Vanially Expanded - Royaltys Patch");
                else
                    Log.Error("[Utility Columns] Failed to patch Vanially Expanded - Royaltys Patch");
            }
        }

        private static bool AddColumns(RoomRequirement_ThingAnyOf rr)
        {
            if (rr.things.Contains(DefOf.Column))
            {
                rr.things.AddRange(ColumnDefs);
                return true;
            }
            return false;
        }

        public static IEnumerable<ThingDef> ColumnDefs => new List<ThingDef>()
        {
            DefOf.LightColumnMod,
            DefOf.DarkColumnMod,
            DefOf.OrbitalTradeColumnMod,
            DefOf.SunColumnMod,
            DefOf.IceColumnMod,
            DefOf.DetColumnMod,
            DefOf.FlameColumnMod,
            DefOf.GraveColumnMod,
        };
    }

    [RimWorld.DefOf]
    public static class DefOf
    {
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

    [HarmonyPatch(typeof(RoomRequirement_ThingCount), "Count")]
    static class Patch_RoomRequirement_ThingCount_Count
    {
        static void Postfix(RoomRequirement_ThingCount __instance, ref int __result, Room r)
        {
            if (__instance.thingDef == DefOf.Column)
            {
                foreach(var def in Main.ColumnDefs)
                    __result += r.ThingCount(def);
            }
        }
    }
}