using System.Collections.Generic;
using Planetbase;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlanetbaseNuclearTransport
{
    internal static class ExpansionModels
    {
        internal static readonly Color Yellow = new Color(0.96f, 0.8f, 0.16f);
        internal static readonly Color Cyan = new Color(0.22f, 0.78f, 0.88f);
        internal static readonly Color Orange = new Color(1f, 0.42f, 0.12f);
        private static readonly Color Steel = new Color(0.63f, 0.7f, 0.74f);
        private static readonly Color Dark = new Color(0.11f, 0.16f, 0.2f);

        internal static GameObject Rover(Transform parent, Vector3 offset, List<Transform> wheels)
        {
            var model=PlanetbaseVisuals.VehicleArchitecture.Create(parent,"ColonyRover",wheels);
            model.name="Colony cargo rover";model.transform.localPosition=offset;return model;
        }
    }

    public sealed class GarageDisplay : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (transform.parent == null) return;
            Planetbase.Module owner = Construction.find(transform.parent.gameObject) as Planetbase.Module;
            if (owner == null) return;
            int available = owner.isOperational() ? 2 - RoverSystem.Used(owner) : 2;
            for (int i = 0; i < 2; i++)
            {
                Transform parked = transform.Find("Parked rover " + i);
                if (parked != null) parked.gameObject.SetActive(i < available);
            }
        }
    }
}
