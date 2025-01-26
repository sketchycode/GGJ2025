using System;
using UnityEngine;

public class Ship : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 500f;
    [SerializeField] private float healthRegenPerSecond = 1f;

    private bool isRepairing;
    private float currentHealth;
    
    public event Action<Ship> Died;
    
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public void Spawn()
    {
        currentHealth = maxHealth;
        isRepairing = false;
    }

    public void BeginAttackCycle()
    {
        isRepairing = false;
    }

    public void BeginRepairCycle()
    {
        isRepairing = true;
    }

    private void Update()
    {
        if (isRepairing) UpdateHealth(healthRegenPerSecond * Time.deltaTime);
    }

    private void UpdateHealth(float healthDelta)
    {
        var tmpHealth = currentHealth;
        currentHealth += healthDelta;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth == 0) Died?.Invoke(this);
    }

    #region IDamageable
    public void TakeDamage(float damage)
    {
        UpdateHealth(-damage);
    }
    #endregion IDamageable
}
