using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldColumns
{
    [StaticConstructorOnStartup]
    public class ClaymoreCharge : IExposable
    {
        private static readonly Material obstructed = MaterialPool.MatFrom("Misc/Obstructed", ShaderDatabase.Transparent);
        private static readonly Material arrow = MaterialPool.MatFrom("Misc/DirectionArrow", ShaderDatabase.Transparent);
        private static readonly Material redArrow = MaterialPool.MatFrom("Misc/DirectionArrow", ShaderDatabase.Transparent, Color.red);
        public List<IntVec3> explosionCells;

        public Building_ClaymoreColumn parent;
        public Rot4 direction;

        private int ticksUntilDetonation = -1;

        private Map Map => parent.Map;
        private IntVec3 Center => parent.Position;

        //activity, safety, obstructions
        public bool[] settings = new []{true, true, true};

        public ClaymoreCharge(){}

        public ClaymoreCharge(Building_ClaymoreColumn parent, Rot4 direction)
        {
            this.parent = parent;
            this.direction = direction;
            explosionCells = TeleUtils.SectorCells(Center, Map, parent.Extension.radius, 90f, direction.AsAngle, false).ToList();
        }

        public void ExposeData()
        {
            DataExposeUtility.LookBoolArray(ref settings, 3, "settingBools");
            Scribe_Values.Look(ref ticksUntilDetonation, "ticksUntilDet");
            Scribe_Values.Look(ref direction, "claymoreDir");
            Scribe_References.Look(ref parent, "claymoreParent");
        }

        public void OnSpawn()
        {
            explosionCells = TeleUtils.SectorCells(Center, Map, parent.Extension.radius, 90f, direction.AsAngle, false).ToList();
        }

        public bool Obstructed(out IEnumerable<Thing> obstructions)
        {
            obstructions = null;
            if (!parent.Spawned) return true;
            if (!UsesSafety) return false;
            obstructions = explosionCells.Where(v => v.InBounds(Map)).Select(c => c.GetFirstBuilding(Map)).Where(b => b?.Faction == Faction.OfPlayer);
            return obstructions.Any(); //explosionCells.Any(c => c.InBounds(map) && c.GetFirstBuilding(map) is Building b && b.Faction == Faction.OfPlayer);
        }

        public bool ShowObstructions => settings[2];
        public bool UsesSafety => settings[1];
        public bool IsActive => settings[0];
        public bool Available => !Obstructed(out _) && !Detonating && parent.RefuelComp.Fuel >= DetonationCost;
        public bool Detonating => ticksUntilDetonation >= 0;
        public float DetonationCost => (parent.Extension.consumptionPercent * parent.RefuelComp.Props.fuelCapacity);

        public float CurrentCost { get; set; }

        private Pawn CurrentTarget = null;

        public void Tick()
        {
            if (!IsActive) return;
            if (Detonating)
            {
                if (ticksUntilDetonation == 0)
                {
                    Detonate();
                }
                ticksUntilDetonation--;
            }
            if(Available && PawnInSector())
                TriggerCharge();

        }

        public void ToggleActive()
        {
            settings[0] = !settings[0];
        }

        public void ToggleSafety()
        {
            settings[1] = !settings[1];
        }

        public void ToggleObstructions()
        {
            settings[2] = !settings[2];
        }

        private bool PawnInSector()
        {
            foreach (var cell in explosionCells)
            {
                if(!cell.InBounds(Map)) continue;
                var pawn = cell.GetFirstPawn(Map);
                if(pawn == null) continue;
                if (!pawn.Downed && pawn.HostileTo(Faction.OfPlayer))
                {
                    CurrentTarget = pawn;
                    return true;
                } 
            }
            return false;
        }

        private void TriggerCharge()
        {
            if (!Detonating)
            {
                ticksUntilDetonation = parent.Extension.detonationDelay;
                parent.RefuelComp.ConsumeFuel(DetonationCost);
            }
        }

        public void Detonate()
        {
            Explosion_Directed explosion = (Explosion_Directed)GenSpawn.Spawn(DefDatabase<ThingDef>.GetNamed("ClaymoreExplosion"), Center, Map, WipeMode.Vanish);
            explosion.radius = parent.Extension.radius;
            explosion.damType = parent.Extension.damageType ?? DamageDefOf.Bomb;
            explosion.instigator = parent;
            explosion.damAmount = (int)parent.Extension.explosionDamage;
            explosion.armorPenetration = (float)explosion.damAmount * 0.015f;
            explosion.weapon = null;
            explosion.projectile = null;
            explosion.intendedTarget = CurrentTarget;
            explosion.preExplosionSpawnThingDef = parent.Extension.preExplosionSpawnThingDef;
            explosion.preExplosionSpawnChance = parent.Extension.preSpawnChance;
            explosion.preExplosionSpawnThingCount = 1;
            explosion.postExplosionSpawnThingDef = parent.Extension.postExplosionSpawnThingDef;
            explosion.postExplosionSpawnChance = parent.Extension.postSpawnChance;
            explosion.postExplosionSpawnThingCount = 1;
            explosion.applyDamageToExplosionCellsNeighbors = false;
            explosion.chanceToStartFire = parent.Extension.chanceToStartFire;
            explosion.damageFalloff = true;
            explosion.needLOSToCell1 = null;
            explosion.needLOSToCell2 = null;
            explosion.PreStartExplosion(explosionCells);
            explosion.StartExplosion(null, null);

            CurrentTarget = null;
        }

        public void DrawExtras()
        {
            if (!IsActive) return;
            if (ShowObstructions && Obstructed(out var obstructions))
            {
                foreach (var obstruction in obstructions)
                {
                    Material mat = obstructed;
                    float num = (Time.realtimeSinceStartup + 397f * (float)(obstruction.thingIDNumber % 571)) * 4f;
                    float num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
                    num2 = 0.3f + num2 * 0.7f;
                    Material material = FadedMaterialPool.FadedVersionOf(mat, num2);
                    var c = obstruction.TrueCenter();
                    Graphics.DrawMesh(MeshPool.plane08, new Vector3(c.x, AltitudeLayer.MetaOverlays.AltitudeFor(), c.z), Quaternion.identity, material, 0);
                }
            }
            else
            {
                Graphics.DrawMesh(MeshPool.plane10, (Center + direction.FacingCell).ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), direction.AsQuat, Detonating ? redArrow : arrow, 0);
            }
        }
    }
}
