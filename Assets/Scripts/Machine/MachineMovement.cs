using Deforestation.Dinosaurus;
using Deforestation.Recolectables;
using UnityEngine;

namespace Deforestation.Machine
{
	public class MachineMovement : MonoBehaviour
	{
		#region Fields
		[SerializeField] private float _speedForce = 50f;
		[SerializeField] private float _speedRotation = 15f;

		[Header("Jump")]
		[SerializeField] private float _jumpForce = 18f;
		[SerializeField] private float _jumpCooldown = 0.35f;
		[SerializeField] private float _ignoreGroundForceAfterJump = 0.35f;

		[Header("Ground Check")]
		[SerializeField] private float _groundCheckDistance = 4f;
		[SerializeField] private float _groundForce = 100000f;

		[Header("Energy")]
		[SerializeField] private float energyDecayRate = 20f;

		private Rigidbody _rb;
		private Vector3 _movementDirection;
		private Inventory _inventory => GameController.Instance.Inventory;

		private float energyTimer = 0f;

		private bool _isGrounded;
		private bool _jumpRequested;
		private float _jumpCooldownTimer;
		private float _ignoreGroundForceTimer;
		#endregion

		#region Unity Callbacks
		private void Awake()
		{
			_rb = GetComponent<Rigidbody>();
		}

		private void Update()
		{
			if (_jumpCooldownTimer > 0f)
				_jumpCooldownTimer -= Time.deltaTime;

			if (_ignoreGroundForceTimer > 0f)
				_ignoreGroundForceTimer -= Time.deltaTime;

			CheckGround();

			if (_inventory.HasResource(RecolectableType.HyperCrystal))
			{
				// Movement
				_movementDirection = new Vector3(Input.GetAxis("Vertical"), 0, 0);
				transform.Rotate(Vector3.up * _speedRotation * Time.deltaTime * Input.GetAxis("Horizontal"));
				Debug.DrawRay(transform.position, transform.InverseTransformDirection(_movementDirection.normalized) * _speedForce);

				// Energy consumption while moving
				if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
				{
					energyTimer += Time.deltaTime;

					if (energyTimer >= energyDecayRate)
					{
						_inventory.UseResource(RecolectableType.HyperCrystal);
						energyTimer = 0f;
					}
				}
				else
				{
					energyTimer = 0f;
				}

				// Jump
				if (Input.GetKeyDown(KeyCode.Space))
				{
					TryRequestJump();
				}
			}
			else
			{
				_movementDirection = Vector3.zero;
				GameController.Instance.MachineController.StopMoving();
			}
		}

		private void FixedUpdate()
		{
			_rb.AddRelativeForce(_movementDirection.normalized * _speedForce, ForceMode.Impulse);

			if (_jumpRequested)
			{
				DoJump();
				_jumpRequested = false;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.tag == "Tree")
			{
				int index = other.GetComponent<Tree>().Index;
				GameController.Instance.TerrainController.DestroyTree(index, other.transform.position);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			// Hacemos daño por contacto a los Stegasaurus
			HealthSystem target = collision.gameObject.GetComponent<HealthSystem>();
			if (target != null)
			{
				target.TakeDamage(10);
			}
		}
		#endregion

		#region Private Methods
		private void TryRequestJump()
		{
			// Solo puede saltar si:
			// - está en el suelo
			// - no está en cooldown
			// - tiene al menos 1 JumpCrystal
			if (!_isGrounded)
				return;

			if (_jumpCooldownTimer > 0f)
				return;

			if (!_inventory.HasResource(RecolectableType.JumpCrystal))
				return;

			_jumpRequested = true;
			_jumpCooldownTimer = _jumpCooldown;
		}

		private void DoJump()
		{
			// Consumimos 1 cristal rosa por salto
			bool consumed = _inventory.UseResource(RecolectableType.JumpCrystal, 1);

			if (!consumed)
				return;

			// Quitamos velocidad vertical negativa para que el salto responda mejor
			Vector3 velocity = _rb.velocity;
			if (velocity.y < 0f)
				velocity.y = 0f;

			_rb.velocity = velocity;

			// Aplicamos impulso hacia arriba
			_rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);

			_isGrounded = false;
			_ignoreGroundForceTimer = _ignoreGroundForceAfterJump;
		}

		private void CheckGround()
		{
			RaycastHit hit;
			Vector3 direction = -transform.up;

			Debug.DrawRay(transform.position, direction * _groundCheckDistance, _isGrounded ? Color.green : Color.red);

			int layerMask = 1 << LayerMask.NameToLayer("Terrain");

			_isGrounded = Physics.Raycast(transform.position, direction, out hit, _groundCheckDistance, layerMask);

			// Si no está en el suelo, aplicamos fuerza hacia abajo,
			// pero ignoramos eso un instante justo después del salto
			if (!_isGrounded && _ignoreGroundForceTimer <= 0f)
			{
				_rb.AddRelativeForce(direction * _groundForce);
			}
		}
		#endregion

		private void OnDrawGizmos()
		{
		}
	}
}