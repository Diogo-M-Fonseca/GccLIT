using System;
using System.Collections;
using UnityEngine;

internal sealed class TurnBattle : MonoBehaviour
{
    [SerializeField] private SpellBook _book;
    [SerializeField] private Combatant _player;
    [SerializeField] private Combatant _enemy;
    [SerializeField] private SpellInput _input;
    [SerializeField] private BattleHud _hud;
    [SerializeField] private float _beatTime = 0.6f;

    private RoomData _room;
    private bool _busy;

    internal event Action<bool> Ended;

    private void OnEnable() { _input.Submitted += OnSubmitted; }
    private void OnDisable() { _input.Submitted -= OnSubmitted; }

    internal void Begin(RoomData room)
    {
        _room = room;
        _busy = false;
        _enemy.ResetFull(room.EnemyHp, room.EnemyName);
        _hud.ShowRoom(room);
        _hud.Log($"A {room.EnemyName} appears!");
        _input.Clear();
        _input.SetInteractable(true);
    }

    private void OnSubmitted(string typed)
    {
        if (_busy || _room == null) return;

        string word = CastResolver.Normalize(typed);
        if (!_book.TryGet(word, out _))
        {
            _hud.Log($"\"{word}\" is not in your grimoire.");
            return;
        }

        _input.Clear();
        StartCoroutine(RunTurn(word));
    }

    private IEnumerator RunTurn(string word)
    {
        _busy = true;
        _input.SetInteractable(false);

        CastResult result = CastResolver.Resolve(word, _book, _room.Filters);
        yield return _hud.PlayTrail(result);

        if (result.Effective != null && result.Effective.Effect != null)
            _hud.Log(result.Effective.Effect.Apply(_player, _enemy, result.Effective.Power));

        yield return new WaitForSeconds(_beatTime);
        if (_enemy.IsDead) { Finish(true); yield break; }

        string burn = _enemy.StartTurn();
        if (burn != null) { _hud.Log(burn); yield return new WaitForSeconds(_beatTime); }
        if (_enemy.IsDead) { Finish(true); yield break; }

        EnemyCast();
        yield return new WaitForSeconds(_beatTime);
        if (_player.IsDead) { Finish(false); yield break; }

        burn = _player.StartTurn();
        if (burn != null) { _hud.Log(burn); yield return new WaitForSeconds(_beatTime); }
        if (_player.IsDead) { Finish(false); yield break; }

        _busy = false;
        _input.SetInteractable(true);
    }

    private void EnemyCast()
    {
        if (_room.EnemyWords.Count == 0) return;
        string word = CastResolver.Normalize(_room.EnemyWords[UnityEngine.Random.Range(0, _room.EnemyWords.Count)]);
        if (!_book.TryGet(word, out SpellEntry entry) || entry.Effect == null) return;

        _hud.Log($"{_enemy.DisplayName} casts \"{word}\"!");
        _hud.Log(entry.Effect.Apply(_enemy, _player, entry.Power));
    }

    private void Finish(bool playerWon)
    {
        _busy = true;
        _input.SetInteractable(false);
        Ended?.Invoke(playerWon);
    }
}
