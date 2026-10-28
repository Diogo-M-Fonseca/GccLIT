using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
internal sealed class RoomData
{
    [SerializeField] private string _roomName = "Room";
    [SerializeField] private string _enemyName = "Goblin";
    [SerializeField] private int _enemyHp = 20;
    [Tooltip("Filters are applied in order to every spell the player casts here.")]
    [SerializeField] private List<LanguageFilter> _filters = new List<LanguageFilter>();
    [Tooltip("Words (from the spell book) the enemy may cast.")]
    [SerializeField] private List<string> _enemyWords = new List<string>();

    internal string RoomName => _roomName;
    internal string EnemyName => _enemyName;
    internal int EnemyHp => _enemyHp;
    internal IReadOnlyList<LanguageFilter> Filters => _filters;
    internal IReadOnlyList<string> EnemyWords => _enemyWords;
}
