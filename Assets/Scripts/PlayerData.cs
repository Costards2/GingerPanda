using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData : MonoBehaviour
{
    public int collectables;

    public PlayerData(MoveFSM player)
    {
        collectables = player.collectables;
    }

}
