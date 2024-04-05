using System;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Scenario.Framework.Live2D;
using Kaede2.Scenario.Framework.Utils;
using live2d;
using live2d.framework;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.Scenario.Framework.Entities
{
    public partial class Live2DActorEntity : Entity, IStateSavable<ActorState>
    {
        public Live2DAssets Assets { get; private set; }

        private bool hidden;
        public bool Hidden
        {
            get => hidden;
            set
            {
                if (value == hidden) return;
                hidden = value;
                if (hidden) Render(); // force hide
            }
        }
        public bool UseEyeBlink { get; set; }
        public bool ManualEyeOpen { get; set; }

        public float MouthOpenY { get; set; }
        public float AddFaceAngleX { get; set; }
        public float AddFaceAngleY { get; set; }
        public float AddBodyAngleX { get; set; }
        public float AddEyeX { get; set; }
        public float AbsoluteEyeX { get; set; }

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
        private bool currentMotionLoop;

        private RenderTexture targetTexture;
        private RawImage rawImage;
        private RectTransform rectTransform;
        private Matrix4x4 live2DCanvasPos;

        private int layer;
        private List<Live2DActorEntity> mouthSynced;

        public static readonly List<Live2DActorEntity> AllActors = new();

        public Dictionary<string, Live2DMotion>.KeyCollection MotionNames => motions.Keys;

        protected override void Awake()
        {
            base.Awake();

            Assets = null;

            Hidden = true;
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
            currentMotionLoop = false;

            targetTexture = null;
            rawImage = null;
            rectTransform = null;
            live2DCanvasPos = Matrix4x4.identity;

            mouthSynced = new();

            AllActors.Add(this);

            motionMgr = new();
            faceMotionMgr = new();
            eyeBlink = new();
        }

        private void Start()
        {
            const float canvasScale = 2.0f;
            const float scale = 1.05f;
            float modelWidth = live2DModel.getCanvasWidth();
            float percentage = (canvasScale / scale - 1.0f) / 2.0f;
            float lowBound = modelWidth * (-percentage);
            float highBound = modelWidth * (1 + percentage);
            live2DCanvasPos = Matrix4x4.Ortho(lowBound, highBound, highBound, lowBound, -50.0f, 50.0f);

            int size = (int)(modelWidth * canvasScale);
            targetTexture = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 8);
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

            if (string.IsNullOrEmpty(currentMotionName))
                StartMotion("mtn_idle00");
            if (string.IsNullOrEmpty(currentFaceMotionName))
                StartFaceMotion("face_idle01");
        }

        private void Update()
        {
            if (live2DModel == null) return;
            if (ScenarioRunMode.Args.BatchMode) return;

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
            live2DModel.addToParamFloat("PARAM_ANGLE_X", AddFaceAngleX);
            live2DModel.addToParamFloat("PARAM_ANGLE_Y", AddFaceAngleY);
            live2DModel.addToParamFloat("PARAM_BODY_ANGLE_X", AddBodyAngleX);

            if (MouthOpenY != 0f)
            {
                live2DModel.setParamFloat("PARAM_MOUTH_OPEN_Y", MouthOpenY);
            }
            if (AddEyeX != 0f)
            {
                live2DModel.addToParamFloat("PARAM_EYE_BALL_X", AddEyeX);
            }
            if (AbsoluteEyeX != 0f)
            {
                live2DModel.setParamFloat("PARAM_EYE_BALL_X", AbsoluteEyeX);
            }

            live2DModel.update();
            live2DModel.loadParam();
        }

        protected override void OnDestroy()
        {
            live2DModel?.releaseModel();
            AllActors.Remove(this);
            RenderTexture.ReleaseTemporary(targetTexture);
            base.OnDestroy();
        }

        public override Color GetColor()
        {
            return rawImage.color;
        }

        public override void SetColor(Color color)
        {
            rawImage.color = color;
        }

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
                objectName = gameObject.name,
                modelName = Assets.modelName,
                currentMotion = currentMotionName,
                currentFaceMotion = currentFaceMotionName,
                currentMotionLoop = currentMotionLoop,

                layer = layer,

                hidden = Hidden,

                eyeBlink = UseEyeBlink,
                manualEyeOpen = ManualEyeOpen,

                mouthSynced = mouthSynced.Select(x => x.gameObject.name).ToList(),

                faceAngle = new Vector2(AddFaceAngleX, AddFaceAngleY),
                bodyAngle = AddBodyAngleX,

                addEye = AddEyeX,
                absoluteEye = AbsoluteEyeX,

                transform = GetTransformState()
            };
        }

        public void RestoreState(ActorState state)
        {
            if (gameObject.name != state.objectName)
            {
                Debug.LogError("Applying state to wrong model!");
                return;
            }

            if (Assets.modelName != state.modelName)
            {
                Debug.LogError("Applying state to wrong model!");
                return;
            }

            layer = state.layer;

            Hidden = state.hidden;

            UseEyeBlink = state.eyeBlink;
            ManualEyeOpen = state.manualEyeOpen;

            AddFaceAngleX = state.faceAngle.x;
            AddFaceAngleY = state.faceAngle.y;
            AddBodyAngleX = state.bodyAngle;

            AddEyeX = state.addEye;
            AbsoluteEyeX = state.absoluteEye;

            StartMotion(state.currentMotion, state.currentMotionLoop);
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