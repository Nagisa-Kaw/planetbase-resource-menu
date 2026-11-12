using Planetbase;
using UnityEngine;

namespace PlanetbaseColonyLeisure
{
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
