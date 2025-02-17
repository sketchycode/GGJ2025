using UnityEngine;

public interface IBubbleable
{
    void Bubble(Bubble bubble);
    void PopBubble();
    Vector3 GetBubbleWorldPosition();
    
    void TakeDamage(float damage);
}
