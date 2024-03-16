using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldColumns
{
    [StaticConstructorOnStartup]
    public class Building_RepairColumn : BuildingWithOverlay, IFXObject
    {       
        private static readonly Material RepairMat = MaterialPool.MatFrom("Misc/NeedRepair", ShaderDatabase.Transparent);

        //
        public Vector3[] DrawPositions => new[] {DrawPos, DrawPos, DrawPos};
        public bool[] DrawBools => new[] {true, true, true};
        
        //
        private IntVec3[] _areaCells;

        private IntVec3[] AreaCells
        {
            get
            {
                if (_areaCells == null)
                {
                    _areaCells = GenRadial.RadialCellsAround(Position, UCDefOf.ColumnSettings.repairColumnRange, false).ToArray();
                }
                return _areaCells;
            }
        }

        public CompRefuelable RefuelComp { get; private set; }
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            RefuelComp = this.GetComp<CompRefuelable>();
        }

        public override void Tick()
        {
            base.Tick();
            if (RefuelComp.HasFuel && this.IsHashIntervalTick(UCDefOf.ColumnSettings.RepairIntervalTicks))
            {
                RepairBuildingsInRange();
            }
        }

        private Dictionary<Thing, Effecter> repairEffects = new Dictionary<Thing, Effecter>();

        private void StartEffectFor(Thing building, out Effecter effecterRes)
        {
            effecterRes = null;
            if (!repairEffects.TryGetValue(building, out var effecter))
            {
                effecter = UCDefOf.ColumnSettings.repairEffecter.Spawn();
                repairEffects.Add(building, effecter);
            }
            effecterRes = effecter;
        }

        private void EndEffectFor(Thing building)
        {
            if (repairEffects.TryGetValue(building, out var effecter))
            {
                effecter.Cleanup();
                repairEffects.Remove(building);
            }
        }
        
        private void RepairBuildingsInRange()
        {
            var repairables = Repairables().TakeRandom(10);
            var enumerable = repairables as Thing[] ?? repairables.ToArray();
            if (!enumerable.Any()) return;
            
            //
            foreach (var building in enumerable)
            {
                var hp = Mathf.CeilToInt(UCDefOf.ColumnSettings.repairHPAmount); //building.def.BaseMaxHitPoints *
                hp = (int) Mathf.Min(hp, RefuelComp.Fuel);
                if (hp == 0) continue;
                    
                StartEffectFor(building, out var effecter);
                building.HitPoints = Mathf.Min(building.HitPoints + hp, building.MaxHitPoints);
                RefuelComp.ConsumeFuel(hp);

                //
                for (int i = 0; i <= 10; i++)
                {
                    effecter.EffectTick(building,building);
                }
                    
                if(building.HitPoints == building.MaxHitPoints)
                    EndEffectFor(building);
            }
        }

        private List<Thing> Repairables()
        {
            var list = new List<Thing>();
            for (int i = 0; i < AreaCells.Length; i++)
            {
                var cell = AreaCells[i];
                if (!cell.InBounds(Map)) continue;
                var b = cell.GetItems(Map);
                foreach (var t in b)
                {
                    if(t == null) continue;
                    //if(t.def.equipmentType == EquipmentType.None) continue;
                    if (t.def.IsWeapon || t.def.IsApparel)
                    {
                        if (t.IsInValidStorage())
                        {
                            if (t.HitPoints < t.MaxHitPoints)
                                list.Add(t);
                        }
                    }
                }
            }

            return list;
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            var repairables = Repairables();
            if (repairables.Any())
            {
                foreach (var building in repairables)
                {
                    float num = (Time.realtimeSinceStartup + 397f * (float) (building.thingIDNumber % 571)) * 4f;
                    float num2 = ((float) Math.Sin((double) num) + 1f) * 0.5f;
                    num2 = 0.3f + num2 * 0.7f;
                    Material material = FadedMaterialPool.FadedVersionOf(RepairMat, num2);
                    var c = building.TrueCenter();
                    Graphics.DrawMesh(MeshPool.plane08, new Vector3(c.x, AltitudeLayer.MetaOverlays.AltitudeFor(), c.z), Quaternion.identity, material, 0);
                }
            }
        }

        private void MakeMatchingStockpile()
        {
            Designator des = DesignatorUtility.FindAllowedDesignator<Designator_ZoneAddStockpile_Resources>();
            des.DesignateMultiCell(from c in _areaCells where des.CanDesignateCell(c).Accepted select c);
        }
        
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            
            if (DesignatorUtility.FindAllowedDesignator<Designator_ZoneAddStockpile_Resources>() != null)
            {
                yield return new Command_Action
                {
                    action = MakeMatchingStockpile,
                    hotKey = KeyBindingDefOf.Misc1,
                    defaultDesc = "CommandMakeBeaconStockpileDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Stockpile", true),
                    defaultLabel = "CommandMakeBeaconStockpileLabel".Translate()
                };
            }
        }
    }
}