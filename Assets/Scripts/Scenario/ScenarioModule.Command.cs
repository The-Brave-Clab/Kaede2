using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Kaede2.Utils;
using Kaede2.Scenario.Commands;

namespace Kaede2.Scenario
{
    public partial class ScenarioModule
    {
        private List<Command> commands;
        private int currentCommandIndex;

        private Command ParseStatement(string statement)
        {
            string[] args = statement.Split(new[] {'\t'}, StringSplitOptions.None);
            string command = args[0];

            Type commandType = CommandTypes.TryGetValue(command, out var type) ? type : typeof(NotImplemented);
            Command commandObj = (Command) System.ComponentModel.TypeDescriptor.CreateInstance(
                provider: null,
                objectType: commandType,
                argTypes: new[] {typeof(ScenarioModule), typeof(string)},
                args: new object[] {this, args});

            return commandObj;
        }

        public abstract class Command
        {
            public enum ExecutionType
            {
                Instant,
                Synchronous,
                Asynchronous
            }

            private readonly string[] args;
            private readonly string[] originalArgs;

            protected readonly ScenarioModule Module;

            public abstract ExecutionType Type { get; }
            public abstract float ExpectedExecutionTime { get; }

            // constructor will be called before the scenario actually starts
            // so any initialization related to the current status of the scene should be done in Setup
            // the constructor should only be used to do initialization in a deterministic way
            // when a command is created, it should be able to be executed, undone, and redone as many times as needed
            protected Command(ScenarioModule module, string[] arguments)
            {
                Module = module;
                args = arguments;
                originalArgs = new string[arguments.Length];
                Array.Copy(arguments, originalArgs, arguments.Length);

                for (int i = 0; i < args.Length; i++)
                {
                    string resolved = Module.ResolveAlias(args[i]);
                    if (resolved != null)
                        args[i] = resolved;
                }
            }

            public virtual IEnumerator Setup()
            {
                yield break;
            }
            public abstract IEnumerator Execute();
            public abstract void Undo();

            public override string ToString()
            {
                string result = "";
                foreach (var s in args)
                {
                    result += s + "\t";
                }

                return result.Trim();
            }

            protected T Arg<T>(int index, T defaultValue = default)
            {
                try
                {
                    if (index >= args.Length) return defaultValue;
                    if (typeof(T) == typeof(string)) return (T)(object)args[index];
                    if (typeof(T) == typeof(bool)) return (T)(object)bool.Parse(args[index]);
                    if (typeof(T) == typeof(Ease)) return (T)(object)CommonUtils.GetEase(args[index]);
                    return Module.Evaluate<T>(args[index]);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"Cannot parse Arg[{index}] = {args[index]} as {typeof(T).Name}. Using default value {defaultValue}.\n{e.Message}");
                    return defaultValue;
                }
            }

            protected string OriginalArg(int index, string defaultValue = "")
            {
                return index >= originalArgs.Length ? defaultValue : originalArgs[index];
            }

            protected static T FindEntity<T>(string name) where T : Entity
            {
                var entities = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var substituteName =
                    CommonUtils.FindClosestMatch(name, entities.Select(e => e.gameObject.name), out var distance);
                var result = entities.First(e => e.gameObject.name == substituteName);
                if (distance != 0)
                    Debug.LogWarning(
                        $"{typeof(T).Name} '{name}' doesn't exist, using '{substituteName}' instead. Distance is {distance}.");
                return result;
            }
        }

