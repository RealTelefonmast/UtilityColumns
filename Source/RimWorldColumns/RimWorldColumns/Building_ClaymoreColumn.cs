using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldColumns;

[StaticConstructorOnStartup]
public class Building_ClaymoreColumn : BuildingWithOverlay, IFXObject
{
    //public List<IntVec3>[] Sectors = new List<IntVec3>[4];
    public List<ClaymoreCharge> Charges;

    public ColumnExtension Extension { get; private set; }
    public CompRefuelable RefuelComp { get; private set; }
    public CompPowerTrader PowerComp { get; private set; }
        
    public Vector3[] DrawPositions => new Vector3[1] { DrawPos };
    public bool[] DrawBools => new bool[1] { RefuelComp.HasFuel || Charges.Any(c => c.Detonating) };

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        Extension = def.GetModExtension<ColumnExtension>();
        RefuelComp = GetComp<CompRefuelable>();
        PowerComp = GetComp<CompPowerTrader>();

        if (Charges.NullOrEmpty())
            Charges = new List<ClaymoreCharge>
            {
                new(this, Rot4.North),
                new(this, Rot4.East),
                new(this, Rot4.South),
                new(this, Rot4.West)
            };
        Charges.ForEach(c => c.OnSpawn());

        //Sectors[0] = TeleUtils.SectorCells(Position, map, 5, 90, 0, false).ToList();
        //Sectors[1] = TeleUtils.SectorCells(Position, map, 5, 90, 90, false).ToList();
        //Sectors[2] = TeleUtils.SectorCells(Position, map, 5, 90, 180, false).ToList();
        //Sectors[3] = TeleUtils.SectorCells(Position, map, 5, 90, 270, false).ToList();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref Charges, "charges", LookMode.Deep);
    }

    public void ToggleSafety(Rot4 forDir)
    {

    }

    public override void Tick()
    {
        base.Tick();
        if (!PowerComp.PowerOn) return;
        foreach (var charge in Charges)
        {
            charge.Tick();
        }
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        foreach (var charge in Charges)
        {
            charge.DrawExtras();
        }
    }

    //TODO: Add Warning message
    public override string GetInspectString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(base.GetInspectString());
        string obstructedDirs = "";
        foreach (var charge in Charges)
        {
            if (charge.Obstructed(out _))
                obstructedDirs += (obstructedDirs.NullOrEmpty() ? "" : ", ") + charge.direction.ToStringHuman();
        }
        if(!obstructedDirs.NullOrEmpty())
            sb.AppendLine(ColoredText.Colorize("RWC_Obstructed".Translate(obstructedDirs), Color.red));
        return sb.ToString().TrimEndNewlines();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }



        yield return new Gizmo_ClaymoreSafetySettings()
        {
            claymoreReference = this,
        };
            
        if (DebugSettings.godMode)
        {
            yield return new Command_Action()
            {
                defaultLabel = "Detonate North",
                action = delegate
                {
                    Charges[0].Detonate();
                }
            };

            yield return new Command_Action()
            {
                defaultLabel = "Detonate All",
                action = delegate
                {
                    Charges[0].Detonate();
                    Charges[1].Detonate();
                    Charges[2].Detonate();
                    Charges[3].Detonate();
                }
            };
        }
            
    }
}