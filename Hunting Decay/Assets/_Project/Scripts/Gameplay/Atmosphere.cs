using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HuntingDecay
{
    /// <summary>
    /// The season: a color grade, music, and falling leaves or snow in the forest, all read from the day.
    /// The grade runs before the dither, so it changes which palette colors get picked.
    /// </summary>
    public class Atmosphere : MonoBehaviour
    {
        [SerializeField] Volume volume;
        [SerializeField] ParticleSystem leaves;
        [SerializeField] ParticleSystem snow;

        ColorAdjustments colorAdjustments;
        WhiteBalance whiteBalance;

        void Awake()
        {
            // volume.profile is a runtime copy, so the profile asset never changes in Play Mode.
            VolumeProfile profile = volume.profile;
            profile.TryGet(out colorAdjustments);
            profile.TryGet(out whiteBalance);
        }

        public void ApplyDay(DayData day)
        {
            SetGrade(day.colorFilter, day.temperature, day.saturation, day.postExposure);
            AudioManager.Instance.PlayMusic(day.music);
        }

        public void ApplyNeutral()
        {
            SetGrade(Color.white, 0f, 0f, 0f);
            AudioManager.Instance.PlayMusic(null);
            LeaveForest();
        }

        public void EnterForest(DayData day)
        {
            SetWeather(leaves, day.weather == Weather.Leaves, day.weatherRate);
            SetWeather(snow, day.weather == Weather.Snow, day.weatherRate);
            AudioManager.Instance.PlayAmbience(day.forestAmbience);
        }

        public void LeaveForest()
        {
            SetWeather(leaves, false, 0f);
            SetWeather(snow, false, 0f);
            AudioManager.Instance.PlayAmbience(null);
        }

        void SetGrade(Color filter, float temperature, float saturation, float postExposure)
        {
            colorAdjustments.colorFilter.value = filter;
            colorAdjustments.saturation.value = saturation;
            colorAdjustments.postExposure.value = postExposure;
            whiteBalance.temperature.value = temperature;
        }

        static void SetWeather(ParticleSystem particles, bool on, float rate)
        {
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = rate;
            if (on && !particles.isPlaying)
                particles.Play();
            else if (!on)
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
