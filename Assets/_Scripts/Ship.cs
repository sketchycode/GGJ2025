using System;
using UnityEngine;

public class Ship : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 500f;
    [SerializeField] private float healthRegenPerSecond = 1f;
    [SerializeField] private float maxCharge = 100f;
    [SerializeField] private float chargeRatePerSecond = 1f;

    private bool isCharging;
    private bool isRepairing;
    private float currentCharge;
    private float currentHealth;
    
    public event Action ChargeChanged;
    public event Action HealthChanged;

    public void Spawn()
    {
        currentHealth = maxHealth;
        currentCharge = 0;
        isCharging = false;
        isRepairing = false;
    }

    public void BeginAttackCycle()
    {
        isCharging = true;
    }

    public void BeginRepairCycle()
    {
        isRepairing = true;
    }

    private void Update()
    {
        if (isCharging) UpdateCharge(chargeRatePerSecond * Time.deltaTime);
        if (isRepairing) UpdateHealth(healthRegenPerSecond * Time.deltaTime);
    }

    private void UpdateCharge(float chargeDelta)
    {
        var tmpCharge = currentCharge;
        currentCharge += chargeDelta;
        currentCharge = Mathf.Clamp(currentCharge, 0, maxCharge);
        if (tmpCharge != currentCharge) ChargeChanged?.Invoke();
    }

    private void UpdateHealth(float healthDelta)
    {
        var tmpHealth = currentHealth;
        currentHealth += healthDelta;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (tmpHealth != currentCharge) HealthChanged?.Invoke();
    }

    #region IDamageable
    public void TakeDamage(float damage)
    {
        UpdateHealth(-damage);
    }
    #endregion IDamageable
}
