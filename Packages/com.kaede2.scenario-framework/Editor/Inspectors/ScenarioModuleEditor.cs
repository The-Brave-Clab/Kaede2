using System;
using System.Collections.Generic;
using System.Reflection;
using NCalc;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kaede2.Scenario.Framework.Editor.Inspectors
{
    [CustomEditor(typeof(ScenarioModule))]
    public class ScenarioModuleEditor : UnityEditor.Editor
    {
        private ScenarioModule component;

        private void OnCommandRepaint(Command _)
        {
            Repaint();
        }

        private void OnEnable()
        {
            component = (ScenarioModule) target;
            component.OnCommand += OnCommandRepaint;
        }

        private void OnDisable()
        {
            component.OnCommand -= OnCommandRepaint;
        }

        public override void OnInspectorGUI()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (iterator.propertyPath != "m_Script") continue;
                using var scope = new EditorGUI.DisabledScope(true);
                EditorGUILayout.PropertyField(iterator, true);
            }

            DrawScenarioControls();
        }

        private bool showResources;
        private bool showResourcesActors;
        private bool showResourcesSprites;
        private bool showResourcesStills;
        private bool showResourcesBackgrounds;
        private bool showResourcesSoundEffects;
        private bool showResourcesBackgroundMusics;
        private bool showResourcesVoices;
        private bool showResourcesTransformImages;
        private bool showVariables;
        private bool showAliases;

        private void DrawScenarioControls()
        {
            EditorGUILayout.LabelField("Scenario Module", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Scenario controls are only available in Play Mode!", MessageType.Info);
                return;
            }

            if (component == null) // should not happen
            {
                EditorGUILayout.HelpBox("Scenario Module not found!", MessageType.Error);
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.LabelField(nameof(component.ScenarioName),
                    component.ScenarioName);
                EditorGUILayout.LabelField(nameof(component.CurrentCommandIndex),
                    $"{component.CurrentCommandIndex + 1} / {component.Commands.Count}");
                // if (component.CurrentCommandIndex < component.Commands.Count && component.CurrentCommandIndex >= 0)
                //     DrawCommand(component.Commands[component.CurrentCommandIndex]);
                EditorGUILayout.ObjectField(nameof(component.UIController),
                    component.UIController, typeof(UIController), false);
                EditorGUILayout.ObjectField(nameof(component.AudioManager),
                    component.AudioManager, typeof(UIController), false);
                showResources = DrawScenarioResources(showResources, nameof(component.ScenarioResource),
                    component.ScenarioResource);
                showVariables = DrawDictionary(showVariables, nameof(component.Variables), component.Variables);
                showAliases = DrawDictionary(showAliases, nameof(component.Aliases), component.Aliases);
            }

            EditorGUILayout.LabelField("Command Controls", EditorStyles.boldLabel);

            bool newActorAutoDelete = EditorGUILayout.Toggle(nameof(component.ActorAutoDelete), component.ActorAutoDelete);
            if (newActorAutoDelete != component.ActorAutoDelete) component.ActorAutoDelete = newActorAutoDelete;

            bool newLipSync = EditorGUILayout.Toggle(nameof(component.LipSync), component.LipSync);
            if (newLipSync != component.LipSync) component.LipSync = newLipSync;

            EditorGUILayout.LabelField("User Controls", EditorStyles.boldLabel);

            bool newAutoMode = EditorGUILayout.Toggle(nameof(component.AutoMode), component.AutoMode);
            if (newAutoMode != component.AutoMode) component.AutoMode = newAutoMode;

            EditorGUILayout.LabelField("Game Settings", EditorStyles.boldLabel);

            bool newFixed16By9 = EditorGUILayout.Toggle(nameof(component.Fixed16By9), component.Fixed16By9);
            if (newFixed16By9 != component.Fixed16By9) component.Fixed16By9 = newFixed16By9;

            float newAudioMasterVolume = EditorGUILayout.Slider(nameof(component.AudioMasterVolume), component.AudioMasterVolume, 0, 1);
            if (Math.Abs(newAudioMasterVolume - component.AudioMasterVolume) > 0.01f) component.AudioMasterVolume = newAudioMasterVolume;

            float newAudioBGMVolume = EditorGUILayout.Slider(nameof(component.AudioBGMVolume), component.AudioBGMVolume, 0, 1);
            if (Math.Abs(newAudioBGMVolume - component.AudioBGMVolume) > 0.01f) component.AudioBGMVolume = newAudioBGMVolume;

            float newAudioSEVolume = EditorGUILayout.Slider(nameof(component.AudioSEVolume), component.AudioSEVolume, 0, 1);
            if (Math.Abs(newAudioSEVolume - component.AudioSEVolume) > 0.01f) component.AudioSEVolume = newAudioSEVolume;

            float newAudioVoiceVolume = EditorGUILayout.Slider(nameof(component.AudioVoiceVolume), component.AudioVoiceVolume, 0, 1);
            if (Math.Abs(newAudioVoiceVolume - component.AudioVoiceVolume) > 0.01f) component.AudioVoiceVolume = newAudioVoiceVolume;
        }

        private bool DrawScenarioResources(bool foldout, string label, ScenarioModule.Resource resources)
        {
            foldout = EditorGUILayout.Foldout(foldout, label);
            if (!foldout) return false;

            EditorGUI.indentLevel += 1;

            EditorGUILayout.ObjectField(nameof(resources.AliasText), resources.AliasText, typeof(TextAsset), false);
            showResourcesActors = DrawDictionary(showResourcesActors, nameof(resources.Actors), resources.Actors, component.Aliases);
            showResourcesSprites = DrawDictionary(showResourcesSprites, nameof(resources.Sprites), resources.Sprites, component.Aliases);
            showResourcesStills = DrawDictionary(showResourcesStills, nameof(resources.Stills), resources.Stills, component.Aliases);
            showResourcesBackgrounds = DrawDictionary(showResourcesBackgrounds, nameof(resources.Backgrounds), resources.Backgrounds, component.Aliases);
            showResourcesSoundEffects = DrawDictionary(showResourcesSoundEffects, nameof(resources.SoundEffects), resources.SoundEffects, component.Aliases);
            showResourcesBackgroundMusics = DrawDictionary(showResourcesBackgroundMusics, nameof(resources.BackgroundMusics), resources.BackgroundMusics, component.Aliases);
            showResourcesVoices = DrawDictionary(showResourcesVoices, nameof(resources.Voices), resources.Voices);
            showResourcesTransformImages = DrawDictionary(showResourcesTransformImages, nameof(resources.TransformImages), resources.TransformImages);

            EditorGUI.indentLevel -= 1;

            return true;
        }

        private bool DrawDictionary<TKey, TValue>(bool foldout, string label, IReadOnlyDictionary<TKey, TValue> dictionary, IReadOnlyDictionary<string, string> keyAliases = null)
        {
            if (dictionary.Count == 0) return foldout;

            foldout = EditorGUILayout.Foldout(foldout, label);
            if (!foldout) return false;

            EditorGUI.indentLevel += 1;
            foreach (var pair in dictionary)
            {
                var key = $"{pair.Key:G}";
                if (keyAliases is { Count: > 0 })
                {
                    foreach ((string k, string v) in keyAliases)
                    {
                        if (v == key)
                        {
                            key = k;
                            break;
                        }
                    }
                }
                if (typeof(TValue).IsSubclassOf(typeof(Object)))
                {
                    EditorGUILayout.ObjectField(key, pair.Value as Object, typeof(TValue), false);
                }
                else if (typeof(TValue) == typeof(Expression))
                {
                    Expression expression = pair.Value as Expression;
                    var originalExpressionField = expression?.GetType().GetField("OriginalExpression", BindingFlags.Instance | BindingFlags.NonPublic);
                    string originalExpression = originalExpressionField?.GetValue(expression) as string;
                    EditorGUILayout.LabelField(key, originalExpression ?? "");
                
                }
                else
                {
                    EditorGUILayout.LabelField(key, pair.Value.ToString());
                }
            }

            EditorGUI.indentLevel -= 1;

            return true;
        }

        private void DrawCommand(Command command)
        {
            EditorGUI.indentLevel += 1;
            if (command.OriginalArgs.Count == 1)
            {
                EditorGUILayout.LabelField(command.OriginalArgs[0]);
            }
            else
            {
                EditorGUILayout.LabelField(command.OriginalArgs[0], command.OriginalArgs[1]);
                for (int i = 2; i < command.OriginalArgs.Count; i++)
                {
                    EditorGUILayout.LabelField("\t", command.OriginalArgs[i]);
                }
            }
            EditorGUI.indentLevel -= 1;
        }
    }
}