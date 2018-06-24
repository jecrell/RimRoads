using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.Planet.Tile;

namespace RimRoads
{
    public class RimRoadLink : IExposable
    {
        public RoadDef def;

        public int indexA;

        public int indexB;

        public RimRoadLink() { }

        public RimRoadLink(RoadDef def, int a, int b)
        {
            this.def = def;
            this.indexA = a;
            this.indexB = b;
        }

        public RoadLink ToRoadLink()
        {
            var link = new RoadLink();
            link.neighbor = indexA;
            link.road = def;
            return link;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look<RoadDef>(ref this.def, "def");
            Scribe_Values.Look<int>(ref this.indexA, "indexA", -1);
            Scribe_Values.Look<int>(ref this.indexB, "indexB", -1);
        }
    }


    [StaticConstructorOnStartup]
    public class RoadTracker : WorldComponent
    {
        private WorldPath roadPath = null;
        private int curStartTile = -1;
        private int curMouseTile = -1;
        private bool enabled = false;
        private List<RimRoadLink> constructedRoads = new List<RimRoadLink>();

        public WorldPath RoadPath { set => roadPath = value; get => roadPath; }
        public int CurStartTile { set => curStartTile = value; get => curStartTile; }
        public int CurMouseTile { set => curMouseTile = value; get => curMouseTile; }
        public bool Enabled { set => enabled = value; get => enabled; }
        public List<RimRoadLink> ConstructedRoads => constructedRoads;


        private static readonly Material WorldLineMatWhite = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.WorldOverlayTransparent, Color.white, WorldMaterials.WorldLineRenderQueue);

        public RoadTracker(World world) : base(world)
        {
        }
    
        public override void WorldComponentUpdate()
        {
            base.WorldComponentUpdate();
            if (Enabled)
            {
                DrawRoadPathTick();
            }
        }

        public void PlanBlueprints(Caravan c, int start, RoadBuildableDef defToBuild, bool instantBuild = false)
        {
            CurStartTile = start; //GenWorld.MouseTile(false);
            Tile curTile = Find.WorldGrid[curStartTile];
            if (!curTile.potentialRoads.NullOrEmpty())
            {
                Messages.Message("RimRoads_ErrorRoadExists", MessageTypeDefOf.RejectInput); //MessageSound.RejectInput);
                return;
            }
            GlobalTargetInfo end = default(GlobalTargetInfo);
            int pathTileInt = GenWorld.MouseTile(false);

            try
            {
                Enabled = true;
                Find.WorldTargeter.BeginTargeting(delegate (GlobalTargetInfo t)
                {
                    end = t;
                    LayDownBlueprints(c, t, defToBuild, instantBuild);
                    //Find.World.grid.OverlayRoad(__instance.Tile, end.Tile, RoadDefOf.DirtRoad);
                    Find.World.renderer.RegenerateAllLayersNow();
                    Find.World.GetComponent<RoadTracker>().Enabled = false;
                    return true;
                },
                true, null, false, null, delegate (GlobalTargetInfo target)
                {
                    return "End Road Here.";
                });
            }
            catch
            {
                Find.World.GetComponent<RoadTracker>().Enabled = false;
                //t1.Dispose();
            }
        }

        public void DrawRoadPathTick()
        {
            try
            {
                int mousePos = GenWorld.MouseTile(false);
                if (CurStartTile > 0 && (CurMouseTile > 0 || CurMouseTile != mousePos))
                {
                    CurMouseTile = mousePos;
                    if (RoadPath != null) roadPath.ReleaseToPool();
                    RoadPath = Find.WorldPathFinder?.FindPath(CurStartTile, CurMouseTile, null) ?? null;
                }
                if (RoadPath != null && RoadPath.Found) RoadPath.DrawPath(null);
                if (Input.GetMouseButtonDown(1) || !Find.WorldTargeter.IsTargeting)
                {
                    Log.Message("Disabled");
                    Enabled = false;
                    return;
                }
            }
            catch
            { }
        }

        public WorldPath lastPath = null;
        public WorldPath LastPath { set => lastPath = value; get => lastPath; }

        //private List<int> nodes = new List<int>(128);
        //private float totalCostInt;
        //private int curNodeIndex;

