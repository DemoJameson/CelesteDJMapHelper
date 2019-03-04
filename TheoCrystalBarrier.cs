using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper {
    [Tracked]
    public class TheoCrystalBarrier : Solid {
        private readonly List<TheoCrystalBarrier> adjacent = new List<TheoCrystalBarrier>();
        private float flash;
        private bool flashing;
        private float offX;
        private float offY;
        private readonly List<Vector2> particles = new List<Vector2>();
        private readonly float[] speeds = new float[3] {12f, 20f, 40f};
        private readonly MTexture temp;

        private TheoCrystalBarrier(Vector2 position, float width, float height)
            : base(position, width, height, false) {
            Collidable = false;
            temp = new MTexture();
            for (int index = 0; (double) index < (double) Width * (double) Height / 16.0; ++index) {
                particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
            }

            offX = position.X;
            offY = position.Y;
            while (offX < 0.0) {
                offX += 128f;
            }

            while (offY < 0.0) {
                offY += 128f;
            }

            Add(new DisplacementRenderHook(RenderDisplacement));
        }

        public TheoCrystalBarrier(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height) { }

        public override void Update() {
            offX += Engine.DeltaTime * 12f;
            offY += Engine.DeltaTime * 12f;
            if (flashing) {
                flash = Calc.Approach(flash, 0.0f, Engine.DeltaTime * 5f);
                if (flash <= 0.0) {
                    flashing = false;
                }
            }

            int length = speeds.Length;
            int index = 0;
            for (int count = particles.Count; index < count; ++index) {
                Vector2 vector2 = particles[index] + Vector2.UnitY * speeds[index % length] * Engine.DeltaTime;
                vector2.Y %= Height - 1f;
                particles[index] = vector2;
            }

            base.Update();
        }

        private void OnReflect() {
            flash = 1f;
            flashing = true;
            Scene.CollideInto(new Rectangle((int) X, (int) Y - 2, (int) Width, (int) Height + 4), adjacent);
            Scene.CollideInto(new Rectangle((int) X - 2, (int) Y, (int) Width + 4, (int) Height), adjacent);
            foreach (TheoCrystalBarrier seekerBarrier in adjacent) {
                if (!seekerBarrier.flashing) {
                    seekerBarrier.OnReflect();
                }
            }

            adjacent.Clear();
        }

        private void RenderDisplacement() {
            MTexture mTexture = GFX.Game["util/displacementBlock"];
            Color color = Color.White * 0.3f;
            for (int index1 = 0; (double) index1 < (double) Width; index1 += 128) {
                for (int index2 = 0; (double) index2 < (double) Height; index2 += 128) {
                    mTexture.GetSubtexture((int) (offX % 128.0), (int) (offY % 128.0),
                        (int) Math.Min(128f, Width - index1), (int) Math.Min(128f, Height - index2), temp);
                    temp.Draw(Position + new Vector2(index1, index2), Vector2.Zero, color);
                }
            }
        }

        public override void Render() {
            Color backgroundColor = Calc.HexToColor("65abff");
            Draw.Rect(Collider, backgroundColor * 0.2f);
            if (flash > 0.0) {
                Draw.Rect(Collider, backgroundColor * flash);
            }

            Color color = Calc.HexToColor("66FF66") * 0.6f;
            foreach (Vector2 particle in particles) {
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
            }
        }

        public static void OnLoad() {
            On.Celeste.TheoCrystal.Update += TheoCrystalOnUpdate;
            On.Celeste.TheoCrystal.OnCollideH += TheoCrystalOnOnCollideH;
            On.Celeste.TheoCrystal.OnCollideV += TheoCrystalOnOnCollideV;
            On.Celeste.Player.Update += PlayerOnUpdate;
            On.Celeste.Player.Pickup += PlayerOnPickup;
        }

        public static void OnUnload() {
            On.Celeste.TheoCrystal.Update -= TheoCrystalOnUpdate;
            On.Celeste.TheoCrystal.OnCollideH -= TheoCrystalOnOnCollideH;
            On.Celeste.TheoCrystal.OnCollideV -= TheoCrystalOnOnCollideV;
            On.Celeste.Player.Update += PlayerOnUpdate;
            On.Celeste.Player.Pickup -= PlayerOnPickup;
        }

        private static void TheoCrystalOnOnCollideH(On.Celeste.TheoCrystal.orig_OnCollideH orig, TheoCrystal self,
            CollisionData data) {
            orig(self, data);
            OnCollide(data);
        }

        private static void TheoCrystalOnOnCollideV(On.Celeste.TheoCrystal.orig_OnCollideV orig, TheoCrystal self,
            CollisionData data) {
            orig(self, data);
            OnCollide(data);
        }

        private new static void OnCollide(CollisionData data) {
            if (data.Hit is TheoCrystalBarrier barrier) {
                barrier.OnReflect();
            }
        }

        private static void TheoCrystalOnUpdate(On.Celeste.TheoCrystal.orig_Update orig, TheoCrystal self) {
            List<Entity> theoCrystalBarrier = self.Scene.Tracker.GetEntities<TheoCrystalBarrier>().ToList();
            theoCrystalBarrier.ForEach(entity => entity.Collidable = true);
            orig(self);
            theoCrystalBarrier.ForEach(entity => entity.Collidable = false);
        }

        private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            List<Entity> theoCrystalBarrier = self.Scene.Tracker.GetEntities<TheoCrystalBarrier>().ToList();
            if (self.Holding?.Entity is TheoCrystal) {
                theoCrystalBarrier.ForEach(entity => entity.Collidable = true);
            }

            CollideCheckOutside(self, Vector2.UnitX);
            CollideCheckOutside(self, -Vector2.UnitX);
            CollideCheckOutside(self, Vector2.UnitY * 3);
            CollideCheckOutside(self, -Vector2.UnitY);

            orig(self);
            theoCrystalBarrier.ForEach(entity => entity.Collidable = false);
        }

        private static void CollideCheckOutside(Player player, Vector2 direction) {
            if (player.CollideFirstOutside<TheoCrystalBarrier>(player.Position + direction) is TheoCrystalBarrier barrier) {
                if (direction.X == 0) {
                    direction = 10 * direction - Vector2.UnitX * (int)player.Facing;
                }
                player.PointBounce(player.Center + direction);
                Audio.Play("event:/game/general/crystalheart_bounce", player.Center + direction);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                barrier.OnReflect();
            }
        }

        private static bool PlayerOnPickup(On.Celeste.Player.orig_Pickup orig, Player self, Holdable pickup) {
            List<Entity> theoCrystalBarrier = self.Scene.Tracker.GetEntities<TheoCrystalBarrier>().ToList();
            theoCrystalBarrier.ForEach(entity => entity.Collidable = true);
            bool collide = self.CollideCheck<TheoCrystalBarrier>();
            theoCrystalBarrier.ForEach(entity => entity.Collidable = false);

            if (collide && pickup.Entity is TheoCrystal) {
                return false;
            }

            return orig(self, pickup);
        }
    }
}