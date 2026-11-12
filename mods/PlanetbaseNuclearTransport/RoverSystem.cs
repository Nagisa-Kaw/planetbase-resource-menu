using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Planetbase;
using UnityEngine;
using Module = Planetbase.Module;

namespace PlanetbaseNuclearTransport
{
    internal static class RoverSystem
    {
        private static readonly Dictionary<Character, Module> Leases = new Dictionary<Character, Module>();
        internal static int Used(Module garage) => Leases.Count(p => p.Value == garage && Eligible(p.Key));
        private static bool Eligible(Character c) => c is Colonist && !c.isDestroyed() && !c.isDead() && !c.isKo() &&
            c.getLocation() == Location.Exterior && !c.anyInteractions() && c.getLoadedResource() != null &&
            !c.getLoadedResource().hasFlag(ResourceType.FlagLight) && !c.isFighting() && !c.isFleeing();

        internal static bool HasRover(Character character)
        {
            if (!Eligible(character)) { Leases.Remove(character); return false; }
            if (Leases.TryGetValue(character, out Module assigned))
            {
                if (!assigned.isDestroyed() && assigned.isOperational()) return true;
                Leases.Remove(character);
            }
            foreach (Module garage in NuclearSystem.Modules.Where(m => m.getModuleType() is ModuleTypeRoverGarage &&
                m.isOperational() && (m.getPosition() - character.getPosition()).sqrMagnitude <= 120f * 120f)
                .OrderBy(m => (m.getPosition() - character.getPosition()).sqrMagnitude).ThenBy(m => m.getId()))
            {
                if (Used(garage) >= 2) continue;
                Leases[character] = garage;
                return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(Human), nameof(Human.getMaxSpeed))]
        private static class DrivingSpeed
        {
            private static void Postfix(Human __instance, ref float __result)
            {
                if (HasRover(__instance)) __result *= 2f;
            }
        }

        [HarmonyPatch]
        private static class DrivingPose
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(Character), "getWalkAnimationType");
                yield return AccessTools.Method(typeof(Character), "getIdleAnimationType");
            }
            private static bool Prefix(Character __instance, ref CharacterAnimationType __result)
            {
                if (!HasRover(__instance)) return true;
                __result = CharacterAnimationType.SitIdle;
                return false;
            }
        }

        [HarmonyPatch(typeof(Specialization), nameof(Specialization.createModelExterior))]
        private static class ExteriorDrivingAnimation
        {
            private static void Postfix(Specialization __instance, GameObject __result, List<string>[] ___mExteriorAnimationNames)
            {
                if (!__instance.hasFlag(Specialization.FlagHuman) || ___mExteriorAnimationNames == null) return;
                Animation animation = __result.GetComponent<Animation>();
                AnimationClip clip = ResourceList.getInstance().PrefabHumanAnimations.GetComponent<AnimationList>().Animations
                    .FirstOrDefault(c => c != null && c.name.Contains("sit_idle"));
                if (animation == null || clip == null) return;
                if (animation[clip.name] == null) animation.AddClip(clip, clip.name);
                ___mExteriorAnimationNames[(int)CharacterAnimationType.SitIdle] = new List<string> { clip.name };
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.update))]
        private static class VehicleVisual
        {
            private static void Postfix(Character __instance)
            {
                if (!(__instance is Colonist) || __instance.isDestroyed()) return;
                RoverDriver driver = __instance.getGameObject().GetComponent<RoverDriver>();
                if (driver == null && HasRover(__instance))
                {
                    driver = __instance.getGameObject().AddComponent<RoverDriver>();
                    driver.Owner = __instance;
                }
                if (driver != null) driver.Sync();
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.destroy))]
        private static class ReleaseVehicle
        {
            private static void Prefix(Character __instance) => Leases.Remove(__instance);
        }

        [HarmonyPatch(typeof(Character), nameof(Character.destroyAll))]
        private static class ReleaseColony
        {
            private static void Prefix() => Leases.Clear();
        }
    }

    public sealed class RoverDriver : MonoBehaviour
    {
        internal Character Owner;
        private GameObject rover;
        private Transform body;
        private Vector3 originalPosition;
        private bool active;
        private Vector3 previous;
        private readonly List<Transform> wheels = new List<Transform>();

        internal void Sync()
        {
            bool driving = RoverSystem.HasRover(Owner);
            if (driving && rover == null)
            {
                rover = ExpansionModels.Rover(transform, Vector3.zero, wheels);
                previous = transform.position;
            }
            if (rover == null) return;
            Transform currentBody = Owner.getSelectionModel().transform;
            if (body != currentBody)
            {
                body = currentBody;
                originalPosition = body.localPosition;
            }
            rover.SetActive(driving);
            if (body != null) body.localPosition = driving ? originalPosition + new Vector3(0f, 0.38f, 0.3f) : originalPosition;
            if (driving != active && Owner.getState() == Character.State.Walking) Owner.playWalkAnimation();
            active = driving;
        }

        private void LateUpdate()
        {
            if (Owner == null || Owner.isDestroyed()) return;
            Sync();
            if (!active) return;
            Resource cargo = Owner.getLoadedResource();
            if (cargo != null)
            {
                cargo.setPosition(transform.TransformPoint(new Vector3(0f, 0.85f, -0.95f)));
                cargo.setRotation(transform.rotation);
            }
            float distance = Vector3.Distance(previous, transform.position);
            foreach (Transform wheel in wheels) wheel.Rotate(Vector3.up, distance * 180f, Space.Self);
            previous = transform.position;
        }
    }
}
