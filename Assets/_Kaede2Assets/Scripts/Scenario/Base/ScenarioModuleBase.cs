using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using biscuit.Scenario.Effect;
using Kaede2.Live2D;
using Kaede2.Scenario.Audio;
using Kaede2.Scenario.Commands;
using Kaede2.Scenario.Entities;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using NCalc;
using UnityEngine;
using Sprite = UnityEngine.Sprite;

namespace Kaede2.Scenario.Base
{
    public abstract class ScenarioModuleBase : Singleton<ScenarioModuleBase>, IStateSavable<ScenarioState>
    {
        public abstract string ScenarioName { get; }

        public abstract IReadOnlyList<string> Statements { get; }
        public abstract IReadOnlyList<Command> Commands { get; }
        public abstract int CurrentCommandIndex { get; protected set; }

        public abstract UIControllerBase UIController { get; }
        public abstract AudioManager AudioManager { get; }

        [SerializeField]
        private List<GameObject> effectPrefabs;
        public IReadOnlyList<GameObject> EffectPrefabs => effectPrefabs;

        private Resource scenarioResource;
        public Resource ScenarioResource => scenarioResource;

        private Dictionary<string, Expression> variables;
        private Dictionary<string, string> aliases;

        private List<ResourceLoader.HandleBase> resourceHandles;

        public bool ActorAutoDelete { get; set; }
        public bool LipSync { get; set; }
        
        public bool AutoMode { get; set; }

        public abstract void InitEnd();
        public abstract void End();

        public class Resource
        {
            public TextAsset AliasText = null;
            public Dictionary<string, Live2DAssets> Actors = new();
            public Dictionary<string, Sprite> Sprites = new();
            public Dictionary<string, Texture2D> Stills = new();
            public Dictionary<string, Texture2D> Backgrounds = new();
            public Dictionary<string, AudioClip> SoundEffects = new();
            public Dictionary<string, AudioClip> BackgroundMusics = new();
            public Dictionary<string, AudioClip> Voices = new();
            public Dictionary<CharacterId, Sprite> TransformImages = new();
        }

        protected override void Awake()
        {
            base.Awake();

            ActorAutoDelete = false;
            LipSync = true;

            scenarioResource = new();
            aliases = new();
            variables = new();
            resourceHandles = new();
        }

        protected virtual void OnDestroy()
        {
            foreach (var handle in resourceHandles)
            {
                handle.Dispose();
            }
        }

        public void RegisterLoadHandle(ResourceLoader.HandleBase handle)
        {
            resourceHandles.Add(handle);
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

#if UNITY_EDITOR
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
#endif

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

        protected IEnumerator Execute()
        {
            while (true)
            {
                ++CurrentCommandIndex;
                if (CurrentCommandIndex >= Commands.Count)
                {
                    yield break;
                }

                var command = Commands[CurrentCommandIndex];
                var execution = ExecuteSingle(command);
                while (execution.MoveNext())
                {
                    yield return execution.Current;
                }
            }
        }

        protected IEnumerator ExecuteSingle(Command command)
        {
#if UNITY_EDITOR
            command.Log();
#endif

            switch (command.Type)
            {
                case Command.ExecutionType.Instant:
                {
                    command.Setup().InstantExecution();
                    command.Execute().InstantExecution();
                    break;
                }
                case Command.ExecutionType.Synchronous:
                {
                    var execution = SyncExecution(command);
                    while (execution.MoveNext())
                    {
                        yield return execution.Current;
                    }
                    break;
                }
                case Command.ExecutionType.Asynchronous:
                {
                    StartCoroutine(SyncExecution(command));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IEnumerator SyncExecution(Command command)
        {
            IEnumerator setup = command.Setup();
            while (setup.MoveNext())
            {
                yield return setup.Current;
            }
            IEnumerator execute = command.Execute();
            while (execute.MoveNext())
            {
                yield return execute.Current;
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
                AnimationPrefabEntity entity = instantiated.AddComponent<AnimationPrefabEntity>();
                entity.prefabName = animPrefabState.prefabName;

                if (animPrefabState.isTransform)
                {
                    CharacterTransformController controller = instantiated.GetComponent<CharacterTransformController>();
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