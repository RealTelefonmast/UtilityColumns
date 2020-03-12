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
    public class ClaymoreCharge
    {
        private static readonly Material arrow = MaterialPool.MatFrom("Misc/DirectionArrow", ShaderDatabase.Transparent);
        private static readonly Material redArrow = MaterialPool.MatFrom("Misc/DirectionArrow", ShaderDatabase.Transparent, Color.red);
        public List<IntVec3> explosionCells;

        public Building_ClaymoreColumn parent;
        public Map map;
        public Rot4 direction;

        private IntVec3 center;
        private int ticksUntilDetonation = -1;

        public ClaymoreCharge(Building_ClaymoreColumn parent, Rot4 direction)
        {
            this.parent = parent;
            this.direction = direction;
            this.map = parent.Map;
            center = parent.Position;
            explosionCells = TeleUtils.SectorCells(center, map, parent.Extension.radius, 90f, direction.AsAngle, false).ToList();
        }

        public bool Obstructed => explosionCells.Any(c => c.InBounds(map) && c.GetFirstBuilding(map) is Building b && b.Faction == Faction.OfPlayer);
        public bool Available => !Obstructed && !Detonating && parent.RefuelComp.Fuel >= DetonationCost;
        public bool Detonating => ticksUntilDetonation >= 0;
        public float DetonationCost => (parent.Extension.consumptionPercent * parent.RefuelComp.Props.fuelCapacity);

        public float CurrentCost { get; set; }

        public void Tick()
        {
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

        private bool PawnInSector()
        {
            return explosionCells.Any(c => c.InBounds(map) && c.GetFirstPawn(map) is Pawn pawn && !pawn.Downed && pawn.HostileTo(Faction.OfPlayer));
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
            Explosion_Directed explosion = (Explosion_Directed)GenSpawn.Spawn(DefDatabase<ThingDef>.GetNamed("ClaymoreExplosion"), center, map, WipeMode.Vanish);
            explosion.radius = parent.Extension.radius;
            explosion.damType = parent.Extension.damageType ?? DamageDefOf.Bomb;
            explosion.instigator = parent;
            explosion.damAmount = (int)parent.Extension.explosionDamage;
            explosion.armorPenetration = 10;
            explosion.weapon = null;
            explosion.projectile = null;
            explosion.intendedTarget = null;
            explosion.preExplosionSpawnThingDef = null;
            explosion.preExplosionSpawnChance = 0;
            explosion.preExplosionSpawnThingCount = 0;
            explosion.postExplosionSpawnThingDef = null;
            explosion.postExplosionSpawnChance = 0;
            explosion.postExplosionSpawnThingCount = 0;
            explosion.applyDamageToExplosionCellsNeighbors = false;
            explosion.chanceToStartFire = 0;
            explosion.damageFalloff = false;
            explosion.needLOSToCell1 = null;
            explosion.needLOSToCell2 = null;
            explosion.PreStartExplosion(explosionCells);
            explosion.StartExplosion(null, null);
		}

        public void DrawExtras()
        {
            if (Obstructed) return;
            Graphics.DrawMesh(MeshPool.plane10, (center + direction.FacingCell).ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), direction.AsQuat, Detonating ? redArrow : arrow, 0);
        }
    }
}
