using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.DJMapHelper.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.DJMapHelper.DebugMode {
    // Ctrl+Q: Add tower viewer.
    public static class LookoutBuilder {
        private static bool? savedInvincible;

        private static readonly MethodInfo InteractMethod = typeof(Lookout).GetPrivateMethod("Interact");
        private static readonly FieldInfo InteractingField = typeof(Lookout).GetPrivateField("interacting");

        public static void OnLoad() {
            On.Celeste.Player.Update += PlayerOnUpdate;
            On.Celeste.Player.Die += PlayerOnDie;
            On.Celeste.Actor.OnGround_int += ActorOnOnGroundInt;
        }

        public static void OnUnload() {
            On.Celeste.Player.Update -= PlayerOnUpdate;
            On.Celeste.Player.Die -= PlayerOnDie;
            On.Celeste.Actor.OnGround_int -= ActorOnOnGroundInt;
        }

        private static bool ActorOnOnGroundInt(On.Celeste.Actor.orig_OnGround_int orig, Actor self, int downCheck) {
            if (self is Player && DJMapHelperModule.Settings.EnableTowerViewer && self.SceneAs<Level>().Tracker
                .GetEntities<Lookout>()
                .Any(entity => entity.Get<LookoutComponent>() != null)) {
                return true;
            }

            return orig(self, downCheck);
        }

        private static PlayerDeadBody PlayerOnDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction,
            bool evenIfInvincible, bool registerDeathInStats) {
            PlayerDeadBody playerDeadBody = orig(self, direction, evenIfInvincible, registerDeathInStats);
            
            if (savedInvincible != null) {
                SaveData.Instance.Assists.Invincible = (bool) savedInvincible;
                savedInvincible = null;
            } 

            return playerDeadBody;
        }

        private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self);

            Level level = self.SceneAs<Level>();

            if (level.Tracker.GetEntities<Lookout>()
                    .All(entity => entity.Get<LookoutComponent>() == null) &&
                savedInvincible != null
            ) {
                SaveData.Instance.Assists.Invincible = (bool) savedInvincible;
                savedInvincible = null;
            }

            if (!DJMapHelperModule.Settings.EnableTowerViewer) {
                return;
            }

            if (self.Dead || level.Paused || self.StateMachine.State == Player.StDummy) {
                return;
            }

            MInput.KeyboardData keyboard = MInput.Keyboard;

            if (keyboard.Pressed(Keys.Q) && (keyboard.Check(Keys.LeftControl) || keyboard.Check(Keys.RightControl))) {
                Lookout lookout = new Lookout(new EntityData {Position = self.Position}, Vector2.Zero) {
                    new LookoutComponent()
                };
                lookout.Add(new Coroutine(Look(lookout, self)));
                level.Add(lookout);
                level.Tracker.GetEntitiesCopy<LookoutBlocker>().ForEach(entity => level.Remove(entity));
            }
        }

        private static IEnumerator Look(Lookout lookout, Player player) {
            yield return null;
            InteractMethod?.Invoke(lookout, new object[] {player});
            savedInvincible = SaveData.Instance.Assists.Invincible;
            SaveData.Instance.Assists.Invincible = true;

            Level level = player.SceneAs<Level>();
            Level.CameraLockModes savedCameraLockMode = level.CameraLockMode;
            Vector2 savedCameraPosition = level.Camera.Position;
            level.CameraLockMode = Level.CameraLockModes.None;

            Entity underfootPlatform = player.CollideFirstOutside<FloatySpaceBlock>(player.Position + Vector2.UnitY);

            bool interacting = (bool) InteractingField?.GetValue(lookout);
            while (!interacting) {
                player.Position = lookout.Position;
                interacting = (bool) InteractingField?.GetValue(lookout);
                yield return null;
            }

            while (interacting) {
                player.Position = lookout.Position;
                interacting = (bool) InteractingField?.GetValue(lookout);
                yield return null;
            }

            lookout.Collidable = lookout.Visible = false;

            if (savedInvincible != null) {
                SaveData.Instance.Assists.Invincible = (bool) savedInvincible;
                savedInvincible = null;
            }

            if (underfootPlatform != null) {
                player.Position.Y = underfootPlatform.Top;
            }

            player.Add(new Coroutine(RestoreCameraLockMode(level, savedCameraLockMode, savedCameraPosition)));

            lookout.RemoveSelf();
        }

        private static IEnumerator RestoreCameraLockMode(Level level, Level.CameraLockModes cameraLockMode,
            Vector2 cameraPosition) {
            while (Vector2.Distance(level.Camera.Position, cameraPosition) > 1) {
                yield return null;
            }

            level.CameraLockMode = cameraLockMode;
        }

        private class LookoutComponent : Component {
            public LookoutComponent() : base(false, false) { }
        }
    }
}