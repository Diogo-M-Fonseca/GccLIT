using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal sealed class RunController : MonoBehaviour
{
    [SerializeField] private TurnBattle _battle;
    [SerializeField] private Combatant _player;
    [SerializeField] private BattleHud _hud;
    [SerializeField] private List<RoomData> _rooms = new List<RoomData>();
    [SerializeField] private int _playerMaxHp = 30;
    [SerializeField] private int _healBetweenRooms = 8;
    [SerializeField] private float _bannerTime = 2f;

    private int _index;

    private void OnEnable() { _battle.Ended += OnBattleEnded; }
    private void OnDisable() { _battle.Ended -= OnBattleEnded; }

    private void Start() { StartRun(); }

    internal void StartRun()
    {
        _index = 0;
        _player.ResetFull(_playerMaxHp);
        _hud.HideBanner();
        _battle.Begin(_rooms[_index]);
    }

    private void OnBattleEnded(bool playerWon)
    {
        StartCoroutine(playerWon ? AfterWin() : AfterLoss());
    }

    private IEnumerator AfterWin()
    {
        _index++;
        if (_index >= _rooms.Count)
        {
            _hud.ShowBanner("You made it through!\nEverything was lost in translation, but you won.");
            yield return new WaitForSeconds(_bannerTime * 1.5f);
            StartRun();
            yield break;
        }

        _hud.ShowBanner("Room cleared!");
        yield return new WaitForSeconds(_bannerTime);
        _hud.HideBanner();
        _player.ClearStatuses();
        _player.Heal(_healBetweenRooms);
        _battle.Begin(_rooms[_index]);
    }

    private IEnumerator AfterLoss()
    {
        _hud.ShowBanner("Your last spell was lost in translation...");
        yield return new WaitForSeconds(_bannerTime);
        StartRun();
    }
}
