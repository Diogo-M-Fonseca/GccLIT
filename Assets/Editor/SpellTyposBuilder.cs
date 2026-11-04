using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

internal enum CombatantKind { Player, Enemy }

[System.Serializable]
internal sealed class CombatantSpec
{
    public CombatantKind Kind;
    public string AssetName = "Player";
    public string DisplayName = "Wizard";
    public int MaxHp = 30;
    public Sprite Portrait;
    public Color Tint = Color.white;
    [Tooltip("Height of the sprite in world units.")]
    public float SpriteHeight = 2f;
    public bool BurnVfx = true;
    public bool FurBallVfx = true;

    internal static CombatantSpec NewPlayer()
    {
        return new CombatantSpec
        {
            Kind = CombatantKind.Player,
            AssetName = "Player",
            DisplayName = "Wizard",
            MaxHp = 30,
            Tint = new Color(0.55f, 0.7f, 1f),
            SpriteHeight = 2f
        };
    }

    internal static CombatantSpec NewEnemy()
    {
        return new CombatantSpec
        {
            Kind = CombatantKind.Enemy,
            AssetName = "Enemy",
            DisplayName = "Goblin",
            MaxHp = 20,
            Tint = new Color(0.95f, 0.5f, 0.45f),
            SpriteHeight = 3f
        };
    }
}

internal sealed class Defaults
{
    internal SpellEffect Damage;
    internal SpellEffect Heal;
    internal SpellEffect Shield;
    internal SpellEffect Burn;
    internal SpellEffect FurBall;
    internal SpellEffect Fear;
    internal SpellEffect Shock;
    internal SpellEffect Wet;
    internal SpellEffect Freeze;
    internal SpellEffect InstantWin;
    internal SpellEffect Meteor;
    internal SpellEffect Cleanse;
    internal SpellEffect Explosion;
    internal SpellEffect Mirror;
    internal SpellEffect Gamble;
    internal SpellEffect Cut;
    internal SpellEffect Salt;
    internal SpellEffect Shatter;
    internal SpellEffect Git;
    internal SpellEffect Rainbow;
    internal SpellEffect Poison;
    internal SpellEffect Poop;
    internal SpellEffect Familiar;
    internal SpellEffect Sacrifice;
    internal LanguageFilter Echo;
    internal LanguageFilter Reverse;
    internal LanguageFilter Ire;
    internal LanguageFilter DropLast;
    internal LanguageFilter Crooked;
    internal SpellBook Book;
}

internal static class SpellTyposBuilder
{
    internal const string Root = "Assets/SpellTypos";
    internal const string DataFolder = Root + "/Data";
    internal const string EffectsFolder = DataFolder + "/Effects";
    internal const string FiltersFolder = DataFolder + "/Filters";
    internal const string BookPath = DataFolder + "/SpellBook.asset";
    internal const string CombatantsFolder = Root + "/Prefabs/Combatants";
    internal const string UiFolder = Root + "/Prefabs/UI";
    internal const string ScenesFolder = Root + "/Scenes";

    private const string WordPrefabPath = UiFolder + "/WordText.prefab";
    private const string RowPrefabPath = UiFolder + "/GrimoireRow.prefab";

    // World layout: 1 world unit = 100 UI reference pixels (camera ortho size 5.4 = 1080 / 2 / 100).
    private const float CameraSize = 5.4f;
    private static readonly Vector3 EnemyPosition = new Vector3(0f, 1.4f, 0f);
    private static readonly Vector3 PlayerPosition = new Vector3(-7.2f, -3.3f, 0f);

    private static readonly Color Gold = new Color(1f, 0.9f, 0.55f);

    // ------------------------------------------------------------------ basics

    internal static bool TmpReady
    {
        get { return TMP_Settings.instance != null; }
    }

    internal static bool EnsureTmp()
    {
        if (TmpReady) return true;
        if (EditorUtility.DisplayDialog("TextMeshPro",
                "TMP Essential Resources are not imported yet and the UI needs them.\nImport them now? (Run the tool again afterwards.)",
                "Import", "Cancel"))
        {
            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
        }
        return false;
    }

