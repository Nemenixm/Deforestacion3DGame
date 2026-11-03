
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Light))]
public class DayNightCycle : MonoBehaviour
{
    [Header("Tiempo")]
    [SerializeField] private float dayDuration = 300f;

    [Range(0f, 24f)]
    [SerializeField] private float startHour = 8f;

    [Header("Cielo nocturno")]
    [SerializeField] private Material nightSkybox;

    [Header("Colores del sol")]
    [SerializeField] private Color morningColor =
        new Color(1f, 0.97f, 0.85f);

    [SerializeField] private Color sunsetColor =
        new Color(1f, 0.48f, 0.2f);

    [SerializeField] private Color nightColor =
        new Color(0.25f, 0.4f, 0.8f);

    [Header("Iluminacion ambiental")]
    [SerializeField] private Color morningAmbient =
        new Color(0.6f, 0.65f, 0.7f);

    [SerializeField] private Color sunsetAmbient =
        new Color(0.7f, 0.35f, 0.2f);

    [SerializeField] private Color nightAmbient =
        new Color(0.04f, 0.06f, 0.13f);

    [Header("Intensidad")]
    [SerializeField] private float dayIntensity = 1.3f;
    [SerializeField] private float sunsetIntensity = 0.7f;

    [Header("Brillo del cielo")]
    [SerializeField] private float dayExposure = 1.2f;
    [SerializeField] private float sunsetExposure = 0.65f;
    [SerializeField] private float nightExposure = 0.08f;
    [SerializeField] private float starsExposure = 1f;

    [Header("Transicion de cielos")]
    [SerializeField] private float minimumSkyExposure = 0.001f;

    private Light sun;
    private float currentHour;
    private float sunDirection;

    private Material originalSkybox;
    private Material daySkyInstance;
    private Material nightSkyInstance;

    private AmbientMode originalAmbientMode;
    private Color originalAmbientColor;
    private Light originalSun;

    private Color originalDayTint = Color.white;
    private Color originalNightTint = Color.white;

    private bool initialized;

    private void Start()
    {
        sun = GetComponent<Light>();
        sun.shadows = LightShadows.Soft;

        currentHour = startHour;
        sunDirection = transform.eulerAngles.y;

        originalSkybox = RenderSettings.skybox;
        originalAmbientMode = RenderSettings.ambientMode;
        originalAmbientColor = RenderSettings.ambientLight;
        originalSun = RenderSettings.sun;

        RenderSettings.sun = sun;
        RenderSettings.ambientMode = AmbientMode.Flat;

        // Mantener el cielo original del proyecto
        if (originalSkybox != null)
        {
            daySkyInstance = new Material(originalSkybox);
            originalDayTint = GetSkyTint(daySkyInstance);
        }

        // Crear una copia del cielo nocturno
        if (nightSkybox != null)
        {
            nightSkyInstance = new Material(nightSkybox);
            originalNightTint = GetSkyTint(nightSkyInstance);
        }
        else
        {
            Debug.LogWarning(
                "Falta asignar el cielo nocturno."
            );
        }

        initialized = true;
        UpdateLighting();
    }

    private void Update()
    {
        currentHour += Time.deltaTime * 24f /
            Mathf.Max(1f, dayDuration);

        currentHour = Mathf.Repeat(currentHour, 24f);

        UpdateLighting();
    }

    private float Transition(float start, float end)
    {
        float t = Mathf.InverseLerp(
            start, end, currentHour);

        return Mathf.SmoothStep(0f, 1f, t);
    }

    private void UpdateLighting()
    {
        if (!initialized) return;

        // Movimiento continuo del sol
        float angle = currentHour * 15f - 90f;

        transform.rotation = Quaternion.Euler(
            angle, sunDirection, 0f
        );

        Color lightColor;
        Color ambientColor;
        float intensity;
        float exposure;

        // AMANECER
        if (currentHour >= 5f && currentHour < 9f)
        {
            float t = Transition(5f, 9f);

            lightColor = Color.Lerp(
                nightColor, morningColor, t);

            ambientColor = Color.Lerp(
                nightAmbient, morningAmbient, t);

            intensity = Mathf.Lerp(
                0f, dayIntensity, t);

            exposure = Mathf.Lerp(
                nightExposure, dayExposure, t);
        }
        // DIA
        else if (currentHour >= 9f && currentHour < 15f)
        {
            lightColor = morningColor;
            ambientColor = morningAmbient;
            intensity = dayIntensity;
            exposure = dayExposure;
        }
        // TARDE
        else if (currentHour >= 15f && currentHour < 18f)
        {
            float t = Transition(15f, 18f);

            lightColor = Color.Lerp(
                morningColor, sunsetColor, t);

            ambientColor = Color.Lerp(
                morningAmbient, sunsetAmbient, t);

            intensity = Mathf.Lerp(
                dayIntensity, sunsetIntensity, t);

            exposure = Mathf.Lerp(
                dayExposure, sunsetExposure, t);
        }
        // ANOCHECER
        else if (currentHour >= 18f && currentHour < 20f)
        {
            float t = Transition(18f, 20f);

            lightColor = Color.Lerp(
                sunsetColor, nightColor, t);

            ambientColor = Color.Lerp(
                sunsetAmbient, nightAmbient, t);

            intensity = Mathf.Lerp(
                sunsetIntensity, 0f, t);

            exposure = Mathf.Lerp(
                sunsetExposure, nightExposure, t);
        }
        // NOCHE
        else
        {
            lightColor = nightColor;
            ambientColor = nightAmbient;
            intensity = 0f;
            exposure = nightExposure;
        }

        sun.color = lightColor;

        float sunHeight = -transform.forward.y;

        float horizonFactor = Mathf.SmoothStep(
            0f, 1f,
            Mathf.InverseLerp(
                -0.08f, 0.15f, sunHeight)
        );

        sun.intensity = intensity * horizonFactor;

        // Luz ambiental gradual
        RenderSettings.ambientLight = ambientColor;

        UpdateSkybox(exposure);
    }

