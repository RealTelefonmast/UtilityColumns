using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimWorldColumns
{
    [StaticConstructorOnStartup]
    public class Gizmo_ClaymoreSafetySettings : Gizmo
    {
        private static readonly Texture2D safetyOption = ContentFinder<Texture2D>.Get("Misc/SafetyOption", ShaderDatabase.Transparent);
        private static readonly Texture2D obstructedOption = ContentFinder<Texture2D>.Get("Misc/ObstructedOption", ShaderDatabase.Transparent);
        private static readonly Texture2D activateOption = ContentFinder<Texture2D>.Get("Misc/ActivateOption", ShaderDatabase.Transparent);

        public Building_ClaymoreColumn claymoreReference;

        public Gizmo_ClaymoreSafetySettings()
        {
            this.Order = -100;
        }

        private static Vector2 Size = new Vector2(160, 115);

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect mainRect = new Rect(topLeft.x, topLeft.y - (115 - 75), Size.x, Size.y);
            Find.WindowStack.ImmediateWindow(283947, mainRect, WindowLayer.GameUI, delegate
            {
                GUI.BeginGroup(mainRect.AtZero().ContractedBy(5));
                float curY = 0;
                DrawTabs(ref curY);
                curY += 5;
                DrawOptionFor(Rot4.North, ref curY);
                DrawOptionFor(Rot4.East, ref curY);
                DrawOptionFor(Rot4.South, ref curY);
                DrawOptionFor(Rot4.West, ref curY);
                GUI.EndGroup();

            }, true, false, 1);

            return new GizmoResult(GizmoState.Clear);
        }

        private void DrawTabs(ref float curY)
        {
            Rect dirLabel = new Rect(0, curY, 75, tabSize);
            Rect active     = new Rect(75, curY, tabSize, tabSize);
            Rect safety     = new Rect(100, curY, tabSize, tabSize);
            Rect obstructed = new Rect(125, curY, tabSize, tabSize);
            Widgets.Label(dirLabel, "RWC_Direction".Translate());
            Widgets.DrawTextureFitted(active, activateOption, 1);
            Widgets.DrawTextureFitted(safety, safetyOption, 1);
            Widgets.DrawTextureFitted(obstructed, obstructedOption, 1);

            //Tooltip
            Rect activeTooltip = new Rect(75, curY, tabSize, Size.y - 10);
            Rect safetyTooltip = new Rect(100, curY, tabSize, Size.y - 10);
            Rect obstructedTooltip = new Rect(125, curY, tabSize, Size.y - 10);
            TooltipHandler.TipRegion(activeTooltip, "RWC_ActiveDesc".Translate());
            TooltipHandler.TipRegion(safetyTooltip, "RWC_SafetyDesc".Translate());
            TooltipHandler.TipRegion(obstructedTooltip, "RWC_ObstructedDesc".Translate());

            curY += tabSize;
        }

        private static float tabSize = 20;
        private static float optionsSize = 20;
        private void DrawOptionFor(Rot4 direction, ref float curY)
        {
            Widgets.DrawHighlightIfMouseover(new Rect(0, curY, Size.x, optionsSize));
            Rect label = new Rect(0, curY, 75, tabSize);
            //Rect active = new Rect(125, curY, 25, 20);
            //Rect safety = new Rect(100, curY, 25, 20);
            //Rect obstructed = new Rect(75, curY, 25, 20);
            Vector2 active = new Vector2(75, curY);
            Vector2 safety = new Vector2(100, curY);
            Vector2 obstructed = new Vector2(125, curY);
            Text.Font = GameFont.Tiny;
            Widgets.Label(label, direction.ToStringHuman());
            Text.Font = GameFont.Small;
            Widgets.Checkbox(active, ref claymoreReference.Charges[direction.AsInt].settings[0], optionsSize);
            Widgets.Checkbox(safety, ref claymoreReference.Charges[direction.AsInt].settings[1], optionsSize);
            Widgets.Checkbox(obstructed, ref claymoreReference.Charges[direction.AsInt].settings[2], optionsSize);
            curY += optionsSize;
        }

        public override float GetWidth(float maxWidth)
        {
            return 160f;
        }
    }
}
