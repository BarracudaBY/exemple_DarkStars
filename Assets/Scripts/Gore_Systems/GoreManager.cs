using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Центральная точка входа для всех gore-эффектов.
/// Ничего не знает про FSM — вызывается из состояний Hit/Death.
/// Повесь на пустой объект в сцене (или сделай DontDestroyOnLoad-персистентным).
/// </summary>
public class GoreManager : MonoBehaviour
{
    public static GoreManager Instance { get; private set; }

    [Header("Пулинг")]
    [SerializeField] private int particlePoolSize = 10;
    [SerializeField] private int decalPoolSize = 30;
    [SerializeField] private int gibPoolSize = 20;

    [SerializeField] private GameObject decalPrefab;  // простой SpriteRenderer + BloodDecal
    [SerializeField] private GameObject gibPrefab;     // Rigidbody2D + SpriteRenderer + Gib

    private readonly Queue<ParticleSystem> _particlePool = new();
    private readonly Queue<GameObject> _decalPool = new();
    private readonly Queue<GameObject> _gibPool = new();
    private readonly List<GameObject> _activeDecals = new();

    [SerializeField] private int maxDecalsOnScreen = 40;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // === Публичное API ===

    /// <summary>Полный набор эффектов на смерть: кровь + декаль + гибы + hit-stop + shake.</summary>
    public void PlayDeathSequence(Vector2 position, Vector2 hitDirection, GoreProfile profile, SpriteRenderer sourceRenderer = null)
    {
        if (profile == null) return;

        SpawnBloodBurst(position, hitDirection, profile);
        SpawnDecal(position, profile);

        if (profile.useDismemberment)
            SpawnGibs(position, profile, sourceRenderer);

        if (profile.useHitStop)
            HitStopService.Instance.DoHitStop(profile.hitStopDuration);

        if (profile.useCameraShake && CameraShake2D.Instance != null)
            CameraShake2D.Instance.Shake(profile.cameraShakeIntensity, profile.cameraShakeDuration);
    }

    /// <summary>Лёгкий эффект на обычный удар (без смерти) — только кровь, без гибов/декали.</summary>
    public void PlayHitSequence(Vector2 position, Vector2 hitDirection, GoreProfile profile)
    {
        if (profile == null) return;

        SpawnBloodBurst(position, hitDirection, profile);

        if (profile.useHitStop)
            HitStopService.Instance.DoHitStop(profile.hitStopDuration * 0.5f);
    }

    // === Частицы крови ===

    public void SpawnBloodBurst(Vector2 position, Vector2 direction, GoreProfile profile)
    {
        if (profile.bloodBurstPrefab == null) return;

        ParticleSystem ps = GetPooledParticle(profile.bloodBurstPrefab);
        ps.transform.position = position;
        ps.transform.right = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector2.right;

        var main = ps.main;
        main.startColor = profile.bloodColor;

        var emission = ps.emission;
        var burst = new ParticleSystem.Burst(0f, (short)profile.bloodParticleCount);
        emission.SetBurst(0, burst);

        ps.gameObject.SetActive(true);
        ps.Play();
        StartCoroutine(ReturnParticleAfterPlay(ps));
    }

    // === Декали ===

    public void SpawnDecal(Vector2 position, GoreProfile profile)
    {
        if (decalPrefab == null || profile.decalSprites == null || profile.decalSprites.Length == 0) return;
        if (Random.value > profile.decalSpawnChance) return;

        GameObject decalObj = GetPooledDecal();
        decalObj.transform.position = position;
        decalObj.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        var sr = decalObj.GetComponent<SpriteRenderer>();
        sr.sprite = profile.decalSprites[Random.Range(0, profile.decalSprites.Length)];
        sr.color = new Color(profile.bloodColor.r, profile.bloodColor.g, profile.bloodColor.b, 1f);

        decalObj.SetActive(true);
        decalObj.GetComponent<BloodDecal>().Activate(profile.decalLifetime, profile.decalFadeDuration, this);

        _activeDecals.Add(decalObj);
        EnforceDecalLimit();
    }

    private void EnforceDecalLimit()
    {
        while (_activeDecals.Count > maxDecalsOnScreen)
        {
            var oldest = _activeDecals[0];
            _activeDecals.RemoveAt(0);
            ReturnDecalToPool(oldest);
        }
    }

    // === Гибы ===

    public void SpawnGibs(Vector2 position, GoreProfile profile, SpriteRenderer sourceRenderer)
    {
        if (gibPrefab == null || profile.gibSprites == null || profile.gibSprites.Length == 0) return;

        int layer = sourceRenderer != null ? sourceRenderer.sortingOrder : 0;

        for (int i = 0; i < profile.gibCount; i++)
        {
            GameObject gibObj = GetPooledGib();
            gibObj.transform.position = position;
            gibObj.transform.rotation = Quaternion.identity;

            var sr = gibObj.GetComponent<SpriteRenderer>();
            sr.sprite = profile.gibSprites[Random.Range(0, profile.gibSprites.Length)];
            sr.sortingOrder = layer;

            gibObj.SetActive(true);

            Vector2 randomDir = Random.insideUnitCircle.normalized + Vector2.up * 0.5f;
            float force = Random.Range(profile.gibMinForce, profile.gibMaxForce);

            gibObj.GetComponent<Gib>().Launch(randomDir * force, profile.gibTorque, profile.gibLifetime, profile.gibFadeDuration, this);
        }
    }

    // === Пулы ===

    private ParticleSystem GetPooledParticle(ParticleSystem prefab)
    {
        if (_particlePool.Count > 0) return _particlePool.Dequeue();
        return Instantiate(prefab, transform);
    }

    private System.Collections.IEnumerator ReturnParticleAfterPlay(ParticleSystem ps)
    {
        yield return new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);
        ps.gameObject.SetActive(false);
        _particlePool.Enqueue(ps);
    }

    private GameObject GetPooledDecal()
    {
        if (_decalPool.Count > 0) return _decalPool.Dequeue();
        return Instantiate(decalPrefab, transform);
    }

    public void ReturnDecalToPool(GameObject decal)
    {
        decal.SetActive(false);
        _decalPool.Enqueue(decal);
    }

    private GameObject GetPooledGib()
    {
        if (_gibPool.Count > 0) return _gibPool.Dequeue();
        return Instantiate(gibPrefab, transform);
    }

    public void ReturnGibToPool(GameObject gib)
    {
        gib.SetActive(false);
        _gibPool.Enqueue(gib);
    }
}
