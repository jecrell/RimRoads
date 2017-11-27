using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;
using JecsTools;
using Verse.AI;

namespace RimRoads
{
    public class CaravanJobDriver_DeconstructRoad : CaravanJobDriver
    {
        private float workLeft = -1000f;
        public int BaseWorkAmount => 4000; //DefToCheck.workToMake;
        public StatDef SpeedStat => StatDefOf.ConstructionSpeed;
        //public RoadBuildableDef DefToCheck => ((RoadBlueprint)TargetA.WorldObject).RoadType;

        public CaravanJobDriver_DeconstructRoad() { }
        public void DoEffect()
        {
            Find.WorldGrid[this.TargetA.Tile].roads = null;
            Find.World.renderer.RegenerateAllLayersNow();
            Messages.Message("RimRoads_DeconstructionFinished".Translate(new object[] { this.GetActor().Label }), new GlobalTargetInfo(this.GetActor()), MessageTypeDefOf.PositiveEvent); //MessageSound.Benefit);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.workLeft, "workLeft", -1000);
        }

        public override IEnumerable<CaravanToil> MakeNewToils()
        {
            yield return new CaravanToil()
            {
                initAction = delegate
                {
                    //Log.Message("WorkStarted");
                }
            };
            //yield return CaravanToils_Goto.GotoObject(Verse.AI.TargetIndex.A);
            CaravanToil nextToil = new CaravanToil()
            {
                initAction = delegate
                {
                    workLeft = BaseWorkAmount;
                },
                tickAction = delegate
                {
                    if (!this.caravan.Resting)
                    {
                        //Log.Message(this.workLeft.ToString());
                        float num = (SpeedStat == null) ? 1f : JecsTools.CaravanJobsUtility.GetStatValueAverage(this.caravan, SpeedStat);
                        workLeft -= num;
                        JecsTools.CaravanJobsUtility.TeachCaravan(this.caravan, SkillDefOf.Construction, 0.22f);
                        if (workLeft <= 0f)
                        {
                            DoEffect();
                            this.ReadyForNextToil();
                            return;
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Never,
                //FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch),
            };
            nextToil.WithProgressBar(TargetIndex.A, () => 1f - this.workLeft / (float)this.BaseWorkAmount, false, -0.5f);
            yield return nextToil;

        }
    }
}
