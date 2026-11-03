
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Deforestation;

public class DeathScreenController : MonoBehaviour
{
    [Header("Pantalla de muerte")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject settingsPanel;

    [Header("Sistemas de vida")]
    [SerializeField] private HealthSystem playerHealth;
    [SerializeField] private HealthSystem machineHealth;

    private bool isDead = false;
    private bool restarting = false;

    private void Awake()
    {
        // Ocultar el panel al iniciar
        if (deathPanel != null)
            deathPanel.SetActive(false);

        // Configurar el boton
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    private void OnEnable()
    {
        // Detectar la muerte del jugador
        if (playerHealth != null)
            playerHealth.OnDeath += ShowDeathScreen;

        // Detectar la muerte de la maquina
        if (machineHealth != null)
            machineHealth.OnDeath += ShowDeathScreen;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDeath -= ShowDeathScreen;

        if (machineHealth != null)
            machineHealth.OnDeath -= ShowDeathScreen;
    }

    private void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);
    }

   

private void ShowDeathScreen()
{
    if (isDead) return;

    isDead = true;

    // Ocultar el panel de ajustes
    if (settingsPanel != null)
        settingsPanel.SetActive(false);

    // Mostrar la pantalla de muerte
    if (deathPanel != null)
    {
        deathPanel.SetActive(true);
        deathPanel.transform.SetAsLastSibling();
    }

    // Pausar el juego
    Time.timeScale = 0f;

    // Mostrar el cursor
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    Debug.Log("PANTALLA DE MUERTE ACTIVADA");
}



   
public void RestartGame()
{
    if (restarting) return;

    restarting = true;

    // Restaurar la velocidad normal del juego
    Time.timeScale = 1f;

    // Recargar la escena desde el principio
    SceneManager.LoadScene(
        SceneManager.GetActiveScene().buildIndex
    );
}


    private IEnumerator RestartRoutine()
    {
        restarting = true;

        // Reactivar el tiempo
        Time.timeScale = 1f;

        // Eliminar el GameController persistente
        // para que no conserve la partida anterior
        if (GameController.Instance != null)
        {
            Destroy(GameController.Instance.gameObject);
        }

        // Esperar a que Unity complete la destruccion
        yield return null;

        // Recargar la escena desde el inicio
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}
