using UnityEngine;

namespace HuntingDecay
{
    /// <summary>
    /// A looping channel, like the music, made of two sources that take turns: the old loop fades out on one
    /// while the new one fades in on the other, so changes never cut. The same Sound keeps playing untouched.
    /// </summary>
    public class CrossfadeLoop
    {
        readonly AudioSource[] sources;
        readonly float[] targets = new float[2];
        readonly float[] speeds = new float[2];
        int current;
        Sound playing;

        /// <param name="source">The scene's source. Its twin is added next to it with the same settings.</param>
        public CrossfadeLoop(AudioSource source)
        {
            AudioSource twin = source.gameObject.AddComponent<AudioSource>();
            twin.outputAudioMixerGroup = source.outputAudioMixerGroup;
            twin.loop = source.loop;
            twin.playOnAwake = false;
            twin.spatialBlend = source.spatialBlend;
            twin.priority = source.priority;
            twin.ignoreListenerPause = source.ignoreListenerPause;
            sources = new[] { source, twin };
        }

        public Sound Playing => playing;

        /// <summary>Fades to a new Sound, or to silence with null.</summary>
        public void Play(Sound sound, float fadeOut, float fadeIn)
        {
            if (sound == playing && (sound == null || sources[current].isPlaying))
                return;
            FadeTo(current, 0f, fadeOut);
            playing = sound;
            AudioClip clip = sound != null ? sound.PickClip() : null;
            if (clip == null)
                return;

            current = 1 - current;
            AudioSource next = sources[current];
            next.clip = clip;
            next.pitch = sound.PickPitch();
            next.volume = 0f;
            next.Play();
            FadeTo(current, sound.Volume, fadeIn);
        }

        /// <summary>Call every frame. Unscaled time, so fades finish even while the game is paused.</summary>
        public void Tick(float deltaTime)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource source = sources[i];
                source.volume = Mathf.MoveTowards(source.volume, targets[i], speeds[i] * deltaTime);
                if (targets[i] <= 0f && source.volume <= 0f && source.isPlaying)
                    source.Stop();
            }
        }

        void FadeTo(int index, float target, float duration)
        {
            targets[index] = target;
            speeds[index] = duration > 0f ? Mathf.Abs(target - sources[index].volume) / duration : float.PositiveInfinity;
        }
    }
}
