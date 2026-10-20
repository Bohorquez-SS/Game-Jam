using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GNight
{
    /// <summary>
    /// Posprocesado URP creado por código: tonemapping ACES, exposición (brillo de opciones), color grading
    /// cálido/frío según el acto, bloom sutil, viñeta suave y opcionalmente VHS (grano, aberración, distorsión).
    /// </summary>
    public class PostFX : MonoBehaviour
    {
        public static PostFX I;
        Volume vol;
        ColorAdjustments color; Bloom bloom; Vignette vig; FilmGrain grain; ChromaticAberration ca; LensDistortion lens; Tonemapping tone; WhiteBalance wb;
        float pulse, shock;

        void Awake()
        {
            I = this;
            vol = gameObject.AddComponent<Volume>();
            vol.isGlobal = true; vol.priority = 10;
            var p = ScriptableObject.CreateInstance<VolumeProfile>();
            tone = p.Add<Tonemapping>(true); tone.mode.Override(TonemappingMode.ACES);
            color = p.Add<ColorAdjustments>(true);
            wb = p.Add<WhiteBalance>(true);
            bloom = p.Add<Bloom>(true); bloom.intensity.Override(0.35f); bloom.threshold.Override(1.05f); bloom.scatter.Override(0.6f);
            vig = p.Add<Vignette>(true); vig.intensity.Override(0.22f); vig.smoothness.Override(0.45f);
            grain = p.Add<FilmGrain>(true); grain.type.Override(FilmGrainLookup.Thin1);
            ca = p.Add<ChromaticAberration>(true);
            lens = p.Add<LensDistortion>(true);
            vol.profile = p;
            Settings.Changed += Apply;
            SetMood(Mood.Warm);
            Apply();
        }

        void OnDestroy() { Settings.Changed -= Apply; }

        public void Apply()
        {
            int v = Settings.Vhs;
            grain.intensity.Override(v == 0 ? 0f : (v == 1 ? 0.18f : 0.38f));
            ca.intensity.Override(v == 0 ? 0f : (v == 1 ? 0.08f : 0.2f));
            lens.intensity.Override(v == 0 ? 0f : (v == 1 ? -0.06f : -0.14f));
        }

        public void SetMood(Mood m)
        {
            switch (m)
            {
                case Mood.Warm: wb.temperature.Override(10f); wb.tint.Override(0f); color.saturation.Override(-4f); color.contrast.Override(8f); break;
                case Mood.Night: wb.temperature.Override(0f); wb.tint.Override(0f); color.saturation.Override(-10f); color.contrast.Override(10f); break;
                case Mood.Dark: wb.temperature.Override(-14f); wb.tint.Override(4f); color.saturation.Override(-18f); color.contrast.Override(14f); break;
                case Mood.Emergency: wb.temperature.Override(-6f); wb.tint.Override(10f); color.saturation.Override(-8f); color.contrast.Override(16f); break;
                case Mood.Dawn: wb.temperature.Override(18f); wb.tint.Override(-4f); color.saturation.Override(4f); color.contrast.Override(4f); break;
            }
        }

        /// <summary>Golpe visual (jumpscare): distorsión y aberración fuertes un instante.</summary>
        public void Shock(float amount) { shock = Mathf.Max(shock, amount); }

        void Update()
        {
            shock = Mathf.MoveTowards(shock, 0f, Time.unscaledDeltaTime * 1.6f);
            float baseExp = Mathf.Lerp(-0.6f, 1.1f, Settings.Brightness);
            color.postExposure.Override(baseExp);
            int v = Settings.Vhs;
            float baseCa = v == 0 ? 0f : (v == 1 ? 0.08f : 0.2f), baseLens = v == 0 ? 0f : (v == 1 ? -0.06f : -0.14f);
            ca.intensity.Override(Mathf.Clamp01(baseCa + shock));
            lens.intensity.Override(Mathf.Clamp(baseLens - shock * 0.45f, -1f, 1f));
            vig.intensity.Override(0.22f + shock * 0.3f);
        }
    }
}
