using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Editor-only. Place under Assets/Editor/ together with SpellTyposBuilder.cs.
// Sets up the attack animation of a combatant prefab:
//   Player: Animator (on the SpriteRenderer object) + CombatantAnimator (on the root). If you do not
//           have an Animator Controller, one is generated from an idle sprite and a cast sprite.
//   Enemy:  normal / rare sprites and the rare chance on EnemyView.
[System.Serializable]
internal sealed class AnimationSpec
{
    [Tooltip("Optional. Use your own controller (it needs a Trigger parameter named like the attack trigger below). Empty = one is generated from the idle and cast sprites.")]
    [SerializeField] private RuntimeAnimatorController _controller;
    [SerializeField] private Sprite _idleSprite;
    [SerializeField] private Sprite _castSprite;
    [Tooltip("Name of the Trigger parameter that starts the attack.")]
    [SerializeField] private string _attackTrigger = "Attacking";
    [Tooltip("How long the cast pose is held, in seconds.")]
    [SerializeField, Min(0.05f)] private float _castTime = 0.333f;

    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _rareSprite;
    [Tooltip("Chance (0 to 1) that a new enemy is the rare variant. 0.05 = 5%.")]
    [SerializeField, Range(0f, 1f)] private float _rareChance = 0.05f;

    [Tooltip("The builder tints the sprite by default, which would colour your art. On = the sprite is reset to white.")]
    [SerializeField] private bool _resetTint = true;

    internal RuntimeAnimatorController Controller => _controller;
    internal Sprite IdleSprite => _idleSprite;
    internal Sprite CastSprite => _castSprite;
    internal string AttackTrigger => string.IsNullOrWhiteSpace(_attackTrigger) ? "Attacking" : _attackTrigger.Trim();
    internal float CastTime => Mathf.Max(0.05f, _castTime);
    internal Sprite NormalSprite => _normalSprite;
    internal Sprite RareSprite => _rareSprite;
    internal float RareChance => _rareChance;
    internal bool ResetTint => _resetTint;
}

internal static class SpellTyposAnimationSetup
{
    private const string AnimationsFolder = SpellTyposBuilder.Root + "/Animations";

    // ------------------------------------------------------------------- GUI

    internal static void Draw(SerializedProperty spec, CombatantKind kind)
    {
        if (kind == CombatantKind.Player)
        {
            EditorGUILayout.HelpBox(
                "Adds an Animator and CombatantAnimator so the wizard plays its cast pose whenever a spell is cast. " +
                "Give it an idle and a cast sprite and a controller is generated, or use your own controller.",
                MessageType.None);
            Field(spec, "_controller", "Animator controller");
            Field(spec, "_idleSprite", "Idle sprite");
            Field(spec, "_castSprite", "Cast sprite");
            Field(spec, "_attackTrigger", "Attack trigger");
            Field(spec, "_castTime", "Cast pose time (s)");
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Sets the enemy sprites. The rare sprite appears with the chance below; the attack is a lunge towards the target.",
                MessageType.None);
            Field(spec, "_normalSprite", "Normal sprite");
            Field(spec, "_rareSprite", "Rare sprite");
            Field(spec, "_rareChance", "Rare chance");
        }

        Field(spec, "_resetTint", "Reset sprite tint to white");

