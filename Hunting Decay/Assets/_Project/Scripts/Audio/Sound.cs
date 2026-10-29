using UnityEngine;

namespace HuntingDecay
{
    /// <summary>
    /// One sound and its mix settings, as an asset. Everything that plays audio points to one of these,
    /// so each sound's volume lives in one place and the same sound can be reused anywhere.
    /// </summary>
    [CreateAssetMenu(menuName = "Hunting Decay/Sound", fileName = "Sound")]
    public class Sound : ScriptableObject
    {
        [Tooltip("One is picked at random each time it plays.")]
        [SerializeField] AudioClip[] clips = new AudioClip[0];
        [SerializeField, Range(0f, 1f)] float volume = 1f;
        [Tooltip("1 is the clip's own pitch. Higher is higher and faster.")]
        [SerializeField, Range(0.5f, 2f)] float pitch = 1f;
        [Tooltip("Random pitch offset in both directions, like 0.05 for 0.95 to 1.05 around Pitch.")]
        [SerializeField, Range(0f, 0.5f)] float pitchVariation;

        public float Volume => volume;

        public AudioClip PickClip() => clips.Length == 0 ? null : clips[Random.Range(0, clips.Length)];

        public float PickPitch() => pitch + Random.Range(-pitchVariation, pitchVariation);
    }
}
