using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimWorldColumns
{
    public class ColumnExtension : DefModExtension
    {
        public float radius = 4.9f;
        public float consumptionPercent = 0.25f;
        public float explosionDamage = 20f;
        public int detonationDelay = 50;
        public DamageDef damageType;
    }
}
