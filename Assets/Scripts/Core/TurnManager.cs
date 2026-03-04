using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    [Header("Board")]
    public TileManager tileManager;

    [Header("Players in turn order")]
    public List<PlayerState> players = new List<PlayerState>();

    [Header("Dice")]
    public int minDice = 1;
    public int maxDice = 6;

    [Header("UI (Optional)")]
    public TMP_Text diceText;
    public TMP_Text turnText;
    public TMP_Text moneyText;
    public EventUI eventUI;

    [Header("Pass Start Rule")]
    public int passStartReward = 200;

    [Header("Ownership Colors")]
    public Color humanOwnedColor = new Color(0.25f, 0.75f, 1f, 1f); // 蓝
    public Color aiOwnedColor = new Color(1f, 0.85f, 0.2f, 1f);     // 黄

    [Header("AI Settings")]
    public float aiRollDelay = 0.7f;
    public float aiDecisionDelay = 0.45f;

    [Header("Bankruptcy")]
    public bool endGameWhenHumanBankrupt = false;

    private int currentPlayerIndex = 0;
    private bool waitingForDecision = false;
    private Tile pendingPropertyTile;
    private Coroutine aiRoutine;

    public int CurrentPlayerIndex => currentPlayerIndex;

    public Transform GetCurrentPlayerTransform()
    {
        if (players == null || players.Count == 0) return null;
        var p = players[currentPlayerIndex];
        return p != null ? p.transform : null;
    }

    private void Start()
    {
        UpdateUI();
        StartTurnAutoIfAI();
    }

    private void Update()
    {
        // 只有非AI玩家允许 Space 掷骰
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!IsCurrentPlayerAI() && !waitingForDecision)
                TryRollDice();
        }
    }

    private bool IsCurrentPlayerAI()
    {
        if (players == null || players.Count == 0) return false;
        var p = players[currentPlayerIndex];
        return p != null && p.isAI && !p.isBankrupt;
    }

    private bool IsHuman(PlayerState p) => p != null && !p.isAI;

    private void StartTurnAutoIfAI()
    {
        if (!IsCurrentPlayerAI()) return;

        if (aiRoutine != null) StopCoroutine(aiRoutine);
        aiRoutine = StartCoroutine(AI_RollAfterDelay());
    }

    private IEnumerator AI_RollAfterDelay()
    {
        yield return new WaitForSeconds(aiRollDelay);
        if (!waitingForDecision) TryRollDice();
    }

    public void TryRollDice()
    {
        if (waitingForDecision) return;

        if (tileManager == null || tileManager.tiles == null || tileManager.tiles.Count == 0)
        {
            Debug.LogError("TurnManager: tileManager.tiles is empty. Generate tiles first.");
            return;
        }
        if (players == null || players.Count == 0)
        {
            Debug.LogError("TurnManager: players list is empty.");
            return;
        }

        PlayerState p = players[currentPlayerIndex];
        if (p == null || p.isBankrupt)
        {
            EndTurn();
            return;
        }

        PlayerMover mover = p.GetComponent<PlayerMover>();
        if (mover == null)
        {
            Debug.LogError("TurnManager: Player missing PlayerMover.");
            return;
        }
        if (mover.IsMoving) return;
        if (mover.tileManager == null) mover.tileManager = tileManager;

        int dice = Random.Range(minDice, maxDice + 1);
        if (diceText) diceText.text = $"Dice: {dice}";

        waitingForDecision = true;

        mover.MoveSteps(
            steps: dice,
            getCurrentIndex: () => p.currentTileIndex,
            setCurrentIndex: (idx) => p.currentTileIndex = idx,
            onPassStart: () =>
            {
                p.money += passStartReward;
                UpdateUI();
            },
            onFinish: () =>
            {
                HandleLanding(p, p.currentTileIndex);
                UpdateUI();
            }
        );
    }

    private void HandleLanding(PlayerState player, int tileIndex)
    {
        pendingPropertyTile = null;

        Transform tileTf = tileManager.tiles[tileIndex];
        Tile tile = tileTf.GetComponent<Tile>();
        if (tile == null)
        {
            Debug.LogError($"TurnManager: Tile component missing on {tileTf.name}");
            waitingForDecision = false;
            StartTurnAutoIfAI();
            return;
        }

        switch (tile.tileType)
        {
            case TileType.Start:
                player.money += tile.value;
                ShowOK($"START!\n+{tile.value}");
                break;

            case TileType.Tax:
                player.money -= tile.value;
                ShowOK($"TAX!\n-{tile.value}");
                break;

            case TileType.Property:
                HandleProperty(player, tile);
                break;

            default:
                ShowOK("Empty tile.");
                break;
        }

        CheckBankrupt(player);

        // AI：自动决定 + 自动关闭弹窗推进回合
        if (IsCurrentPlayerAI())
        {
            if (aiRoutine != null) StopCoroutine(aiRoutine);
            aiRoutine = StartCoroutine(AI_DecideAndAutoClick());
        }
    }

    private void HandleProperty(PlayerState player, Tile tile)
    {
        int pIndex = currentPlayerIndex;

        if (!tile.isOwned)
        {
            pendingPropertyTile = tile;
            ShowBuy($"Unowned Property\nPrice: {tile.price}\nRent: {tile.rent}\nBuy it?");
            return;
        }

        if (tile.ownerPlayerIndex == pIndex)
        {
            ShowOK("Your own property.");
            return;
        }

        int rent = tile.rent;
        player.money -= rent;

        if (tile.ownerPlayerIndex >= 0 && tile.ownerPlayerIndex < players.Count)
        {
            var owner = players[tile.ownerPlayerIndex];
            if (owner != null && !owner.isBankrupt) owner.money += rent;
        }

        ShowOK($"Pay rent: {rent}");
    }

    private IEnumerator AI_DecideAndAutoClick()
    {
        yield return new WaitForSeconds(aiDecisionDelay);

        var ai = players[currentPlayerIndex];
        if (ai == null || ai.isBankrupt)
        {
            ForceClosePanel();
            EndTurn();
            yield break;
        }

        // 买地弹窗
        if (pendingPropertyTile != null && !pendingPropertyTile.isOwned)
        {
            bool shouldBuy = AI_ShouldBuy(ai, pendingPropertyTile);

            if (shouldBuy)
            {
                if (eventUI != null) eventUI.OnClickBuy(); else OnEventBuy();
            }
            else
            {
                if (eventUI != null) eventUI.OnClickSkip(); else OnEventSkip();
            }
            yield break;
        }

        // 其他事件：OK
        if (eventUI != null) eventUI.OnClickOK(); else OnEventOK();
    }

    // AI 策略：攻击性买地，但保留现金避免自爆
    private bool AI_ShouldBuy(PlayerState ai, Tile t)
    {
        int reserve = 200; // 留底现金：越大越稳，越小越激进
        int afterBuy = ai.money - t.price;
        if (afterBuy < reserve) return false;

        // 如果人类更有钱，AI更激进抢地盘
        int humanMoney = GetHumanMoney();
        if (humanMoney > ai.money) return true;

        // 简单ROI判断（租金/价格）
        float roi = t.price <= 0 ? 0f : (float)t.rent / t.price;
        if (roi >= 0.20f) return true;

        return true;
    }

    private int GetHumanMoney()
    {
        int best = 0;
        foreach (var p in players)
        {
            if (p != null && IsHuman(p) && !p.isBankrupt)
                best = Mathf.Max(best, p.money);
        }
        return best;
    }

    // ===== UI callbacks =====
    private void OnEventOK() => EndTurn();

    private void OnEventBuy()
    {
        PlayerState p = players[currentPlayerIndex];

        if (pendingPropertyTile != null && !pendingPropertyTile.isOwned && p.money >= pendingPropertyTile.price)
        {
            p.money -= pendingPropertyTile.price;
            pendingPropertyTile.SetOwner(currentPlayerIndex);

            // 买地变色
            PaintOwnedTile(pendingPropertyTile, p.isAI);
        }
        EndTurn();
    }

    private void OnEventSkip() => EndTurn();

    private void EndTurn()
    {
        pendingPropertyTile = null;
        waitingForDecision = false;

        currentPlayerIndex = GetNextAlivePlayerIndex(currentPlayerIndex);

        UpdateUI();
        StartTurnAutoIfAI();
    }

    private int GetNextAlivePlayerIndex(int from)
    {
        if (players == null || players.Count == 0) return 0;

        int n = players.Count;
        for (int k = 1; k <= n; k++)
        {
            int idx = (from + k) % n;
            if (players[idx] != null && !players[idx].isBankrupt)
                return idx;
        }
        return from;
    }

    private void UpdateUI()
    {
        if (players == null || players.Count == 0) return;

        PlayerState p = players[currentPlayerIndex];
        if (p == null) return;

        if (turnText != null)
        {
            string type = p.isAI ? "AI" : "Player";
            turnText.text = $"{type} Turn: {p.playerName}";
        }
        if (moneyText != null) moneyText.text = $"Money: {p.money}";
    }

    private void ShowOK(string msg)
    {
        if (eventUI == null) { OnEventOK(); return; }
        eventUI.ShowOK(msg, OnEventOK);
    }

    private void ShowBuy(string msg)
    {
        if (eventUI == null) { OnEventSkip(); return; }
        eventUI.ShowBuy(msg, OnEventBuy, OnEventSkip);
    }

    private void PaintOwnedTile(Tile tile, bool ownerIsAI)
    {
        if (tile == null) return;
        Renderer r = tile.GetComponent<Renderer>();
        if (r == null) return;

        r.material.color = ownerIsAI ? aiOwnedColor : humanOwnedColor;
    }

    private void ForceClosePanel()
    {
        if (eventUI != null && eventUI.panel != null)
            eventUI.panel.SetActive(false);
    }

    private void CheckBankrupt(PlayerState p)
    {
        if (p == null) return;

        if (p.money < 0 && !p.isBankrupt)
        {
            p.isBankrupt = true;
            Debug.Log($"{p.playerName} BANKRUPT!");

            if (endGameWhenHumanBankrupt && IsHuman(p))
            {
                Debug.Log("Human bankrupt. Game Over.");
                // Time.timeScale = 0f;
            }
        }
    }
}