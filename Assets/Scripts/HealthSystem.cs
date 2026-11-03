using System;
using UnityEngine;

namespace Deforestation
{

	public class HealthSystem : MonoBehaviour
	{
		public event Action<float> OnHealthChanged;
		public event Action OnDeath;

		[SerializeField]
		private float _maxHealth = 100f;
		private float _currentHealth;

		private void Awake()
		{
			_currentHealth = _maxHealth;
		}

		
	public void TakeDamage(float damage)
	{
		if (_currentHealth <= 0 || damage <= 0)
			return;
		_currentHealth = Mathf.Max(0f, _currentHealth - damage);
		OnHealthChanged?.Invoke(_currentHealth);
		Debug.Log(gameObject.name + " - Vida: " + _currentHealth);
		if (_currentHealth <= 0f)
		{
			Debug.Log(gameObject.name + " HA MUERTO");
			Die();
		}
	}


		public void Heal(float amount)
		{
			_currentHealth += amount;
			_currentHealth = Mathf.Min(_currentHealth, _maxHealth);
			OnHealthChanged?.Invoke(_currentHealth);
		}

		public void SetHealth(float value)
		{
			_currentHealth = value;
			_currentHealth = Mathf.Min(_currentHealth, _maxHealth);
			OnHealthChanged?.Invoke(_currentHealth);
		}

		private void Die()
		{
			OnDeath?.Invoke();
			// Aqu� puedes a�adir l�gica adicional para la muerte, como destruir el objeto.
		}
	}

}