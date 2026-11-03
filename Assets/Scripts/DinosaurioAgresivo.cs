
using UnityEngine;
using UnityEngine.AI;

namespace Deforestation.Dinosaurus
{
    public class DinosaurioAgresivo : Dinosaur
    {
        [Header("Jugador")]
        [SerializeField] private Transform jugador;
        [SerializeField] private HealthSystem saludJugador;

        [Header("Deteccion")]
        [SerializeField] private float distanciaDeteccion = 25f;
        [SerializeField] private float distanciaPersecucion = 35f;
        [SerializeField] private float distanciaAtaque = 3f;

        [Header("Ataque")]
        [SerializeField] private float danoAtaque = 10f;
        [SerializeField] private float intervaloAtaque = 2f;

        private bool persiguiendo;
        private bool atacando;
        private float siguienteAtaque;

        private void Start()
        {
            if (jugador != null && saludJugador == null)
                saludJugador = jugador.GetComponent<HealthSystem>();

            if (_agent != null && _agent.isOnNavMesh)
                _agent.stoppingDistance = distanciaAtaque;

            CambiarAnimacion(false, false);
        }

        private void Update()
        {
            if (Time.timeScale == 0f)
                return;

            if (jugador == null || saludJugador == null)
                return;

            if (_agent == null || !_agent.isOnNavMesh)
                return;

            // Mientras el jugador conduce, no lo persigue.
            if (GameController.Instance.MachineModeOn)
            {
                Detener();
                return;
            }

            float distancia = Vector3.Distance(
                transform.position,
                jugador.position
            );

            if (!persiguiendo && distancia <= distanciaDeteccion)
                persiguiendo = true;

            if (!persiguiendo)
            {
                DetenerMovimiento();
                return;
            }

            if (distancia > distanciaPersecucion)
            {
                Detener();
                return;
            }

            if (distancia <= distanciaAtaque)
                Atacar();
            else
                Perseguir();
        }

        private void Perseguir()
        {
            atacando = false;
            _agent.isStopped = false;
            _agent.SetDestination(jugador.position);

            CambiarAnimacion(true, false);
        }

        private void Atacar()
        {
            _agent.isStopped = true;

            if (!atacando)
            {
                atacando = true;
                siguienteAtaque = Time.time + 0.6f;
            }

            CambiarAnimacion(false, true);

            if (Time.time >= siguienteAtaque)
            {
                saludJugador.TakeDamage(danoAtaque);
                siguienteAtaque = Time.time + intervaloAtaque;
            }
        }

        private void Detener()
        {
            persiguiendo = false;
            atacando = false;
            DetenerMovimiento();
        }

        private void DetenerMovimiento()
        {
            _agent.isStopped = true;

            if (_agent.hasPath)
                _agent.ResetPath();

            CambiarAnimacion(false, false);
        }

        private void CambiarAnimacion(bool correr, bool atacar)
        {
            if (_anim == null)
                return;

            _anim.SetBool("Run", correr);
            _anim.SetBool("Attack", atacar);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(
                transform.position, distanciaDeteccion
            );

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(
                transform.position, distanciaAtaque
            );
        }
    }
}
