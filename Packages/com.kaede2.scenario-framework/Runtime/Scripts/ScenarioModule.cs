using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using biscuit.Scenario.Effect;
using Kaede2.Scenario.Framework.Commands;
using Kaede2.Scenario.Framework.Entities;
using Kaede2.Scenario.Framework.Live2D;
using Kaede2.Scenario.Framework.Utils;
using NCalc;
using UnityEngine;
using Sprite = UnityEngine.Sprite;

namespace Kaede2.Scenario.Framework
{
    public abstract class ScenarioModule : MonoBehaviour, IStateSavable<ScenarioState>
    {
        public abstract string ScenarioName { get; }

        public abstract IReadOnlyList<string> Statements { get; }
        public abstract IReadOnlyList<Command> Commands { get; }
        public abstract int CurrentCommandIndex { get; protected set; }

        public abstract UIController UIController { get; }
        public abstract AudioManager AudioManager { get; }

        private Resource scenarioResource;
        public Resource ScenarioResource => scenarioResource;

        private Dictionary<string, Expression> variables;
        private Dictionary<string, string> aliases;

        public IReadOnlyDictionary<string, Expression> Variables => variables;
        public IReadOnlyDictionary<string, string> Aliases => aliases;

        private List<Entity> entities;
        public IReadOnlyList<Entity> Entities => entities.Where(e => e != null && e.gameObject != null && !e.IsBeingDestroyed).ToList();

        public virtual bool ActorAutoDelete { get; set; }
        public virtual bool LipSync { get; set; }

        private bool autoMode;
        public virtual bool AutoMode
        {
            get => autoMode;
            set
            {
                autoMode = value;
                if (UIController == null) return;
                if (UIController.MessageBox == null) return;
                UIController.MessageBox.AutoMode = value;
            }
        }

        public void ToggleAutoMode()
        {
            AutoMode = !AutoMode;
        }

        private bool continuousMode;
        public virtual bool ContinuousMode
        {
            get => continuousMode;
            set
            {
                continuousMode = value;
                if (UIController == null) return;
                if (UIController.MessageBox == null) return;
                UIController.MessageBox.ContinuousMode = value;
            }
        }

        public void ToggleContinuousMode()
        {
            ContinuousMode = !ContinuousMode;
        }

        public abstract bool Fixed16By9 { get; set; }
        public abstract float AudioMasterVolume { get; set; }
        public abstract float AudioBGMVolume { get; set; }
        public abstract float AudioSEVolume { get; set; }
        public abstract float AudioVoiceVolume { get; set; }

        public virtual bool Paused { get; set; }

        public abstract bool MesClicked { get; }

        public abstract IEnumerator InitEnd();
        public abstract IEnumerator End();

        public Action<Command> OnCommand;
        public Action<string, string, string> OnMesCommand; // speaker, voiceId, message

        private List<GameObject> effectPrefabs;
        public IReadOnlyList<GameObject> EffectPrefabs
        {
            get
            {
                if (effectPrefabs != null) return effectPrefabs;

                effectPrefabs = new List<GameObject>();
                foreach (var prefab in Resources.LoadAll<GameObject>("biscuit/scenario/effectsatadv/prefabs"))
                {
                    effectPrefabs.Add(prefab);
                }

                return effectPrefabs;
            }
        }

        public abstract IEnumerator LoadResource(Resource.Type type, string resourceName);

