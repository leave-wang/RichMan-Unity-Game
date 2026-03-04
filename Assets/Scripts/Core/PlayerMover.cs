using System;
using System.Collections;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [Header("Board")]
    public TileManager tileManager;

    [Header("Movement")]
    public float secondsPerTile = 0.25f;
    public float jumpHeight = 0.25f;
    public float rotateSpeed = 12f;

    public bool IsMoving { get; private set; }

    public void MoveSteps(
        int steps,
        Func<int> getCurrentIndex,
        Action<int> setCurrentIndex,
        Action onPassStart,
        Action onFinish)
    {
        if (IsMoving) return;
        StartCoroutine(MoveRoutine(steps, getCurrentIndex, setCurrentIndex, onPassStart, onFinish));
    }

    private IEnumerator MoveRoutine(
        int steps,
        Func<int> getCurrentIndex,
        Action<int> setCurrentIndex,
        Action onPassStart,
        Action onFinish)
    {
        if (tileManager == null || tileManager.tiles == null || tileManager.tiles.Count == 0)
        {
            Debug.LogError("PlayerMover: tileManager tiles not ready.");
            yield break;
        }

        IsMoving = true;
        int count = tileManager.tiles.Count;

        for (int s = 0; s < steps; s++)
        {
            int current = getCurrentIndex();
            int next = (current + 1) % count;

            if (next == 0) onPassStart?.Invoke();

            Transform nextTile = tileManager.tiles[next];
            Vector3 startPos = transform.position;
            Vector3 endPos = nextTile.position + Vector3.up * 0.5f;

            // 朝向（可选）
            Vector3 dir = endPos - startPos; dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                StartCoroutine(SmoothRotate(targetRot, secondsPerTile));
            }

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, secondsPerTile);
                float eased = EaseInOutCubic(Mathf.Clamp01(t));

                Vector3 p = Vector3.Lerp(startPos, endPos, eased);
                p.y += Mathf.Sin(eased * Mathf.PI) * jumpHeight;
                transform.position = p;

                yield return null;
            }

            transform.position = endPos;
            setCurrentIndex(next);

            yield return new WaitForSeconds(0.03f);
        }

        IsMoving = false;
        onFinish?.Invoke();
    }

    private IEnumerator SmoothRotate(Quaternion target, float duration)
    {
        Quaternion start = transform.rotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, duration);
            float eased = EaseInOutCubic(Mathf.Clamp01(t));
            transform.rotation = Quaternion.Slerp(start, target, eased);
            yield return null;
        }
        transform.rotation = target;
    }

    private float EaseInOutCubic(float x)
        => x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
}