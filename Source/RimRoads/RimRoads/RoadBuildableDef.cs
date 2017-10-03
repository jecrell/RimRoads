using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace RimRoads
{
    public class RoadBuildableDef : JecsTools.WorldObjectRecipeDef
    {
        public RoadDef roadDef;
        public override Def FinishedThing => roadDef;
    }
}
