using System.Collections;
using UnityEngine;

/// <summary>
/// Кусок разлетевшегося тела. Требует Rigidbody2D + Collider2D + SpriteRenderer на префабе.
/// Летит по физике, через lifetime секунд плавно тухнет и уходит в пул.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Gib : MonoBehaviour
{
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Coroutine _routine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
    }

    public void Launch(Vector2 impulse, float torque, float lifetime, float fadeDuration, GoreManager owner)
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.AddForce(impulse, ForceMode2D.Impulse);
        _rb.AddTorque(Random.Range(-torque, torque), ForceMode2D.Impulse);

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(LifeCycle(lifetime, fadeDuration, owner));
    }

    private IEnumerator LifeCycle(float lifetime, float fadeDuration, GoreManager owner)
    {
        Color start = _sr.color;
        yield return new WaitForSeconds(lifetime);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(start.a, 0f, t / fadeDuration);
            _sr.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }

        _sr.color = start;
        owner.ReturnGibToPool(gameObject);
    }
}
