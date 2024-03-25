using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using Kaede2.Scenario.Framework.Commands;
using Kaede2.Scenario.Framework.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using Color = Kaede2.Scenario.Framework.Commands.Color;
using Sprite = Kaede2.Scenario.Framework.Commands.Sprite;

namespace Kaede2.Scenario.Framework
{
    public abstract class Command
    {
        public enum ExecutionType
        {
            Instant,
            Synchronous,
            Asynchronous
        }

        private readonly string[] originalArgs;
        public IReadOnlyList<string> OriginalArgs => originalArgs;

        protected readonly ScenarioModule Module;

        public abstract ExecutionType Type { get; }

        // a minus value means that the time is indeterminate, but its absolute value can be used for UI hints
        public abstract float ExpectedExecutionTime { get; }

        // constructor will be called before the scenario actually starts
        // so any initialization related to the current status of the scene should be done in Setup
        // the constructor should only be used to do initialization in a deterministic way
        // when a command is created, it should be able to be executed, undone, and redone as many times as needed
        protected Command(ScenarioModule module, string[] arguments)
        {
            Module = module;
            originalArgs = arguments;
        }

        public virtual IEnumerator Setup()
        {
            yield break;
        }

        public abstract IEnumerator Execute();

        public override string ToString()
        {
            string result = "";
            foreach (var s in originalArgs)
            {
                result += s + "\t";
            }

            return result.Trim();
        }

        protected T Arg<T>(int index, T defaultValue = default)
        {
            try
            {
                if (index >= originalArgs.Length) return defaultValue;

                string resolved = Module.ResolveAlias(originalArgs[index]) ?? originalArgs[index];

                if (typeof(T) == typeof(string)) return (T)(object)resolved;
                if (typeof(T) == typeof(bool)) return (T)(object)bool.Parse(resolved);
                if (typeof(T) == typeof(Ease)) return (T)(object)CommonUtils.GetEase(resolved);
                return Module.Evaluate<T>(resolved);
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Cannot parse Arg[{index}] = {originalArgs[index]} as {typeof(T).Name}. Using default value {defaultValue}.\n{e.Message}");
                return defaultValue;
            }
        }

        protected string OriginalArg(int index, string defaultValue = "")
        {
            return index >= originalArgs.Length ? defaultValue : originalArgs[index];
        }

        protected int ArgLength()
        {
            return originalArgs.Length;
        }

        protected static int FindEntity<T>(string name, out T result) where T : Entity
        {
            var entities = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (entities == null || entities.Length == 0)
            {
                Debug.LogError($"No entities with Type {typeof(T).Name} '{name}' found.");
                result = null;
                return -1;
            }

            var substituteName =
                CommonUtils.FindClosestMatch(name, entities.Select(e => e.gameObject.name), out var distance);
            result = entities.First(e => e.gameObject.name == substituteName);
            if (distance > 5)
            {
                Debug.LogError($"{typeof(T).Name} '{name}' doesn't exist and no substitute found.");
                result = null;
                return -1;
            }

            if (distance != 0)
                Debug.LogWarning(
                    $"{typeof(T).Name} '{name}' doesn't exist, using '{substituteName}' instead. Distance is {distance}.");
            return distance;
        }

