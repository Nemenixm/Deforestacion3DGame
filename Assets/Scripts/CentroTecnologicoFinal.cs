
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CentroTecnologicoFinal : MonoBehaviour
{
    [Header("Maquina")]
    [SerializeField] private Transform theMachine;

    [Header("Panel de Victoria")]
    [SerializeField] private GameObject panelVictoria;

    [Header("Panel de Muerte")]
    [SerializeField] private GameObject deathPanel;

    [Header("Reinicio Automatico")]
    [SerializeField] private float tiempoReinicio = 10f;

    private bool victoriaActivada = false;

    private void Start()
    {
        // El panel comienza oculto
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // No activar la victoria dos veces
        if (victoriaActivada)
            return;

        // No ganar si ya estamos en Game Over
        if (deathPanel != null &&
            deathPanel.activeInHierarchy)
            return;

        // No ganar mientras el juego esta pausado
        if (Time.timeScale == 0f)
            return;

        if (theMachine == null)
        {
            Debug.LogWarning(
                "No se ha asignado TheMachine."
            );
            return;
        }

        // Comprobar si el collider pertenece
        // a la maquina o a alguno de sus hijos
        bool esMaquina =
            other.transform == theMachine ||
            other.transform.IsChildOf(theMachine);

        // Comprobar tambien el Rigidbody asociado
        if (!esMaquina && other.attachedRigidbody != null)
        {
            Transform rbTransform =
                other.attachedRigidbody.transform;

            esMaquina =
                rbTransform == theMachine ||
                rbTransform.IsChildOf(theMachine);
        }

        // Si no es la maquina, ignorar
        if (!esMaquina)
            return;

        ActivarVictoria();
    }

    private void ActivarVictoria()
    {
        victoriaActivada = true;

        Debug.Log("¡MISION COMPLETADA!");

        // Mostrar pantalla de victoria
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }

        // Congelar el juego
        Time.timeScale = 0f;

        // Mostrar el raton
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Esperar 10 segundos y reiniciar
        StartCoroutine(ReinicioAutomatico());
    }

    private IEnumerator ReinicioAutomatico()
    {
        // Espera en tiempo real aunque
        // Time.timeScale sea 0
        yield return new WaitForSecondsRealtime(
            tiempoReinicio
        );

        // Restaurar tiempo y audio
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Reiniciar la escena actual
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