        // RimWorld.Planet.WorldRoutePlanner
        public void TryRemoveBlueprint(RoadBlueprint blueprint, bool playSound = true)
        {
            if (blueprint.prevBlueprint != null) blueprint.prevBlueprint.nextBlueprint = null;
            if (blueprint.nextBlueprint != null) blueprint.nextBlueprint.prevBlueprint = null;
            Find.WorldObjects.Remove(blueprint);
            //if (this.cantRemoveFirstWaypoint && this.waypoints.Any<RoutePlannerWaypoint>() && point == this.waypoints[0])
            //{
            //    Messages.Message("MessageCantRemoveWaypointBecauseFirst".Translate(), MessageSound.RejectInput);
            //    return;
            //}
            //Find.WorldObjects.Remove(point);
            //this.waypoints.Remove(point);
            //for (int i = this.waypoints.Count - 1; i >= 1; i--)
            //{
            //    if (this.waypoints[i].Tile == this.waypoints[i - 1].Tile)
            //    {
            //        Find.WorldObjects.Remove(this.waypoints[i]);
            //        this.waypoints.RemoveAt(i);
            //    }
            //}
            //this.RecreatePaths();
            if (playSound)
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
            }
        }


        public void LayDownBlueprints(Caravan c, GlobalTargetInfo end, RoadBuildableDef defToBuild, bool instantBuild = false)
        {
            if (CurStartTile > 0 && end.Tile > 0)
            {
                //CurMouseTile = GenWorld.MouseTile(false);
                if (RoadPath != null) roadPath.ReleaseToPool();
                RoadPath = Find.WorldPathFinder?.FindPath(CurStartTile, end.Tile, null) ?? null;
            }
            
            if (RoadPath != null && RoadPath.Found && RoadPath.NodesLeftCount > 0)
            {
                WorldGrid worldGrid = Find.WorldGrid;
                RoadBlueprint prevBlueprint = null;
                //RoadBlueprint firstBlueprint = null;
                RoadBlueprint[] bluePrints = new RoadBlueprint[RoadPath.NodesLeftCount];
                for (int i = 0; i < RoadPath.NodesLeftCount; i++)
                {
                    int curTileInt = RoadPath.Peek(i);
                    Tile curTile = worldGrid[curTileInt];
                    RoadBlueprint curBlueprint = (RoadBlueprint)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("RimRoads_RoadBlueprint"));
                    //if (firstBlueprint == null) firstBlueprint = curBlueprint;
                    curBlueprint.Tile = curTileInt;
                    curBlueprint.RoadType = defToBuild;
                    if (prevBlueprint != null) prevBlueprint.nextBlueprint = curBlueprint;
                    curBlueprint.prevBlueprint = prevBlueprint;
                    Find.WorldObjects.Add(curBlueprint);
                    bluePrints[i] = curBlueprint;
                    if (!instantBuild)
                        Find.World.GetComponent<JecsTools.CaravanJobGiver>().Tracker(c).jobQueue.EnqueueLast(new JecsTools.CaravanJob(DefDatabase<JecsTools.CaravanJobDef>.GetNamed("RimRoads_JobConstructRoads"), curBlueprint));
                    prevBlueprint = curBlueprint;
                }
                if (instantBuild)
                {
                    for (int j = 0; j < bluePrints.Length; j++)
                    {
                        bluePrints[j].Notify_Built();
                    }
                }
                //Find.World.GetComponent<JecsTools.CaravanJobGiver>().Tracker(c).TryTakeOrderedJob(new JecsTools.CaravanJob(DefDatabase<JecsTools.CaravanJobDef>.GetNamed("RimRoads_JobConstructRoads"), firstBlueprint));
            }

        }

        // RimWorld.Planet.WorldPath
        public void DrawPath(WorldPath RoadPath, Caravan pathingCaravan)
        {
            if (!RoadPath.Found)
            {
                return;
            }
            if (RoadPath.NodesLeftCount > 0)
            {
                WorldGrid worldGrid = Find.WorldGrid;
                float d = 0.05f;
                for (int i = 0; i < RoadPath.NodesLeftCount - 1; i++)
                {
                    Vector3 a = worldGrid.GetTileCenter(RoadPath.Peek(i));
                    Vector3 vector = worldGrid.GetTileCenter(RoadPath.Peek(i + 1));
                    a += a.normalized * d;
                    vector += vector.normalized * d;
                    GenDraw.DrawWorldLineBetween(a, vector);
                }
                if (pathingCaravan != null)
                {
                    Vector3 a2 = pathingCaravan.DrawPos;
                    Vector3 vector2 = worldGrid.GetTileCenter(RoadPath.Peek(0));
                    a2 += a2.normalized * d;
                    vector2 += vector2.normalized * d;
                    if ((a2 - vector2).sqrMagnitude > 0.005f)
                    {
                        RoadTracker.DrawBlueprintPath(a2, vector2);
                    }
                }
            }
            LastPath = RoadPath;
        }

        // Verse.GenDraw
        public static void DrawBlueprintPath(Vector3 A, Vector3 B)
        {
            GenDraw.DrawWorldLineBetween(A, B, RoadTracker.WorldLineMatWhite, 1.5f);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<RimRoadLink>(ref this.constructedRoads, "constructedRoads", LookMode.Deep, new object[0]);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (!ConstructedRoads.NullOrEmpty())
                {
                    foreach (RimRoadLink l in ConstructedRoads)
                    {
                        if (l.indexA > 0 && l.indexB > 0)
                            Find.WorldGrid.OverlayRoad(l.indexA, l.indexB, l.def);
                    }
                }
            }
        }
    }
}
