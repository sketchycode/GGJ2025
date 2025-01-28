using UnityEngine;
using UnityEngine.VFX;

public class VisualEffectObjectPool : ObjectPool<VisualEffect>
{
    public VisualEffect Spawn(Transform spawnPoint)
    {
        var vfx = Spawn();
        vfx.transform.position = spawnPoint.position;

        return vfx;
    }
}