using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;

namespace RimWorldColumns;

public class Explosion_Directed : Explosion
{
	private int startTick;
	private List<IntVec3> cellsToAffect;
	private List<Thing> damagedThings;
	private List<Thing> ignoredThings;
	private HashSet<IntVec3> addedCellsAffectedOnlyByDamage;
	private const float DamageFactorAtEdge = 0.2f;
	private static HashSet<IntVec3> temporaryCells = new HashSet<IntVec3>();

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad)
		{
			cellsToAffect = SimplePool<List<IntVec3>>.Get();
			cellsToAffect.Clear();
			damagedThings = SimplePool<List<Thing>>.Get();
			damagedThings.Clear();
			addedCellsAffectedOnlyByDamage = SimplePool<HashSet<IntVec3>>.Get();
			addedCellsAffectedOnlyByDamage.Clear();
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		base.DeSpawn(mode);
		cellsToAffect.Clear();
		SimplePool<List<IntVec3>>.Return(cellsToAffect);
		cellsToAffect = null;

		damagedThings.Clear();
		SimplePool<List<Thing>>.Return(damagedThings);
		damagedThings = null;

		addedCellsAffectedOnlyByDamage.Clear();
		SimplePool<HashSet<IntVec3>>.Return(addedCellsAffectedOnlyByDamage);
		addedCellsAffectedOnlyByDamage = null;
	}

	public void PreStartExplosion(List<IntVec3> explosionCells = null)
	{
		cellsToAffect.Clear();
		cellsToAffect.AddRange(explosionCells);
	}

	public override void StartExplosion(SoundDef explosionSound, List<Thing> ignoredThings)
	{
		if (!Spawned)
		{
			Log.Error("Called StartExplosion() on unspawned thing.");
			return;
		}
		startTick = Find.TickManager.TicksGame;
		this.ignoredThings = ignoredThings;
		damagedThings.Clear();
		addedCellsAffectedOnlyByDamage.Clear();
		//cellsToAffect.AddRange(damType.Worker.ExplosionCellsToHit(this));
		if (applyDamageToExplosionCellsNeighbors)
		{
			AddCellsNeighbors(cellsToAffect);
		}
		damType.Worker.ExplosionStart(this, this.cellsToAffect);
		PlayExplosionSound(explosionSound);
		FleckMaker.WaterSplash(DrawPos, Map, radius * 6f, 20f);
		cellsToAffect.Sort((IntVec3 a, IntVec3 b) => this.GetCellAffectTick(b).CompareTo(this.GetCellAffectTick(a)));
		RegionTraverser.BreadthFirstTraverse(base.Position, base.Map, (Region from, Region to) => true, delegate (Region x)
		{
			List<Thing> allThings = x.ListerThings.AllThings;
			for (int i = allThings.Count - 1; i >= 0; i--)
			{
				if (allThings[i].Spawned)
				{
					allThings[i].Notify_Explosion(this);
				}
			}
			return false;
		}, 25, RegionType.Set_Passable);
	}


	public override void Tick()
	{
		int ticksGame = Find.TickManager.TicksGame;
		int num = this.cellsToAffect.Count - 1;
		while (num >= 0 && ticksGame >= this.GetCellAffectTick(this.cellsToAffect[num]))
		{
			try
			{
				this.AffectCell(this.cellsToAffect[num]);
			}
			catch (Exception ex)
			{
				Log.Error($"Explosion could not affect cell {cellsToAffect[num]}: {ex}");
			}
			this.cellsToAffect.RemoveAt(num);
			num--;
		}
		if (!this.cellsToAffect.Any<IntVec3>())
		{
			this.Destroy(DestroyMode.Vanish);
		}
	}

	public int GetDamageAmountAt(IntVec3 c)
	{
		if (!damageFalloff) return damAmount;
		var t = c.DistanceTo(Position) / radius;
		return Mathf.Max(GenMath.RoundRandom(Mathf.Lerp(damAmount, damAmount * 0.2f, t)), 1);
	}

	public float GetArmorPenetrationAt(IntVec3 c)
	{
		if (!this.damageFalloff)
		{
			return this.armorPenetration;
		}
		float t = c.DistanceTo(base.Position) / this.radius;
		return Mathf.Lerp(this.armorPenetration, this.armorPenetration * 0.2f, t);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref radius, "radius");
		Scribe_Defs.Look<DamageDef>(ref damType, "damType");
		Scribe_Values.Look(ref damAmount, "damAmount");
		Scribe_Values.Look(ref armorPenetration, "armorPenetration");
		Scribe_References.Look<Thing>(ref instigator, "instigator");
		Scribe_Defs.Look<ThingDef>(ref weapon, "weapon");
		Scribe_Defs.Look<ThingDef>(ref projectile, "projectile");
		Scribe_References.Look<Thing>(ref intendedTarget, "intendedTarget");
		Scribe_Values.Look(ref applyDamageToExplosionCellsNeighbors, "applyDamageToExplosionCellsNeighbors");
		Scribe_Defs.Look<ThingDef>(ref preExplosionSpawnThingDef, "preExplosionSpawnThingDef");
		Scribe_Values.Look(ref preExplosionSpawnChance, "preExplosionSpawnChance");
		Scribe_Values.Look(ref preExplosionSpawnThingCount, "preExplosionSpawnThingCount", 1);
		Scribe_Defs.Look<ThingDef>(ref postExplosionSpawnThingDef, "postExplosionSpawnThingDef");
		Scribe_Values.Look(ref postExplosionSpawnChance, "postExplosionSpawnChance");
		Scribe_Values.Look(ref postExplosionSpawnThingCount, "postExplosionSpawnThingCount", 1);
		Scribe_Values.Look(ref chanceToStartFire, "chanceToStartFire");
		Scribe_Values.Look(ref damageFalloff, "dealMoreDamageAtCenter");
		Scribe_Values.Look(ref needLOSToCell1, "needLOSToCell1");
		Scribe_Values.Look(ref needLOSToCell2, "needLOSToCell2");
		Scribe_Values.Look(ref startTick, "startTick");
		Scribe_Collections.Look(ref cellsToAffect, "cellsToAffect", LookMode.Value, Array.Empty<object>());
		Scribe_Collections.Look(ref damagedThings, "damagedThings", LookMode.Reference, Array.Empty<object>());
		Scribe_Collections.Look(ref ignoredThings, "ignoredThings", LookMode.Reference, Array.Empty<object>());
		Scribe_Collections.Look(ref addedCellsAffectedOnlyByDamage, "addedCellsAffectedOnlyByDamage", LookMode.Value);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			damagedThings?.RemoveAll(x => x == null);
			ignoredThings?.RemoveAll(x => x == null);
		}
	}

	private int GetCellAffectTick(IntVec3 cell)
	{
		return startTick + (int)((cell - Position).LengthHorizontal * 1.5f);
	}

	private void AffectCell(IntVec3 c)
	{
		if (!c.InBounds(Map)) return;
		var flag = ShouldCellBeAffectedOnlyByDamage(c);
		if (!flag && Rand.Chance(preExplosionSpawnChance) && c.Walkable(Map))
			TrySpawnExplosionThing(preExplosionSpawnThingDef, c, preExplosionSpawnThingCount);
		damType.Worker.ExplosionAffectCell(this, c, damagedThings, ignoredThings, !flag);
		if (!flag && Rand.Chance(postExplosionSpawnChance) && c.Walkable(Map))
			TrySpawnExplosionThing(postExplosionSpawnThingDef, c, postExplosionSpawnThingCount);
		var num = chanceToStartFire;
		if (damageFalloff) num *= Mathf.Lerp(1f, 0.2f, c.DistanceTo(Position) / radius);
		if (Rand.Chance(num)) FireUtility.TryStartFireIn(c, Map, Rand.Range(0.1f, 0.925f), instigator);
	}

	private void TrySpawnExplosionThing(ThingDef thingDef, IntVec3 c, int count)
	{
		if (thingDef == null)
		{
			return;
		}
		if (thingDef.IsFilth)
		{
			FilthMaker.TryMakeFilth(c, base.Map, thingDef, count, FilthSourceFlags.None);
			return;
		}
		Thing thing = ThingMaker.MakeThing(thingDef, null);
		thing.stackCount = count;
		GenSpawn.Spawn(thing, c, base.Map, WipeMode.Vanish);
	}

	private void PlayExplosionSound(SoundDef explosionSound)
	{
		bool flag;
		if (Prefs.DevMode)
		{
			flag = (explosionSound != null);
		}
		else
		{
			flag = !explosionSound.NullOrUndefined();
		}
		if (flag)
		{
			explosionSound.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			return;
		}
		this.damType.soundExplosion.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
	}

	private void AddCellsNeighbors(List<IntVec3> cells)
	{
		Explosion_Directed.temporaryCells.Clear();
		this.addedCellsAffectedOnlyByDamage.Clear();
		for (int i = 0; i < cells.Count; i++)
		{
			Explosion_Directed.temporaryCells.Add(cells[i]);
		}
		for (int j = 0; j < cells.Count; j++)
		{
			if (cells[j].Walkable(base.Map))
			{
				for (int k = 0; k < GenAdj.AdjacentCells.Length; k++)
				{
					IntVec3 intVec = cells[j] + GenAdj.AdjacentCells[k];
					if (intVec.InBounds(base.Map) && Explosion_Directed.temporaryCells.Add(intVec))
					{
						this.addedCellsAffectedOnlyByDamage.Add(intVec);
					}
				}
			}
		}
		cells.Clear();
		foreach (IntVec3 item in Explosion_Directed.temporaryCells)
		{
			cells.Add(item);
		}
		Explosion_Directed.temporaryCells.Clear();
	}

	private bool ShouldCellBeAffectedOnlyByDamage(IntVec3 c)
	{
		return this.applyDamageToExplosionCellsNeighbors && this.addedCellsAffectedOnlyByDamage.Contains(c);
	}
}