        protected IEnumerator PreloadResources()
        {
            HashSet<LoadData> allLoadData = new();

            foreach (var statement in Statements)
            {
                string[] statementArgs = statement.Split(new[] { '\t' }, StringSplitOptions.None);

                // we ignore all "_load" commands since the resources they specify are sometimes not used
                // since we are doing a full scan, we can just find the actually used ones
                switch (statementArgs[0])
                {
                    case "人物":
                    case "actor_setup":
                    {
                        string argWithResource = statementArgs[1];
                        string resourceName = ResolveAlias(argWithResource.Split(':')[0]);
                        allLoadData.Add(new(Resource.Type.Actor, resourceName));
                        break;
                    }
                    case "font":
                    {
                        break;
                    }
                    case "sprite":
                    {
                        string argWithResource = statementArgs[1];
                        string resourceName = ResolveAlias(argWithResource.Split(':')[0]);
                        allLoadData.Add(new(Resource.Type.Sprite, resourceName));
                        break;
                    }
                    case "still":
                    {
                        string argWithResource = statementArgs[1];
                        string still = ResolveAlias(argWithResource.Split(':')[0]);
                        allLoadData.Add(new(Resource.Type.Still, still));
                        break;
                    }
                    case "背景":
                    case "bg":
                    case "replace":
                    {
                        string bg = ResolveAlias(statementArgs[1]);
                        allLoadData.Add(new(Resource.Type.Background, bg));
                        break;
                    }
                    case "se":
                    // case "se_load":
                    case "se_loop":
                    {
                        string assetName = ResolveAlias(statementArgs[1]);
                        allLoadData.Add(new(Resource.Type.SE, assetName));
                        break;
                    }
                    case "bgm":
                    // case "bgm_load":
                    {
                        string assetName = ResolveAlias(statementArgs[1]);
                        allLoadData.Add(new(Resource.Type.BGM, assetName));
                        break;
                    }
                    case "mes":
                    case "mes_auto":
                    case "voice_play":
                    {
                        allLoadData.Add(new(Resource.Type.Voice, statementArgs[2]));
                        break;
                    }
                    // case "voice_load":
                    // {
                    //     allLoadData.Add(new(Resource.Type.Voice, statementArgs[1]));
                    //     break;
                    // }
                    case "transform_prefab":
                    {
                        string id = statementArgs[2];
                        allLoadData.Add(new(Resource.Type.TransformPrefab, id));
                        break;
                    }
                }
            }

            Debug.Log($"Pre-loading {allLoadData.Count} assets...");

            // sort the hashset by load type
            var loadDataList = allLoadData.ToList();
            loadDataList.Sort((a, b) => a.Type.CompareTo(b.Type));

            CoroutineGroup group = new(loadDataList.Select(d => LoadResource(d.Type, d.Name)).ToList(), this);
            yield return group.WaitForAll();

            Debug.Log("Resource Pre-loaded");
        }

        private class LoadData : IEquatable<LoadData>
        {
            public readonly Resource.Type Type;
            public readonly string Name;

            public LoadData(Resource.Type type, string name)
            {
                Type = type;
                Name = name;
            }

            public bool Equals(LoadData other)
            {
                if (other == null)
                {
                    return false;
                }

                return Type == other.Type && Name == other.Name;
            }

            public override bool Equals(object obj)
            {
                if (obj is LoadData data)
                {
                    return Type == data.Type && Name == data.Name;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return (int)Type * 1234 + (Name == null ? 0 : Name.GetHashCode());
            }
        }

        public class Resource
        {
            public TextAsset AliasText = null;
            public readonly Dictionary<string, Live2DAssets> Actors = new();
            public readonly Dictionary<string, Sprite> Sprites = new();
            public readonly Dictionary<string, Texture2D> Stills = new();
            public readonly Dictionary<string, Texture2D> Backgrounds = new();
            public readonly Dictionary<string, AudioClip> SoundEffects = new();
            public readonly Dictionary<string, AudioClip> BackgroundMusics = new();
            public readonly Dictionary<string, AudioClip> Voices = new();
            public readonly Dictionary<CharacterId, Sprite> TransformImages = new();

            // the order of these enum values is important
            // it's used to hint the general size of the asset to be loaded
            // so larger assets are loaded first to save time
            public enum Type
            {
                BGM,
                Voice,
                SE,
                Actor,
                Background,
                Still,
                Sprite,
                TransformPrefab,
                AliasText,
            }
        }

        protected virtual void Awake()
        {
            ActorAutoDelete = false;
            LipSync = true;

            scenarioResource = new();
            aliases = new();
            variables = new();
            entities = new();

            UIController.Module = this;
            AudioManager.Module = this;
        }

        protected Command ParseStatement(string statement)
        {
            string[] args = statement.Split(new[] {'\t'}, StringSplitOptions.None);
            string command = args[0];

            Type commandType = Command.Types.TryGetValue(command, out var type) ? type : typeof(NotImplemented);
            Command commandObj = (Command) System.ComponentModel.TypeDescriptor.CreateInstance(
                provider: null,
                objectType: commandType,
                argTypes: new[] {typeof(ScenarioModule), typeof(string)},
                args: new object[] {this, args});
            // Command commandObj = (Command)Activator.CreateInstance(commandType, this, args);

            return commandObj;
        }

