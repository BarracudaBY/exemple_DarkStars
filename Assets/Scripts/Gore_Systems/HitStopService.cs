using System.Collections;
using UnityEngine;

/// <summary>
/// Кратковременная заморозка Time.timeScale для "смачности" ударов.
/// Безопасен к повторным вызовам подряд (не ломает timeScale, если стопы накладываются).
/// </summary>
public class HitStopService : MonoBehaviour
{
    public static HitStopService Instance { get; private set; }

    private int _activeStops = 0;
    private float _originalTimeScale = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void DoHitStop(float duration)
    {
        if (duration <= 0f) return;
        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        if (_activeStops == 0)
            _originalTimeScale = Time.timeScale;

        _activeStops++;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        _activeStops--;
        if (_activeStops <= 0)
        {
            _activeStops = 0;
            Time.timeScale = _originalTimeScale;
        }
    }
}
