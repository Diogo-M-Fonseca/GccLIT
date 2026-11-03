using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Tools > SpellTypos > Setup Window
// Place under Assets/Editor/ together with SpellTyposBuilder.cs and SpellTyposUi.cs.
internal sealed class SpellTyposSetupWindow : EditorWindow
{
    private static readonly string[] TabNames = { "Setup", "Combatants", "Spells", "Sounds", "Mixes", "Filters", "Rooms" };

    [SerializeField] private int _tab;
    [SerializeField] private Vector2 _scroll;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private SpellBook _book;
    [SerializeField] private string _sceneName = "Battle";

    [SerializeField] private CombatantSpec _spec = CombatantSpec.NewPlayer();

    [SerializeField] private string _newWord = "";
    [SerializeField] private SpellEffect _newEffect;
    [SerializeField] private int _newPower = 1;
    [SerializeField] private SpellVfxKind _newVfx = SpellVfxKind.None;

    [SerializeField] private string _soundFilter = "";
    [SerializeField] private bool _onlyMissingSounds;

    [System.NonSerialized] private DraftPanel _effectPanel;
    [System.NonSerialized] private DraftPanel _filterPanel;
    [System.NonSerialized] private SerializedObject _bookSo;
    [System.NonSerialized] private SerializedObject _runSo;

    [MenuItem("Tools/SpellTypos/Setup Window")]
    private static void Open()
    {
        var w = GetWindow<SpellTyposSetupWindow>("SpellTypos Setup");
        w.minSize = new Vector2(480f, 540f);
    }

    [MenuItem("Tools/SpellTypos/Build Everything (defaults)")]
    private static void BuildEverythingMenu()
    {
        GetWindow<SpellTyposSetupWindow>("SpellTypos Setup").BuildEverything();
    }

    private void OnEnable()
    {
        _effectPanel = new DraftPanel(typeof(SpellEffect), SpellTyposBuilder.EffectsFolder);
        _filterPanel = new DraftPanel(typeof(LanguageFilter), SpellTyposBuilder.FiltersFolder);
        if (_spec == null) _spec = CombatantSpec.NewPlayer();
    }

    private void OnDisable()
    {
        if (_effectPanel != null) _effectPanel.Dispose();
        if (_filterPanel != null) _filterPanel.Dispose();
        StopPreview();
    }

    private void OnGUI()
    {
        _tab = GUILayout.Toolbar(_tab, TabNames);
        EditorGUILayout.Space();

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        switch (_tab)
        {
            case 0: DrawSetup(); break;
            case 1: DrawCombatants(); break;
            case 2: DrawSpells(); break;
            case 3: DrawSounds(); break;
            case 4: DrawMixes(); break;
            case 5: DrawFilters(); break;
            case 6: DrawRooms(); break;
        }
        EditorGUILayout.EndScrollView();
    }

    private static void Header(string text)
    {
        EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
    }

    // ------------------------------------------------------------------ setup

