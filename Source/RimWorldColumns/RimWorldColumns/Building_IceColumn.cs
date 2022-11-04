using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldColumns
{
    public class Building_IceColumn : BuildingWithOverlay
    {
        public CompTempControl compTempControl;
        public CompPowerTrader compPowerTrader;
        
        public float TempDiff => Mathf.Abs(compTempControl.targetTemperature + Position.GetTemperature(Map));
        public bool ShouldCool => Position.GetTemperature(Map) > compTempControl.targetTemperature;
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compTempControl = GetComp<CompTempControl>();
            this.compPowerTrader = GetComp<CompPowerTrader>();
        }

        public override void TickRare()
        {
            if (compPowerTrader.PowerOn)
            {
                float ambientTemperature = AmbientTemperature;
                float coolingEnergy;
                if (ambientTemperature > compTempControl.Props.defaultTargetTemperature)
                {
                    coolingEnergy = 1f;
                }
                else if (ambientTemperature < compTempControl.Props.minTargetTemperature)
                {
                    coolingEnergy = 0f;
                }
                else
                {
                    coolingEnergy = Mathf.InverseLerp(compTempControl.Props.minTargetTemperature, compTempControl.Props.defaultTargetTemperature, ambientTemperature);
                }
                
                float energyLimit = compTempControl.Props.energyPerSecond * coolingEnergy * 4.1666665f;
                float num2 = GenTemperature.ControlTemperatureTempChange(Position, Map, energyLimit, compTempControl.targetTemperature);
                bool isWorking = !Mathf.Approximately(num2, 0f);
                
                CompProperties_Power props = compPowerTrader.Props;
                if (isWorking)
                {
                    this.GetRoom().Temperature += num2;
                    compPowerTrader.PowerOutput = -props.PowerConsumption;
                }
                else
                {
                    compPowerTrader.PowerOutput = -props.PowerConsumption * compTempControl.Props.lowPowerConsumptionFactor;
                }
                compTempControl.operatingAtHighPower = isWorking;
            }
        }
    }
}