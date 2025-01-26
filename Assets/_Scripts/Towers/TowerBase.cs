using UnityEngine;

public class TowerBase : MonoBehaviour, IPowerUppable
{
    [SerializeField] private TowerObjectPool frostTowerPool;
    [SerializeField] private TowerShotConfig frostTowerConfig;
    
    [SerializeField] private TowerObjectPool fireTowerPool;
    [SerializeField] private TowerShotConfig fireTowerConfig;
    
    [SerializeField] private Transform powerUpDragPoint;
    [SerializeField] private Transform modelTransform;

    private bool isPoweredUp = false;

    public bool IsPowerUppable => !isPoweredUp;
    public Transform AttachPoint => powerUpDragPoint;
    
    public void CollectPowerUp(PowerUp powerUp)
    {
        isPoweredUp = true;
        powerUp.DragToTower(this, () => BeginInstallTower(powerUp));
    }

    private void BeginInstallTower(PowerUp powerUp)
    {
        var sequence = LeanTween.sequence();
        sequence.append(LeanTween.moveLocal(modelTransform.gameObject, modelTransform.localPosition + Vector3.down * 5f, 2f));
        sequence.append(() =>
        {
            SpawnTower(powerUp);
            Destroy(gameObject);
        });
    }

    private Tower SpawnTower(PowerUp powerUp)
    {
        return powerUp.Modifier.IsFireModifier
            ? fireTowerPool.Spawn(fireTowerConfig, transform)
            : frostTowerPool.Spawn(frostTowerConfig, transform);
    }
}
