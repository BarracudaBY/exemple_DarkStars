using System.Collections;
using UnityEngine;

/// <summary>
/// Повесь на объект с камерой (или на дочерний объект внутри камеры-рига).
/// Работает на unscaled time, поэтому тряска видна даже во время HitStopService.
/// Если используешь Cinemachine — замени тело Shake() на CinemachineImpulseSource.GenerateImpulse().
/// </summary>
public class CameraShake2D : MonoBehaviour
{
    public static CameraShake2D Instance { get; private set; }

    private Vector3 _originalLocalPos;
    private Coroutine _routine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _originalLocalPos = transform.localPosition;
    }

    public void Shake(float intensity, float duration)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float damper = 1f - (t / duration);
            Vector2 offset = Random.insideUnitCircle * intensity * damper;
            transform.localPosition = _originalLocalPos + (Vector3)offset;
            yield return null;
        }
        transform.localPosition = _originalLocalPos;
    }
}
