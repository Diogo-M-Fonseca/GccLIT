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
        _battle.ResetUses();
        _battle.Begin(_rooms[_index]);
    }

    private void OnBattleEnded(bool playerWon)
    {
        StartCoroutine(playerWon ? AfterWin() : AfterLoss());
    }

    private IEnumerator AfterWin()
    {
        _index++;
        bool pyrrhic = _battle.PyrrhicWin;
        if (_index >= _rooms.Count || _battle.WinsRun)
        {
            _hud.ShowBanner(_battle.WinsRun
                ? "Love conquers all!\nOne spell won the whole game."
                : pyrrhic
                    ? "You win, but at what cost...\nThe dust settles over the ruins."
                    : "You made it through!\nEverything was lost in translation, but you won.");
            yield return new WaitForSeconds(_bannerTime * 1.5f);
            StartRun();
            yield break;
        }

        _hud.ShowBanner(pyrrhic ? "You win, but at what cost...\nYou crawl out of the rubble." : "Room cleared!");
        yield return new WaitForSeconds(_bannerTime);
        _hud.HideBanner();
        _player.ClearStatuses();
        _player.Heal(_healBetweenRooms);
        _battle.Begin(_rooms[_index]);
    }

    private IEnumerator AfterLoss()
    {
        _hud.ShowBanner(_battle.SacrificeDeath
            ? "You gave your last drop of life to the ritual...\nThe power was yours, but there was no one left to wield it."
            : "Your last spell was lost in translation...");
        yield return new WaitForSeconds(_bannerTime);
        StartRun();
    }
}
