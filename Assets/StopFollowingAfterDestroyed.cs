using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class YourCinemachineController : MonoBehaviour
{
    private GameObject player;
    private CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (player != null)
        {
            virtualCamera.Follow = player.transform;
        }
        else
        {
            Debug.LogWarning("PlayerNull");
            virtualCamera.Follow = null;
        }
    }
}