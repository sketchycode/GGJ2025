using UnityEngine;

public interface IBubbleable
{
    bool MoveToPlayer { get; }
    void Bubble(Bubble bubble);
    void PopBubble();
    Vector3 GetBubbleWorldPosition();
    
    void TakeDamage(float damage);
}
