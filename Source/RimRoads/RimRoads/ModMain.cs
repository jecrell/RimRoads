using UnityEngine;
using Verse;

namespace RimCinema
{
    [StaticConstructorOnStartup]
    public static class ModInfo
    {
        //public static bool enableFloatingNames = true;
        //public static bool enableThoughtBubbles = true;
        //public static bool enableSpeechBubbles = true;
    }

    public class ModMain : Mod
    {
        //Settings settings;

        public ModMain(ModContentPack content) : base(content)
        {
            //this.settings = GetSettings<Settings>();
            //ModInfo.enableFloatingNames = this.settings.enableFloatingNames;
            //ModInfo.enableThoughtBubbles = this.settings.enableFloatingNames;
            //ModInfo.enableFloatingNames = this.settings.enableFloatingNames;
        }
//
//        public override string SettingsCategory() => "Rim Roads";
//
//        public override void DoSettingsWindowContents(Rect inRect)
//        {
//            var label = "";
//            if (this.settings.romSpiderFactor < 0.25f)
//            {
//                label = "ROM_SettingsSpiderMultiplier_None".Translate();
//            }
//            else
//            {
//                label = "ROM_SettingsSpiderMultiplier_Num".Translate(this.settings.romSpiderFactor);
//            }
//            this.settings.romSpiderFactor = Widgets.HorizontalSlider(inRect.TopHalf().TopHalf().TopHalf(), this.settings.romSpiderFactor, 0.0f, 10f, false, label, null, null, 0.25f);
//
//            this.WriteSettings();
//
//        }
//
//        public override void WriteSettings()
//        {
//            //base.WriteSettings();
//            //if (Find.World?.GetComponent<WorldComponent_ModSettings>() is WorldComponent_ModSettings modSettings)
//            //{
//            //    ModInfo.romSpiderFactor = this.settings.romSpiderFactor;
//            //    modSettings.SpiderDefsModified = false;
//            //}
//        }
//
//    }
//
//    public class Settings : ModSettings
//    {
//        public bool enableFloatingNames = true;
//        public bool enableThoughtBubbles = true;
//        public bool enableSpeechBubbles = true;
//
//        public float romSpiderFactor = 1;
//        public string romSpiderFactorBuffer;
//
//        public override void ExposeData()
//        {
//            base.ExposeData();
//            Scribe_Values.Look(ref this.romSpiderFactor, "romSpiderFactor", 0);
//        }
    }
}
