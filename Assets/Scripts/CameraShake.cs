using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineImpulseSource impulse;
    private void Awake()
    {
        Instance = this;
        impulse = GetComponent<CinemachineImpulseSource>();
    }

    public void ShakeCamera()
    {
        impulse.GenerateImpulse();
    }
}