    private void DrawSetup()
    {
        if (!SpellTyposBuilder.TmpReady)
        {
            EditorGUILayout.HelpBox("TextMeshPro Essential Resources are not imported. The generated UI needs them.",
                MessageType.Warning);
            if (GUILayout.Button("Import TMP Essentials"))
                EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
            EditorGUILayout.Space();
        }

        Header("1. Content");
        EditorGUILayout.HelpBox(
            "Creates the Assets/SpellTypos folders plus default effects, filters and a starter spell book. Existing assets are never overwritten.",
            MessageType.None);
        if (GUILayout.Button("Create folders + default content"))
        {
            Defaults d = SpellTyposBuilder.EnsureDefaults();
            _book = d.Book;
            EditorGUIUtility.PingObject(d.Book);
        }

        EditorGUILayout.Space();
        Header("2. Combatants");
        _playerPrefab = (GameObject)EditorGUILayout.ObjectField("Player prefab", _playerPrefab, typeof(GameObject), false);
        _enemyPrefab = (GameObject)EditorGUILayout.ObjectField("Enemy prefab", _enemyPrefab, typeof(GameObject), false);
        if (GUILayout.Button("Create default player + enemy prefabs"))
        {
            _playerPrefab = SpellTyposBuilder.EnsureCombatantPrefab(CombatantSpec.NewPlayer());
            _enemyPrefab = SpellTyposBuilder.EnsureCombatantPrefab(CombatantSpec.NewEnemy());
        }
        EditorGUILayout.LabelField("Use the Combatants tab to design your own.", EditorStyles.miniLabel);

        EditorGUILayout.Space();
        Header("3. Battle scene");
        _book = (SpellBook)EditorGUILayout.ObjectField("Spell book", _book, typeof(SpellBook), false);
        _sceneName = EditorGUILayout.TextField("Scene name", _sceneName);

        using (new EditorGUI.DisabledScope(_playerPrefab == null || _enemyPrefab == null))
        {
            if (GUILayout.Button("Build battle scene", GUILayout.Height(26)))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) BuildScene();
            }
        }

        EditorGUILayout.Space(16f);
        if (GUILayout.Button("Do everything with defaults", GUILayout.Height(34)))
            BuildEverything();
    }

    private void BuildEverything()
    {
        if (!SpellTyposBuilder.EnsureTmp()) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Defaults d = SpellTyposBuilder.EnsureDefaults();
        if (_book == null) _book = d.Book;
        if (_playerPrefab == null) _playerPrefab = SpellTyposBuilder.EnsureCombatantPrefab(CombatantSpec.NewPlayer());
        if (_enemyPrefab == null) _enemyPrefab = SpellTyposBuilder.EnsureCombatantPrefab(CombatantSpec.NewEnemy());
        BuildScene();
    }

    private void BuildScene()
    {
        string name = string.IsNullOrWhiteSpace(_sceneName) ? "Battle" : _sceneName.Trim();
        string path = $"{SpellTyposBuilder.ScenesFolder}/{name}.unity";

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null &&
            !EditorUtility.DisplayDialog("Overwrite scene?", $"{path} already exists. Rebuild it from scratch?",
                "Overwrite", "Cancel"))
            return;

        string result = SpellTyposBuilder.BuildBattleScene(_playerPrefab, _enemyPrefab, _book, name);
        if (result == null) return;

        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(result));
        Debug.Log($"[SpellTypos] Battle scene saved at {result}. Press Play.");
    }

    // ------------------------------------------------------------- combatants

    private void DrawCombatants()
    {
        Header("Prefab designer");
        EditorGUILayout.HelpBox(
            "Builds a 2D prefab: a SpriteRenderer (the sprite is scaled to the height below) plus a world-space HUD under the feet with name, HP bar, shield and status text, and optional burn / fur-ball overlays. Enemies also get EnemyView. Without a sprite you get a tinted placeholder box.",
            MessageType.None);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Player preset")) _spec = CombatantSpec.NewPlayer();
            if (GUILayout.Button("Enemy preset")) _spec = CombatantSpec.NewEnemy();
        }

        _spec.Kind = (CombatantKind)EditorGUILayout.EnumPopup("Kind", _spec.Kind);
        _spec.AssetName = EditorGUILayout.TextField("Prefab name", _spec.AssetName);
        _spec.DisplayName = EditorGUILayout.TextField("Display name", _spec.DisplayName);
        _spec.MaxHp = Mathf.Max(1, EditorGUILayout.IntField("Max HP", _spec.MaxHp));
        _spec.Portrait = (Sprite)EditorGUILayout.ObjectField("Portrait sprite", _spec.Portrait, typeof(Sprite), false);
        _spec.Tint = EditorGUILayout.ColorField("Tint", _spec.Tint);
        _spec.SpriteHeight = EditorGUILayout.Slider("Sprite height (units)", _spec.SpriteHeight, 0.5f, 6f);
        _spec.BurnVfx = EditorGUILayout.Toggle("Burn overlay", _spec.BurnVfx);
        _spec.FurBallVfx = EditorGUILayout.Toggle("Fur ball overlay", _spec.FurBallVfx);

        if (GUILayout.Button("Create / overwrite prefab", GUILayout.Height(28)))
        {
            string path = SpellTyposBuilder.CombatantPath(_spec);
            bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
            if (exists && !EditorUtility.DisplayDialog("Overwrite prefab?",
                    $"{path} already exists. Rebuild it from these settings?", "Overwrite", "Cancel"))
                return;

            GameObject prefab = SpellTyposBuilder.CreateCombatantPrefab(_spec);
            if (prefab != null)
            {
                if (_spec.Kind == CombatantKind.Player) _playerPrefab = prefab;
                else _enemyPrefab = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
        }

        EditorGUILayout.Space(12f);
        Header("Existing combatant prefabs");
        if (!AssetDatabase.IsValidFolder(SpellTyposBuilder.CombatantsFolder))
        {
            EditorGUILayout.LabelField("None yet.", EditorStyles.miniLabel);
            return;
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { SpellTyposBuilder.CombatantsFolder }))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
                if (GUILayout.Button("As player", GUILayout.Width(70f))) _playerPrefab = prefab;
                if (GUILayout.Button("As enemy", GUILayout.Width(70f))) _enemyPrefab = prefab;
            }
        }
    }

    // ----------------------------------------------------------------- spells

    private void DrawSpells()
    {
        Header("Abilities (spell effects)");
        EditorGUILayout.HelpBox(
            "An effect is what a spell does (damage, heal, burn...). Pick a type, tweak it and create the asset. New SpellEffect subclasses you write show up here automatically.",
            MessageType.None);
        _effectPanel.OnGUI();

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Existing effects", EditorStyles.miniBoldLabel);
        DrawAssetList("SpellEffect");

        EditorGUILayout.Space(14f);
        Header("Spell book");
        DrawSpellBook();
    }

    private void DrawSpellBook()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            _book = (SpellBook)EditorGUILayout.ObjectField("Spell book", _book, typeof(SpellBook), false);
            if (GUILayout.Button("New", GUILayout.Width(60f)))
            {
                _book = SpellTyposBuilder.CreateEmptyBook();
                _bookSo = null;
            }
        }

        if (_book == null)
        {
            EditorGUILayout.HelpBox("Pick a spell book or create one.", MessageType.Info);
            return;
        }

        if (_bookSo == null || _bookSo.targetObject != _book) _bookSo = new SerializedObject(_book);
        _bookSo.Update();
        EditorGUILayout.PropertyField(_bookSo.FindProperty("_entries"), true);
        _bookSo.ApplyModifiedProperties();

        var problems = new List<string>();
        var seen = new HashSet<string>();
        foreach (SpellEntry e in _book.Entries)
        {
            if (e == null) continue;
            string word = CastResolver.Normalize(e.Word);
            if (string.IsNullOrEmpty(word)) { problems.Add("An entry has an empty word."); continue; }
            if (e.Effect == null) problems.Add($"\"{word}\" has no effect.");
            if (!seen.Add(word)) problems.Add($"\"{word}\" is defined more than once.");
        }
        if (problems.Count > 0)
            EditorGUILayout.HelpBox(string.Join("\n", problems), MessageType.Warning);

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Quick add", EditorStyles.miniBoldLabel);
        _newWord = EditorGUILayout.TextField("Word", _newWord);
        _newEffect = (SpellEffect)EditorGUILayout.ObjectField("Effect", _newEffect, typeof(SpellEffect), false);
        _newPower = Mathf.Max(1, EditorGUILayout.IntField("Power", _newPower));
        _newVfx = (SpellVfxKind)EditorGUILayout.EnumPopup("Animation", _newVfx);

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_newWord) || _newEffect == null))
        {
            if (GUILayout.Button("Add spell"))
            {
                SpellTyposBuilder.AddSpell(_book, CastResolver.Normalize(_newWord), _newEffect, _newPower, _newVfx);
                _newWord = "";
            }
        }

        EditorGUILayout.Space(6f);
        EditorGUILayout.HelpBox(
            "Gives the default animation to spells that still have none (fire, bolt, heal, spark, fear, water, ice, isis, meteor, cleanse, explosion, mirror, gamble, cut, salt, shatter, git, rainbow, gay). Switches \"fear\" to the Fear effect, and \"bolt\" and \"spark\" to the Shock damage (extra damage on wet targets), when they still use the default Damage asset. Adds the \"water\" (makes the target wet), \"ice\" (freezes the target), \"isis\" (instantly wins the game, with hearts everywhere), \"meteor\" (3 waves of meteors that hurt you and the enemy), \"cleanse\", \"explosion\" (2 uses per run), \"mirror\", \"gamble\" (slot machine), \"cut\" (one slash per \"cut\" typed in the same line, e.g. cut cut cut; every slash leaves a scar), \"salt\" (stings every scar for damage, then closes them), \"shatter\" (huge damage to a frozen target, which thaws), \"git\" (hurls the binary code of 1710: every 1 hits, every 0 misses), \"rainbow\" (plus \"gay\", a second word for the same spell: a rainbow that gives the enemy burn, fear, wet and freeze), \"poison\" (stacking poison that ignores shield) and \"poop\" (also \"shit\": poison for every 2 scars on the enemy) spells if the book has no entry for those words. Also adds alternative words that copy an existing spell (fireball and fire ball for fire, lightning for bolt, slice and slash for cut, zap, frost, splash, scare...). Nothing else is changed.",
            MessageType.None);
        if (GUILayout.Button("Upgrade spell book (animations, Fear, Shock, new spells incl. Cut, Salt, Shatter, Git, Rainbow, Poison, Poop)"))
        {
            SpellTyposBuilder.UpgradeBook(_book, SpellTyposBuilder.EnsureDefaults());
            AssetDatabase.SaveAssets();
            _bookSo = null;
        }
    }

    // ----------------------------------------------------------------- sounds

    private void DrawSounds()
    {
        Header("Spell sounds");
        EditorGUILayout.HelpBox(
            "Assign an audio clip to every spell word. It plays when the spell is cast, by you or by an enemy, and is fitted to the spell's animation: \"Fit To Animation\" stretches/trims it to the animation length, \"Fit And Loop\" loops short clips to fill it, \"Natural\" plays it untouched. \"Hit at (s)\" is the moment inside the clip where the impact sounds; it is lined up with the animation's first impact. \"Per hit\" fits the clip to the gap between hits (cut cut cut, git, meteor waves). Gamble: the main clip is the background (loops while the reels spin), plus a Reel stop sound (pitch rises per reel) and a Jackpot sound. Pitch limits and reel/jackpot semitones are on the SpellAudioPlayer component. Spells with no clip are silent.",
            MessageType.None);

        _book = (SpellBook)EditorGUILayout.ObjectField("Spell book", _book, typeof(SpellBook), false);
        if (_book == null)
        {
            EditorGUILayout.HelpBox("Pick a spell book (or create one in the Spells tab).", MessageType.Info);
            return;
        }

        if (_bookSo == null || _bookSo.targetObject != _book) _bookSo = new SerializedObject(_book);
        _bookSo.Update();
        SerializedProperty entries = _bookSo.FindProperty("_entries");

        int withSound = 0;
        for (int i = 0; i < entries.arraySize; i++)
            if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("_sound").objectReferenceValue != null) withSound++;
        EditorGUILayout.LabelField($"{withSound} of {entries.arraySize} spells have a sound.", EditorStyles.miniLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            _soundFilter = EditorGUILayout.TextField("Search", _soundFilter);
            _onlyMissingSounds = EditorGUILayout.ToggleLeft("Only without sound", _onlyMissingSounds, GUILayout.Width(140f));
        }

        EditorGUILayout.Space(4f);

        string filter = (_soundFilter ?? "").Trim().ToLowerInvariant();
        int shown = 0;
        for (int i = 0; i < entries.arraySize; i++)
        {
            SerializedProperty e = entries.GetArrayElementAtIndex(i);
            string word = e.FindPropertyRelative("_word").stringValue;
            SerializedProperty sound = e.FindPropertyRelative("_sound");

            if (filter.Length > 0 && (word ?? "").ToLowerInvariant().IndexOf(filter, System.StringComparison.Ordinal) < 0) continue;
            if (_onlyMissingSounds && sound.objectReferenceValue != null) continue;
            shown++;

            var effect = e.FindPropertyRelative("_effect").objectReferenceValue as SpellEffect;
            string info = effect != null ? effect.Label : "no effect";

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(string.IsNullOrEmpty(word) ? "(empty word)" : word, EditorStyles.boldLabel,
                        GUILayout.MinWidth(80f));
                    EditorGUILayout.LabelField(info, EditorStyles.miniLabel);

                    using (new EditorGUI.DisabledScope(sound.objectReferenceValue == null))
                    {
                        if (GUILayout.Button("Play", GUILayout.Width(44f))) PlayPreview(sound.objectReferenceValue as AudioClip);
                        if (GUILayout.Button("Stop", GUILayout.Width(44f))) StopPreview();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(sound, GUIContent.none);
                    EditorGUILayout.PropertyField(e.FindPropertyRelative("_soundVolume"), GUIContent.none, GUILayout.Width(110f));
                    EditorGUILayout.PropertyField(e.FindPropertyRelative("_soundPerHit"), new GUIContent("Per hit"), GUILayout.Width(70f));
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(e.FindPropertyRelative("_soundFit"), GUIContent.none);
                    EditorGUILayout.LabelField("Hit at (s)", GUILayout.Width(60f));
                    EditorGUILayout.PropertyField(e.FindPropertyRelative("_soundImpactTime"), GUIContent.none, GUILayout.Width(60f));
                }

                // The slot machine has two more sounds: the reel stop and the jackpot.
                if (e.FindPropertyRelative("_vfx").enumValueIndex == (int)SpellVfxKind.Slots)
                {
                    EditorGUILayout.LabelField("Background = the sound above (loops while the reels spin).", EditorStyles.miniLabel);
                    DrawExtraSound("Reel stop", e.FindPropertyRelative("_reelStopSound"), e.FindPropertyRelative("_reelStopVolume"));
                    DrawExtraSound("Jackpot", e.FindPropertyRelative("_jackpotSound"), e.FindPropertyRelative("_jackpotVolume"));
                }
            }
        }

        if (shown == 0) EditorGUILayout.LabelField("No spells match.", EditorStyles.miniLabel);

        _bookSo.ApplyModifiedProperties();

        EditorGUILayout.Space(6f);
        EditorGUILayout.HelpBox(
            "Copies the sound (volume and per-hit setting too) from another spell with the same effect, power and animation to the ones that have none, so \"fireball\" gets the sound of \"fire\" and \"gay\" the one of \"rainbow\". Spells that already have a sound are not changed. The preview plays the clip at full volume.",
            MessageType.None);
        if (GUILayout.Button("Fill empty sounds from matching spells"))
        {
            int filled = SpellTyposBuilder.FillMissingSounds(_book);
            AssetDatabase.SaveAssets();
            _bookSo = null;
            Debug.Log($"[SpellTypos] Copied a sound to {filled} spell(s).");
        }
    }

    private static void DrawExtraSound(string label, SerializedProperty clip, SerializedProperty volume)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(label, GUILayout.Width(70f));
            EditorGUILayout.PropertyField(clip, GUIContent.none);
            EditorGUILayout.PropertyField(volume, GUIContent.none, GUILayout.Width(110f));
            using (new EditorGUI.DisabledScope(clip.objectReferenceValue == null))
            {
                if (GUILayout.Button("Play", GUILayout.Width(44f))) PlayPreview(clip.objectReferenceValue as AudioClip);
                if (GUILayout.Button("Stop", GUILayout.Width(44f))) StopPreview();
            }
        }
    }

    // Plays a clip in the editor through Unity's internal AudioUtil (the same thing the inspector preview uses).
    // It lives in an internal class and its name changed between Unity versions, so it is looked up by reflection.
    private static void PlayPreview(AudioClip clip)
    {
        if (clip == null) return;
        StopPreview();

        System.Type util = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
        MethodInfo play = null;
        if (util != null)
        {
            var args = new[] { typeof(AudioClip), typeof(int), typeof(bool) };
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            play = util.GetMethod("PlayPreviewClip", flags, null, args, null)
                   ?? util.GetMethod("PlayClip", flags, null, args, null);
        }

        if (play == null)
        {
            Debug.LogWarning("[SpellTypos] Could not find the editor audio preview in this Unity version. Select the clip and use the preview in the Inspector instead.");
            EditorGUIUtility.PingObject(clip);
            return;
        }

        play.Invoke(null, new object[] { clip, 0, false });
    }

    private static void StopPreview()
    {
        System.Type util = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
        if (util == null) return;

        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        MethodInfo stop = util.GetMethod("StopAllPreviewClips", flags, null, System.Type.EmptyTypes, null)
                          ?? util.GetMethod("StopAllClips", flags, null, System.Type.EmptyTypes, null);
        if (stop != null) stop.Invoke(null, null);
    }

    // ---------------------------------------------------------------- filters

    private void DrawFilters()
    {
        Header("Language filters");
        EditorGUILayout.HelpBox(
            "Filters rewrite every spell word typed in a room, in order. If the result is a word in the spell book it becomes the effective spell, otherwise it is just noise. New LanguageFilter subclasses show up here automatically.",
            MessageType.None);
        _filterPanel.OnGUI();

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Existing filters", EditorStyles.miniBoldLabel);
        DrawAssetList("LanguageFilter");
    }

    // ------------------------------------------------------------------ mixes

    private void DrawMixes()
    {
        Header("Spell mixes");
        EditorGUILayout.HelpBox(
            "Type two spells in one line, in any order, to mix them: \"fire cut\" casts a cut that also sets the target on fire. " +
            "The Body is the spell that is cast (animation, sound, power, repeats: \"cut fire cut\" = 2 flaming slashes). " +
            "The Rider is mixed into it: its effect is added (once, or on every hit) and its colour tints the animation. " +
            "Rainbow cycles every colour instead. Mixes are matched by effect, so aliases of a spell work too. " +
            "With the Familiar as body the rider is not cast: the pet casts it on the enemy once every turn (familiar fire = a fire pet). " +
            "Mixes use up the uses of both spells. Enemies can cast a mix by listing its two words in their room words (\"fire cut\").",
            MessageType.None);

        _book = (SpellBook)EditorGUILayout.ObjectField("Spell book", _book, typeof(SpellBook), false);
        if (_book == null)
        {
            EditorGUILayout.HelpBox("Pick a spell book (or create one in the Spells tab).", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Add default mixes (fire cut, git rainbow...)"))
        {
            int added = SpellTyposBuilder.AddDefaultMixes(_book);
            ShowNotification(new GUIContent(added == 0 ? "No new mixes to add" : $"Added {added} mixes"));
            _bookSo = null;
        }

        if (_bookSo == null || _bookSo.targetObject != _book) _bookSo = new SerializedObject(_book);
        _bookSo.Update();
        EditorGUILayout.PropertyField(_bookSo.FindProperty("_mixes"), new GUIContent("Mixes"), true);
        _bookSo.ApplyModifiedProperties();
    }

    private static void DrawAssetList(string typeName)
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeName}");
        if (guids.Length == 0)
        {
            EditorGUILayout.LabelField("None yet.", EditorStyles.miniLabel);
            return;
        }

        foreach (string guid in guids)
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
            EditorGUILayout.ObjectField(asset, typeof(Object), false);
        }
    }

    // ------------------------------------------------------------------ rooms

    private void DrawRooms()
    {
        Header("Rooms (RunController in the open scene)");

        RunController run = FindFirstObjectByType<RunController>();
        if (run == null)
        {
            EditorGUILayout.HelpBox("No RunController in the open scene. Open your battle scene or build one in the Setup tab.",
                MessageType.Info);
            return;
        }

        AutoAssignFilters(run);

        EditorGUILayout.HelpBox(
            "Rooms with the default names (Echo Cave, Hall of Ire, Crooked Library, Mirror Maze, Throne of Tongues) get their filters assigned automatically when this tab opens. Rooms that already have filters are never changed.",
            MessageType.None);

        if (_runSo == null || _runSo.targetObject != run) _runSo = new SerializedObject(run);
        _runSo.Update();
        EditorGUILayout.PropertyField(_runSo.FindProperty("_playerMaxHp"));
        EditorGUILayout.PropertyField(_runSo.FindProperty("_healBetweenRooms"));
        EditorGUILayout.PropertyField(_runSo.FindProperty("_bannerTime"));
        EditorGUILayout.PropertyField(_runSo.FindProperty("_rooms"), true);
        _runSo.ApplyModifiedProperties();

        EditorGUILayout.Space(6f);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Validate rooms")) ValidateRooms(run);
            if (GUILayout.Button("Assign missing filters"))
            {
                int changed = SpellTyposBuilder.AssignMissingFilters(run);
                Debug.Log($"[SpellTypos] Assigned default filters to {changed} room(s).");
                _runSo = null;
            }
            if (GUILayout.Button("Replace with default rooms") &&
                EditorUtility.DisplayDialog("Replace rooms?", "This replaces every room on the RunController.",
                    "Replace", "Cancel"))
            {
                SpellTyposBuilder.FillDefaultRooms(run, SpellTyposBuilder.EnsureDefaults());
                _runSo = null;
            }
        }
    }

    // Fills in the missing filters of the default rooms the first time this RunController is shown in the editor
    // session. SessionState survives recompiles, so a filter you remove on purpose is not put back later.
    private void AutoAssignFilters(RunController run)
    {
        string key = $"SpellTypos.FiltersAssigned.{run.GetInstanceID()}";
        if (SessionState.GetBool(key, false)) return;
        SessionState.SetBool(key, true);

        if (SpellTyposBuilder.AssignMissingFilters(run) > 0) _runSo = null;
    }

    private static void ValidateRooms(RunController run)
    {
        SpellBook book = null;
        TurnBattle battle = FindFirstObjectByType<TurnBattle>();
        if (battle != null)
            book = new SerializedObject(battle).FindProperty("_book").objectReferenceValue as SpellBook;

        SerializedProperty rooms = new SerializedObject(run).FindProperty("_rooms");
        int problems = 0;

        if (rooms.arraySize == 0)
        {
            Debug.LogWarning("[SpellTypos] RunController has no rooms.");
            problems++;
        }

        for (int i = 0; i < rooms.arraySize; i++)
        {
            SerializedProperty room = rooms.GetArrayElementAtIndex(i);
            string roomName = room.FindPropertyRelative("_roomName").stringValue;

            SerializedProperty filters = room.FindPropertyRelative("_filters");
            for (int f = 0; f < filters.arraySize; f++)
            {
                if (filters.GetArrayElementAtIndex(f).objectReferenceValue != null) continue;
                Debug.LogWarning($"[SpellTypos] Room '{roomName}': filter #{f} is empty.");
                problems++;
            }

            SerializedProperty words = room.FindPropertyRelative("_enemyWords");
            if (words.arraySize == 0)
                Debug.LogWarning($"[SpellTypos] Room '{roomName}': the enemy has no words, so it never attacks.");

            for (int w = 0; w < words.arraySize; w++)
            {
                string word = CastResolver.Normalize(words.GetArrayElementAtIndex(w).stringValue);
                if (book == null || book.TryGet(word, out _)) continue;
                Debug.LogWarning($"[SpellTypos] Room '{roomName}': enemy word \"{word}\" is not in the spell book.");
                problems++;
            }
        }

        Debug.Log($"[SpellTypos] Room check finished: {problems} problem(s).");
    }

    // ------------------------------------------------- generic "create asset"

    // Shows the default inspector of a throwaway instance of a chosen ScriptableObject subtype
    // and saves it as an asset on demand.
    private sealed class DraftPanel
    {
        private readonly System.Type _baseType;
        private readonly string _folder;

        private System.Type[] _types;
        private string[] _names;
        private int _typeIndex;
        private ScriptableObject _draft;
        private Editor _editor;
        private string _fileName = "";

        internal DraftPanel(System.Type baseType, string folder)
        {
            _baseType = baseType;
            _folder = folder;
        }

        internal void OnGUI()
        {
            if (_types == null) Refresh();
            if (_types.Length == 0)
            {
                EditorGUILayout.HelpBox($"No concrete {_baseType.Name} types found.", MessageType.Info);
                return;
            }

            if (_draft == null) Rebuild();

            EditorGUI.BeginChangeCheck();
            int index = EditorGUILayout.Popup("Type", _typeIndex, _names);
            if (EditorGUI.EndChangeCheck())
            {
                _typeIndex = index;
                DestroyDraft();
                return;
            }

            if (_editor != null) _editor.OnInspectorGUI();

            _fileName = EditorGUILayout.TextField("File name", _fileName);
            if (GUILayout.Button("Create asset")) Save();
        }

        internal void Dispose()
        {
            DestroyDraft();
        }

        private void Refresh()
        {
            _types = TypeCache.GetTypesDerivedFrom(_baseType)
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.Name)
                .ToArray();
            _names = _types.Select(t => ObjectNames.NicifyVariableName(t.Name)).ToArray();
        }

        private void Rebuild()
        {
            DestroyDraft();
            _typeIndex = Mathf.Clamp(_typeIndex, 0, _types.Length - 1);
            _draft = ScriptableObject.CreateInstance(_types[_typeIndex]);
            _editor = Editor.CreateEditor(_draft);
            _fileName = _types[_typeIndex].Name;
        }

        private void Save()
        {
            string fileName = string.IsNullOrWhiteSpace(_fileName) ? _types[_typeIndex].Name : _fileName.Trim();
            SpellTyposBuilder.EnsureFolder(_folder);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{_folder}/{fileName}.asset");

            ScriptableObject created = _draft;
            if (_editor != null) Object.DestroyImmediate(_editor);
            _editor = null;
            _draft = null;

            AssetDatabase.CreateAsset(created, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = created;
            EditorGUIUtility.PingObject(created);
        }

        private void DestroyDraft()
        {
            if (_editor != null) Object.DestroyImmediate(_editor);
            if (_draft != null && !AssetDatabase.Contains(_draft)) Object.DestroyImmediate(_draft);
            _editor = null;
            _draft = null;
        }
    }
}
