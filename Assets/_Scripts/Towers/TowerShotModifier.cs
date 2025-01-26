using UnityEngine;

[CreateAssetMenu(fileName = "TowerShotModifier", menuName = "TowerShotModifier")]
public class TowerShotModifier : ScriptableObject
{
    public ParticleSystem launchParticleSystem;
    public ParticleSystem travelParticleSystem;
}