        private static Dictionary<string, Type> CommandTypes => new()
        {
            { "mes", typeof(NotImplemented) },
            { "mes_auto", typeof(NotImplemented) },
            { "anim", typeof(NotImplemented) },
            { "layer", typeof(NotImplemented) },
            { "move", typeof(NotImplemented) },
            { "pos", typeof(NotImplemented) },
            { "rename", typeof(NotImplemented) },
            { "rotate", typeof(NotImplemented) },
            { "scale", typeof(NotImplemented) },
            { "font", typeof(NotImplemented) },
            { "sprite", typeof(NotImplemented) },
            { "sprite_hide", typeof(NotImplemented) },
            { "animation_prefab", typeof(NotImplemented) },
            { "animation_prefab_hide", typeof(NotImplemented) },
            { "transform_prefab", typeof(NotImplemented) },
            { "transform_prefab_hide", typeof(NotImplemented) },
            { "bg_effect_prefab", typeof(NotImplemented) },
            { "bg_effect_prefab_hide", typeof(NotImplemented) },
            { "del", typeof(NotImplemented) },
            { "replace", typeof(NotImplemented) },
            { "clone", typeof(NotImplemented) },
            { "color", typeof(NotImplemented) },
            { "alias_text", typeof(NotImplemented) },
            { "set", typeof(NotImplemented) },
            { "log_message_load", typeof(NotImplemented) },
            { "auto_load", typeof(NotImplemented) },
            { "init_end", typeof(NotImplemented) },
            { "mes_speed", typeof(NotImplemented) },
            { "move_anim", typeof(NotImplemented) },
            { "rotate_anim", typeof(NotImplemented) },
            { "scale_anim", typeof(NotImplemented) },
            { "move_anim_stop", typeof(NotImplemented) },
            { "rotate_anim_stop", typeof(NotImplemented) }, // Not tested
            { "scale_anim_stop", typeof(NotImplemented) },
            { "pivot", typeof(NotImplemented) },
            { "include", typeof(NotImplemented) },
            { "bg", typeof(NotImplemented) },
            { "bg_hide", typeof(NotImplemented) },
            { "bg_move", typeof(NotImplemented) },
            { "actor", typeof(NotImplemented) },
            { "actor_setup", typeof(NotImplemented) },
            { "actor_move", typeof(NotImplemented) },
            { "actor_scale", typeof(NotImplemented) },
            { "actor_show", typeof(NotImplemented) },
            { "actor_hide", typeof(NotImplemented) },
            { "actor_eye", typeof(NotImplemented) },
            { "actor_face", typeof(NotImplemented) },
            { "actor_enter", typeof(NotImplemented) },
            { "actor_exit", typeof(NotImplemented) },
            { "spot_on", typeof(NotImplemented) },
            { "spot_off", typeof(NotImplemented) },
            { "actor_angle", typeof(NotImplemented) },
            { "actor_body_angle", typeof(NotImplemented) },
            { "actor_auto_mouth", typeof(NotImplemented) },
            { "actor_mouth_sync", typeof(NotImplemented) },
            { "actor_eye_add", typeof(NotImplemented) },
            { "actor_eye_abs", typeof(NotImplemented) },
            { "actor_eye_off", typeof(NotImplemented) },
            { "actor_auto_del", typeof(NotImplemented) },
            { "pane_create", typeof(NotImplemented) },
            { "pane_select", typeof(NotImplemented) },
            { "pane_show", typeof(NotImplemented) },
            { "pane_hide", typeof(NotImplemented) },
            { "pane_scale", typeof(NotImplemented) },
            { "pane_del", typeof(NotImplemented) },
            { "pane_rename", typeof(NotImplemented) },
            { "pane_layer", typeof(NotImplemented) },
            { "msg_box_color", typeof(NotImplemented) },
            { "msg_box_show", typeof(NotImplemented) },
            { "msg_box_hide", typeof(NotImplemented) },
            { "msg_box_change", typeof(NotImplemented) },
            { "msg_box_name_show", typeof(NotImplemented) },
            { "ui_show", typeof(NotImplemented) },
            { "ui_hide", typeof(NotImplemented) },
            { "camera_all_on", typeof(NotImplemented) },
            { "camera_all_off", typeof(NotImplemented) },
            { "bg_blur_on", typeof(NotImplemented) },
            { "bg_blur_off", typeof(NotImplemented) },
            { "shake", typeof(NotImplemented) },
            { "shake_on", typeof(NotImplemented) },
            { "shake_off", typeof(NotImplemented) },
            { "shake_mes", typeof(NotImplemented) },
            { "focus_on", typeof(NotImplemented) },
            { "focus_off", typeof(NotImplemented) },
            { "fade_in", typeof(NotImplemented) },
            { "fade_out", typeof(NotImplemented) },
            { "camera_lookat", typeof(NotImplemented) },
            { "camera_move", typeof(NotImplemented) },
            { "camera_zoom", typeof(NotImplemented) }, // Not tested
            { "camera_default", typeof(NotImplemented) },
            { "still", typeof(NotImplemented) },
            { "still_off", typeof(NotImplemented) },
            { "still_move", typeof(NotImplemented) },
            { "bgm", typeof(NotImplemented) },
            { "bgm_load", typeof(NotImplemented) },
            { "bgm_stop", typeof(NotImplemented) },
            { "se", typeof(NotImplemented) },
            { "se_load", typeof(NotImplemented) },
            { "se_stop", typeof(NotImplemented) },
            { "se_loop", typeof(NotImplemented) },
            { "voice", typeof(NotImplemented) },
            { "voice_load", typeof(NotImplemented) },
            { "voice_stop", typeof(NotImplemented) },
            { "voice_play", typeof(NotImplemented) },
            { "asset_load", typeof(NotImplemented) },
            { "asset_unload", typeof(NotImplemented) },
            { "debug_log_show", typeof(NotImplemented) },
            { "caption", typeof(NotImplemented) },
            { "caption_hide", typeof(NotImplemented) },
            { "caption_color", typeof(NotImplemented) },
            { "caption_font_color", typeof(NotImplemented) },
            { "caption_font_size", typeof(NotImplemented) },
            { "wait", typeof(NotImplemented) },
            { "function", typeof(IntentionallyNotImplemented) },
            { "endfunction", typeof(IntentionallyNotImplemented) },
            { "sub", typeof(IntentionallyNotImplemented) },
            { "end", typeof(NotImplemented) },
        };
    }
}