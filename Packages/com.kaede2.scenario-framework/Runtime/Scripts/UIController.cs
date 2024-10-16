﻿using Kaede2.Scenario.Framework.Entities;
using Kaede2.Scenario.Framework.Live2D;
using Kaede2.Scenario.Framework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.Scenario.Framework
{
    public abstract class UIController : MonoBehaviour
    {
        public ScenarioModule Module { get; set; }

        private GameObject emptyUIObjectPrefab;
        private GameObject backgroundPrefab;

        public abstract Canvas UICanvas { get; }
        public abstract Canvas ContentCanvas { get; }
        public abstract Canvas LoadingCanvas { get; }
        public abstract FillerImageController Filler { get; }

        public abstract Canvas BackgroundCanvas { get; }
        public abstract Canvas Live2DCanvas { get; }
        public abstract Canvas StillCanvas { get; }
        public abstract Canvas SpriteCanvas { get; }

        public abstract CaptionBox CaptionBox { get; }
        public abstract MessageBox MessageBox { get; }

        public abstract FadeTransition Fade { get; }

        public Color CaptionDefaultColor { get; set; }

        private RectTransform live2DCanvasRectTransform;
        private RectTransform backgroundCanvasRectTransform;

        protected virtual void Awake()
        {
            CaptionDefaultColor = Color.white;

            live2DCanvasRectTransform = Live2DCanvas.GetComponent<RectTransform>();
            backgroundCanvasRectTransform = BackgroundCanvas.GetComponent<RectTransform>();

            CameraPos = CameraPosDefault;
            CameraScale = CameraScaleDefault;

            // on awake, there are some things that need to be disabled
            CaptionBox.gameObject.SetActive(false);
            UICanvas.gameObject.SetActive(false);
            ContentCanvas.gameObject.SetActive(false);
            MessageBox.Enabled = false;

            MessageBox.UIController = this;
            Fade.UIController = this;
            Filler.UIController = this;

            emptyUIObjectPrefab = Resources.Load<GameObject>("Prefabs/Scenario/EmptyUIObject");
            backgroundPrefab = Resources.Load<GameObject>("Prefabs/Scenario/Background");
        }

        public Vector2 CameraPos
        {
            get => live2DCanvasRectTransform.anchoredPosition * -1;
            set
            {
                live2DCanvasRectTransform.anchoredPosition = value * -1.0f;
                backgroundCanvasRectTransform.anchoredPosition = value * -1.0f;
            }
        }

        public static Vector2 CameraPosDefault => Vector2.zero;

        public float CameraScale
        {
            get => live2DCanvasRectTransform.localScale.x;
            set
            {
                live2DCanvasRectTransform.localScale = new Vector3(value, value, live2DCanvasRectTransform.localScale.z);
                backgroundCanvasRectTransform.localScale = new Vector3(value, value, backgroundCanvasRectTransform.localScale.z);
            }
        }

        public static float CameraScaleDefault => 1.0f;

        public bool CameraEnabled
        {
            get => Live2DCanvas.gameObject.activeSelf;
            set => ContentCanvas.gameObject.SetActive(value);
        }

        public T CreateEntity<T>(GameObject go) where T : Entity
        {
            bool activated = go.activeSelf;
            if (activated) go.SetActive(false); // we need to disable the object to prevent Awake from being called

            var entity = go.AddComponent<T>();
            entity.Module = Module;
            Module.RegisterEntity(entity);

            if (activated) go.SetActive(true);
            return entity;
        }

        public BackgroundEntity CreateBackground(string objectName, string resourceName, Texture2D texture)
        {
            return CreateBackgroundObjectWithParent(BackgroundCanvas.transform, objectName, resourceName, texture);
        }

        public BackgroundEntity CreateStill(string objectName, string resourceName, Texture2D texture)
        {
            return CreateBackgroundObjectWithParent(StillCanvas.transform, objectName, resourceName, texture);
        }

        public Live2DActorEntity CreateActor(Vector2 position, string readableName, Live2DAssets asset)
        {
            var newModel = CreateEmptyUIObject(Live2DCanvas.transform);
            newModel.GetComponent<RectTransform>().anchoredPosition = position;
            newModel.name = readableName;
            var entity = CreateEntity<Live2DActorEntity>(newModel);
            entity.CreateWithAssets(asset);
            entity.Hidden = true;
            return entity;
        }

        public SpriteEntity CreateSprite(string objectName, string resourceName, Sprite sprite)
        {
            var newSprite = CreateEmptyUIObject(SpriteCanvas.transform);
            newSprite.name = objectName;
            var image = newSprite.AddComponent<Image>();
            image.sprite = sprite;
            var entity = CreateEntity<SpriteEntity>(newSprite);
            entity.resourceName = resourceName;
            entity.SetColor(new(1, 1, 1, 0));
            var rectTransform = newSprite.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
            rectTransform.pivot = Vector2.zero;
            rectTransform.anchoredPosition3D = Vector3.zero;
            return entity;
        }

        private BackgroundEntity CreateBackgroundObjectWithParent(Transform parent, string objectName, string resourceName, Texture2D texture)
        {
            var newObj = Instantiate(backgroundPrefab, parent, false);
            var imgObj = newObj.transform.Find("Mask/Image").gameObject;
            newObj.SetActive(false);
            var entity = CreateEntity<BackgroundEntity>(newObj);
            newObj.name = objectName;
            entity.resourceName = resourceName;
            entity.image = imgObj.GetComponent<RawImage>();
            entity.Canvas = ContentCanvas.transform as RectTransform;
            entity.SetImage(texture);
            newObj.SetActive(true);
            return entity;
        }

        private GameObject CreateEmptyUIObject(Transform parent)
        {
            return Instantiate(emptyUIObjectPrefab, parent, false);
        }
    }
}