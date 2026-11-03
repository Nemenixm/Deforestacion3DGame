
using UnityEngine;
using Deforestation;
using Deforestation.Machine;

public class HydrogenWaterDamage : MonoBehaviour
{
    [Header("Agua")]
    [SerializeField] private Renderer waterRenderer;

    [Header("Jugador")]
    [SerializeField] private CharacterController player;
    [SerializeField] private HealthSystem playerHealth;

    [Header("Maquina")]
    [SerializeField] private MachineController machine;
    [SerializeField] private float machineCheckYOffset = 0f;

    [Header("Daño")]
    [SerializeField] private float playerDamagePerSecond = 10f;
    [SerializeField] private float machineDamagePerSecond = 5f;
    [SerializeField] private float minimumDepth = 0.1f;

    private float playerTimer;
    private float machineTimer;

    private bool playerDead;
    private bool machineDead;

    private void Awake()
    {
        if (waterRenderer == null)
            waterRenderer = GetComponentInChildren<Renderer>();

        if (player != null && playerHealth == null)
            playerHealth = player.GetComponent<HealthSystem>();
    }

    private void Start()
    {
        if (playerHealth != null)
            playerHealth.OnDeath += OnPlayerDeath;

        if (machine != null && machine.HealthSystem != null)
            machine.HealthSystem.OnDeath += OnMachineDeath;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDeath -= OnPlayerDeath;

        if (machine != null && machine.HealthSystem != null)
            machine.HealthSystem.OnDeath -= OnMachineDeath;
    }

    private void Update()
    {
        if (waterRenderer == null) return;

        Bounds water = waterRenderer.bounds;

        // DAÑO AL JUGADOR
        if (player != null && playerHealth != null && !playerDead)
        {
            bool active =
                player.enabled &&
                player.gameObject.activeInHierarchy &&
                !GameController.Instance.MachineModeOn;

            bool submerged =
                active && IsSubmerged(player.bounds, water);

            if (submerged)
            {
                playerTimer += Time.deltaTime;

                if (playerTimer >= 1f)
                {
                    playerTimer -= 1f;
                    playerHealth.TakeDamage(playerDamagePerSecond);
                }
            }
            else
            {
                playerTimer = 0f;
            }
        }

        // DAÑO A LA MÁQUINA
        if (machine != null &&
            machine.HealthSystem != null &&
            !machineDead)
        {
            Vector3 machinePoint = machine.transform.position
                + Vector3.up * machineCheckYOffset;

            bool submerged =
                IsPointInWater(machinePoint, water);

            if (submerged)
            {
                machineTimer += Time.deltaTime;

                if (machineTimer >= 1f)
                {
                    machineTimer -= 1f;

                    machine.HealthSystem.TakeDamage(
                        machineDamagePerSecond
                    );
                }
            }
            else
            {
                machineTimer = 0f;
            }
        }
    }

    private bool IsSubmerged(Bounds subject, Bounds water)
    {
        bool overlapsXZ =
            subject.max.x >= water.min.x &&
            subject.min.x <= water.max.x &&
            subject.max.z >= water.min.z &&
            subject.min.z <= water.max.z;

        bool belowSurface =
            subject.min.y < water.max.y - minimumDepth;

        return overlapsXZ && belowSurface;
    }

    private bool IsPointInWater(Vector3 point, Bounds water)
    {
        return
            point.x >= water.min.x &&
            point.x <= water.max.x &&
            point.z >= water.min.z &&
            point.z <= water.max.z &&
            point.y < water.max.y - minimumDepth;
    }

    private void OnPlayerDeath()
    {
        playerDead = true;
    }

    private void OnMachineDeath()
    {
        machineDead = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (machine == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            machine.transform.position
                + Vector3.up * machineCheckYOffset,
            0.5f
        );
    }
}
