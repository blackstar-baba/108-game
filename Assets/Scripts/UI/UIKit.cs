using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Liangshan
{
    public static class UIKit
    {
        public static readonly Color Ink = GameConfig.Hex("141516");
        public static readonly Color InkSoft = GameConfig.Hex("1D2021");
        public static readonly Color Paper = GameConfig.Hex("EFE3CE");
        public static readonly Color PaperDim = GameConfig.Hex("B9AD97");
        public static readonly Color Gold = GameConfig.Hex("C9A45C");
        public static readonly Color Vermilion = GameConfig.Hex("A8322B");
        public static readonly Color Jade = GameConfig.Hex("4F8FA0");
        public static readonly Color Night = GameConfig.Hex("101214");

        public static Canvas CreateCanvas(Transform parent)
        {
            var go = new GameObject("UI Canvas");
            go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            EnsureEventSystem(parent);
            return canvas;
        }

        static void EnsureEventSystem(Transform parent)
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.transform.SetParent(parent, false);
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        public static RectTransform Rect(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            return rt;
        }

        public static GameObject Empty(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = Rect(go);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            return go;
        }

        public static RectTransform Full(Transform parent, string name)
        {
            var go = Empty(parent, name);
            var rt = Rect(go);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image Panel(Transform parent, string name, Vector2 size, Vector2 position, Color color,
            float radius = 14f, bool raycast = false)
        {
            var go = Empty(parent, name);
            var rt = Rect(go);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            var image = go.AddComponent<Image>();
            image.sprite = radius > 10f ? RuntimeAssets.RoundedSprite : RuntimeAssets.RoundedThinSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        public static Image PanelAnchored(Transform parent, string name, Vector2 size, Vector2 anchor, Vector2 pivot,
            Vector2 position, Color color, float radius = 14f, bool raycast = false)
        {
            var image = Panel(parent, name, size, position, color, radius, raycast);
            var rt = image.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            return image;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color,
            TextAnchor alignment = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal, float width = 400f, float height = 60f)
        {
            var go = Empty(parent, name);
            var rt = Rect(go);
            rt.sizeDelta = new Vector2(width, height);
            var label = go.AddComponent<Text>();
            label.font = size >= 44 ? RuntimeAssets.TitleFont : RuntimeAssets.BodyFont;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = color;
            label.alignment = alignment;
            label.text = text;
            label.raycastTarget = false;
            label.supportRichText = true;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.lineSpacing = 1.08f;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(1.2f, -1.4f);
            return label;
        }

        public static Text LabelAnchored(Transform parent, string name, string text, int size, Color color,
            Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 dimensions,
            TextAnchor alignment = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var label = Label(parent, name, text, size, color, alignment, style, dimensions.x, dimensions.y);
            var rt = label.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = dimensions;
            return label;
        }

        public static Button Button(Transform parent, string name, string text, Vector2 size, Vector2 position,
            Color background, Color textColor, Action onClick, int fontSize = 24, bool outline = false)
        {
            var image = Panel(parent, name, size, position, background, 12f, true);
            if (outline)
            {
                var glow = image.gameObject.AddComponent<Outline>();
                glow.effectColor = Gold;
                glow.effectDistance = new Vector2(1.6f, -1.6f);
                glow.useGraphicAlpha = true;
            }
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.06f, 1.04f, 0.96f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.8f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.12f;
            button.colors = colors;
            if (onClick != null)
                button.onClick.AddListener(() => onClick());
            var label = Label(image.transform, "label", text, fontSize, textColor, TextAnchor.MiddleCenter, FontStyle.Bold, size.x, size.y);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            var hover = image.gameObject.AddComponent<HoverLift>();
            hover.Target = image.rectTransform;
            return button;
        }

        public static Image Divider(Transform parent, Vector2 size, Vector2 position, Color color)
        {
            return Panel(parent, "divider", size, position, color, 2f);
        }

        public static Image Seal(Transform parent, string text, Vector2 size, Vector2 position, int fontSize = 22)
        {
            var go = Empty(parent, "seal");
            var rt = Rect(go);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            var image = go.AddComponent<Image>();
            image.sprite = RuntimeAssets.SealSprite;
            image.color = Vermilion;
            image.raycastTarget = false;
            var label = Label(go.transform, "seal text", text, fontSize, new Color(1f, 0.92f, 0.82f), TextAnchor.MiddleCenter, FontStyle.Bold, size.x, size.y);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(4f, 4f);
            label.rectTransform.offsetMax = new Vector2(-4f, -4f);
            return image;
        }

        public static Image Vignette(Transform parent, float alpha)
        {
            var rt = Full(parent, "vignette");
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = RuntimeAssets.SpriteFrom(RuntimeAssets.VignetteTexture);
            image.color = new Color(0f, 0f, 0f, alpha);
            image.raycastTarget = false;
            return image;
        }

        public static Image Brush(Transform parent, Vector2 size, Vector2 position, Color color, float rotation = 0f)
        {
            var image = Panel(parent, "brush", size, position, color, 2f);
            image.sprite = RuntimeAssets.BrushSprite;
            image.type = Image.Type.Simple;
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            return image;
        }

        public static void SetRect(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        public static string Stars(int count)
        {
            return new string('★', Mathf.Clamp(count, 0, 5));
        }
    }

    public sealed class Bar
    {
        public Image Background;
        public Image Fill;
        public Image Glow;
        public Text Label;

        public void Set(float value01)
        {
            value01 = Mathf.Clamp01(value01);
            if (Fill != null) Fill.fillAmount = value01;
            if (Glow != null) Glow.fillAmount = Mathf.Clamp01(value01 + 0.06f);
        }

        public static Bar Create(Transform parent, Vector2 size, Vector2 position, Color background, Color fillColor,
            bool showLabel = false, string text = "", int fontSize = 14)
        {
            var bar = new Bar();
            bar.Background = UIKit.Panel(parent, "bar bg", size, position, background, 6f);
            var fill = UIKit.Panel(bar.Background.transform, "fill", size - new Vector2(4f, 4f), Vector2.zero, fillColor, 5f);
            fill.rectTransform.anchorMin = new Vector2(0f, 0f);
            fill.rectTransform.anchorMax = new Vector2(1f, 1f);
            fill.rectTransform.offsetMin = new Vector2(2f, 2f);
            fill.rectTransform.offsetMax = new Vector2(-2f, -2f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 1f;
            fill.raycastTarget = false;
            bar.Fill = fill;
            var glow = UIKit.Panel(fill.transform, "glow", Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.18f), 3f);
            glow.rectTransform.anchorMin = Vector2.zero;
            glow.rectTransform.anchorMax = Vector2.one;
            glow.rectTransform.offsetMin = Vector2.zero;
            glow.rectTransform.offsetMax = Vector2.zero;
            glow.type = Image.Type.Filled;
            glow.fillMethod = Image.FillMethod.Horizontal;
            glow.fillAmount = 1f;
            bar.Glow = glow;
            if (showLabel)
            {
                bar.Label = UIKit.Label(bar.Background.transform, "bar label", text, fontSize, UIKit.Paper, TextAnchor.MiddleCenter, FontStyle.Normal, size.x, size.y);
                bar.Label.rectTransform.anchorMin = Vector2.zero;
                bar.Label.rectTransform.anchorMax = Vector2.one;
                bar.Label.rectTransform.offsetMin = Vector2.zero;
                bar.Label.rectTransform.offsetMax = Vector2.zero;
            }
            return bar;
        }
    }

    public sealed class HoverLift : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform Target;
        public float Amount = 1.035f;
        public float Lift = 6f;
        Vector3 _baseScale = Vector3.one;
        Vector2 _basePosition;
        bool _hover;
        bool _ready;

        void Start() { Capture(); }

        void Capture()
        {
            if (Target == null) Target = transform as RectTransform;
            if (Target == null) return;
            _baseScale = Target.localScale;
            _basePosition = Target.anchoredPosition;
            _ready = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_ready) Capture();
            _hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
        }

        void Update()
        {
            if (!_ready || Target == null) return;
            Vector3 targetScale = _hover ? _baseScale * Amount : _baseScale;
            Vector2 targetPos = _hover ? _basePosition + Vector2.up * Lift : _basePosition;
            float k = 1f - Mathf.Exp(-14f * Time.unscaledDeltaTime);
            Target.localScale = Vector3.Lerp(Target.localScale, targetScale, k);
            Target.anchoredPosition = Vector2.Lerp(Target.anchoredPosition, targetPos, k);
        }
    }

    public sealed class UIFader : MonoBehaviour
    {
        public CanvasGroup Group;
        public float Duration = 0.22f;
        float _target = 1f;

        public void Show(bool show)
        {
            _target = show ? 1f : 0f;
            gameObject.SetActive(true);
            if (Group == null) Group = GetComponent<CanvasGroup>();
            if (Group == null) Group = gameObject.AddComponent<CanvasGroup>();
            Group.blocksRaycasts = show;
            Group.interactable = show;
        }

        void Update()
        {
            if (Group == null) return;
            Group.alpha = Mathf.MoveTowards(Group.alpha, _target, Time.unscaledDeltaTime / Mathf.Max(0.01f, Duration));
            if (Group.alpha <= 0.001f && _target <= 0f)
                gameObject.SetActive(false);
        }
    }

    public sealed class Spin : MonoBehaviour
    {
        public float Speed = 8f;
        void Update() { transform.Rotate(0f, 0f, Speed * Time.unscaledDeltaTime); }
    }
}
