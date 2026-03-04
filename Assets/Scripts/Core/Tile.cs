using UnityEngine;

public enum TileType { Empty, Start, Tax, Property }

public class Tile : MonoBehaviour
{
    [Header("Type")]
    public TileType tileType = TileType.Empty;

    // Start/Tax 使用 value：Start=奖励，Tax=扣钱
    public int value = 0;

    [Header("Property")]
    public int price = 200;
    public int rent = 50;

    [Header("Ownership")]
    public bool isOwned = false;
    public int ownerPlayerIndex = -1;

    public void SetOwner(int playerIndex)
    {
        isOwned = true;
        ownerPlayerIndex = playerIndex;
    }

    public void ClearOwner()
    {
        isOwned = false;
        ownerPlayerIndex = -1;
    }
}