        protected static ExecutionType ExecutionTypeBasedOnWaitAndDuration(bool wait, float duration)
        {
            if (duration == 0) return ExecutionType.Instant;
            if (wait) return ExecutionType.Synchronous;
            return ExecutionType.Asynchronous;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Log()
        {
#if UNITY_EDITOR
            const bool colorTag = true;
#else
            const bool colorTag = false;
#endif
            StringBuilder sb = new();
            sb.Append(colorTag ? $"<color=#00FF00>[{Time.frameCount}]</color>\t" : $"[{Time.frameCount}]\t");
            sb.Append(colorTag ? $"<color=#FFFF00>{originalArgs[0]}</color>\n" : $"{originalArgs[0]}\n");

            for (int i = 1; i < originalArgs.Length; i++)
            {
                var resolved = Module.ResolveAlias(originalArgs[i]) ?? originalArgs[i];
                sb.Append(colorTag ? $"\t<color=#00FFFF>{originalArgs[i]}</color>" : $"\t{originalArgs[i]}");

                if (originalArgs[0] != "set")
                {
                    if (resolved != originalArgs[i])
                        sb.Append(colorTag ? $" <color=#FF00FF>({resolved})</color>" : $" ({resolved})");
                    if (originalArgs[i] is not ("true" or "false"))
                    {
                        var parsed = Module.ResolveExpression(resolved);
                        if (parsed != resolved)
                            sb.Append(colorTag ? $" <color=#FFFFFF>=> {parsed}</color>" : $" => {parsed}");
                    }
                }

                if (i < originalArgs.Length - 1)
                    sb.Append("\n");
            }

            Debug.Log(sb.ToString());
        }
#endif

        public static IReadOnlyDictionary<string, Type> Types => new Dictionary<string, Type>()
        {
            { "mes", typeof(Mes) },
            { "mes_auto", typeof(MesAuto) },
            { "anim", typeof(Anim) },
            { "layer", typeof(Layer) },
            { "move", typeof(Move) },
            { "pos", typeof(Pos) },
            { "rename", typeof(NotImplemented) },
            { "rotate", typeof(Rotate) },
            { "scale", typeof(Scale) },
            { "font", typeof(NotImplemented) },
            { "sprite", typeof(Sprite) },
            { "sprite_hide", typeof(SpriteHide) },
            { "animation_prefab", typeof(AnimationPrefab) },
            { "animation_prefab_hide", typeof(AnimationPrefabHide) },
            { "transform_prefab", typeof(TransformPrefab) },
            { "transform_prefab_hide", typeof(NotImplemented) },
            { "bg_effect_prefab", typeof(AnimationPrefab) },
            { "bg_effect_prefab_hide", typeof(AnimationPrefabHide) },
            { "del", typeof(Del) },
            { "replace", typeof(Replace) },
            { "clone", typeof(NotImplemented) },
            { "color", typeof(Color) },
            { "alias_text", typeof(AliasText) },
            { "set", typeof(Set) },
            { "log_message_load", typeof(NotImplemented) },
            { "auto_load", typeof(AutoLoad) },
            { "init_end", typeof(InitEnd) },
            { "mes_speed", typeof(NotImplemented) },
            { "move_anim", typeof(MoveAnim) },
            { "rotate_anim", typeof(RotateAnim) },
            { "scale_anim", typeof(NotImplemented) },
            { "move_anim_stop", typeof(MoveAnimStop) },
            { "rotate_anim_stop", typeof(RotateAnimStop) }, // Not tested
            { "scale_anim_stop", typeof(NotImplemented) },
            { "pivot", typeof(Pivot) },
            { "include", typeof(IntentionallyNotImplemented) },
            { "bg", typeof(BG) },
            { "bg_hide", typeof(BGHide) },
            { "bg_move", typeof(NotImplemented) },
            { "actor", typeof(NotImplemented) },
            { "actor_setup", typeof(ActorSetup) },
            { "actor_move", typeof(NotImplemented) },
            { "actor_scale", typeof(ActorScale) },
            { "actor_show", typeof(ActorShow) },
            { "actor_hide", typeof(ActorHide) },
            { "actor_eye", typeof(ActorEye) },
            { "actor_face", typeof(ActorFace) },
            { "actor_enter", typeof(ActorEnter) },
            { "actor_exit", typeof(ActorExit) },
            { "spot_on", typeof(SpotOn) },
            { "spot_off", typeof(SpotOff) },
            { "actor_angle", typeof(ActorAngle) },
            { "actor_body_angle", typeof(ActorBodyAngle) },
            { "actor_auto_mouth", typeof(ActorAutoMouth) },
            { "actor_mouth_sync", typeof(ActorMouthSync) },
            { "actor_eye_add", typeof(ActorEyeAdd) },
            { "actor_eye_abs", typeof(ActorEyeAbs) },
            { "actor_eye_off", typeof(ActorEyeOff) },
            { "actor_auto_del", typeof(ActorAutoDel) },
            { "pane_create", typeof(NotImplemented) },
            { "pane_select", typeof(NotImplemented) },
            { "pane_show", typeof(NotImplemented) },
            { "pane_hide", typeof(NotImplemented) },
            { "pane_scale", typeof(NotImplemented) },
            { "pane_del", typeof(NotImplemented) },
            { "pane_rename", typeof(NotImplemented) },
            { "pane_layer", typeof(NotImplemented) },
            { "msg_box_color", typeof(NotImplemented) },
            { "msg_box_show", typeof(MsgBoxShow) },
            { "msg_box_hide", typeof(MsgBoxHide) },
            { "msg_box_change", typeof(NotImplemented) },
            { "msg_box_name_show", typeof(NotImplemented) },
            { "ui_show", typeof(UIShow) },
            { "ui_hide", typeof(UIHide) },
            { "camera_all_on", typeof(CameraAllOn) },
            { "camera_all_off", typeof(CameraAllOff) },
            { "bg_blur_on", typeof(NotImplemented) },
            { "bg_blur_off", typeof(NotImplemented) },
            { "shake", typeof(Shake) },
            { "shake_on", typeof(NotImplemented) },
            { "shake_off", typeof(NotImplemented) },
            { "shake_mes", typeof(ShakeMes) },
            { "focus_on", typeof(NotImplemented) },
            { "focus_off", typeof(NotImplemented) },
            { "fade_in", typeof(FadeIn) },
            { "fade_out", typeof(FadeOut) },
            { "camera_lookat", typeof(CameraLookAt) },
            { "camera_move", typeof(CameraMove) },
            { "camera_zoom", typeof(CameraZoom) }, // Not tested
            { "camera_default", typeof(CameraDefault) },
            { "still", typeof(Still) },
            { "still_off", typeof(StillOff) },
            { "still_move", typeof(NotImplemented) },
            { "bgm", typeof(BGM) },
            { "bgm_load", typeof(IntentionallyNotImplemented) },
            { "bgm_stop", typeof(BGMStop) },
            { "se", typeof(SE) },
            { "se_load", typeof(IntentionallyNotImplemented) },
            { "se_stop", typeof(SEStop) },
            { "se_loop", typeof(SELoop) },
            { "voice", typeof(NotImplemented) },
            { "voice_load", typeof(IntentionallyNotImplemented) },
            { "voice_stop", typeof(NotImplemented) },
            { "voice_play", typeof(NotImplemented) },
            { "asset_load", typeof(NotImplemented) },
            { "asset_unload", typeof(NotImplemented) },
            { "debug_log_show", typeof(NotImplemented) },
            { "caption", typeof(Caption) },
            { "caption_hide", typeof(CaptionHide) },
            { "caption_color", typeof(CaptionColor) },
            { "caption_font_color", typeof(NotImplemented) },
            { "caption_font_size", typeof(NotImplemented) },
            { "wait", typeof(Wait) },
            { "function", typeof(IntentionallyNotImplemented) },
            { "endfunction", typeof(IntentionallyNotImplemented) },
            { "sub", typeof(IntentionallyNotImplemented) },
            { "end", typeof(End) },
        };
    }
}