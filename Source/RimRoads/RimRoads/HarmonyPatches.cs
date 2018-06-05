using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using System.Reflection;
using UnityEngine;
using RimWorld.Planet;
using System.Diagnostics;
using System.Threading;
using JecsTools;

namespace RimRoads
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.jecrell.rimroad");
            harmony.Patch(AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DoListingItems_World"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(DoListingItems_World_PostFix)), null);
            harmony.Patch(AccessTools.Method(typeof(Caravan), "GetGizmos"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(GetGizmos_RoadButtons)), null);
        }


        // RimWorld.Planet.Caravan

        public static void GetGizmos_RoadButtons(Caravan __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance.IsPlayerControlled)
            {
                Tile curTile = Find.WorldGrid[__instance.Tile];
                if (DebugSettings.godMode && curTile?.roads == null || curTile?.roads?.Count == 0)
                {
                    __result = __result.Concat(new[] {new Command_Action()
                    {
                        defaultLabel = "Dev: Build Roads Instantly",
                        defaultDesc = "Null",
                        action = delegate
                        {
                            List<FloatMenuOption> list = new List<FloatMenuOption>();
                            foreach (RoadBuildableDef current in DefDatabase<RoadBuildableDef>.AllDefs)
                            {
                                //if (current.CanMake()) //&& (DebugSettings.godMode || base.Map.listerThings.ThingsOfDef(current).Count > 0))
                                //{
                                    list.Add(new FloatMenuOption(current.LabelCap, delegate
                                    {
                                        Find.World.GetComponent<RoadTracker>().PlanBlueprints(__instance, __instance.Tile, current, true);
                                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                                //}
                            }
                            if (list.Count == 0)
                            {

                            }
                            else
                            {
                                FloatMenu floatMenu = new FloatMenu(list);
                                floatMenu.vanishIfMouseDistant = true;
                                Find.WindowStack.Add(floatMenu);
                                //Find.DesignatorManager.Select(this);
                            }
                        } }
                    });
                }

                if (curTile?.roads == null || curTile?.roads?.Count == 0)
                {
                    __result = __result.Concat(new[] {new Command_Action()
                    {
                        defaultLabel = "RimRoads_BuildRoad".Translate(),
                        defaultDesc = "RimRoads_BuildRoadDesc".Translate(),
                        icon = RoadButtons.Build,
                        action = delegate
                        {
                            List<FloatMenuOption> list = new List<FloatMenuOption>();
                            foreach (RoadBuildableDef current in DefDatabase<RoadBuildableDef>.AllDefs)
                            {
                                if (current.CanMake()) //&& (DebugSettings.godMode || base.Map.listerThings.ThingsOfDef(current).Count > 0))
                                {
                                    list.Add(new FloatMenuOption(current.FloatMenuString, delegate
                                    {
                                        Find.World.GetComponent<RoadTracker>().PlanBlueprints(__instance, __instance.Tile, current);
                                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                                }
                            }
                            if (list.Count == 0)
                            {
                                Messages.Message("RimRoads_NoRoadsConstructable".Translate(), MessageTypeDefOf.RejectInput); //MessageSound.RejectInput);
                            }
                            else
                            {
                                FloatMenu floatMenu = new FloatMenu(list);
                                floatMenu.vanishIfMouseDistant = true;
                                Find.WindowStack.Add(floatMenu);
                                //Find.DesignatorManager.Select(this);
                            }
                        } }
                    });
                }
                //Deconstruct Roads
                if (curTile.roads != null && curTile?.roads?.Count() > 0)
                {
                    if (Find.World.GetComponent<CaravanJobGiver>().CurJob(__instance)?.def != CaravanJobDef.Named("RimRoads_JobDeconstructRoad"))
                    {
                        __result = __result.Concat(new[] {new Command_Action()
                        {
                        defaultLabel = "RimRoads_DeconstructRoad".Translate(),
                        defaultDesc = "RimRoads_DeconstructRoadDesc".Translate(),
                        icon = RoadButtons.Deconstruct,
                        action = delegate
                        {
                            Find.World.GetComponent<JecsTools.CaravanJobGiver>().Tracker(__instance).TryTakeOrderedJob(new JecsTools.CaravanJob(CaravanJobDef.Named("RimRoads_JobDeconstructRoad"), __instance));
                        }
                        } });
                        if (DebugSettings.godMode)
                        {
                            __result = __result.Concat(new[] {new Command_Action()
                                {
                                defaultLabel = "Dev: Deconstruct Road",
                                defaultDesc = "Remove the road at this location",
                                action = delegate
                                {
                                    curTile.roads = null;
                                    Find.World.renderer.RegenerateAllLayersNow();
                                    Log.Message("Done!");
                                }
                            } });
                        }
                    }

                }
            }
        }

        // Verse.Dialog_DebugActionsMenu
        public static void DoListingItems_World_PostFix(Dialog_DebugActionsMenu __instance)
        {
            //Traverse.Create(__instance).Method("DoLabel", new object[] { "Tools - Spawning" });

            AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DoLabel").Invoke(__instance, new object[] { "Tools - Spawning" });
            AccessTools.Method(typeof(Dialog_DebugActionsMenu), "DebugToolWorld").Invoke(__instance, new object[] {
                "Place Road", new Action(()=>
            //Traverse.Create(__instance).Method("DebugToolWorld", new object[] {"Place Road", new Action(delegate
            {
                int num = GenWorld.MouseTile(false);
                //Tile tile = Find.WorldGrid[num];
                
                //GlobalTargetInfo startInfo = default(GlobalTargetInfo);
                GlobalTargetInfo endInfo = default(GlobalTargetInfo);
                //Find.WorldTargeter.StopTargeting();
                //Find.WorldTargeter.BeginTargeting(delegate(GlobalTargetInfo t)
                //{
                //    startInfo = t;
                //    return true;
                //},
                //    true, null, false, null, delegate(GlobalTargetInfo target)
                //    {
                //        return "Start Road Here.";
                //    });

                //Find.WorldTargeter.StopTargeting();
                DebugTools.curTool = null;
                Find.WorldTargeter.BeginTargeting(delegate(GlobalTargetInfo s)
                {
                    endInfo = s;
                    Find.World.grid.OverlayRoad(num, endInfo.Tile, RoadDefOf.DirtRoad);
                    Find.World.renderer.RegenerateAllLayersNow();
                    return true;
                },
                true, null, false, null, delegate(GlobalTargetInfo target)
                {
                    return "End Road Here.";
                });

            })});
        }

    }
}
