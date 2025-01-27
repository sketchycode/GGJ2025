using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private float progressValue = 0;

    private Image backgroundImage;
    private Image fillImage;

    public Color FillColor
    {
        get => fillImage.color;
        set => fillImage.color = value;
    }

    public Color BackgroundColor
    {
        get => backgroundImage.color;
        set => backgroundImage.color = value;
    }
    
    public float Value
    {
        get => progressValue;
        set
        {
            progressValue = Mathf.Clamp01(value);
        }
    }

    private void Awake()
    {
        fillImage = fillRect.GetComponent<Image>();
        backgroundImage = backgroundRect.GetComponent<Image>();
        
    }

    private void Update()
    {
        UpdateFill();
    }

    private void UpdateFill()
    {
        fillRect.anchorMax = new Vector2(Value, 1);
    }
}
