using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TextMeshProUGUI waveLabel;
    [SerializeField] private TextMeshProUGUI enemiesRemainingLabel;
    [SerializeField] private TextMeshProUGUI timeRemainingLabel;
    [SerializeField] private GameObject enemiesRemainingLabelContainer;
    [SerializeField] private GameObject timeRemainingLabelContainer;
    [SerializeField] private TextMeshProUGUI shipHealthRemainingLabel;
    
    private void Update()
    {
        waveLabel.text = $"Wave: {gameManager.CurrentWave} / {gameManager.MaxWaves}";
        shipHealthRemainingLabel.text = $"Ship: {(gameManager.ShipHealth / (float)gameManager.ShipMaxHealth)*100:F1}%";

        if (gameManager.InAttackPhase)
        {
            enemiesRemainingLabelContainer.SetActive(true);
            timeRemainingLabelContainer.SetActive(false);
            enemiesRemainingLabel.text = $"Enemies: {gameManager.EnemiesRemainingCurrentWave}";
        }
        else
        {
            enemiesRemainingLabelContainer.SetActive(false);
            timeRemainingLabelContainer.SetActive(true);
            timeRemainingLabel.text = $"{gameManager.BuildPhaseRemainingTime:F1}s";
        }
    }
}
