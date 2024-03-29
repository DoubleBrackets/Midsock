using Cinemachine;
using FishNet;
using UnityEngine;

public class CinemachineBrainUpdater : MonoBehaviour
{
    [SerializeField]
    private CinemachineBrain _cinemachineBrain;

    private void Awake()
    {
        InstanceFinder.TimeManager.OnPostTick += OnPostTick;
    }

    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager == null)
        {
            return;
        }

        InstanceFinder.TimeManager.OnPostTick -= OnPostTick;
    }

    private void OnPostTick()
    {
        _cinemachineBrain.ManualUpdate();
    }
}