        #region Variables

        public void AddVariable(string variable, string value)
        {
            if (variable == value)
            {
                Debug.LogError("Variable cannot be equal to value");
                return;
            }

            variables[variable] = new Expression(value);
        }

        public T Evaluate<T>(string expression)
        {
            var exp = new Expression(expression);
            foreach (var v in variables)
            {
                exp.Parameters[v.Key] = v.Value;
            }

            var result = exp.Evaluate();
            return (T) Convert.ChangeType(result, typeof(T));
        }

        public string ResolveExpression(string expression)
        {
            try
            {
                var exp = new Expression(expression);
                foreach (var v in variables)
                {
                    exp.Parameters[v.Key] = v.Value;
                }

                exp.Evaluate();
                return exp.Evaluate().ToString();
            }
            catch (Exception)
            {
                return expression;
            }
        }

        #endregion

        #region Alias

        public void AddAlias(string orig, string alias)
        {
            aliases[alias] = orig;
        }

        public string ResolveAlias(string token)
        {
            if (aliases == null) return token;
            string result = token;
            var sortedKeys = aliases.Keys.ToList();
            sortedKeys.Sort((k2, k1) => k1.Length.CompareTo(k2.Length));
            while (true)
            {
                var replace = sortedKeys.Aggregate(result, 
                    (current, key) => 
                        current.Replace(key, aliases[key]));

                if (replace == result)
                    break;

                result = replace;
            }

            return result;
        }

        #endregion

        public void RegisterEntity(Entity entity)
        {
            entities.Add(entity);
        }

        public void UnregisterEntity(Entity entity)
        {
            entities.Remove(entity);
        }

        protected IEnumerator Execute()
        {
            while (true)
            {
                yield return new WaitUntil(() => !Paused);
                ++CurrentCommandIndex;
                if (CurrentCommandIndex >= Commands.Count)
                {
                    yield break;
                }

                var command = Commands[CurrentCommandIndex];
                OnCommand?.Invoke(command);
                var execution = ExecuteSingle(command);
                while (execution.MoveNext())
                {
                    yield return execution.Current;
                    yield return new WaitUntil(() => !Paused);
                }
            }
        }

