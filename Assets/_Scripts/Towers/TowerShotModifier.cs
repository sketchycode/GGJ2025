using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "TowerShotModifier", menuName = "TowerShotModifier")]
public class TowerShotModifier : ScriptableObject
{
    public ParticleSystem launchParticleSystem;
    public ParticleSystem travelParticleSystem;

    public float CooldownModifier = .95f;
    public float RangeModifier = 1.05f;
    
    public float DamageModifier = 1.1f;
    public float SpeedModifier = 1.1f;
    [FormerlySerializedAs("RadiusModifier")] public float SplashRadiusModifier = 1.1f;
}