        if (GUILayout.Button("Auto-find art in the project"))
            AutoFill(spec);
    }

    private static void Field(SerializedProperty spec, string name, string label)
    {
        EditorGUILayout.PropertyField(spec.FindPropertyRelative(name), new GUIContent(label));
    }

    // Looks for WizardIdle, WizardCast, ElfEnemy1, ElfEnemy2 and an Animator Controller named Player.
    internal static void AutoFill(SerializedProperty spec)
    {
        var missing = new System.Collections.Generic.List<string>();
        Assign(spec, "_idleSprite", FindSprite("WizardIdle"), "WizardIdle", missing);
        Assign(spec, "_castSprite", FindSprite("WizardCast"), "WizardCast", missing);
        Assign(spec, "_normalSprite", FindSprite("ElfEnemy1"), "ElfEnemy1", missing);
        Assign(spec, "_rareSprite", FindSprite("ElfEnemy2"), "ElfEnemy2", missing);

        RuntimeAnimatorController controller = FindController("Player");
        if (controller != null) spec.FindPropertyRelative("_controller").objectReferenceValue = controller;

        if (missing.Count > 0)
            Debug.LogWarning($"[SpellTypos] Auto-find did not locate: {string.Join(", ", missing)}.");
    }

    private static void Assign(SerializedProperty spec, string name, Sprite sprite, string label,
        System.Collections.Generic.List<string> missing)
    {
        if (sprite == null) { missing.Add(label); return; }
        spec.FindPropertyRelative(name).objectReferenceValue = sprite;
    }

    private static Sprite FindSprite(string name)
    {
        foreach (string guid in AssetDatabase.FindAssets(name + " t:Sprite"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) != name) continue;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
        }
        return null;
    }

    private static RuntimeAnimatorController FindController(string name)
    {
        foreach (string guid in AssetDatabase.FindAssets(name + " t:AnimatorController"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) != name) continue;
            return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
        }
        return null;
    }

    // Used as the prefab's sprite (and size reference) when the designer has no portrait.
    internal static Sprite DefaultPortrait(CombatantKind kind, AnimationSpec a)
    {
        return kind == CombatantKind.Player ? a.IdleSprite : a.NormalSprite;
    }

    // ----------------------------------------------------------------- apply

    internal static bool Apply(GameObject prefabAsset, CombatantKind kind, AnimationSpec a)
    {
        if (prefabAsset == null) return false;
        string path = AssetDatabase.GetAssetPath(prefabAsset);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("[SpellTypos] Animation setup needs a prefab asset, not a scene object.");
            return false;
        }

        RuntimeAnimatorController controller = null;
        if (kind == CombatantKind.Player)
        {
            controller = a.Controller != null ? a.Controller : BuildController(prefabAsset.name, a);
            if (controller == null)
            {
                Debug.LogWarning("[SpellTypos] Give the player an Animator controller, or both an idle and a cast sprite.");
                return false;
            }
        }

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var combatant = root.GetComponent<Combatant>();
            SpriteRenderer sr = root.GetComponentInChildren<SpriteRenderer>();
            if (combatant == null || sr == null)
            {
                Debug.LogWarning($"[SpellTypos] {prefabAsset.name} needs a Combatant on the root and a SpriteRenderer.");
                return false;
            }

            if (kind == CombatantKind.Player)
            {
                Animator animator = sr.GetComponent<Animator>();
                if (animator == null) animator = sr.gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;

                var attack = root.GetComponent<CombatantAnimator>();
                if (attack == null) attack = root.AddComponent<CombatantAnimator>();
                SpellTyposBuilder.Set(attack, "_combatant", combatant);
                SpellTyposBuilder.Set(attack, "_animator", animator);
                SpellTyposBuilder.Set(attack, "_attackTrigger", a.AttackTrigger);

                if (a.IdleSprite != null) sr.sprite = a.IdleSprite;
            }
            else
            {
                var view = root.GetComponent<EnemyView>();
                if (view == null)
                {
                    Debug.LogWarning($"[SpellTypos] {prefabAsset.name} has no EnemyView, so it is not an enemy prefab.");
                    return false;
                }

                Sprite normal = a.NormalSprite != null ? a.NormalSprite : sr.sprite;
                SpellTyposBuilder.Set(view, "_normalSprite", normal);
                SpellTyposBuilder.Set(view, "_rareSprite", a.RareSprite);
                SpellTyposBuilder.Set(view, "_rareChance", a.RareChance);
                if (normal != null) sr.sprite = normal;
            }

            if (a.ResetTint) sr.color = Color.white;

            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[SpellTypos] Animation set up on {prefabAsset.name}.", prefabAsset);
        return true;
    }

    // ------------------------------------------------------------ controller

    // Idle (one sprite) -> Attack (cast sprite, held for CastTime) -> Idle. The Trigger starts the attack.
    private static RuntimeAnimatorController BuildController(string name, AnimationSpec a)
    {
        if (a.IdleSprite == null || a.CastSprite == null) return null;

        SpellTyposBuilder.EnsureFolder(AnimationsFolder);
        string trigger = a.AttackTrigger;

        AnimationClip idle = SpriteClip($"{AnimationsFolder}/{name}Idle.anim", $"{name}Idle", a.IdleSprite, 1f / 60f);
        AnimationClip cast = SpriteClip($"{AnimationsFolder}/{name}Attack.anim", $"{name}Attack", a.CastSprite, a.CastTime);

        string path = $"{AnimationsFolder}/{name}.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;

            AnimatorState idleState = machine.AddState($"{name}Idle");
            idleState.motion = idle;
            AnimatorState attackState = machine.AddState($"{name}Attack");
            attackState.motion = cast;
            machine.defaultState = idleState;

            AnimatorStateTransition toAttack = idleState.AddTransition(attackState);
            toAttack.hasExitTime = false;
            toAttack.duration = 0f;
            toAttack.AddCondition(AnimatorConditionMode.If, 0f, trigger);

            AnimatorStateTransition back = attackState.AddTransition(idleState);
            back.hasExitTime = true;
            back.exitTime = 1f;
            back.duration = 0f;
        }

        bool hasTrigger = false;
        foreach (AnimatorControllerParameter p in controller.parameters)
            if (p.name == trigger) hasTrigger = true;
        if (!hasTrigger) controller.AddParameter(trigger, AnimatorControllerParameterType.Trigger);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    // A clip that shows one sprite for its whole length. Reuses (and updates) the asset if it already exists.
    private static AnimationClip SpriteClip(string path, string clipName, Sprite sprite, float length)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        bool isNew = clip == null;
        if (isNew) clip = new AnimationClip { name = clipName, frameRate = 60f };

        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, new[]
        {
            new ObjectReferenceKeyframe { time = 0f, value = sprite },
            new ObjectReferenceKeyframe { time = length, value = sprite }
        });

        if (isNew) AssetDatabase.CreateAsset(clip, path);
        else EditorUtility.SetDirty(clip);
        return clip;
    }
}
