using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HuntingDecay
{
    /// <summary>
    /// The season: a color grade, music, foliage colors, falling leaves or snow, and random sounds in the forest,
    /// all read from the day. The grade runs before the dither, so it changes which palette colors get picked.
    /// </summary>
    public class Atmosphere : MonoBehaviour
    {
        [SerializeField] Volume volume;
        [SerializeField] ParticleSystem leaves;
        [SerializeField] ParticleSystem snow;
        [SerializeField] RandomSounds randomSounds;

        [Header("Foliage")]
        [Tooltip("Off shows the photos' own green, on follows each day's foliage settings. "
            + "Only sprites using the Seasonal Sprite material change.")]
        [SerializeField] bool seasonalFoliage = true;
        [Tooltip("Which hues count as foliage, in degrees: the center of the band.")]
        [SerializeField, Range(0f, 360f)] float foliageHueCenter = 95f;
        [Tooltip("Degrees on each side of the center that change fully.")]
        [SerializeField, Range(0f, 180f)] float foliageHueRange = 45f;
        [Tooltip("Degrees past the range where the change fades out, so the edge isn't visible.")]
        [SerializeField, Range(0f, 90f)] float foliageHueSoftness = 20f;
        [Tooltip("How much of the foliage's own hue variation survives the change. 0 is one flat hue.")]
        [SerializeField, Range(0f, 1f)] float foliageHueVariation = 0.35f;

        [Header("House at night")]
        [Tooltip("Brightens the dinner photos, which are much darker than breakfast. "
            + "Midtones mostly, so the blacks stay deep. 0 is no change.")]
        [SerializeField, Range(-1f, 1f)] float nightMidtones = 0.9f;
        [SerializeField, Range(-1f, 1f)] float nightShadows = 0.25f;

        static readonly int SeasonStrength = Shader.PropertyToID("_SeasonStrength");
        static readonly int SeasonHue = Shader.PropertyToID("_SeasonHue");
        static readonly int SeasonSaturation = Shader.PropertyToID("_SeasonSaturation");
        static readonly int SeasonWhiten = Shader.PropertyToID("_SeasonWhiten");
        static readonly int SeasonFoliageBand = Shader.PropertyToID("_SeasonFoliageBand");

        ColorAdjustments colorAdjustments;
        WhiteBalance whiteBalance;
        ShadowsMidtonesHighlights shadowsMidtonesHighlights;
        DayData today;

        void Awake()
        {
            // volume.profile is a runtime copy, so the profile asset never changes in Play Mode.
            VolumeProfile profile = volume.profile;
            profile.TryGet(out colorAdjustments);
            profile.TryGet(out whiteBalance);
            profile.TryGet(out shadowsMidtonesHighlights);
            SetNight(false);
        }

        public void ApplyDay(DayData day)
        {
            today = day;
            SetGrade(day.colorFilter, day.temperature, day.saturation, day.postExposure);
            SetFoliage(seasonalFoliage, day.foliageHue, day.foliageHueAmount, day.foliageSaturation, day.foliageWhiten);
            AudioManager.Instance.PlayMusic(day.music);
        }

        /// <summary>Lifts the dark dinner photos. Breakfast and the endings keep their own levels.</summary>
        public void SetNight(bool night)
        {
            float shadows = night ? nightShadows : 0f;
            float midtones = night ? nightMidtones : 0f;
            shadowsMidtonesHighlights.shadows.value = new Vector4(1f, 1f, 1f, shadows);
            shadowsMidtonesHighlights.midtones.value = new Vector4(1f, 1f, 1f, midtones);
        }

        /// <summary>The title screen: one day's grade and foliage on the forest photo, with its own music.</summary>
        public void ApplyTitle(DayData look, Sound music)
        {
            today = null;
            LeaveForest();
            SetGrade(look.colorFilter, look.temperature, look.saturation, look.postExposure);
            SetFoliage(seasonalFoliage, look.foliageHue, look.foliageHueAmount, look.foliageSaturation, look.foliageWhiten);
            SetNight(false);
            SetWeather(leaves, look.weather == Weather.Leaves, look.weatherRate);
            SetWeather(snow, look.weather == Weather.Snow, look.weatherRate);
            AudioManager.Instance.PlayMusic(music);
        }

        /// <summary>The final-day endings keep the last hunt's look, so the photos don't jump in color.</summary>
        public void ApplyHuntGrade()
        {
            if (today != null && today.customHuntGrade)
                SetGrade(today.colorFilter, today.huntTemperature, today.huntSaturation, today.huntPostExposure);
        }

        public void EnterForest(DayData day)
        {
            SetWeather(leaves, day.weather == Weather.Leaves, day.weatherRate);
            SetWeather(snow, day.weather == Weather.Snow, day.weatherRate);
            AudioManager.Instance.PlayAmbience(day.forestAmbience);
            randomSounds.Begin(day.randomSounds);
            if (day.customHuntGrade)
                SetGrade(day.colorFilter, day.huntTemperature, day.huntSaturation, day.huntPostExposure);
        }

        public void LeaveForest()
        {
            StopWeather();
            AudioManager.Instance.PlayAmbience(null);
            randomSounds.Stop();
            // Back to the day's own grade for the house and the endings.
            if (today != null)
                SetGrade(today.colorFilter, today.temperature, today.saturation, today.postExposure);
        }

        void SetGrade(Color filter, float temperature, float saturation, float postExposure)
        {
            colorAdjustments.colorFilter.value = filter;
            colorAdjustments.saturation.value = saturation;
            colorAdjustments.postExposure.value = postExposure;
            whiteBalance.temperature.value = temperature;
        }

        /// <summary>Clears the falling leaves or snow, like when the title closes and the day begins indoors.</summary>
        public void StopWeather()
        {
            SetWeather(leaves, false, 0f);
            SetWeather(snow, false, 0f);
        }

        void SetFoliage(bool on, float hue, float hueAmount, float saturation, float whiten)
        {
            Shader.SetGlobalFloat(SeasonStrength, on ? 1f : 0f);
            Shader.SetGlobalVector(SeasonHue, new Vector4(hue / 360f, hueAmount, foliageHueVariation, 0f));
            Shader.SetGlobalFloat(SeasonSaturation, saturation);
            Shader.SetGlobalFloat(SeasonWhiten, whiten);
            Shader.SetGlobalVector(SeasonFoliageBand,
                new Vector4(foliageHueCenter / 360f, foliageHueRange / 360f, foliageHueSoftness / 360f, 0f));
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
