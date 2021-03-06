﻿using System;
using Celeste.Mod.DJMapHelper.Cutscenes;
using Celeste.Mod.DJMapHelper.DebugFeatures;
using Celeste.Mod.DJMapHelper.Entities;
using Celeste.Mod.DJMapHelper.Triggers;

namespace Celeste.Mod.DJMapHelper {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DJMapHelperModule : EverestModule {
        // Only one alive module instance can exist at any given time.
        // ReSharper disable once NotAccessedField.Global
        public static DJMapHelperModule Instance { get; private set; }

        public DJMapHelperModule() {
            Instance = this;
        }

        // If you don't need to store any settings, => null
        public override Type SettingsType => typeof(DJMapHelperSettings);
        public override Type SessionType => typeof(DJMapHelperSession);
        public static DJMapHelperSettings Settings => (DJMapHelperSettings) Instance._Settings;
        public static DJMapHelperSession Session => (DJMapHelperSession) Instance._Session;

        // If you don't need to store any save data, => null
        public override Type SaveDataType => null;


        // Set up any hooks, event handlers and your mod in general here.
        // Load runs before Celeste itself has initialized properly.
        public override void Load() {
            BadelineBoostDown.OnLoad();
            BadelineProtector.OnLoad();
            ChangeBossPatternTrigger.OnLoad();
            ChangeSpinnerColorTrigger.OnLoad();
            ClimbBlockerTrigger.OnLoad();
            ColorfulFlyFeather.OnLoad();
            ColorfulRefill.OnLoad();
            CS_BoostTeleport.OnLoad();
            FeatherBarrier.OnLoad();
            FinalBossReversed.OnLoad();
            FlingBirdReversed.OnLoad();
            LookoutBuilder.OnLoad();
            MaxDashesTrigger.OnLoad();
            TheoCrystalBarrier.OnLoad();
            SpringGreen.OnLoad();
        }

        // Unload the entirety of your mod's content, remove any event listeners and undo all hooks.
        public override void Unload() {
            BadelineBoostDown.OnUnLoad();
            BadelineProtector.OnUnload();
            ChangeBossPatternTrigger.OnUnload();
            ChangeSpinnerColorTrigger.OnUnload();
            ClimbBlockerTrigger.OnUnLoad();
            ColorfulFlyFeather.OnUnload();
            ColorfulRefill.OnUnload();
            CS_BoostTeleport.OnUnload();
            FeatherBarrier.OnUnload();
            FinalBossReversed.OnUnload();
            FlingBirdReversed.OnUnLoad();
            LookoutBuilder.OnUnload();
            MaxDashesTrigger.OnUnload();
            TheoCrystalBarrier.OnUnload();
            SpringGreen.OnUnLoad();
        }
    }
}