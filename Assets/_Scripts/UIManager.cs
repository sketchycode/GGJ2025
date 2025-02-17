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
    [SerializeField] private ProgressBar shipHealthBar;
    [SerializeField] private Color MaxHealthColor = ColorExtensions.FromHex(0x41E052);
    [SerializeField] private Color MinHealthColor = ColorExtensions.FromHex(0xE0414D);
    [SerializeField] private GameObject interactButton;
    [SerializeField] private TextMeshProUGUI interactDescription;
    
    private void Update()
    {
        waveLabel.text = $"Wave: {gameManager.CurrentWave} / {gameManager.MaxWaves}";
        var shipHealthNormalized = gameManager.ShipHealth / (float)gameManager.ShipMaxHealth;
        shipHealthRemainingLabel.text = $"{shipHealthNormalized*100:F0}%";
        shipHealthBar.Value = shipHealthNormalized;
        shipHealthBar.FillColor = Color.Lerp(MinHealthColor, MaxHealthColor, shipHealthNormalized);

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

        interactButton.SetActive(gameManager.Player.ClosestInteractable != null);
        interactDescription.text = GetInteractableDescription();
    }

    private string GetInteractableDescription()
    {
        switch (gameManager.Player.ClosestInteractable)
        {
            case Bubble _: return "Pop Bubble";
            case PowerUp _: return "Bubble PowerUp";
            case Enemy enemy: return $"Bubble {enemy.Description}";
            default: return "";
        }
    }
}
