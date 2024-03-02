using System;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Live2D;
using Kaede2.Utils;
using live2d;
using live2d.framework;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.Scenario.Entities
{
    public class Live2DActorEntity : ScenarioModule.Entity, IStateSavable<ActorState>
    {
        public Live2DAssets Assets { get; set; }

        public bool Hidden { get; set; }
        public bool UseEyeBlink { get; set; }
        public bool ManualEyeOpen { get; set; }

        private Live2DModelUnity live2DModel;
        private L2DPose pose;
        private Live2DMotion idleMotion;
        private Dictionary<string, Live2DMotion> motions;

        private MotionQueueManager motionMgr;
        private MotionQueueManager faceMotionMgr;
        private Live2DMotion nextMotion;
        private EyeBlinkMotion eyeBlink;

        private string currentMotionName;
        private string currentFaceMotionName;

        private RenderTexture targetTexture;
        private RawImage rawImage;
        private RectTransform rectTransform;
        private Matrix4x4 live2DCanvasPos;

        private int layer;
        private List<Live2DActorEntity> mouthSynced;
        public float mouthOpenY = 0.0f;
        public float addAngleX = 0.0f;
        public float addAngleY = 0.0f;
        public float addBodyAngleX = 0.0f;
        public float addEyeX = 0.0f;
        public float absoluteEyeX = 0.0f;

        public static readonly List<Live2DActorEntity> AllActors = new();

        public Dictionary<string, Live2DMotion>.KeyCollection MotionNames => motions.Keys;

        protected override void Awake()
        {
            base.Awake();

            Assets = null;

            Hidden = false;
            UseEyeBlink = true;
            ManualEyeOpen = true;

            live2DModel = null;
            pose = null;
            idleMotion = null;
            motions = new();

            motionMgr = null;
            faceMotionMgr = null;
            nextMotion = null;
            eyeBlink = null;

            currentMotionName = "";
            currentFaceMotionName = "";

            targetTexture = null;
            rawImage = null;
            rectTransform = null;
            live2DCanvasPos = Matrix4x4.identity;

            mouthSynced = new();
        }

        private void Start()
        {
            if (Assets == null)
            {
                Debug.LogError($"Live2D model is not loaded! Please set {nameof(Assets)} first.");
                Destroy(gameObject);
                return;
            }

            CreateWithAssets();

            AllActors.Add(this);

            motionMgr = new();
            faceMotionMgr = new();
            eyeBlink = new();
            
            const float canvasScale = 2.0f;
            const float scale = 1.05f;
            float modelWidth = live2DModel.getCanvasWidth();
            float percentage = (canvasScale / scale - 1.0f) / 2.0f;
            float lowBound = modelWidth * (-percentage);
            float highBound = modelWidth * (1 + percentage);
            live2DCanvasPos = Matrix4x4.Ortho(lowBound, highBound, highBound, lowBound, -50.0f, 50.0f);

            int size = (int)(modelWidth * canvasScale);
            targetTexture = RenderTexture.GetTemporary(size, size, 32);
            targetTexture.name = $"{gameObject.name} RT";

            RenderTexture.active = targetTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = null;

            rawImage = gameObject.AddComponent<RawImage>();
            rawImage.texture = targetTexture;
            rawImage.color = Color.white;
            rawImage.raycastTarget = false;
            rawImage.maskable = false;

            rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(modelWidth, modelWidth) * 2;

            StartMotion("mtn_idle00");
            StartFaceMotion("face_idle01");
        }

        private void Update()
        {
            // skip live2d updating in batch mode
            // if (BatchMode) return;

            if (live2DModel == null) return;

            live2DModel.setMatrix(Matrix4x4.identity);

            if (nextMotion != null)
            {
                motionMgr.startMotion(nextMotion);
                nextMotion = null;
            }
            else if (motionMgr.isFinished() && idleMotion != null)
            {
                motionMgr.startMotion(idleMotion);
            }

            double timeSec = UtSystem.getUserTimeMSec() / 1000.0;
            double t = timeSec * 2 * Math.PI;
            live2DModel.setParamFloat("PARAM_BREATH", (float) (0.5f + 0.5f * Math.Sin(t / 3.0)));

            motionMgr.updateParam(live2DModel);
            faceMotionMgr.updateParam(live2DModel);

            if (UseEyeBlink)
            {
                eyeBlink.setParam(live2DModel);
            }
            else
            {
                live2DModel.setParamFloat("PARAM_EYE_L_OPEN", ManualEyeOpen ? 1.0f : 0.0f);
                live2DModel.setParamFloat("PARAM_EYE_R_OPEN", ManualEyeOpen ? 1.0f : 0.0f);
            }
            if (pose != null)
            {
                pose.updateParam(live2DModel);
            }
            live2DModel.saveParam();
            live2DModel.addToParamFloat("PARAM_ANGLE_X", addAngleX);
            live2DModel.addToParamFloat("PARAM_ANGLE_Y", addAngleY);
            live2DModel.addToParamFloat("PARAM_BODY_ANGLE_X", addBodyAngleX);

            if (mouthOpenY != 0f)
            {
                live2DModel.setParamFloat("PARAM_MOUTH_OPEN_Y", mouthOpenY);
            }
            if (addEyeX != 0f)
            {
                live2DModel.addToParamFloat("PARAM_EYE_BALL_X", addEyeX);
            }
            if (absoluteEyeX != 0f)
            {
                live2DModel.setParamFloat("PARAM_EYE_BALL_X", absoluteEyeX);
            }

            live2DModel.update();
            live2DModel.loadParam();
        }

        protected override void OnDestroy()
        {
            live2DModel?.releaseModel();
            AllActors.Remove(this);
            base.OnDestroy();
        }

        public void StartMotion(string motionName, bool loop = false)
        {
            if (!FixMotionName(ref motionName)) return;

            var motion = motions[motionName];
            motion.setLoop(loop);
            nextMotion = motion;
            currentMotionName = motionName;
        }

        public void StartFaceMotion(string motionName)
        {
            if (!FixMotionName(ref motionName)) return;

            var motion = motions[motionName];
            faceMotionMgr.startMotion(motion);
            currentFaceMotionName = motionName;
        }

        public void SetLip(float volume, List<Live2DActorEntity> traversalBuffer = null)
        {
            mouthOpenY = volume;

            traversalBuffer ??= new List<Live2DActorEntity>();

            traversalBuffer.Add(this);
            
            foreach (var modelController in mouthSynced.Where(e => !traversalBuffer.Contains(e)))
            {
                modelController.SetLip(volume, traversalBuffer);
            }
        }

        public void AddMouthSync(Live2DActorEntity model)
        {
            if (model == this) return;
            if (mouthSynced.Contains(model)) return;
            mouthSynced.Add(model);
            model.AddMouthSync(this);
        }

        public void RemoveAllMouthSync(List<Live2DActorEntity> traversalBuffer = null)
        {
            traversalBuffer ??= new List<Live2DActorEntity>();

            traversalBuffer.Add(this);

            foreach (var modelController in mouthSynced.Where(e => !traversalBuffer.Contains(e)))
            {
                modelController.RemoveAllMouthSync(traversalBuffer);
            }

            mouthSynced.RemoveAll(x => true);
        }

        public void SetEye(string motion)
        {
            if (motion == "閉じ")
            {
                UseEyeBlink = false;
                ManualEyeOpen = false;
            }
            else if (motion == "開き")
            {
                UseEyeBlink = false;
                ManualEyeOpen = true;
            }
            else
            {
                UseEyeBlink = true;
                ManualEyeOpen = false;
            }
        }

        public void Render()
        {
            // skip live2d rendering in batch mode
            // if (BatchMode) return;

            RenderTexture active = RenderTexture.active;
            RenderTexture.active = targetTexture;

            GL.Clear(true, true, Color.clear);

            if (isActiveAndEnabled && !Hidden)
            {
                if (live2DModel == null) return;
                if (live2DModel.getRenderMode() == live2d.Live2D.L2D_RENDER_DRAW_MESH_NOW)
                {
                    GL.PushMatrix();
                    GL.LoadIdentity();
                    GL.LoadProjectionMatrix(live2DCanvasPos);

                    // this will generate one and only one error but it is absolutely okay
                    // the cause is that in the draw() call, live2d SDK checks if the stacktrace string
                    // contains "OnPostRender()" while in the extracted stacktrace the function string
                    // is actually "OnPostRender ()" instead
                    // other than an error log, the SDK does absolutely nothing so it's benign
                    // on iOS platform it doesn't generate the error message
                    live2DModel.draw();

                    GL.PopMatrix();
                }
            }
            
            RenderTexture.active = active;
        }

        private void CreateWithAssets()
        {
            live2DModel = Live2DModelUnity.loadModel(Assets.mocFile.bytes);
            live2DModel.setRenderMode(live2d.Live2D.L2D_RENDER_DRAW_MESH_NOW);

            for (int i = 0; i < Assets.textures.Length; i++)
            {
                if (Assets.textures[i] == null) continue;
                live2DModel.setTexture(i, Assets.textures[i]);
            }

            foreach (var motionFile in Assets.motionFiles)
            {
                if (motionFile.files.Count < 1) continue;
                // even if the json definition allows multiple files, we only load the first one
                var loadedMotion = Live2DMotion.loadMotion(motionFile.files[0].bytes);
                motions[motionFile.name] = loadedMotion;
                if (motionFile.name == "mtn_idle") idleMotion = loadedMotion;
            }

            if (Assets.poseFile != null)
            {
                pose = L2DPose.load(Assets.poseFile.text);
            }

            live2DModel.update();

            gameObject.name = Assets.modelName;
        }

        public static void ReorderLayers()
        {
            if (AllActors == null) return;

            AllActors.Sort((a, b) => a.layer.CompareTo(b.layer));
            for (int i = 0; i < AllActors.Count; i++)
            {
                AllActors[i].transform.SetSiblingIndex(i);
            }
        }

        public override Color GetColor()
        {
            return rawImage.color;
        }

        public override void SetColor(Color color)
        {
            rawImage.color = color;
        }

        public override float ScaleScalar => 1.0f;

        protected override Vector3 TransformVector(Vector3 vec)
        {
            Vector3 result = new Vector3(vec.x * ScreenWidthScalar, vec.y, vec.z);
            return result;
        }

        protected override Vector3 UntransformVector(Vector3 vec)
        {
            Vector3 result = new Vector3(vec.x / ScreenWidthScalar, vec.y, vec.z);
            return result;
        }

        private bool FixMotionName(ref string motionName)
        {
            if (motions.ContainsKey(motionName)) return true;

            var substituteMotionName = CommonUtils.FindClosestMatch(motionName, motions.Keys, out var distance);
            var acceptable = distance <= 5;

            if (acceptable)
            {
                Debug.LogWarning(
                    $"Motion '{motionName}' doesn't exist in model '{name}', using '{substituteMotionName}' instead. Distance is {distance}.");
                motionName = substituteMotionName;
            }
            else
            {
                Debug.LogError($"Motion '{motionName}' doesn't exist in model '{name}' and no acceptable substitution is found.");
            }

            return acceptable;
        }

        public ActorState GetState()
        {
            var t = transform;

            return new()
            {
                name = gameObject.name,
                currentMotion = currentMotionName,
                currentFaceMotion = currentFaceMotionName,

                layer = layer,

                hidden = Hidden,

                eyeBlink = UseEyeBlink,
                manualEyeOpen = ManualEyeOpen,

                mouthSynced = mouthSynced.Select(x => x.gameObject.name).ToList(),

                faceAngle = new Vector2(addAngleX, addAngleY),
                bodyAngle = addBodyAngleX,

                addEye = addEyeX,

                transform = GetTransformState()
            };
        }

        public void RestoreState(ActorState state)
        {
            if (gameObject.name != state.name)
            {
                Debug.LogError("Applying state to wrong model!");
                return;
            }

            layer = state.layer;

            Hidden = state.hidden;

            UseEyeBlink = state.eyeBlink;
            ManualEyeOpen = state.manualEyeOpen;

            addAngleX = state.faceAngle.x;
            addAngleY = state.faceAngle.y;
            addBodyAngleX = state.bodyAngle;

            addEyeX = state.addEye;

            StartMotion(state.currentMotion);
            StartFaceMotion(state.currentFaceMotion);

            foreach (var model in state.mouthSynced)
            {
                var targetController = AllActors.Find(a => a.gameObject.name == model);
                if (targetController != null)
                    AddMouthSync(targetController);
                else
                    Debug.LogError($"Cannot find model '{model}' to sync mouth with.");
            }

            RestoreTransformState(state.transform);
        }
    }
}