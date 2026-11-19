using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RDR2;
using RDR2.Math;

namespace AdvancedThrowingSystem
{
    // Source starter for ScriptHookRDR2 .NET API.
    // Controls: E = pick up, R = throw, G = drop.
    public sealed class AdvancedThrowingSystem : Script
    {
        private const int HandBone = 57005;
        private const float PickupRadius = 1.8f;
        private const float ThrowSpeed = 20.0f;
        private const float ThrowLift = 3.2f;
        private const int HitDamage = 12;
        private const float HitRadius = 1.25f;
        private const int MaxFlightTicks = 100;

        private Entity carried;
        private bool carriedAnimal;
        private Entity thrown;
        private int flightTicks;
        private readonly HashSet<int> hitHandles = new HashSet<int>();

        // Small-animal model whitelist. Unknown models are deliberately not picked up.
        private readonly HashSet<int> smallAnimalModels = new HashSet<int>
        {
            Game.Joaat("A_C_SNAKE"),
            Game.Joaat("A_C_RABBIT_01"),
            Game.Joaat("A_C_RAT_01"),
            Game.Joaat("A_C_SQUIRREL_01"),
            Game.Joaat("A_C_CHICKEN_01"),
            Game.Joaat("A_C_TOAD_01"),
            Game.Joaat("A_C_FROG_01")
        };

        public AdvancedThrowingSystem()
        {
            Interval = 0;
            KeyDown += OnKeyDown;
            Tick += OnTick;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.E)
                TryPickup();

            if (e.KeyCode == Keys.R)
                ThrowCarried();

            if (e.KeyCode == Keys.G)
                DropCarried();
        }

        private Ped PlayerPed
        {
            get
            {
                if (Game.Player == null) return null;
                return Game.Player.Character;
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (carried != null && !carried.Exists())
                ClearCarried();

            if (thrown != null)
                ProcessThrownObject();
        }

        private void TryPickup()
        {
            if (carried != null) return;

            Ped player = PlayerPed;
            if (player == null || !player.Exists() || player.IsDeadOrDying)
                return;

            Entity best = null;
            bool bestIsAnimal = false;
            float bestDistance = PickupRadius;

            // First search nearby small animals.
            Ped[] peds = World.GetAllPeds();
            if (peds != null)
            {
                foreach (Ped ped in peds)
                {
                    if (ped == null || !ped.Exists() || ped.IsPlayer || ped.IsDeadOrDying)
                        continue;
                    if (!ped.IsAnimal || !smallAnimalModels.Contains(ped.Model.Hash))
                        continue;

                    float d = Vector3.Distance(player.Position, ped.Position);
                    if (d < bestDistance)
                    {
                        best = ped;
                        bestDistance = d;
                        bestIsAnimal = true;
                    }
                }
            }

            // If no small animal is closer, search nearby physical props.
            Entity[] objects = World.GetAllObjects();
            if (objects != null)
            {
                foreach (Entity obj in objects)
                {
                    if (obj == null || !obj.Exists() || !obj.IsObject || obj.IsAttached)
                        continue;

                    float d = Vector3.Distance(player.Position, obj.Position);
                    if (d < bestDistance)
                    {
                        best = obj;
                        bestDistance = d;
                        bestIsAnimal = false;
                    }
                }
            }

            if (best == null) return;

            try
            {
                best.RequestOwnership();
                best.AttachTo(player, HandBone,
                    new Vector3(0.22f, 0.18f, -0.08f),
                    new Vector3(0f, 0f, 0f));
                carried = best;
                carriedAnimal = bestIsAnimal;
            }
            catch
            {
                ClearCarried();
            }
        }

        private void ThrowCarried()
        {
            if (carried == null || !carried.Exists()) return;

            Ped player = PlayerPed;
            if (player == null || !player.Exists()) return;

            Entity item = carried;
            bool wasAnimal = carriedAnimal;
            carried = null;
            carriedAnimal = false;

            try
            {
                item.Detach();
                item.Position = player.Position + player.ForwardVector * 0.8f
                    + new Vector3(0f, 0f, 1.0f);

                Vector3 direction = GameplayCamera.Direction;
                if (direction.Length() < 0.001f)
                    direction = player.ForwardVector;
                direction.Normalize();

                item.Velocity = direction * ThrowSpeed + new Vector3(0f, 0f, ThrowLift);

                thrown = item;
                flightTicks = 0;
                hitHandles.Clear();
            }
            catch
            {
                thrown = null;
            }
        }

        private void DropCarried()
        {
            if (carried == null || !carried.Exists())
            {
                ClearCarried();
                return;
            }

            try
            {
                Ped player = PlayerPed;
                carried.Detach();
                if (player != null && player.Exists())
                    carried.Position = player.Position + player.ForwardVector * 0.65f;
                carried.Velocity = Vector3.Zero;
            }
            catch { }
            ClearCarried();
        }

        private void ProcessThrownObject()
        {
            if (thrown == null || !thrown.Exists())
            {
                ResetFlight();
                return;
            }

            flightTicks++;
            ApplyGameImpactReactions();

            if (flightTicks > MaxFlightTicks ||
                (flightTicks > 12 && thrown.Speed < 0.65f))
            {
                thrown = null;
                hitHandles.Clear();
            }
        }

        // Lightweight gameplay impact approximation: check nearby NPCs around the
        // moving item and apply a small in-game health reduction once per throw.
        private void ApplyGameImpactReactions()
        {
            Ped[] peds = World.GetAllPeds();
            if (peds == null || thrown == null || !thrown.Exists()) return;

            foreach (Ped target in peds)
            {
                if (target == null || !target.Exists() || target.IsPlayer ||
                    target.IsDeadOrDying || target.IsAnimal)
                    continue;

                if (hitHandles.Contains(target.Handle))
                    continue;

                if (Vector3.Distance(thrown.Position, target.Position) <= HitRadius)
                {
                    try
                    {
                        target.Health = System.Math.Max(0, target.Health - HitDamage);
                        hitHandles.Add(target.Handle);
                    }
                    catch { }
                }
            }
        }

        private void ClearCarried()
        {
            carried = null;
            carriedAnimal = false;
        }

        private void ResetFlight()
        {
            thrown = null;
            flightTicks = 0;
            hitHandles.Clear();
        }
    }
}
