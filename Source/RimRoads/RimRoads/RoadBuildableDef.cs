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
        public string FloatMenuString {
            get {
                string result = roadDef.LabelCap + " ";

                if(!costList.NullOrEmpty()) 
                {
                    result += costList[0];
                    foreach(var cost in costList.Skip(1))
                        result += ", " + cost;
                }

                if(!stuffCostList.NullOrEmpty()) 
                {
                    if(!costList.NullOrEmpty())
                        result += " " + "RimRoads_And".Translate() + " ";
                    result += stuffCostList[0];
                    foreach(var cost in stuffCostList.Skip(1))
                        result += ", " + cost;
                    result += " " + "RimRoads_Stuff".Translate();
                }

                return result;
            }
        }
    }
}
