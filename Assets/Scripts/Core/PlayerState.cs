using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public string playerName = "Player";
    public bool isAI = false;

    public int money = 1000;
    public int currentTileIndex = 0;

    public bool isBankrupt = false;
}