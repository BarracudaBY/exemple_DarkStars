using System.Collections;
using UnityEngine;

/// <summary>
/// Вешается на префаб декали (SpriteRenderer). Живёт lifetime секунд,
/// затем плавно исчезает за fadeDuration и возвращается в пул.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BloodDecal : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Coroutine _routine;

    private void Awake() => _sr = GetComponent<SpriteRenderer>();

    public void Activate(float lifetime, float fadeDuration, GoreManager owner)
    {
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

        _sr.color = start; // сброс альфы для переиспользования из пула
        owner.ReturnDecalToPool(gameObject);
    }
}
