﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldColumns
{
    public class CompFX : ThingComp
    {
        private List<FXGraphic> Graphics = new List<FXGraphic>();

        public CompProperties_Overlays Props => (CompProperties_Overlays)base.props;

        public bool IsPowered => parent.GetComp<CompPowerTrader>()?.PowerOn ?? true;

        public IFXObject FXParent => parent as IFXObject;


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            for (var index = 0; index < Props.overlays.Count; index++)
            {
                var fxGraphicData = Props.overlays[index];
                Graphics.Add(new FXGraphic(this, fxGraphicData, index));
            }
        }

        public bool CanDraw(int index)
        {
            if (Graphics[index].data.needsPower && !IsPowered)
                return false;
            return FXParent?.DrawBools[index] ?? true;
        }

        public Vector3 DrawPos(int index)
        {
            return FXParent?.DrawPositions[index] ?? parent.DrawPos;
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (!parent.Spawned) return;
            if (signal == "PowerTurnedOn" || signal == "PowerTurnedOff" || signal == "FlickedOn" || signal == "FlickedOff" || signal == "Refueled" || signal == "RanOutOfFuel" || signal == "ScheduledOn" || signal == "ScheduledOff")
            {
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
            }
        }

        public override void PostPrintOnto(SectionLayer layer)
        {
            base.PostPrintOnto(layer);
            for (var index = 0; index < Graphics.Count; index++)
            {
                var graphic = Graphics[index];
                if (CanDraw(index))
                    graphic.Graphic.Print(layer, parent);//Print(layer, DrawPos(index), parent.Rotation);
            }
        }
    }

    public class CompProperties_Overlays : CompProperties
    {
        public List<FXGraphicData> overlays;

        public CompProperties_Overlays()
        {
            compClass = typeof(CompFX);
        }
    }

    public interface IFXObject
    {
        Vector3[] DrawPositions { get; }
        bool[] DrawBools { get; }
    }

    public class FXGraphic
    {
        private CompFX parentComp;
        private Graphic graphicInt;
        public FXGraphicData data;
        public float altitude;
        public int index = 0;

        public FXGraphic(CompFX parent, FXGraphicData data, int index)
        {
            this.parentComp = parent;
            this.data = data;
            this.index = index;

            altitude = data.altitude?.AltitudeFor() ?? (parent.parent.def.altitudeLayer.AltitudeFor() + (0.125f * (index + 1)));
        }

        public Graphic Graphic
        {
            get
            {
                if (graphicInt == null)
                {
                    graphicInt = data.data.Graphic;
                }
                return graphicInt;
            }
        }

        public void Print(SectionLayer layer, Vector3 drawPos, Rot4 rot)
        {
            //var info = new GraphicDrawInfo(Graphic, drawPos, rot, ((FXThingDef)parent.def).extraData, parent.def);
            Printer_Plane.PrintPlane(layer, new Vector3(drawPos.x, altitude, drawPos.z), data.data.drawSize, Graphic.MatAt(rot));
        }
    }

    public class FXGraphicData
    {
        public GraphicData data;
        public AltitudeLayer? altitude = null;
        public bool needsPower = false;

        public Graphic Graphic()
        {
            return GraphicDatabase.Get(data.graphicClass, data.texPath, data.shaderType.Shader, data.drawSize, data.color, data.colorTwo);
        }
    }
}
