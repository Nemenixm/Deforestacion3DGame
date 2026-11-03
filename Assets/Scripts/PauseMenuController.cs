
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PauseMenuController : MonoBehaviour
{[SerializeField] private GameObject menuPausa;

[SerializeField] private Button botonContinuar;
[SerializeField] private Button botonSalir;

[SerializeField] private Slider sliderMusic;
[SerializeField] private Slider sliderFX;

[SerializeField] private AudioMixer audioMixer;

[SerializeField] private GameObject deathPanel;

    private const string MUSIC_KEY = "MusicVolume";
    private const string FX_KEY = "FXVolume";

    private bool isPaused = false;

    private void Start()
    {
        // El menu comienza oculto
        if (menuPausa != null)
            menuPausa.SetActive(false);

        // Conectar los botones automaticamente
        if (botonContinuar != null)
            botonContinuar.onClick.AddListener(Continuar);

        if (botonSalir != null)
            botonSalir.onClick.AddListener(Salir);

        // Configurar slider de musica
        if (sliderMusic != null)
        {
            sliderMusic.minValue = 0f;
            sliderMusic.maxValue = 1f;
            sliderMusic.wholeNumbers = false;

            float volume = PlayerPrefs.GetFloat(
                MUSIC_KEY, 1f
            );

            sliderMusic.SetValueWithoutNotify(volume);

            sliderMusic.onValueChanged.AddListener(
                CambiarVolumenMusica
            );

            CambiarVolumenMusica(volume);
        }

        // Configurar slider de efectos
        if (sliderFX != null)
        {
            sliderFX.minValue = 0f;
            sliderFX.maxValue = 1f;
            sliderFX.wholeNumbers = false;

            float volume = PlayerPrefs.GetFloat(
                FX_KEY, 1f
            );

            sliderFX.SetValueWithoutNotify(volume);

            sliderFX.onValueChanged.AddListener(
                CambiarVolumenFX
            );

            CambiarVolumenFX(volume);
        }
    }

    private void Update()
    {
        // Abrir/cerrar con la tecla 1
        if (Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            if (isPaused)
                Continuar();
            else
                AbrirPausa();
        }
    }

    private bool HayGameOver()
    {
        return deathPanel != null &&
               deathPanel.activeInHierarchy;
    }

    public void AbrirPausa()
    {
        // No abrir durante Game Over
        if (HayGameOver() || menuPausa == null)
            return;

        if (isPaused)
            return;

        menuPausa.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;

        // Mostrar el raton
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Continuar()
    {
        // No reanudar si el jugador ha muerto
        if (HayGameOver() || !isPaused)
            return;

        menuPausa.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        // Volver a controlar al personaje
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayerPrefs.Save();
    }

    public void Salir()
    {
        PlayerPrefs.Save();

        Time.timeScale = 1f;

        #if UNITY_EDITOR
        // Detener Play en el editor
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Cerrar el juego compilado
        Application.Quit();
        #endif
    }

    public void CambiarVolumenMusica(float volumen)
    {
        CambiarVolumen("MusicVolume", volumen);

        PlayerPrefs.SetFloat(
            MUSIC_KEY, volumen
        );
    }

    public void CambiarVolumenFX(float volumen)
    {
        CambiarVolumen("FXVolume", volumen);

        PlayerPrefs.SetFloat(
            FX_KEY, volumen
        );
    }

    private void CambiarVolumen(
        string parametro, float volumen)
    {
        if (audioMixer == null)
            return;

        // Convertir de 0-1 a decibelios
        float decibelios;

        if (volumen <= 0.0001f)
        {
            decibelios = -80f;
        }
        else
        {
            decibelios = Mathf.Log10(volumen) * 20f;
        }

        bool resultado = audioMixer.SetFloat(
            parametro, decibelios
        );

        if (!resultado)
        {
            Debug.LogWarning(
                "Parametro del AudioMixer no encontrado: "
                + parametro
            );
        }
    }

    private void OnDestroy()
    {
        if (botonContinuar != null)
            botonContinuar.onClick.RemoveListener(Continuar);

        if (botonSalir != null)
            botonSalir.onClick.RemoveListener(Salir);

        if (sliderMusic != null)
            sliderMusic.onValueChanged.RemoveListener(
                CambiarVolumenMusica
            );

        if (sliderFX != null)
            sliderFX.onValueChanged.RemoveListener(
                CambiarVolumenFX
            );
    }
}
