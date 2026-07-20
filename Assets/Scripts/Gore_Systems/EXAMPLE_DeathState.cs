using UnityEngine;

/// <summary>
/// ПРИМЕР интеграции. Адаптируй под свой интерфейс состояний (Enter/Update/Exit),
/// свой EnemyController и способ получения "направления удара".
/// Идея: DeathState ничего не делает сgore-логикой сам — только просит GoreManager сыграть эффект.
/// </summary>
public class EXAMPLE_DeathState /* : IState */
{
    private readonly Transform _entityTransform;
    private readonly SpriteRenderer _spriteRenderer;
    private readonly Collider2D _collider;
    private readonly GoreProfile _goreProfile;
    private readonly Vector2 _lastHitDirection;

    public EXAMPLE_DeathState(Transform entityTransform, SpriteRenderer spriteRenderer,
        Collider2D collider, GoreProfile goreProfile, Vector2 lastHitDirection)
    {
        _entityTransform = entityTransform;
        _spriteRenderer = spriteRenderer;
        _collider = collider;
        _goreProfile = goreProfile;
        _lastHitDirection = lastHitDirection;
    }

    public void Enter()
    {
        // 1. Отключаем взаимодействие — сущность больше не участвует в геймплее
        if (_collider != null) _collider.enabled = false;

        // 2. Запускаем gore-эффекты одним вызовом
        GoreManager.Instance.PlayDeathSequence(
            position: _entityTransform.position,
            hitDirection: _lastHitDirection,
            profile: _goreProfile,
            sourceRenderer: _spriteRenderer
        );

        // 3. Прячем/убираем оригинальный спрайт — его "заменяют" разлетевшиеся гибы.
        //    Если хочешь оставить труп на полу вместо гибов — просто отключи
        //    useDismemberment в профиле и вместо этого проиграй анимацию Death.
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;

        // 4. Дальше — твоя обычная логика: таймер на удаление объекта,
        //    начисление опыта, дроп лута и т.п.
    }

    public void Update() { }
    public void Exit() { }
}
