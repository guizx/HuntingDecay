using System.Collections;
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
        [Tooltip("The open photo's own loop, like the river, on top of the day's ambience.")]
        [SerializeField] AudioSource photoAmbience;
        [SerializeField] AudioSource bloomHum;
        [SerializeField] AudioSource breathing;
        [Tooltip("A distant choir that grows louder with each missed anomaly.")]
        [SerializeField] Sound bloomHumSound;
        [Tooltip("Multiplies the hum's volume for 0, 1 and 2 missed anomalies.")]
        [SerializeField] float[] bloomHumLevels = { 0.1f, 0.4f, 0.8f };
        [Tooltip("The Hunter's breathing while aiming.")]
        [SerializeField] Sound breathingSound;

        [Header("Fades (seconds)")]
        [Tooltip("How long the old music takes to fade out when the music changes or stops.")]
        [SerializeField, Min(0f)] float musicFadeOut = 1.5f;
        [Tooltip("How long new music takes to fade in. It overlaps the old one's fade out.")]
        [SerializeField, Min(0f)] float musicFadeIn = 2.5f;
        [Tooltip("Fade in and out for the ambience loops, like the birds or the river.")]
        [SerializeField, Min(0f)] float ambienceFade = 1f;

        [Header("Effects")]
        [Tooltip("Several sources so overlapping effects can each have their own pitch. "
            + "A sound only takes a free one, so a playing sound is never retuned.")]
        [SerializeField] AudioSource[] effectVoices;
        [SerializeField] Subtitles subtitles;

        [Header("Random sounds")]
        [Tooltip("Its own source, so an anomaly sound can cut a random forest sound short.")]
        [SerializeField] AudioSource randomVoice;

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
        // Each voice's volume before a fade, put back once it has stopped.
        readonly Dictionary<AudioSource, float> effectVolumes = new();
        Coroutine fadingEffects;
        // When each effect voice's current sound ends, so a full set gives up the one closest to done.
        float[] voiceEnds;
        bool isBreathing;
        CrossfadeLoop musicLoop;
        CrossfadeLoop ambienceLoop;
        CrossfadeLoop photoAmbienceLoop;

        public float MasterVolume => AudioListener.volume;

        void Awake()
        {
            Instance = this;
            AudioListener.volume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            // Unity doesn't save this AudioSource setting with the scene, so it has to be set at runtime.
            interfaceVoice.ignoreListenerPause = true;
            voiceEnds = new float[effectVoices.Length];
            musicLoop = new CrossfadeLoop(music);
            ambienceLoop = new CrossfadeLoop(ambience);
            photoAmbienceLoop = new CrossfadeLoop(photoAmbience);
        }

        void Update()
        {
            musicLoop.Tick(Time.unscaledDeltaTime);
            ambienceLoop.Tick(Time.unscaledDeltaTime);
            photoAmbienceLoop.Tick(Time.unscaledDeltaTime);
        }

        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        }

        /// <summary>Crossfades to new music, or fades it out with null.</summary>
        public void PlayMusic(Sound sound) => musicLoop.Play(sound, musicFadeOut, musicFadeIn);

        public void PlayAmbience(Sound sound) => ambienceLoop.Play(sound, ambienceFade, ambienceFade);

        /// <summary>The open photo's loop, like the river. Null fades it out.</summary>
        public void PlayPhotoAmbience(Sound sound) => photoAmbienceLoop.Play(sound, ambienceFade, ambienceFade);

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
            int index = TakeVoice();
            AudioSource voice = effectVoices[index];
            KeepFromFade(voice);
            voice.pitch = sound.PickPitch();
            voice.PlayOneShot(clip, sound.Volume);
            voiceEnds[index] = Time.unscaledTime + clip.length / Mathf.Abs(voice.pitch);
        }

        /// <summary>
        /// A free voice, or else the one whose sound ends soonest, cut short. Pitch is per source, so reusing
        /// a voice that's still playing would bend the sound it's playing, like an anomaly's scare.
        /// </summary>
        int TakeVoice()
        {
            int soonest = 0;
            for (int i = 0; i < effectVoices.Length; i++)
            {
                if (!effectVoices[i].isPlaying)
                    return i;
                if (voiceEnds[i] < voiceEnds[soonest])
                    soonest = i;
            }
            effectVoices[soonest].Stop();
            return soonest;
        }

        /// <summary>True until a random forest sound ends or something cuts it short.</summary>
        public bool IsRandomPlaying => randomVoice.isPlaying;

        public void PlayRandom(Sound sound)
        {
            AudioClip clip = sound != null ? sound.PickClip() : null;
            if (clip == null)
                return;
            KeepFromFade(randomVoice);
            randomVoice.clip = clip;
            randomVoice.volume = sound.Volume;
            randomVoice.pitch = sound.PickPitch();
            randomVoice.Play();
        }

        public void StopRandom() => randomVoice.Stop();

        /// <summary>
        /// Fades out and stops whatever effects are still ringing, like a scope sound or the jumpscare,
        /// so they don't carry into the next scene. Sounds started after the call play normally, so the next
        /// scene can open with its own sound. Loops and the interface aren't touched.
        /// </summary>
        public void FadeOutEffects(float duration)
        {
            if (fadingEffects != null)
            {
                StopCoroutine(fadingEffects);
                RestoreEffectVolumes();
            }
            fadingEffects = StartCoroutine(FadeEffects(duration));
        }

        IEnumerator FadeEffects(float duration)
        {
            // Only the voices ringing now. KeepFromFade takes a voice back out when a new sound reuses it.
            effectVolumes.Clear();
            foreach (AudioSource voice in new List<AudioSource>(effectVoices) { randomVoice })
                if (voice.isPlaying)
                    effectVolumes[voice] = voice.volume;

            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = 1f - t / duration;
                foreach (KeyValuePair<AudioSource, float> pair in effectVolumes)
                    pair.Key.volume = pair.Value * k;
                yield return null;
            }
            foreach (AudioSource voice in effectVolumes.Keys)
                voice.Stop();
            RestoreEffectVolumes();
            fadingEffects = null;
        }

        /// <summary>A voice about to play a new sound leaves the fade, back at its own volume.</summary>
        void KeepFromFade(AudioSource voice)
        {
            if (effectVolumes.Remove(voice, out float volume))
                voice.volume = volume;
        }

        void RestoreEffectVolumes()
        {
            foreach (KeyValuePair<AudioSource, float> pair in effectVolumes)
                pair.Key.volume = pair.Value;
            effectVolumes.Clear();
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
