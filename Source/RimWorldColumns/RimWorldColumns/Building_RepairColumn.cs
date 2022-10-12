using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldColumns
{
    public class Building_RepairColumn : BuildingWithOverlay, IFXObject
    {       
        private static readonly Material RepairMat = MaterialPool.MatFrom("Misc/NeedRepair", ShaderDatabase.Transparent);
        
        //
        public Vector3[] DrawPositions => new[] {DrawPos};
        public bool[] DrawBools => new[] {true};
        
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
            if (RefuelComp.HasFuel && this.IsHashIntervalTick(100))
            {
                RepairBuildingsInRange();
            }
        }

        private Dictionary<Building, Effecter> repairEffects = new Dictionary<Building, Effecter>();

        private void StartEffectFor(Building building)
        {
            if (!repairEffects.TryGetValue(building, out var effecter))
            {
                effecter = building.def.repairEffect.Spawn();
                repairEffects.Add(building, effecter);
            }
            effecter.EffectTick(null,building);
        }

        private void EndEffectFor(Building building)
        {
            if (repairEffects.TryGetValue(building, out var effecter))
            {
                effecter.Cleanup();
                repairEffects.Remove(building);
            }
        }
        
        private void RepairBuildingsInRange()
        {
            var repairables = Repairables();
            if (repairables.Any())
            {
                foreach (var building in repairables)
                {
                    var hp = (int) (building.def.BaseMaxHitPoints * 0.01f);
                    if (RefuelComp.Fuel <= hp) continue;
                    
                    StartEffectFor(building);
                    building.HitPoints = Mathf.Min(building.HitPoints + hp, building.MaxHitPoints);
                    RefuelComp.ConsumeFuel(hp);
                    
                    if(building.HitPoints == building.MaxHitPoints)
                        EndEffectFor(building);
                }
            }
        }

        public IEnumerable<Building> Repairables()
        {
            List<Building> building = new List<Building>();
            for (int i =0; i < AreaCells.Length; i++)
            {
                var cell = AreaCells[i];
                var b = cell.GetFirstBuilding(Map);
                if(b == null) continue;
                if(!b.Position.InBounds(Map)) continue;
                if(!b.def.BuildableByPlayer) continue;
                if(!b.def.BuildableByPlayer) continue;
                if (b.HitPoints < b.MaxHitPoints)
                    yield return b;
            }
        }


        public override void Draw()
        {
            base.Draw();
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
    }
}