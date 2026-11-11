using System.Collections.Generic;
using Planetbase;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlanetbaseColonyLeisure
{
    internal static class LeisureModels
    {
        internal static void AddBuildingSign(GameObject model, LeisureDome dome, int sizeIndex)
        {
            float radius = Module.ValidSizes[sizeIndex] * 0.5f;
            var surfaces = new List<Renderer>();
            surfaces.Add(Box(model.transform, new Vector3(0f, 1.6f, -radius * 0.75f),
                new Vector3(2.4f, 0.5f, 0.18f), "Leisure sign"));
            float signZ = -radius * 0.75f;
            AddText(model.transform, dome.Sign, new Vector3(0f, 1.6f, signZ - 0.1f), 0.17f);
            AddText(model.transform, dome.Sign, new Vector3(0f, 1.6f, signZ + 0.1f), 0.17f, 180f);
            AddAccents(model, surfaces, dome.Accent, false);
        }

        internal static void DecorateEquipment(GameObject model, LeisureEquipment equipment)
        {
            var surfaces = new List<Renderer>();
            if (equipment is DanceFloor)
            {
                // Preserve the native interaction markers while replacing the plant's visible mesh.
                foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
                    renderer.enabled = false;
                for (int x = -1; x <= 1; x++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        surfaces.Add(Box(model.transform, new Vector3(x * 0.5f, 0.06f, z * 0.5f),
                            new Vector3(0.47f, 0.1f, 0.47f), "Dance tile"));
                    }
                }
                surfaces.Add(Box(model.transform, new Vector3(0f, 0.6f, 0.85f),
                    new Vector3(1.5f, 1.2f, 0.2f), "Club speakers"));
                var anchor = new GameObject("Club guest position");
                anchor.tag = "AnchorPoint";
                anchor.transform.SetParent(model.transform, false);
                model.AddComponent<ClubAmbience>();
            }
            else if (equipment is MeditationSeat)
            {
                surfaces.Add(Box(model.transform, new Vector3(-0.9f, 0.45f, 0f),
                    new Vector3(0.12f, 0.9f, 0.7f), "Calming light"));
                surfaces.Add(Box(model.transform, new Vector3(0.9f, 0.45f, 0f),
                    new Vector3(0.12f, 0.9f, 0.7f), "Calming light"));
            }
            else
            {
                surfaces.Add(Box(model.transform, new Vector3(-0.8f, 0.95f, 0.3f),
                    new Vector3(0.1f, 1.9f, 0.7f), "Cabinet side"));
                surfaces.Add(Box(model.transform, new Vector3(0.8f, 0.95f, 0.3f),
                    new Vector3(0.1f, 1.9f, 0.7f), "Cabinet side"));
                surfaces.Add(Box(model.transform, new Vector3(0f, 1.9f, 0.3f),
                    new Vector3(1.7f, 0.25f, 0.7f), "Cabinet marquee"));
                string label = equipment is CasinoTerminal ? "2 C" : "PLAY";
                AddText(model.transform, label, new Vector3(0f, 1.9f, -0.1f), 0.12f);
            }
            AddAccents(model, surfaces, equipment.Accent, equipment is DanceFloor);
        }

        private static Renderer Box(Transform parent, Vector3 position, Vector3 size, string name)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = position;
            box.transform.localScale = size;
            box.layer = parent.gameObject.layer;
            Collider collider = box.GetComponent<Collider>();
            collider.enabled = false;
            Object.Destroy(collider);
            Renderer renderer = box.GetComponent<Renderer>();
            renderer.sharedMaterial = ResourceList.getInstance().Materials.PlainColor;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return renderer;
        }

        private static void AddText(Transform parent, string text, Vector3 position, float size, float yaw = 0f)
        {
            var label = new GameObject("Leisure label");
            label.transform.SetParent(parent, false);
            label.transform.localPosition = position;
            label.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var mesh = label.AddComponent<TextMesh>();
            mesh.font = ResourceList.getInstance().FontMain;
            mesh.text = text;
            mesh.fontSize = 40;
            mesh.characterSize = size;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = Color.white;
            label.GetComponent<Renderer>().sharedMaterial = mesh.font.material;
        }

        private static void AddAccents(GameObject model, List<Renderer> surfaces, Color accent, bool pulse)
        {
            var animation = model.AddComponent<LeisureAccent>();
            animation.Surfaces = surfaces.ToArray();
            animation.Accent = accent;
            animation.Pulse = pulse;
        }
    }

    public sealed class LeisureAccent : MonoBehaviour
    {
        public Renderer[] Surfaces;
        public Color Accent;
        public bool Pulse;
        private MaterialPropertyBlock properties;

        private void LateUpdate()
        {
            if (properties == null)
                properties = new MaterialPropertyBlock();

            float brightness = Pulse ? 0.75f + 0.25f * Mathf.Sin(Time.time * 3f) : 1f;
            for (int i = 0; i < Surfaces.Length; i++)
            {
                Renderer renderer = Surfaces[i];
                if (renderer == null)
                    continue;
                renderer.GetPropertyBlock(properties);
                Color color = Pulse ? Color.Lerp(Accent, Color.cyan, (i % 3) * 0.3f) * brightness : Accent;
                color.a = 1f;
                properties.SetColor("_Color", color);
                renderer.SetPropertyBlock(properties);
            }
        }
    }

    public sealed class ClubAmbience : MonoBehaviour
    {
        private static AudioClip music;
        private AudioSource source;
        private ConstructionComponent owner;

        private void Start()
        {
            if (transform.parent == null)
                return;
            owner = ConstructionComponent.find(transform.parent.gameObject);
            if (owner == null)
                return;

            if (music == null)
                music = CreateMusic();
            source = gameObject.AddComponent<AudioSource>();
            source.clip = music;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.minDistance = 2f;
            source.maxDistance = 16f;
        }

        private void Update()
        {
            if (source == null || owner == null)
                return;
            bool playing = owner.isOperational() && owner.anyInteractions() &&
                !Singleton<TimeManager>.getInstance().isPaused();
            source.volume = Singleton<AudioPlayer>.getInstance().getMusicVolume() * 0.16f;
            if (playing && !source.isPlaying)
                source.Play();
            if (!playing && source.isPlaying)
                source.Stop();
        }

        private static AudioClip CreateMusic()
        {
            const int sampleRate = 22050;
            const float duration = 4f;
            var samples = new float[(int)(sampleRate * duration)];
            float[] notes = { 130.81f, 164.81f, 196f, 164.81f, 146.83f, 174.61f, 220f, 174.61f };
            for (int i = 0; i < samples.Length; i++)
            {
                float time = (float)i / sampleRate;
                float beat = time % 0.5f;
                float envelope = Mathf.Exp(-beat * 10f);
                float note = notes[(int)(time * 2f) % notes.Length];
                float melody = Mathf.Sin(time * note * 2f * Mathf.PI);
                float bass = Mathf.Sin(beat * 55f * 2f * Mathf.PI);
                samples[i] = (melody * 0.2f + bass * 0.15f) * envelope;
            }
            AudioClip clip = AudioClip.Create("Colony club original loop", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