        protected IEnumerator ExecuteSingle(Command command)
        {
            if (ScenarioRunMode.Args.TestMode || Application.isEditor)
                command.Log();

            command.Setup();

            switch (command.Type)
            {
                case Command.ExecutionType.Instant:
                    command.Execute().InstantExecution();
                    break;
                case Command.ExecutionType.Synchronous:
                    yield return command.Execute().WithException();
                    break;
                case Command.ExecutionType.Asynchronous:
                    StartCoroutine(command.Execute().WithException());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ScenarioState GetState()
        {
            return new()
            {
                currentCommandIndex = CurrentCommandIndex,
                actorAutoDelete = ActorAutoDelete,
                lipSync = LipSync,

                uiOn = UIController.UICanvas.gameObject.activeSelf,
                cameraOn = UIController.ContentCanvas.gameObject.activeSelf,
                cameraPosition = UIController.CameraPos,
                cameraScale = UIController.CameraScale,

                actors = Live2DActorEntity.AllActors.Select(c => c.GetState()).ToList(),
                sprites = UIController.SpriteCanvas.GetComponentsInChildren<SpriteEntity>().Select(s => s.GetState()).ToList(),
                backgrounds = UIController.BackgroundCanvas.GetComponentsInChildren<BackgroundEntity>().Select(b => b.GetState()).ToList(),
                stills = UIController.StillCanvas.GetComponentsInChildren<BackgroundEntity>().Select(b => b.GetState()).ToList(),
                animationPrefabs = FindObjectsByType<AnimationPrefabEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(p => p.GetState()).Where(s => s != null).ToList(),
                caption = UIController.CaptionBox.GetState(),
                messageBox = UIController.MessageBox.GetState(),
                fade = UIController.Fade.GetState(),
                audio = AudioManager.GetState()
            };
        }

        public void RestoreState(ScenarioState state)
        {
            void CleanAndRestoreStates<T>(Transform parent, List<T> states, Action<T> restoreState) where T : State<T>
            {
                foreach (var o in parent)
                {
                    Transform t = (Transform)o;
                    Destroy(t.gameObject);
                }

                if (states == null) return;

                foreach (var s in states)
                {
                    restoreState(s);
                }
            }

            void RestoreActorState(ActorState actorState)
            {
                if (!ScenarioResource.Actors.TryGetValue(actorState.modelName, out var asset))
                {
                    Debug.LogError($"Live2D model {actorState.modelName} not found");
                    return;
                }

                var entity = UIController.CreateActor(actorState.transform.position, actorState.objectName, asset);
                entity.RestoreState(actorState);
            }

            void RestoreSpriteState(CommonResourceState spriteState)
            {
                if (!ScenarioResource.Sprites.TryGetValue(spriteState.resourceName, out var sprite))
                {
                    Debug.LogError($"Sprite {spriteState.resourceName} not found");
                    return;
                }

                var entity = UIController.CreateSprite(spriteState.objectName, spriteState.resourceName, sprite);
                entity.RestoreState(spriteState);
            }

            void RestoreBackgroundState(CommonResourceState backgroundState)
            {
                if (!ScenarioResource.Backgrounds.TryGetValue(backgroundState.resourceName, out var tex))
                {
                    Debug.LogError($"Background texture {backgroundState.resourceName} not found");
                    return;
                }

                var entity = UIController.CreateBackground(backgroundState.objectName, backgroundState.resourceName, tex);
                entity.RestoreState(backgroundState);
            }

            void RestoreStillState(CommonResourceState stillState)
            {
                if (!ScenarioResource.Stills.TryGetValue(stillState.resourceName, out var tex))
                {
                    Debug.LogError($"Background texture {stillState.resourceName} not found");
                    return;
                }

                var entity = UIController.CreateStill(stillState.objectName, stillState.resourceName, tex);
                entity.RestoreState(stillState);
            }

            CleanAndRestoreStates(UIController.Live2DCanvas.transform, state.actors, RestoreActorState);
            CleanAndRestoreStates(UIController.SpriteCanvas.transform, state.sprites, RestoreSpriteState);
            CleanAndRestoreStates(UIController.BackgroundCanvas.transform, state.backgrounds, RestoreBackgroundState);
            CleanAndRestoreStates(UIController.StillCanvas.transform, state.stills, RestoreStillState);

            // clean and restore animation prefab states
            foreach (var animPrefab in FindObjectsByType<AnimationPrefabEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Destroy(animPrefab.gameObject);
            }

            foreach (var animPrefabState in state.animationPrefabs)
            {
                var prefab = EffectPrefabs.FirstOrDefault(p =>
                    p.name.Equals(animPrefabState.prefabName, StringComparison.InvariantCultureIgnoreCase));
                if (prefab == null)
                {
                    Debug.LogError($"Animation Prefab {animPrefabState.prefabName} not found");
                    continue;
                }

                GameObject instantiated = Instantiate(prefab);
                instantiated.name = animPrefabState.objectName;
                AnimationPrefabEntity entity = UIController.CreateEntity<AnimationPrefabEntity>(instantiated);
                entity.prefabName = animPrefabState.prefabName;

                if (animPrefabState.isTransform)
                {
                    CharacterTransformController controller = instantiated.GetComponent<CharacterTransformController>();
                    controller.Module = this;
                    controller.Setup(animPrefabState.id);
                }
                else
                {
                    Transform animTransform = instantiated.transform;
                    animTransform.localScale = Vector3.one * animPrefabState.scale;
                    entity.Position = animPrefabState.position;
                }
            }

            UIController.CaptionBox.RestoreState(state.caption);
            UIController.MessageBox.RestoreState(state.messageBox);
            UIController.Fade.RestoreState(state.fade);
            AudioManager.RestoreState(state.audio);

            CurrentCommandIndex = state.currentCommandIndex;
            ActorAutoDelete = state.actorAutoDelete;
            LipSync = state.lipSync;

            UIController.UICanvas.gameObject.SetActive(state.uiOn);
            UIController.ContentCanvas.gameObject.SetActive(state.cameraOn);
            UIController.CameraPos = state.cameraPosition;
            UIController.CameraScale = state.cameraScale;
        }
    }
}