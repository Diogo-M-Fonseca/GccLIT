using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

internal sealed class BattleHud : MonoBehaviour
{
    [SerializeField] private TMP_Text _roomTitle;
    [SerializeField] private TMP_Text _filterText;
    [SerializeField] private TMP_Text _logText;
    [SerializeField] private TMP_Text _banner;
    [SerializeField] private WordTrailView _trail;
    [SerializeField] private int _logLines = 4;

    private readonly Queue<string> _log = new Queue<string>();

    private void Awake()
    {
        _banner.gameObject.SetActive(false);
    }

    internal void ShowRoom(RoomData room)
    {
        _roomTitle.text = room.RoomName;

        var sb = new StringBuilder("The room speaks:\n");
        if (room.Filters.Count == 0) sb.Append("<color=#AAAAAA>plain words</color>");
        foreach (LanguageFilter f in room.Filters)
        {
            if (f == null) continue;
            sb.Append($"<b>{f.DisplayName}</b>");
            if (!string.IsNullOrEmpty(f.Hint)) sb.Append($" <color=#AAAAAA>({f.Hint})</color>");
            sb.Append('\n');
        }
        _filterText.text = sb.ToString();

        _log.Clear();
        _logText.text = "";
        _trail.Clear();
    }

    internal void Log(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        _log.Enqueue(line);
        while (_log.Count > _logLines) _log.Dequeue();
        _logText.text = string.Join("\n", _log);
    }

    internal IEnumerator PlayTrail(CastResult result)
    {
        yield return _trail.Play(result);
    }

    internal void ShowBanner(string text)
    {
        _banner.text = text;
        _banner.gameObject.SetActive(true);
    }

    internal void HideBanner()
    {
        _banner.gameObject.SetActive(false);
    }
}
