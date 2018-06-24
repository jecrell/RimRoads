using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Verse;
using JecsTools;

namespace RimRoads
{
    public class RoadBlueprint : JecsTools.WorldObjectBlueprint
    {
        private RoadBuildableDef roadType = null;

        public RoadBuildableDef RoadType { set => roadType = value; get => roadType; }

        public override string Label => roadType.LabelCap + " " + base.Label;

        public override WorldObjectRecipeDef Recipe => RoadType;

        public override CaravanJobDef ConstructJobDef => DefDatabase<JecsTools.CaravanJobDef>.GetNamed("RimRoads_JobConstructRoads");

        public RoadBlueprint prevBlueprint = null;
        public RoadBlueprint nextBlueprint = null;
        public override WorldObjectBlueprint NextBlueprint => nextBlueprint;
        public override WorldObjectBlueprint PrevBlueprint => prevBlueprint;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<RoadBuildableDef>(ref this.roadType, "roadType");
            Scribe_References.Look<RoadBlueprint>(ref this.prevBlueprint, "prevBlueprint");
            Scribe_References.Look<RoadBlueprint>(ref this.nextBlueprint, "nextBlueprint");
        }

        public void Notify_Building(Caravan c)
        {

        }

        public void Notify_Built()
        {

            List<int> neighbors = new List<int>();
            Find.WorldGrid.GetTileNeighbors(this.Tile, neighbors);

            int forwardTile = (nextBlueprint != null) ? nextBlueprint.Tile : -1;
            int fforwardTile = (nextBlueprint?.nextBlueprint != null) ? nextBlueprint.nextBlueprint.Tile : -1;
            int backwardTile = (prevBlueprint != null) ? prevBlueprint.Tile : -1;
            int bbackwardTile = (prevBlueprint?.prevBlueprint != null) ? prevBlueprint.prevBlueprint.Tile : -1;
            //int forkTile = -1;

            if (forwardTile < 0)
            {
                if (!neighbors.NullOrEmpty())
                {
                    foreach (int n in neighbors)
                    {
                        if (n != backwardTile && Find.WorldGrid[n].potentialRoads is List<Tile.RoadLink> r && !r.NullOrEmpty() && r.Any(x => x.road == this.RoadType.roadDef))
                        {
                            forwardTile = n;
                            break;
                        }
                    }
                }
            }

            if (backwardTile < 0)
            {
                if (!neighbors.NullOrEmpty())
                {
                    foreach (int n in neighbors)
                    {
                        if (n != forwardTile && Find.WorldGrid[n].potentialRoads is List<Tile.RoadLink> r && !r.NullOrEmpty() && r.Any(x => x.road == this.RoadType.roadDef))
                        {
                            backwardTile = n;
                            break;
                        }
                    }
                }
            }

            //if (!neighbors.NullOrEmpty())
            //{
            //    foreach (int n in neighbors)
            //    {
            //        if (n != forwardTile && n != backwardTile && n != fforwardTile && n != bbackwardTile && Find.WorldGrid[n].potentialRoads is List<Tile.RoadLink> r && !r.NullOrEmpty() && r.Any(x => x.road == this.RoadType.roadDef))
            //        {
            //            forkTile = n;
            //            break;
            //        }
            //    }
            //}

            //int adjToDraw;
            //if (prevBlueprint != null) adjToDraw = prevBlueprint.Tile;
            //else if (nextBlueprint != null) adjToDraw = nextBlueprint.Tile;
            //else
            //{
            //    adjToDraw = neighbors[0];
            //    if (!neighbors.NullOrEmpty())
            //    {
            //        foreach (int n in neighbors)
            //        {
            //            if (Find.WorldGrid[n].potentialRoads is List<Tile.RoadLink> r && !r.NullOrEmpty() && r.Any(x => x.road == this.RoadType.roadDef))
            //            {
            //                adjToDraw = n;
            //                break;
            //            }
            //        }
            //    }
            //}
            if (forwardTile > 0) OverlayRoad(this.Tile, forwardTile, RoadType.roadDef);
            if (backwardTile > 0) OverlayRoad(this.Tile, backwardTile, RoadType.roadDef);
            //if (forkTile > 0) OverlayRoad(this.Tile, forkTile, RoadType.roadDef);

            Find.World.GetComponent<RoadTracker>().TryRemoveBlueprint(this);
            Find.World.renderer.RegenerateAllLayersNow();
        }

        // RimWorld.Planet.WorldGrid
        public void OverlayRoad(int fromTile, int toTile, RoadDef roadDef)
        {
            if (roadDef == null)
            {
                Log.ErrorOnce("Attempted to remove road with overlayRoad; not supported", 90292249);
                return;
            }
            RoadDef roadDef2 = Find.WorldGrid.GetRoadDef(fromTile, toTile, false);
            if (roadDef2 == roadDef)
            {
                return;
            }
            Tile tile = Find.WorldGrid[fromTile];
            Tile tile2 = Find.WorldGrid[toTile];
            if (roadDef2 != null)
            {
                if (roadDef2.priority >= roadDef.priority)
                {
                    return;
                }
                tile.potentialRoads.RemoveAll((Tile.RoadLink rl) => rl.neighbor == toTile);
                tile2.potentialRoads.RemoveAll((Tile.RoadLink rl) => rl.neighbor == fromTile);
            }
            if (tile.potentialRoads == null)
            {
                tile.potentialRoads = new List<Tile.RoadLink>();
            }
            if (tile2.potentialRoads == null)
            {
                tile2.potentialRoads = new List<Tile.RoadLink>();
            }
            Tile.RoadLink first = new Tile.RoadLink
            {
                neighbor = toTile,
                road = roadDef
            };
            tile.potentialRoads.Add(first);
            Tile.RoadLink second = new Tile.RoadLink
            {
                neighbor = fromTile,
                road = roadDef
            };
            tile2.potentialRoads.Add(second);
            Find.World.GetComponent<RoadTracker>().ConstructedRoads.Add(new RimRoadLink(first.road, second.neighbor, first.neighbor));
        }



        public override void Draw()
        {
            //base.Draw();
            if (nextBlueprint != null && nextBlueprint.Tile > 0)
            {
                WorldGrid worldGrid = Find.WorldGrid;
                float d = 0.05f;
                Vector3 a = worldGrid.GetTileCenter(this.Tile);
                Vector3 vector = worldGrid.GetTileCenter(nextBlueprint.Tile);
                a += a.normalized * d;
                vector += vector.normalized * d;
                GenDraw.DrawWorldLineBetween(a, vector);
            }
            if (prevBlueprint == null || nextBlueprint == null)
                base.Draw();
        }

        public override bool Cancel()
        {
            if (base.Cancel())
                Find.World.GetComponent<RoadTracker>().TryRemoveBlueprint(this);
            return true;
        }

        public override void Finish()
        {
            base.Finish();
            Notify_Built();
        }
    }
}
