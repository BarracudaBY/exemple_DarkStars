using UnityEngine;

/// <summary>
/// Настройки Gore-эффектов для конкретного типа сущности (враг, босс, игрок).
/// Создаётся через Assets/Create/Gore/Gore Profile.
/// Один профиль можно переиспользовать на несколько похожих врагов.
/// </summary>
[CreateAssetMenu(fileName = "New Gore Profile", menuName = "Gore/Gore Profile")]
public class GoreProfile : ScriptableObject
{
    [Header("Кровь — частицы")]
    public ParticleSystem bloodBurstPrefab;
    [Range(1, 50)] public int bloodParticleCount = 10;
    public Color bloodColor = new Color(0.6f, 0f, 0f);

    [Header("Кровь — декали (пятна на земле/стенах)")]
    public Sprite[] decalSprites;
    [Range(0f, 1f)] public float decalSpawnChance = 0.8f;
    public float decalLifetime = 15f;
    public float decalFadeDuration = 2f;

    [Header("Разлёт частей тела (гибы)")]
    public bool useDismemberment = true;
    public Sprite[] gibSprites;
    [Range(0, 12)] public int gibCount = 4;
    public float gibMinForce = 2f;
    public float gibMaxForce = 5f;
    public float gibTorque = 180f;
    public float gibLifetime = 4f;
    public float gibFadeDuration = 1f;

    [Header("Кинематографичность")]
    public bool useHitStop = true;
    [Range(0f, 0.3f)] public float hitStopDuration = 0.06f;
    public bool useCameraShake = true;
    [Range(0f, 2f)] public float cameraShakeIntensity = 0.3f;
    public float cameraShakeDuration = 0.2f;
}