    private void UpdateSkybox(float normalExposure)
    {
        // Horarios de transicion:
        // 18:00 - 19:30: apagar cielo diurno
        // 19:30 - 21:00: encender estrellas
        // 04:00 - 05:30: apagar estrellas
        // 05:30 - 07:00: encender cielo diurno

        bool useNightSky =
            (currentHour >= 19.5f ||
             currentHour < 5.5f) &&
            nightSkyInstance != null;

        Material selectedSkybox =
            useNightSky ? nightSkyInstance : daySkyInstance;

        if (selectedSkybox == null)
            return;

        float targetExposure = normalExposure;

        if (useNightSky)
        {
            // Aparecen las estrellas progresivamente
            if (currentHour >= 19.5f &&
                currentHour < 21f)
            {
                float t = Transition(19.5f, 21f);

                targetExposure = Mathf.Lerp(
                    minimumSkyExposure,
                    starsExposure,
                    t
                );
            }
            // Desaparecen antes del amanecer
            else if (currentHour >= 4f &&
                     currentHour < 5.5f)
            {
                float t = Transition(4f, 5.5f);

                targetExposure = Mathf.Lerp(
                    starsExposure,
                    minimumSkyExposure,
                    t
                );
            }
            else
            {
                targetExposure = starsExposure;
            }
        }
        else
        {
            // El cielo de dia se oscurece
            if (currentHour >= 18f &&
                currentHour < 19.5f)
            {
                float t = Transition(18f, 19.5f);

                targetExposure = Mathf.Lerp(
                    sunsetExposure,
                    minimumSkyExposure,
                    t
                );
            }
            // El cielo de dia vuelve al amanecer
            else if (currentHour >= 5.5f &&
                     currentHour < 7f)
            {
                float t = Transition(5.5f, 7f);

                targetExposure = Mathf.Lerp(
                    minimumSkyExposure,
                    normalExposure,
                    t
                );
            }
        }

        // Cambiar cuando ambos cielos estan oscuros
        if (RenderSettings.skybox != selectedSkybox)
        {
            RenderSettings.skybox = selectedSkybox;
        }

        ApplySkyAppearance(
            selectedSkybox,
            targetExposure,
            useNightSky
        );
    }

    private Color GetSkyTint(Material sky)
    {
        if (sky.HasProperty("_SkyTint"))
            return sky.GetColor("_SkyTint");

        if (sky.HasProperty("_Tint"))
            return sky.GetColor("_Tint");

        return Color.white;
    }

    private void ApplySkyAppearance(
        Material sky,
        float exposure,
        bool isNight)
    {
        // Brillo gradual
        bool hasExposure = sky.HasProperty("_Exposure");

        if (hasExposure)
        {
            sky.SetFloat("_Exposure", exposure);
        }

        // Color original del material
        Color tint = isNight
            ? originalNightTint
            : originalDayTint;

        if (!isNight)
        {
            // Naranja durante el atardecer
            float sunsetAmount = 0f;

            if (currentHour >= 15f &&
                currentHour < 18f)
            {
                sunsetAmount = Transition(15f, 18f);
            }
            else if (currentHour >= 18f &&
                     currentHour < 19.5f)
            {
                sunsetAmount = 1f;
            }

            Color warmTint =
                new Color(1f, 0.75f, 0.62f);

            tint *= Color.Lerp(
                Color.white,
                warmTint,
                sunsetAmount
            );
        }

        // Alternativa para shaders sin Exposure:
        // oscurecer el color del material
        if (!hasExposure)
        {
            float maximum = isNight
                ? Mathf.Max(0.01f, starsExposure)
                : Mathf.Max(0.01f, dayExposure);

            tint *= Mathf.Clamp01(
                exposure / maximum
            );
        }

        if (sky.HasProperty("_SkyTint"))
        {
            sky.SetColor("_SkyTint", tint);
        }
        else if (sky.HasProperty("_Tint"))
        {
            sky.SetColor("_Tint", tint);
        }
    }

    private void OnDestroy()
    {
        if (!initialized) return;

        RenderSettings.ambientMode = originalAmbientMode;
        RenderSettings.ambientLight = originalAmbientColor;
        RenderSettings.sun = originalSun;
        RenderSettings.skybox = originalSkybox;

        if (daySkyInstance != null)
            Destroy(daySkyInstance);

        if (nightSkyInstance != null)
            Destroy(nightSkyInstance);
    }
}
