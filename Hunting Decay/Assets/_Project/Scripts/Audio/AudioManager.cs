using System.Collections.Generic;
using UnityEngine;

namespace HuntingDecay
{
    /// <summary>
    /// Plays all music, loops and effects. Each sound's clip and volume live in its Sound asset.
    /// Every Sound is optional, so a missing one is silent. Captions go to the subtitles even without a sound.
    /// It's a single scene-wide instance because prefab views (reveals, the cave voice) need to play
    /// sounds and can't reference scene objects.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class AudioManager : MonoBehaviour
    {
        const string MasterVolumeKey = "MasterVolume";

        public static AudioManager Instance { get; private set; }

        [Header("Loops")]
        [SerializeField] AudioSource music;
        [SerializeField] AudioSource ambience;
        [SerializeField] AudioSource bloomHum;
        [SerializeField] AudioSource breathing;
        [Tooltip("A distant choir that grows louder with each missed anomaly.")]
        [SerializeField] Sound bloomHumSound;
        [Tooltip("Multiplies the hum's volume for 0, 1 and 2 missed anomalies.")]
        [SerializeField] float[] bloomHumLevels = { 0.1f, 0.4f, 0.8f };
        [Tooltip("The Hunter's breathing while aiming.")]
        [SerializeField] Sound breathingSound;

        [Header("Effects")]
        [Tooltip("Several sources so overlapping effects can each have their own pitch.")]
        [SerializeField] AudioSource[] effectVoices;
        [SerializeField] Subtitles subtitles;

        [Header("Interface")]
        [Tooltip("Ignores the listener pause, so the pause menu still makes sounds.")]
        [SerializeField] AudioSource interfaceVoice;
        [SerializeField] Sound hoverSound;
        [SerializeField] Sound clickSound;

        [Header("Typing")]
        [Tooltip("Used by every Typewriter with Type Sound on, unless its text has a voice of its own.")]
        [SerializeField] Sound typeSound;
        [Tooltip("Letters between sounds, picked at random in this range. Spaces don't count.")]
        [SerializeField] Vector2Int lettersBetweenTypeSounds = new(2, 3);

        // Which Sound each loop source is playing, so a new one replaces it and the same one keeps going.
        readonly Dictionary<AudioSource, Sound> loopSounds = new();
        int nextVoice;
        bool isBreathing;

        public float MasterVolume => AudioListener.volume;

        void Awake()
        {
            Instance = this;
            AudioListener.volume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            // Unity doesn't save this AudioSource setting with the scene, so it has to be set at runtime.
            interfaceVoice.ignoreListenerPause = true;
        }

        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        }

        public void PlayMusic(Sound sound) => PlayLoop(music, sound);

        public void PlayAmbience(Sound sound) => PlayLoop(ambience, sound);

        public void SetBloom(int missedAnomalies) =>
            PlayLoop(bloomHum, bloomHumSound, bloomHumLevels[Mathf.Clamp(missedAnomalies, 0, bloomHumLevels.Length - 1)]);

        public void SetBreathing(bool on)
        {
            if (on == isBreathing)
                return;
            isBreathing = on;
            if (on)
                PlayLoop(breathing, breathingSound);
            else
                breathing.Stop();
        }

        public int LettersUntilNextTypeSound() => Random.Range(lettersBetweenTypeSounds.x, lettersBetweenTypeSounds.y + 1);

        /// <param name="voice">A speaker's own typing sound, or null for the default.</param>
        public void PlayTypeSound(Sound voice) => Play(voice != null ? voice : typeSound);

        public void Play(Sound sound, string caption = null)
        {
            ShowCaption(caption);
            AudioClip clip = sound != null ? sound.PickClip() : null;
            if (clip == null)
                return;
            AudioSource voice = effectVoices[nextVoice];
            nextVoice = (nextVoice + 1) % effectVoices.Length;
            voice.pitch = sound.PickPitch();
            voice.PlayOneShot(clip, sound.Volume);
        }

        public void PlayHover() => PlayInterface(hoverSound);

        public void PlayClick() => PlayInterface(clickSound);

        /// <summary>Removes a caption early, if it's the one on screen.</summary>
        public void HideCaption(string caption) => subtitles.Hide(caption);

        void PlayInterface(Sound sound)
        {
            AudioClip clip = sound != null ? sound.PickClip() : null;
            if (clip == null)
                return;
            interfaceVoice.pitch = sound.PickPitch();
            interfaceVoice.PlayOneShot(clip, sound.Volume);
        }

        void ShowCaption(string caption)
        {
            if (!string.IsNullOrEmpty(caption))
                subtitles.Show(caption);
        }

        void PlayLoop(AudioSource source, Sound sound, float volumeScale = 1f)
        {
            loopSounds.TryGetValue(source, out Sound playing);
            loopSounds[source] = sound;
            if (sound == null)
            {
                source.Stop();
                return;
            }
            // Volume updates right away, so the Bloom hum can grow without restarting.
            source.volume = sound.Volume * volumeScale;
            if (sound == playing && source.isPlaying)
                return;
            source.clip = sound.PickClip();
            source.pitch = sound.PickPitch();
            if (source.clip != null)
                source.Play();
            else
                source.Stop();
        }
    }
}
