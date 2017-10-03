using System;
using UnityEngine;
using Verse;

namespace RimRoads
{
    [StaticConstructorOnStartup]
    public static class RoadButtons
    {
        public static readonly Texture2D Build = ContentFinder<Texture2D>.Get("UI/Commands/BuildRoads", true);
        public static readonly Texture2D Deconstruct = ContentFinder<Texture2D>.Get("UI/Commands/DeconstructSorryTynan", true);

    }
}