    internal static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
    }

    internal static void EnsureFolders()
    {
        EnsureFolder(EffectsFolder);
        EnsureFolder(FiltersFolder);
        EnsureFolder(CombatantsFolder);
        EnsureFolder(UiFolder);
        EnsureFolder(ScenesFolder);
    }

    // Writes a private [SerializeField] through SerializedObject.
    internal static void Set(Object target, string property, object value)
    {
        var so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(property);
        if (p == null)
        {
            Debug.LogWarning($"[SpellTypos] {target.GetType().Name} has no serialized field '{property}'.");
            return;
        }

        switch (value)
        {
            case null: p.objectReferenceValue = null; break;
            case Object o: p.objectReferenceValue = o; break;
            case int i: p.intValue = i; break;
            case float f: p.floatValue = f; break;
            case bool b: p.boolValue = b; break;
            case string s: p.stringValue = s; break;
            case Color c: p.colorValue = c; break;
            default:
                Debug.LogWarning($"[SpellTypos] Unsupported value type {value.GetType().Name} for '{property}'.");
                break;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static T EnsureAsset<T>(string path, System.Action<T> init) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<T>();
        if (init != null) init(asset);
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    // ------------------------------------------------------------ default data

    internal static Defaults EnsureDefaults()
    {
        EnsureFolders();
        var d = new Defaults();

        d.Damage = EnsureAsset<DamageEffect>($"{EffectsFolder}/Damage.asset", a =>
        {
            Set(a, "_label", "Damage");
            Set(a, "_description", "Hurts the target.");
        });
        d.Heal = EnsureAsset<HealEffect>($"{EffectsFolder}/Heal.asset", a =>
        {
            Set(a, "_label", "Heal");
            Set(a, "_description", "Restores the caster's HP.");
        });
        d.Shield = EnsureAsset<ShieldEffect>($"{EffectsFolder}/Shield.asset", a =>
        {
            Set(a, "_label", "Shield");
            Set(a, "_description", "Absorbs damage before HP is lost.");
        });
        d.Burn = EnsureAsset<BurnEffect>($"{EffectsFolder}/Burn.asset", a =>
        {
            Set(a, "_label", "Burn");
            Set(a, "_description", "Damage every turn for a few turns (power = damage per turn).");
        });
        d.FurBall = EnsureAsset<FurBallEffect>($"{EffectsFolder}/FurBall.asset", a =>
        {
            Set(a, "_label", "Fur Ball");
            Set(a, "_description", "A fur ball swallows incoming hits (power = hits blocked).");
        });
        d.Fear = EnsureAsset<FearEffect>($"{EffectsFolder}/Fear.asset", a =>
        {
            Set(a, "_label", "Fear");
            Set(a, "_description", "The target shakes and misses its attacks (power = attacks missed).");
        });
        d.Shock = EnsureAsset<DamageEffect>($"{EffectsFolder}/Shock.asset", a =>
        {
            Set(a, "_label", "Shock");
            Set(a, "_description", "Hurts the target, and hurts it more while it is wet.");
            Set(a, "_wetMultiplier", 1.5f);
        });
        d.Wet = EnsureAsset<WetEffect>($"{EffectsFolder}/Wet.asset", a =>
        {
            Set(a, "_label", "Wet");
            Set(a, "_description", "Soaks the target for a few turns, so Shock spells hit harder.");
        });
        d.Freeze = EnsureAsset<FreezeEffect>($"{EffectsFolder}/Freeze.asset", a =>
        {
            Set(a, "_label", "Freeze");
            Set(a, "_description", "The target loses its next few actions. Fire thaws it early.");
        });
        d.InstantWin = EnsureAsset<InstantWinEffect>($"{EffectsFolder}/InstantWin.asset", a =>
        {
            Set(a, "_label", "Instant Win");
            Set(a, "_description", "Defeats the target on the spot and wins the whole game.");
        });
        d.Meteor = EnsureAsset<MeteorShowerEffect>($"{EffectsFolder}/MeteorShower.asset", a =>
        {
            Set(a, "_label", "Meteor Shower");
            Set(a, "_description", "Meteors rain on the whole room: each wave hurts you AND the enemy (power = damage per wave, 3 waves).");
        });

        d.Cleanse = EnsureAsset<CleanseEffect>($"{EffectsFolder}/Cleanse.asset", a =>
        {
            Set(a, "_label", "Cleanse");
            Set(a, "_description", "Removes every status effect, with all its stacks, from the caster (burn, fear, wet, freeze, fur ball, mirror). Shield is kept.");
        });
        d.Explosion = EnsureAsset<ExplosionEffect>($"{EffectsFolder}/Explosion.asset", a =>
        {
            Set(a, "_label", "Explosion");
            Set(a, "_description", "Kills you AND the enemy. You still win the room, but at what cost. Limited uses per run (see the spell entry).");
        });

        d.Mirror = EnsureAsset<MirrorEffect>($"{EffectsFolder}/Mirror.asset", a =>
        {
            Set(a, "_label", "Mirror");
            Set(a, "_description", "Bounces the next spell aimed at you back at its caster (power = spells bounced).");
        });
        d.Gamble = EnsureAsset<SlotMachineEffect>($"{EffectsFolder}/SlotMachine.asset", a =>
        {
            Set(a, "_label", "Gamble");
            Set(a, "_description", "A slot machine: jackpot (three matching reels) deals big damage, anything else does nothing and you lose the turn.");
            Set(a, "_jackpotChance", 0.2f);
        });
        d.Cut = EnsureAsset<CutEffect>($"{EffectsFolder}/Cut.asset", a =>
        {
            Set(a, "_label", "Cut");
            Set(a, "_description", "One slash for every \"cut\" typed in the same line (cut cut cut). Power = damage per slash, and every slash leaves a scar on the target.");
            Set(a, "_maxHits", 20);
        });
        d.Salt = EnsureAsset<SaltEffect>($"{EffectsFolder}/Salt.asset", a =>
        {
            Set(a, "_label", "Salt");
            Set(a, "_description", "Salt pours into every scar on the target (power = damage per scar), then the scars close. Pairs with Cut.");
        });
        d.Shatter = EnsureAsset<ShatterEffect>($"{EffectsFolder}/Shatter.asset", a =>
        {
            Set(a, "_label", "Shatter");
            Set(a, "_description", "Breaks the target like glass: huge damage if it is frozen (and the ice is gone), only a few shards otherwise. Pairs with Ice.");
            Set(a, "_frozenMultiplier", 5f);
        });
        d.Git = EnsureAsset<GitEffect>($"{EffectsFolder}/Git.asset", a =>
        {
            Set(a, "_label", "Git");
            Set(a, "_description", "Hurls the binary code of 1710 (11010101110) at the enemy: every 1 hits, every 0 misses, and a run of 1s is a combo (power x1, x2, x3...).");
            Set(a, "_number", 1710);
        });
        d.Rainbow = EnsureAsset<RainbowEffect>($"{EffectsFolder}/Rainbow.asset", a =>
        {
            Set(a, "_label", "Rainbow");
            Set(a, "_description", "Shoots a rainbow that gives the target every harmful status at once: burn (power = damage per turn), fear, wet and freeze. Also cast as \"gay\".");
            Set(a, "_burnTurns", 3);
            Set(a, "_fearTurns", 2);
            Set(a, "_wetTurns", 4);
            Set(a, "_freezeTurns", 2);
        });
        d.Poison = EnsureAsset<PoisonEffect>($"{EffectsFolder}/Poison.asset", a =>
        {
            Set(a, "_label", "Poison");
            Set(a, "_description", "Poisons the target (power = stacks). Every turn it loses HP equal to its stacks, ignoring shield and fur ball, then one stack wears off. Stacks add up.");
        });
        d.Poop = EnsureAsset<PoopEffect>($"{EffectsFolder}/Poop.asset", a =>
        {
            Set(a, "_label", "Poop");
            Set(a, "_description", "Adds poison to the target for every 2 scars it has (left by Cut). Power = poison stacks per 2 scars. Also cast as \"shit\".");
            Set(a, "_scarsPerStack", 2);
        });

        d.Familiar = EnsureAsset<FamiliarEffect>($"{EffectsFolder}/Familiar.asset", a =>
        {
            Set(a, "_label", "Familiar");
            Set(a, "_description", "Summons a pet (power = its HP) that takes hits for you. Mix it with another spell (\"familiar fire\") and the pet also casts that spell on the enemy once every turn: fire burns, ice freezes, water soaks, poison, fear, rainbow gives everything. Also cast as \"pet\".");
        });
        d.Sacrifice = EnsureAsset<SacrificeEffect>($"{EffectsFolder}/Sacrifice.asset", a =>
        {
            Set(a, "_label", "Sacrifice");
            Set(a, "_description", "Give something up for double damage for the rest of the room (it stacks). After casting, type what to give: hp (power x your current bonus in HP, it can kill you), familiar, or a spell word (sealed for the rest of the run).");
            Set(a, "_hpCostScalesWithBonus", true);
        });

        d.Ire = EnsureAsset<SubstitutionFilter>($"{FiltersFolder}/TongueOfIre.asset", a =>
        {
            Set(a, "_displayName", "Tongue of Ire");
            Set(a, "_hint", "\"ear\" becomes \"ire\"");
            Set(a, "_from", "ear");
            Set(a, "_to", "ire");
        });
        d.DropLast = EnsureAsset<DropLetterFilter>($"{FiltersFolder}/ClippedTongue.asset", a =>
        {
            Set(a, "_displayName", "Clipped Tongue");
            Set(a, "_hint", "the last letter vanishes");
            Set(a, "_index", -1);
        });
        d.Crooked = EnsureAsset<SwapLettersFilter>($"{FiltersFolder}/CrookedTongue.asset", a =>
        {
            Set(a, "_displayName", "Crooked Tongue");
            Set(a, "_hint", "the 2nd and 3rd letters trade places");
            Set(a, "_a", 1);
            Set(a, "_b", 2);
        });
        d.Echo = EnsureAsset<EchoFilter>($"{FiltersFolder}/EchoingTongue.asset", a =>
        {
            Set(a, "_displayName", "Echoing Tongue");
            Set(a, "_hint", "only the first half gets through: say it twice");
        });
        d.Reverse = EnsureAsset<ReverseFilter>($"{FiltersFolder}/MirrorTongue.asset", a =>
        {
            Set(a, "_displayName", "Mirror Tongue");
            Set(a, "_hint", "words come out backwards");
        });

        bool freshBook = AssetDatabase.LoadAssetAtPath<SpellBook>(BookPath) == null;
        d.Book = EnsureAsset<SpellBook>(BookPath, null);
        if (freshBook)
        {
            AddSpell(d.Book, "spark", d.Shock, 5, SpellVfxKind.Spark);
            AddSpell(d.Book, "bolt", d.Shock, 8, SpellVfxKind.Thunder);
            AddSpell(d.Book, "blot", d.Damage, 1);
            AddSpell(d.Book, "fear", d.Fear, 2, SpellVfxKind.DarkAura);
            AddSpell(d.Book, "fury", d.Damage, 6);
            AddSpell(d.Book, "fire", d.Burn, 2, SpellVfxKind.Fireball);
            AddSpell(d.Book, "heal", d.Heal, 6, SpellVfxKind.Heal);
            AddSpell(d.Book, "shield", d.Shield, 5);
            AddSpell(d.Book, "fur", d.FurBall, 1);
            AddSpell(d.Book, "water", d.Wet, 1, SpellVfxKind.Splash);
            AddSpell(d.Book, "ice", d.Freeze, 1, SpellVfxKind.Frost);
            AddSpell(d.Book, "isis", d.InstantWin, 1, SpellVfxKind.Hearts);
            AddSpell(d.Book, "meteor", d.Meteor, 4, SpellVfxKind.Meteor);
            AddSpell(d.Book, "cleanse", d.Cleanse, 1, SpellVfxKind.Heal);
            AddSpell(d.Book, "explosion", d.Explosion, 1, SpellVfxKind.Explosion, 2);
            AddSpell(d.Book, "mirror", d.Mirror, 1, SpellVfxKind.Mirror);
            AddSpell(d.Book, "gamble", d.Gamble, 25, SpellVfxKind.Slots, soundFit: SpellSoundFit.FitAndLoop);
            AddSpell(d.Book, "cut", d.Cut, 1, SpellVfxKind.Cut);
            AddSpell(d.Book, "salt", d.Salt, 2, SpellVfxKind.Salt);
            AddSpell(d.Book, "shatter", d.Shatter, 3, SpellVfxKind.Shatter);
            AddSpell(d.Book, "git", d.Git, 1, SpellVfxKind.Git);
            AddSpell(d.Book, "rainbow", d.Rainbow, 1, SpellVfxKind.Rainbow);
            AddSpell(d.Book, "gay", d.Rainbow, 1, SpellVfxKind.Rainbow);
            AddSpell(d.Book, "poison", d.Poison, 3, SpellVfxKind.Poison);
            AddSpell(d.Book, "poop", d.Poop, 1, SpellVfxKind.Poop);
            AddSpell(d.Book, "familiar", d.Familiar, 4, SpellVfxKind.Familiar);
            AddSpell(d.Book, "sacrifice", d.Sacrifice, 5, SpellVfxKind.Sacrifice);
            AddAliases(d.Book);
        AddDefaultMixes(d.Book);
        }
        else
        {
            UpgradeBook(d.Book, d);
        }

        AssetDatabase.SaveAssets();
        return d;
    }

    private static SpellVfxKind DefaultVfx(string word)
    {
        switch (word)
        {
            case "fire": return SpellVfxKind.Fireball;
            case "bolt": return SpellVfxKind.Thunder;
            case "heal": return SpellVfxKind.Heal;
            case "spark": return SpellVfxKind.Spark;
            case "fear": return SpellVfxKind.DarkAura;
            case "water": return SpellVfxKind.Splash;
            case "ice": return SpellVfxKind.Frost;
            case "isis": return SpellVfxKind.Hearts;
            case "meteor": return SpellVfxKind.Meteor;
            case "cleanse": return SpellVfxKind.Heal;
            case "explosion": return SpellVfxKind.Explosion;
            case "mirror": return SpellVfxKind.Mirror;
            case "gamble": return SpellVfxKind.Slots;
            case "cut": return SpellVfxKind.Cut;
            case "salt": return SpellVfxKind.Salt;
            case "shatter": return SpellVfxKind.Shatter;
            case "git": return SpellVfxKind.Git;
            case "rainbow":
            case "gay": return SpellVfxKind.Rainbow;
            case "poison": return SpellVfxKind.Poison;
            case "poop":
            case "shit": return SpellVfxKind.Poop;
            case "familiar":
            case "pet": return SpellVfxKind.Familiar;
            case "sacrifice": return SpellVfxKind.Sacrifice;
            default: return SpellVfxKind.None;
        }
    }

    // Brings an older spell book up to date without touching anything the user already set:
    // entries whose animation is still None get the default one for their word, "fear" switches
    // from the default Damage asset to the Fear effect, "bolt" and "spark" switch to the Shock damage
    // (bonus on wet targets), and the "water", "ice", "isis" and "meteor" spells are added if the book has no entry for them.
    internal static void UpgradeBook(SpellBook book, Defaults d)
    {
        if (book == null) return;

        var so = new SerializedObject(book);
        SerializedProperty entries = so.FindProperty("_entries");
        bool changed = false;
        var words = new HashSet<string>();

        for (int i = 0; i < entries.arraySize; i++)
        {
            SerializedProperty e = entries.GetArrayElementAtIndex(i);
            string word = CastResolver.Normalize(e.FindPropertyRelative("_word").stringValue);
            SerializedProperty effect = e.FindPropertyRelative("_effect");
            SerializedProperty vfx = e.FindPropertyRelative("_vfx");
            words.Add(word);

            // The explosion can only be cast twice per run. (Set it back to 0 in the entry for unlimited uses,
            // but note that running this upgrade again limits it again.)
            SerializedProperty maxUses = e.FindPropertyRelative("_maxUses");
            if (word == "explosion" && maxUses != null && maxUses.intValue == 0)
            {
                maxUses.intValue = 2;
                changed = true;
            }

            if (word == "fear" && effect.objectReferenceValue == d.Damage)
            {
                effect.objectReferenceValue = d.Fear;
                changed = true;
            }

            if ((word == "bolt" || word == "spark") && effect.objectReferenceValue == d.Damage)
            {
                effect.objectReferenceValue = d.Shock;
                changed = true;
            }

            SpellVfxKind wanted = DefaultVfx(word);
            if (vfx != null && vfx.enumValueIndex == (int)SpellVfxKind.None && wanted != SpellVfxKind.None)
            {
                vfx.enumValueIndex = (int)wanted;
                changed = true;
            }
        }

        if (changed)
        {
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(book);
        }

        // AddSpell uses its own SerializedObject, so it runs after the changes above are applied.
        if (!words.Contains("water")) AddSpell(book, "water", d.Wet, 1, SpellVfxKind.Splash);
        if (!words.Contains("ice")) AddSpell(book, "ice", d.Freeze, 1, SpellVfxKind.Frost);
        if (!words.Contains("isis")) AddSpell(book, "isis", d.InstantWin, 1, SpellVfxKind.Hearts);
        if (!words.Contains("meteor")) AddSpell(book, "meteor", d.Meteor, 4, SpellVfxKind.Meteor);
        if (!words.Contains("cleanse")) AddSpell(book, "cleanse", d.Cleanse, 1, SpellVfxKind.Heal);
        if (!words.Contains("explosion")) AddSpell(book, "explosion", d.Explosion, 1, SpellVfxKind.Explosion, 2);
        if (!words.Contains("mirror")) AddSpell(book, "mirror", d.Mirror, 1, SpellVfxKind.Mirror);
        if (!words.Contains("gamble")) AddSpell(book, "gamble", d.Gamble, 25, SpellVfxKind.Slots, soundFit: SpellSoundFit.FitAndLoop);
        if (!words.Contains("cut")) AddSpell(book, "cut", d.Cut, 1, SpellVfxKind.Cut);
        if (!words.Contains("salt")) AddSpell(book, "salt", d.Salt, 2, SpellVfxKind.Salt);
        if (!words.Contains("shatter")) AddSpell(book, "shatter", d.Shatter, 3, SpellVfxKind.Shatter);
        if (!words.Contains("git")) AddSpell(book, "git", d.Git, 1, SpellVfxKind.Git);
        if (!words.Contains("rainbow")) AddSpell(book, "rainbow", d.Rainbow, 1, SpellVfxKind.Rainbow);
        if (!words.Contains("gay")) AddSpell(book, "gay", d.Rainbow, 1, SpellVfxKind.Rainbow);
        if (!words.Contains("poison")) AddSpell(book, "poison", d.Poison, 3, SpellVfxKind.Poison);
        if (!words.Contains("poop")) AddSpell(book, "poop", d.Poop, 1, SpellVfxKind.Poop);
        if (!words.Contains("familiar")) AddSpell(book, "familiar", d.Familiar, 4, SpellVfxKind.Familiar);
        if (!words.Contains("sacrifice")) AddSpell(book, "sacrifice", d.Sacrifice, 5, SpellVfxKind.Sacrifice);
        AddAliases(book);
        AddDefaultMixes(book);
    }

    // Alternative words for spells that already exist (alias, original). An alias is one more spell book entry
    // that copies the original's effect, power, animation and uses when it is created, so a spell can be
    // cast with any of its words. Add your own pairs here, or just add an entry by hand in the spell book.
    private static readonly (string Alias, string Original)[] Aliases =
    {
        ("fireball", "fire"), ("fire ball", "fire"),
        ("lightning", "bolt"),
        ("slice", "cut"), ("slash", "cut"),
        ("zap", "spark"),
        ("frost", "ice"),
        ("splash", "water"),
        ("scare", "fear"),
        ("shit", "poop"),
        ("pet", "familiar")
    };

    // Adds the aliases the book does not have yet (and whose original spell is in the book).
    // Existing entries are never changed. Returns how many aliases were added.
    internal static int AddAliases(SpellBook book)
    {
        if (book == null) return 0;
        int added = 0;

        foreach ((string alias, string original) in Aliases)
        {
            var so = new SerializedObject(book);
            SerializedProperty entries = so.FindProperty("_entries");
            SerializedProperty source = null;
            bool exists = false;

            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty e = entries.GetArrayElementAtIndex(i);
                string word = CastResolver.Normalize(e.FindPropertyRelative("_word").stringValue);
                if (word == alias) exists = true;
                else if (word == original && source == null) source = e;
            }
            if (exists || source == null) continue;

            // The arguments are read before AddSpell opens its own SerializedObject.
            AddSpell(book, alias,
                (SpellEffect)source.FindPropertyRelative("_effect").objectReferenceValue,
                source.FindPropertyRelative("_power").intValue,
                (SpellVfxKind)source.FindPropertyRelative("_vfx").enumValueIndex,
                source.FindPropertyRelative("_maxUses").intValue,
                (AudioClip)source.FindPropertyRelative("_sound").objectReferenceValue,
                source.FindPropertyRelative("_soundVolume").floatValue,
                source.FindPropertyRelative("_soundPerHit").boolValue,
                (SpellSoundFit)source.FindPropertyRelative("_soundFit").enumValueIndex,
                source.FindPropertyRelative("_soundImpactTime").floatValue,
                (AudioClip)source.FindPropertyRelative("_reelStopSound").objectReferenceValue,
                source.FindPropertyRelative("_reelStopVolume").floatValue,
                (AudioClip)source.FindPropertyRelative("_jackpotSound").objectReferenceValue,
                source.FindPropertyRelative("_jackpotVolume").floatValue);
            added++;
        }
        return added;
    }

    // Adds the mixes the book does not have yet (the full table lives in SpellTyposMixTable) and gives every spell
    // its signature colour. Existing mixes are never changed. Returns how many were added.
    internal static int AddDefaultMixes(SpellBook book)
    {
        return SpellTyposMixTable.Apply(book, false).Added;
    }

    // Same, but also rewrites the rules of the mixes the book already has. Returns how many were added and updated.
    internal static (int Added, int Updated) UpdateMixRules(SpellBook book)
    {
        return SpellTyposMixTable.Apply(book, true);
    }

    internal static SpellBook CreateEmptyBook()
    {
        EnsureFolders();
        string path = AssetDatabase.GenerateUniqueAssetPath(BookPath);
        var book = ScriptableObject.CreateInstance<SpellBook>();
        AssetDatabase.CreateAsset(book, path);
        AssetDatabase.SaveAssets();
        return book;
    }

    // InsertArrayElementAtIndex duplicates the last element, so every field is set explicitly here
    // (otherwise a new spell would inherit the previous one's sound).
    internal static void AddSpell(SpellBook book, string word, SpellEffect effect, int power,
        SpellVfxKind vfx = SpellVfxKind.None, int maxUses = 0,
        AudioClip sound = null, float soundVolume = 1f, bool soundPerHit = false,
        SpellSoundFit soundFit = SpellSoundFit.FitToAnimation, float soundImpactTime = 0f,
        AudioClip reelStopSound = null, float reelStopVolume = 1f, AudioClip jackpotSound = null, float jackpotVolume = 1f)
    {
        var so = new SerializedObject(book);
        SerializedProperty entries = so.FindProperty("_entries");
        int i = entries.arraySize;
        entries.InsertArrayElementAtIndex(i);
        SerializedProperty e = entries.GetArrayElementAtIndex(i);
        e.FindPropertyRelative("_word").stringValue = word;
        e.FindPropertyRelative("_effect").objectReferenceValue = effect;
        e.FindPropertyRelative("_power").intValue = power;
        e.FindPropertyRelative("_vfx").enumValueIndex = (int)vfx;
        e.FindPropertyRelative("_maxUses").intValue = maxUses;
        e.FindPropertyRelative("_sound").objectReferenceValue = sound;
        e.FindPropertyRelative("_soundVolume").floatValue = soundVolume;
        e.FindPropertyRelative("_soundPerHit").boolValue = soundPerHit;
        e.FindPropertyRelative("_soundFit").enumValueIndex = (int)soundFit;
        e.FindPropertyRelative("_soundImpactTime").floatValue = soundImpactTime;
        e.FindPropertyRelative("_reelStopSound").objectReferenceValue = reelStopSound;
        e.FindPropertyRelative("_reelStopVolume").floatValue = reelStopVolume;
        e.FindPropertyRelative("_jackpotSound").objectReferenceValue = jackpotSound;
        e.FindPropertyRelative("_jackpotVolume").floatValue = jackpotVolume;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(book);
    }

    // Gives a sound to every spell that has none by copying it from another entry with the same effect, power and
    // animation (so "fireball" gets the sound of "fire", "gay" the one of "rainbow"). Entries that already have a
    // sound are never touched. Returns how many entries were filled.
    internal static int FillMissingSounds(SpellBook book)
    {
        if (book == null) return 0;

        var so = new SerializedObject(book);
        SerializedProperty entries = so.FindProperty("_entries");
        int filled = 0;

        for (int i = 0; i < entries.arraySize; i++)
        {
            SerializedProperty e = entries.GetArrayElementAtIndex(i);
            SerializedProperty sound = e.FindPropertyRelative("_sound");
            Object effect = e.FindPropertyRelative("_effect").objectReferenceValue;
            if (sound.objectReferenceValue != null || effect == null) continue;

            int power = e.FindPropertyRelative("_power").intValue;
            int vfx = e.FindPropertyRelative("_vfx").enumValueIndex;

            for (int j = 0; j < entries.arraySize; j++)
            {
                if (j == i) continue;
                SerializedProperty o = entries.GetArrayElementAtIndex(j);
                Object otherSound = o.FindPropertyRelative("_sound").objectReferenceValue;
                if (otherSound == null) continue;
                if (o.FindPropertyRelative("_effect").objectReferenceValue != effect) continue;
                if (o.FindPropertyRelative("_power").intValue != power) continue;
                if (o.FindPropertyRelative("_vfx").enumValueIndex != vfx) continue;

                sound.objectReferenceValue = otherSound;
                e.FindPropertyRelative("_soundVolume").floatValue = o.FindPropertyRelative("_soundVolume").floatValue;
                e.FindPropertyRelative("_soundPerHit").boolValue = o.FindPropertyRelative("_soundPerHit").boolValue;
                e.FindPropertyRelative("_soundFit").enumValueIndex = o.FindPropertyRelative("_soundFit").enumValueIndex;
                e.FindPropertyRelative("_soundImpactTime").floatValue = o.FindPropertyRelative("_soundImpactTime").floatValue;
                filled++;
                break;
            }
        }

        if (filled > 0)
        {
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(book);
        }
        return filled;
    }

    internal static void FillDefaultRooms(RunController run, Defaults d)
    {
        var so = new SerializedObject(run);
        SerializedProperty rooms = so.FindProperty("_rooms");
        rooms.arraySize = 0;

        // Every room gets the filters that belong to its name (see DefaultFiltersFor).
        AddRoom(rooms, "Goblin Cellar", "Goblin", 18, new[] { "spark" });
        AddRoom(rooms, "Echo Cave", "Bat", 20, new[] { "spark", "fear" });
        AddRoom(rooms, "Hall of Ire", "Imp", 22, new[] { "fire", "spark" });
        AddRoom(rooms, "Crooked Library", "Gremlin", 26, new[] { "bolt", "shield" });
        AddRoom(rooms, "Mirror Maze", "Mimic", 28, new[] { "bolt", "shield" });
        AddRoom(rooms, "Throne of Tongues", "Lich", 34, new[] { "bolt", "heal", "fury" });

        so.ApplyModifiedProperties();
    }

    private static void AddRoom(SerializedProperty rooms, string room, string enemy, int hp, string[] words)
    {
        LanguageFilter[] filters = DefaultFiltersFor(room) ?? new LanguageFilter[0];
        int i = rooms.arraySize;
        rooms.arraySize = i + 1;
        SerializedProperty r = rooms.GetArrayElementAtIndex(i);
        r.FindPropertyRelative("_roomName").stringValue = room;
        r.FindPropertyRelative("_enemyName").stringValue = enemy;
        r.FindPropertyRelative("_enemyHp").intValue = hp;

        SerializedProperty f = r.FindPropertyRelative("_filters");
        f.arraySize = filters.Length;
        for (int j = 0; j < filters.Length; j++)
            f.GetArrayElementAtIndex(j).objectReferenceValue = filters[j];

        SerializedProperty w = r.FindPropertyRelative("_enemyWords");
        w.arraySize = words.Length;
        for (int j = 0; j < words.Length; j++)
            w.GetArrayElementAtIndex(j).stringValue = words[j];
    }

    // ------------------------------------------------------- room filters

    // Loads a default filter straight from disk. References kept from EnsureDefaults() can go stale after
    // NewScene(), which used to leave the filter slots of new rooms empty.
    private static LanguageFilter LoadFilter(string fileName)
    {
        return AssetDatabase.LoadAssetAtPath<LanguageFilter>($"{FiltersFolder}/{fileName}.asset");
    }

    // The filters each default room should have, in order. Returns null for rooms without defaults
    // (the Goblin Cellar speaks plain words on purpose, and rooms with a custom name are your own).
    private static LanguageFilter[] DefaultFiltersFor(string roomName)
    {
        switch (roomName == null ? "" : roomName.Trim())
        {
            case "Echo Cave": return new[] { LoadFilter("EchoingTongue") };
            case "Hall of Ire": return new[] { LoadFilter("TongueOfIre") };
            case "Crooked Library": return new[] { LoadFilter("CrookedTongue") };
            case "Mirror Maze": return new[] { LoadFilter("MirrorTongue") };
            case "Throne of Tongues": return new[] { LoadFilter("ClippedTongue"), LoadFilter("TongueOfIre") };
            default: return null;
        }
    }

    // Gives the default filters to every room of the run that has none, matching rooms by name
    // (Echo Cave, Hall of Ire, Crooked Library, Mirror Maze, Throne of Tongues). Rooms that already have
    // filters, and rooms with a custom name, are left alone. Returns how many rooms were changed.
    internal static int AssignMissingFilters(RunController run)
    {
        if (run == null) return 0;

        // Make sure the filter assets exist before looking for them.
        if (LoadFilter("EchoingTongue") == null) EnsureDefaults();

        var so = new SerializedObject(run);
        SerializedProperty rooms = so.FindProperty("_rooms");
        int changed = 0;

        for (int i = 0; i < rooms.arraySize; i++)
        {
            SerializedProperty room = rooms.GetArrayElementAtIndex(i);
            string roomName = room.FindPropertyRelative("_roomName").stringValue;
            SerializedProperty filters = room.FindPropertyRelative("_filters");
            if (HasAnyFilter(filters)) continue;

            LanguageFilter[] wanted = DefaultFiltersFor(roomName);
            if (wanted == null || wanted.Length == 0) continue;

            filters.arraySize = wanted.Length;
            for (int j = 0; j < wanted.Length; j++)
                filters.GetArrayElementAtIndex(j).objectReferenceValue = wanted[j];

            changed++;
            Debug.Log($"[SpellTypos] Room '{roomName}': assigned its default filters.");
        }

        if (changed > 0)
        {
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(run);
            EditorSceneManager.MarkSceneDirty(run.gameObject.scene);
        }
        return changed;
    }

    private static bool HasAnyFilter(SerializedProperty filters)
    {
        for (int i = 0; i < filters.arraySize; i++)
            if (filters.GetArrayElementAtIndex(i).objectReferenceValue != null) return true;
        return false;
    }

    // ------------------------------------------------------------- UI prefabs

    private static TMP_Text EnsureTextPrefab(string path, float size, TextAlignmentOptions align)
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing.GetComponent<TMP_Text>();

        TextMeshProUGUI t = SpellTyposUi.NewText(System.IO.Path.GetFileNameWithoutExtension(path), null,
            "Text", size, align);
        GameObject saved = PrefabUtility.SaveAsPrefabAsset(t.gameObject, path);
        Object.DestroyImmediate(t.gameObject);
        return saved.GetComponent<TMP_Text>();
    }

    private static void EnsureUiPrefabs(out TMP_Text word, out TMP_Text row)
    {
        EnsureFolder(UiFolder);
        word = EnsureTextPrefab(WordPrefabPath, 46f, TextAlignmentOptions.Center);
        row = EnsureTextPrefab(RowPrefabPath, 28f, TextAlignmentOptions.MidlineLeft);
    }

    // Loads a text prefab fresh from disk (references kept across NewScene() can go stale).
    private static TMP_Text LoadTextPrefab(string path)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        return go != null ? go.GetComponent<TMP_Text>() : null;
    }

    // ------------------------------------------------------ combatant prefabs

    internal static string CombatantPath(CombatantSpec s)
    {
        string name = string.IsNullOrWhiteSpace(s.AssetName) ? "Combatant" : s.AssetName.Trim();
        foreach (char c in System.IO.Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        return $"{CombatantsFolder}/{name}.prefab";
    }

    internal static GameObject EnsureCombatantPrefab(CombatantSpec s)
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(CombatantPath(s));
        return existing != null ? existing : CreateCombatantPrefab(s);
    }

    // Prefab layout (feet at the root origin):
    //   Root (Combatant, CombatantView, [EnemyView])
    //     Visual (SpriteRenderer)         <- sprite, bobs/shakes for enemies
    //       BurnVfx / FurBallVfx          <- optional sprite overlays, toggled by Combatant
    //     Hud (world-space Canvas, 0.01 scale) <- name, HP bar, shield and status text below the feet
    internal static GameObject CreateCombatantPrefab(CombatantSpec s)
    {
        if (!EnsureTmp()) return null;
        EnsureFolders();

        bool isEnemy = s.Kind == CombatantKind.Enemy;
        string path = CombatantPath(s);
        float height = Mathf.Max(0.5f, s.SpriteHeight);

        var root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
        var combatant = root.AddComponent<Combatant>();
        var view = root.AddComponent<CombatantView>();

        // Visual
        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        var sprite = visual.AddComponent<SpriteRenderer>();
        sprite.color = s.Tint;
        sprite.sortingOrder = 0;

        Vector2 localSize;
        Vector2 localCenter;
        float scale = 1f;
        if (s.Portrait != null)
        {
            sprite.sprite = s.Portrait;
            Bounds b = s.Portrait.bounds;
            scale = height / Mathf.Max(0.01f, b.size.y);
            localSize = b.size;
            localCenter = b.center;
        }
        else
        {
            // Placeholder: tinted rounded box until a real sprite is assigned.
            sprite.sprite = SpellTyposUi.PanelSprite;
            sprite.drawMode = SpriteDrawMode.Sliced;
            localSize = new Vector2(height * 0.8f, height);
            sprite.size = localSize;
            localCenter = Vector2.zero;
        }
        visual.transform.localScale = new Vector3(scale, scale, 1f);
        visual.transform.localPosition = new Vector3(-localCenter.x * scale, height * 0.5f - localCenter.y * scale, 0f);

        GameObject burnVfx = null;
        if (s.BurnVfx)
            burnVfx = NewOverlay("BurnVfx", visual.transform, localSize, localCenter,
                new Color(1f, 0.45f, 0.1f, 0.4f), 1);

        GameObject furVfx = null;
        if (s.FurBallVfx)
            furVfx = NewOverlay("FurBallVfx", visual.transform, localSize + Vector2.one * (0.4f / scale), localCenter,
                new Color(0.8f, 0.65f, 0.4f, 0.5f), 2);

        // World-space HUD below the feet
        float hudWidth = 360f;
        float barWidth = 320f;

        var hud = new GameObject("Hud", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
        hud.transform.SetParent(root.transform, false);
        var canvas = hud.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;
        var hudRect = (RectTransform)hud.transform;
        hudRect.pivot = new Vector2(0.5f, 1f);
        hudRect.localScale = Vector3.one * 0.01f;
        hudRect.localPosition = new Vector3(0f, -0.1f, 0f);

        float y = 0f;

        TextMeshProUGUI nameText = SpellTyposUi.NewText("Name", hudRect, s.DisplayName, 36f, TextAlignmentOptions.Center);
        nameText.fontStyle = FontStyles.Bold;
        SpellTyposUi.PlaceTop(nameText.rectTransform, y, hudWidth, 42f);
        y += 46f;

        Color fill = isEnemy ? new Color(0.85f, 0.2f, 0.25f) : new Color(0.3f, 0.8f, 0.4f);
        Slider bar = SpellTyposUi.NewBar("HpBar", hudRect, new Color(0.12f, 0.06f, 0.06f), fill);
        SpellTyposUi.PlaceTop((RectTransform)bar.transform, y, barWidth, 28f);

        TextMeshProUGUI hpText = SpellTyposUi.NewText("HpText", hudRect, $"{s.MaxHp} / {s.MaxHp}", 22f,
            TextAlignmentOptions.Center);
        SpellTyposUi.PlaceTop(hpText.rectTransform, y, barWidth, 28f);
        y += 32f;

        TextMeshProUGUI shieldText = SpellTyposUi.NewText("ShieldText", hudRect, "", 22f,
            TextAlignmentOptions.Center, new Color(0.55f, 0.85f, 1f));
        SpellTyposUi.PlaceTop(shieldText.rectTransform, y, barWidth, 28f);
        y += 28f;

        TextMeshProUGUI statusText = SpellTyposUi.NewText("StatusText", hudRect, "", 22f, TextAlignmentOptions.Center);
        SpellTyposUi.PlaceTop(statusText.rectTransform, y, barWidth, 28f);
        y += 28f;

        hudRect.sizeDelta = new Vector2(hudWidth, y + 6f);

        Set(combatant, "_displayName", s.DisplayName);
        Set(combatant, "_maxHp", s.MaxHp);
        Set(combatant, "_burnVfx", burnVfx);
        Set(combatant, "_furBallVfx", furVfx);

        // Spell animations start/land at the middle of the body (stays still when the enemy shakes).
        var anchor = new GameObject("VfxAnchor");
        anchor.transform.SetParent(root.transform, false);
        anchor.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
        Set(combatant, "_vfxAnchor", anchor.transform);

        Set(view, "_combatant", combatant);
        Set(view, "_nameText", nameText);
        Set(view, "_hpText", hpText);
        Set(view, "_shieldText", shieldText);
        Set(view, "_statusText", statusText);
        Set(view, "_hpBar", bar);

        if (isEnemy)
        {
            var enemyView = root.AddComponent<EnemyView>();
            Set(enemyView, "_combatant", combatant);
            Set(enemyView, "_sprite", sprite);
            Set(enemyView, "_hudGroup", hud.GetComponent<CanvasGroup>());
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    private static GameObject NewOverlay(string name, Transform parent, Vector2 size, Vector2 center, Color color,
        int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(center.x, center.y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpellTyposUi.PanelSprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.color = color;
        sr.sortingOrder = sortingOrder;

        go.SetActive(false);
        return go;
    }

    // Returns null when the prefab is usable, otherwise a human-readable reason.
    private static string CheckCombatantPrefab(GameObject prefab, string role)
    {
        if (prefab == null) return $"The {role} prefab is missing.";
        if (prefab.transform is RectTransform)
            return $"{prefab.name} is a UI prefab from the old version. Rebuild it in the Combatants tab.";
        if (prefab.GetComponent<Combatant>() == null || prefab.GetComponent<CombatantView>() == null)
            return $"{prefab.name} needs Combatant and CombatantView on its root.";
        if (prefab.GetComponentInChildren<SpriteRenderer>() == null)
            return $"{prefab.name} has no SpriteRenderer. Rebuild it in the Combatants tab.";
        return null;
    }

    // ------------------------------------------------------------------ scene

    internal static string BuildBattleScene(GameObject playerPrefab, GameObject enemyPrefab, SpellBook book,
        string sceneName)
    {
        if (!EnsureTmp()) return null;

        string problem = CheckCombatantPrefab(playerPrefab, "player") ?? CheckCombatantPrefab(enemyPrefab, "enemy");
        if (problem != null)
        {
            EditorUtility.DisplayDialog("SpellTypos", problem, "OK");
            return null;
        }

        Defaults d = EnsureDefaults();
        if (book == null) book = d.Book;
        UpgradeBook(book, d);
        EnsureUiPrefabs(out TMP_Text wordPrefab, out TMP_Text rowPrefab);
        AssetDatabase.SaveAssets();

        // Fail early (before the open scene is replaced) if the UI prefabs are not on disk.
        if (LoadTextPrefab(WordPrefabPath) == null || LoadTextPrefab(RowPrefabPath) == null)
        {
            EditorUtility.DisplayDialog("SpellTypos",
                $"The UI prefabs could not be created or found in {UiFolder}. The scene was not built.", "OK");
            return null;
        }

        string safeName = string.IsNullOrWhiteSpace(sceneName) ? "Battle" : sceneName.Trim();
        string path = $"{ScenesFolder}/{safeName}.unity";

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Reload from disk: references held across NewScene() may be stale, which left the fields empty.
        wordPrefab = LoadTextPrefab(WordPrefabPath);
        rowPrefab = LoadTextPrefab(RowPrefabPath);

        // 2D camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = CameraSize;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.06f, 0.12f);
        }

        // Combatants live in the world, not in the Canvas
        GameObject enemy = PlaceCombatant(enemyPrefab, "Enemy", EnemyPosition);
        GameObject player = PlaceCombatant(playerPrefab, "Player", PlayerPosition);

        // Canvas + event system (HUD only)
        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        Transform root = canvasGo.transform;

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
        AddInputModule(eventSystem);

        // Room title + filter list (top-left)
        TextMeshProUGUI roomTitle = SpellTyposUi.NewText("RoomTitle", root, "Room", 52f,
            TextAlignmentOptions.TopLeft, Gold);
        roomTitle.fontStyle = FontStyles.Bold;
        SpellTyposUi.Place(roomTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(40f, -30f), new Vector2(800f, 70f));

        TextMeshProUGUI filterText = SpellTyposUi.NewText("FilterText", root, "", 30f, TextAlignmentOptions.TopLeft);
        SpellTyposUi.Place(filterText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(40f, -110f), new Vector2(700f, 380f));

        // Word trail (middle)
        RectTransform trailRect = SpellTyposUi.NewRect("WordTrail", root);
        SpellTyposUi.PlaceCenter(trailRect, new Vector2(0f, -110f), new Vector2(760f, 130f));
        var trail = trailRect.gameObject.AddComponent<WordTrailView>();

        RectTransform words = SpellTyposUi.NewRect("Words", trailRect);
        SpellTyposUi.Stretch(words);
        var wordsLayout = words.gameObject.AddComponent<HorizontalLayoutGroup>();
        wordsLayout.childAlignment = TextAnchor.MiddleCenter;
        wordsLayout.spacing = 28f;
        wordsLayout.childControlWidth = true;
        wordsLayout.childControlHeight = true;
        wordsLayout.childForceExpandWidth = false;
        wordsLayout.childForceExpandHeight = false;

        TextMeshProUGUI resultLabel = SpellTyposUi.NewText("ResultLabel", root, "", 34f,
            TextAlignmentOptions.Center, Gold);
        SpellTyposUi.PlaceCenter(resultLabel.rectTransform, new Vector2(0f, -200f), new Vector2(600f, 50f));

        Set(trail, "_wordPrefab", wordPrefab);
        Set(trail, "_container", words);
        Set(trail, "_resultLabel", resultLabel);

        // Log (bottom-right)
        Image logPanel = SpellTyposUi.NewImage("LogPanel", root, new Color(0f, 0f, 0f, 0.45f));
        logPanel.raycastTarget = false;
        SpellTyposUi.PlaceCenter(logPanel.rectTransform, new Vector2(610f, -250f), new Vector2(560f, 200f));
        TextMeshProUGUI logText = SpellTyposUi.NewText("LogText", logPanel.transform, "", 26f,
            TextAlignmentOptions.BottomLeft);
        SpellTyposUi.Stretch(logText.rectTransform, 14f, 10f, 14f, 10f);

        // Spell input (bottom-center)
        RectTransform inputRect = SpellTyposUi.NewRect("SpellInput", root);
        SpellTyposUi.PlaceCenter(inputRect, new Vector2(0f, -470f), new Vector2(940f, 70f));
        var spellInput = inputRect.gameObject.AddComponent<SpellInput>();

        TMP_InputField field = SpellTyposUi.NewInputField("InputField", inputRect, "type a spell...", 34f);
        SpellTyposUi.Place((RectTransform)field.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            Vector2.zero, new Vector2(770f, 70f));
        Button castButton = SpellTyposUi.NewButton("CastButton", inputRect, "Cast", 34f, new Color(0.3f, 0.45f, 0.85f));
        SpellTyposUi.Place((RectTransform)castButton.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            Vector2.zero, new Vector2(150f, 70f));

        Set(spellInput, "_field", field);
        Set(spellInput, "_castButton", castButton);

        // Grimoire (right)
        RectTransform grimoireRect = SpellTyposUi.NewRect("Grimoire", root);
        SpellTyposUi.PlaceCenter(grimoireRect, new Vector2(720f, 160f), new Vector2(420f, 540f));
        TextMeshProUGUI grimoireTitle = SpellTyposUi.NewText("Title", grimoireRect, "Grimoire", 38f,
            TextAlignmentOptions.Center, Gold);
        grimoireTitle.fontStyle = FontStyles.Bold;
        SpellTyposUi.PlaceTop(grimoireTitle.rectTransform, 0f, 420f, 48f);

        RectTransform scroll = SpellTyposUi.NewScroll("Scroll", grimoireRect, out RectTransform content);
        SpellTyposUi.Stretch(scroll, 0f, 0f, 0f, 56f);

        var grimoire = grimoireRect.gameObject.AddComponent<GrimoireView>();
        Set(grimoire, "_book", book);
        Set(grimoire, "_rowPrefab", rowPrefab);
        Set(grimoire, "_container", content);

        // Banner (always last so it draws on top)
        TextMeshProUGUI banner = SpellTyposUi.NewText("Banner", root, "", 60f, TextAlignmentOptions.Center, Gold);
        banner.fontStyle = FontStyles.Bold;
        SpellTyposUi.PlaceCenter(banner.rectTransform, Vector2.zero, new Vector2(1400f, 260f));
        banner.gameObject.SetActive(false);

        // Game systems
        var systems = new GameObject("Systems");
        var hud = systems.AddComponent<BattleHud>();
        var battle = systems.AddComponent<TurnBattle>();
        var run = systems.AddComponent<RunController>();
        var vfx = systems.AddComponent<SpellVfxPlayer>();
        var audio = systems.AddComponent<SpellAudioPlayer>();

        var playerCombatant = player.GetComponent<Combatant>();
        var enemyCombatant = enemy.GetComponent<Combatant>();

        Set(hud, "_roomTitle", roomTitle);
        Set(hud, "_filterText", filterText);
        Set(hud, "_logText", logText);
        Set(hud, "_banner", banner);
        Set(hud, "_trail", trail);

        Set(battle, "_book", book);
        Set(battle, "_player", playerCombatant);
        Set(battle, "_enemy", enemyCombatant);
        Set(battle, "_input", spellInput);
        Set(battle, "_hud", hud);
        Set(battle, "_vfx", vfx);
        Set(battle, "_audio", audio);

        Set(grimoire, "_battle", battle);

        Set(run, "_battle", battle);
        Set(run, "_player", playerCombatant);
        Set(run, "_hud", hud);
        FillDefaultRooms(run, d);
        // Safety net: fills in any default room that still ended up without its filters.
        AssignMissingFilters(run);

        // Sanity check: warn loudly if any critical reference ended up empty.
        VerifyReference(trail, "_wordPrefab");
        VerifyReference(trail, "_container");
        VerifyReference(trail, "_resultLabel");
        VerifyReference(grimoire, "_rowPrefab");
        VerifyReference(grimoire, "_container");
        VerifyReference(grimoire, "_book");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, path);
        AddToBuildSettings(path);
        AssetDatabase.SaveAssets();
        return path;
    }

    private static void VerifyReference(Object target, string property)
    {
        SerializedProperty p = new SerializedObject(target).FindProperty(property);
        if (p == null || p.objectReferenceValue == null)
            Debug.LogError($"[SpellTypos] {target.GetType().Name}.{property} is empty after building the scene.", target);
    }

    private static GameObject PlaceCombatant(GameObject prefab, string name, Vector3 position)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = name;
        go.transform.position = position;

        // Prefabs made before the animations existed have no VfxAnchor: add one at the sprite's center.
        var combatant = go.GetComponent<Combatant>();
        if (combatant != null && new SerializedObject(combatant).FindProperty("_vfxAnchor").objectReferenceValue == null)
        {
            SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();
            var anchor = new GameObject("VfxAnchor");
            anchor.transform.SetParent(go.transform, false);
            anchor.transform.position = sr != null ? sr.bounds.center : position + Vector3.up;
            Set(combatant, "_vfxAnchor", anchor.transform);
        }
        return go;
    }

    private static void AddToBuildSettings(string path)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ------------------------------------------------------------ input module

    private static void AddInputModule(GameObject eventSystem)
    {
        System.Type uiModule =
            System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");

        if (ActiveInputHandler() != 0 && uiModule != null)
        {
            Component module = eventSystem.AddComponent(uiModule);
            var assign = module.GetType().GetMethod("AssignDefaultActions");
            if (assign != null) assign.Invoke(module, null);
        }
        else
        {
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }

    // 0 = old Input Manager, 1 = Input System package, 2 = both
    private static int ActiveInputHandler()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
        if (assets == null || assets.Length == 0) return 0;
        SerializedProperty p = new SerializedObject(assets[0]).FindProperty("activeInputHandler");
        return p != null ? p.intValue : 0;
    }
}
