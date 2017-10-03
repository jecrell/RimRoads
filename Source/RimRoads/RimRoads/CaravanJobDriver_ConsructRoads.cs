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
    public class CaravanJobDriver_ConstructRoads : CaravanJobDriver
    {
        private float workLeft = -1000f;

        public int BaseWorkAmount => DefToCheck.workToMake;
        public StatDef SpeedStat => StatDefOf.ConstructionSpeed;
        public RoadBlueprint BlueprintToWorkOn => ((RoadBlueprint)TargetA.WorldObject);
        public RoadBuildableDef DefToCheck => ((RoadBlueprint)TargetA.WorldObject).RoadType;

        public CaravanJobDriver_ConstructRoads() { }
        
        public void DoEffect()
        {
            BlueprintToWorkOn.Notify_Built();
            Messages.Message("RimRoads_BuildRoadFinished".Translate(this.GetActor().LabelCap), new GlobalTargetInfo(this.GetActor()), MessageSound.Benefit);
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
            yield return CaravanToils_Goto.GotoObject(Verse.AI.TargetIndex.A);
            CaravanToil nextToil = new CaravanToil()
            {
                initAction = delegate
                {
                    if (BlueprintToWorkOn.nextBlueprint is RoadBlueprint nextBlueprint)
                    {
                        int fromTile = this.GetActor().Tile;
                        int toTile = nextBlueprint.Tile;

                        RoadDef roadDef2 = Find.WorldGrid.GetRoadDef(fromTile, toTile, false);
                        if (roadDef2 != null)
                        {
                            if (roadDef2.priority >= DefToCheck.roadDef.priority)
                            {
                                Find.World.GetComponent<RoadTracker>().TryRemoveBlueprint(BlueprintToWorkOn);
                                this.ReadyForNextToil();
                                return;
                            }
                            //Find.WorldGrid[fromTile].roads.RemoveAll((Tile.RoadLink rl) => rl.neighbor == toTile);
                            //Find.WorldGrid[toTile].roads.RemoveAll((Tile.RoadLink rl) => rl.neighbor == fromTile);
                        }
                    }

                    //Log.Message("WorkStarted2");
                    this.workLeft = BaseWorkAmount;
                    if (!BlueprintToWorkOn.CheckAndConsumeResources(this.GetActor(), DefToCheck, true))
                    {
                        Find.World.GetComponent<CaravanJobGiver>().Tracker(this.GetActor()).StopAll();
                    }
                },
                tickAction = delegate
                {
                    if (this.caravan == null || !this.caravan.Spawned)
                        this.Cleanup(JobCondition.Incompletable);

                    if (!this.caravan.Resting)
                    {
                        //Log.Message(this.workLeft.ToString());
                        float num = (SpeedStat == null) ? 1f : JecsTools.CaravanJobsUtility.GetStatValueTotal(this.caravan, SpeedStat);
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
