using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Liangshan
{
    public sealed class BoonView : MonoBehaviour
    {
        Game _game;
        GameObject _root;
        CanvasGroup _group;
        Transform _cardsRoot;
        Text _header;
        Action<int> _callback;
        readonly List<GameObject> _cards = new List<GameObject>();

        public void Initialize(Game game, Transform parent)
        {
            _game = game;
            var full = UIKit.Full(parent, "Boon Screen");
            _root = full.gameObject;
            _group = _root.AddComponent<CanvasGroup>();
            _root.SetActive(false);
            var dim = full.gameObject.AddComponent<Image>();
            dim.color = new Color(0.012f, 0.016f, 0.018f, 0.82f);
            dim.raycastTarget = true;

            var panel = UIKit.Panel(full, "boon panel", new Vector2(1500f, 820f), Vector2.zero, new Color(0.045f, 0.052f, 0.052f, 0.95f), 12f, true);
            UIKit.Brush(panel.transform, new Vector2(900f, 140f), new Vector2(0f, 316f), new Color(0.7f, 0.62f, 0.48f, 0.10f), -1f);
            _header = UIKit.Label(panel.transform, "header", "天 降 机 缘", 62, UIKit.Paper, TextAnchor.MiddleCenter, FontStyle.Bold, 900f, 80f);
            UIKit.SetRect(_header.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(900f, 80f));
            var sub = UIKit.Label(panel.transform, "sub", "择一而取  ·  机缘只此一次", 20, UIKit.Gold, TextAnchor.MiddleCenter, FontStyle.Normal, 700f, 28f);
            UIKit.SetRect(sub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -116f), new Vector2(700f, 28f));
            UIKit.Divider(panel.transform, new Vector2(620f, 1f), new Vector2(0f, 258f), new Color(0.5f, 0.42f, 0.3f, 0.6f));

            _cardsRoot = UIKit.Empty(panel.transform, "cards").transform;
            var cardsRect = _cardsRoot.GetComponent<RectTransform>();
            UIKit.SetRect(cardsRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 112f), new Vector2(1300f, 500f));
            var hint = UIKit.Label(panel.transform, "hint", "按  1 / 2 / 3  选择    ·    鼠标点击亦可", 15, new Color(0.62f, 0.58f, 0.5f), TextAnchor.MiddleCenter, FontStyle.Normal, 800f, 24f);
            UIKit.SetRect(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 46f), new Vector2(800f, 24f));
        }

        public void Show(List<BoonDefinition> boons, Action<int> callback)
        {
            _callback = callback;
            foreach (var card in _cards) Destroy(card);
            _cards.Clear();
            _root.SetActive(true);
            _group.alpha = 0f;
            _group.blocksRaycasts = true;
            _group.interactable = true;
            for (int i = 0; i < boons.Count; i++)
                BuildCard(boons[i], i);
            StartCoroutine(FadeIn());
        }

        IEnumerator FadeIn()
        {
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Clamp01(t / 0.25f);
                yield return null;
            }
            _group.alpha = 1f;
        }

        void BuildCard(BoonDefinition boon, int index)
        {
            var go = UIKit.Empty(_cardsRoot, "boon " + boon.Name);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(390f, 440f);
            var x = (index - 1) * 420f;
            UIKit.SetRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(390f, 440f));

            var border = UIKit.Panel(go.transform, "border", new Vector2(394f, 444f), Vector2.zero, new Color(boon.Accent.r, boon.Accent.g, boon.Accent.b, 0.5f), 10f);
            var bg = UIKit.Panel(go.transform, "bg", new Vector2(388f, 438f), Vector2.zero, new Color(0.06f, 0.068f, 0.068f, 0.98f), 8f, true);
            var button = bg.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => Choose(index));
            var hover = bg.gameObject.AddComponent<HoverLift>();
            hover.Target = rt;
            hover.Amount = 1.025f;
            hover.Lift = 10f;

            var accent = UIKit.Divider(go.transform, new Vector2(70f, 4f), Vector2.zero, boon.Accent);
            UIKit.SetRect(accent.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(70f, 4f));
            var seal = UIKit.Seal(go.transform, GameConfig.ChineseNumber(index + 1), new Vector2(58f, 58f), Vector2.zero, 26);
            UIKit.SetRect(seal.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(58f, 58f));
            seal.color = boon.Accent;

            var title = UIKit.Label(go.transform, "title", boon.Title, 14, new Color(0.65f, 0.6f, 0.5f), TextAnchor.MiddleCenter, FontStyle.Normal, 320f, 22f);
            UIKit.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -116f), new Vector2(320f, 22f));
            var name = UIKit.Label(go.transform, "name", boon.Name, 36, UIKit.Paper, TextAnchor.MiddleCenter, FontStyle.Bold, 340f, 54f);
            UIKit.SetRect(name.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(340f, 54f));
            var desc = UIKit.Label(go.transform, "desc", boon.Description, 20, boon.Accent, TextAnchor.UpperCenter, FontStyle.Normal, 320f, 100f);
            UIKit.SetRect(desc.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -214f), new Vector2(320f, 100f));
            var flavor = UIKit.Label(go.transform, "flavor", "「" + boon.Flavor + "」", 16, new Color(0.56f, 0.61f, 0.57f), TextAnchor.UpperCenter, FontStyle.Italic, 310f, 80f);
            UIKit.SetRect(flavor.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -316f), new Vector2(310f, 80f));
            var key = UIKit.Label(go.transform, "key", "按  " + (index + 1), 15, new Color(0.48f, 0.45f, 0.4f), TextAnchor.MiddleCenter, FontStyle.Normal, 200f, 24f);
            UIKit.SetRect(key.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(200f, 24f));

            _cards.Add(go);
            go.transform.localScale = Vector3.one * 0.94f;
            StartCoroutine(Pop(go.transform, index * 0.07f));
        }

        IEnumerator Pop(Transform target, float delay)
        {
            float t = 0f;
            while (t < 0.32f + delay)
            {
                t += Time.unscaledDeltaTime;
                if (t < delay) { yield return null; continue; }
                float u = Mathf.Clamp01((t - delay) / 0.32f);
                float ease = SimpleTween.EaseOutBack(u);
                target.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, ease);
                yield return null;
            }
            target.localScale = Vector3.one;
        }

        void Choose(int index)
        {
            if (_callback == null) return;
            var callback = _callback;
            _callback = null;
            Hide();
            callback(index);
        }

        public void Hide()
        {
            _group.blocksRaycasts = false;
            _group.interactable = false;
            _root.SetActive(false);
        }
    }

    public sealed class PauseView : MonoBehaviour
    {
        Game _game;
        GameObject _root;
        CanvasGroup _group;
        Slider _slider;

        public void Initialize(Game game, Transform parent)
        {
            _game = game;
            var full = UIKit.Full(parent, "Pause Screen");
            _root = full.gameObject;
            _group = _root.AddComponent<CanvasGroup>();
            _root.SetActive(false);
            var dim = full.gameObject.AddComponent<Image>();
            dim.color = new Color(0.01f, 0.015f, 0.02f, 0.78f);
            dim.raycastTarget = true;

            var panel = UIKit.Panel(full, "pause panel", new Vector2(640f, 620f), Vector2.zero, new Color(0.045f, 0.052f, 0.052f, 0.97f), 10f, true);
            UIKit.Brush(panel.transform, new Vector2(540f, 120f), new Vector2(0f, 250f), new Color(0.7f, 0.62f, 0.48f, 0.08f), 1f);
            var title = UIKit.Label(panel.transform, "title", "暂 歇", 54, UIKit.Paper, TextAnchor.MiddleCenter, FontStyle.Bold, 500f, 70f);
            UIKit.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(500f, 70f));
            var sub = UIKit.Label(panel.transform, "sub", "风雪未停，好汉且饮一碗", 17, UIKit.Gold, TextAnchor.MiddleCenter, FontStyle.Normal, 500f, 26f);
            UIKit.SetRect(sub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -108f), new Vector2(500f, 26f));

            float y = 178f;
            UIKit.Button(panel.transform, "resume", "继 续 战 斗", new Vector2(430f, 62f), new Vector2(0f, y), UIKit.Vermilion,
                new Color(1f, 0.93f, 0.82f), () => _game.TogglePause(), 23, true);
            UIKit.Button(panel.transform, "restart", "重 整 旗 鼓", new Vector2(430f, 56f), new Vector2(0f, y - 82f),
                new Color(0.1f, 0.115f, 0.115f, 0.95f), UIKit.Paper, () => _game.RestartRun(), 21);
            UIKit.Button(panel.transform, "menu", "返 回 梁 山", new Vector2(430f, 56f), new Vector2(0f, y - 150f),
                new Color(0.1f, 0.115f, 0.115f, 0.95f), UIKit.Paper, () => _game.QuitToMenu(), 21);
            UIKit.Button(panel.transform, "quit", "离 开 游 戏", new Vector2(430f, 50f), new Vector2(0f, y - 216f),
                new Color(0.08f, 0.09f, 0.09f, 0.9f), new Color(0.62f, 0.55f, 0.46f), () => Application.Quit(), 18);

            var sens = UIKit.Label(panel.transform, "sens label", "鼠标灵敏度", 16, new Color(0.7f, 0.66f, 0.58f), TextAnchor.MiddleLeft, FontStyle.Normal, 220f, 26f);
            UIKit.SetRect(sens.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(106f, 82f), new Vector2(220f, 26f));
            _slider = BuildSlider(panel.transform, new Vector2(0f, 0f), new Vector2(106f, 48f), 430f);
            var controls = UIKit.Label(panel.transform, "controls", "WASD 移动  ·  鼠标 视角  ·  左键 连击  ·  右键 重击\nSpace 闪避  ·  E 技能  ·  R 怒气", 14,
                new Color(0.52f, 0.5f, 0.44f), TextAnchor.MiddleLeft, FontStyle.Normal, 500f, 50f);
            UIKit.SetRect(controls.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(500f, 50f));
        }

        Slider BuildSlider(Transform parent, Vector2 anchor, Vector2 position, float width)
        {
            var go = UIKit.Empty(parent, "sensitivity");
            var rt = go.GetComponent<RectTransform>();
            UIKit.SetRect(rt, anchor, new Vector2(0f, 0.5f), position, new Vector2(width, 24f));
            var bg = UIKit.Panel(go.transform, "bg", new Vector2(width, 6f), Vector2.zero, new Color(0.18f, 0.18f, 0.16f), 3f);
            var fillArea = UIKit.Empty(go.transform, "fill area");
            var fillRect = fillArea.GetComponent<RectTransform>();
            UIKit.SetRect(fillRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(width, 6f));
            var fill = UIKit.Panel(fillArea.transform, "fill", new Vector2(width, 6f), Vector2.zero, UIKit.Gold, 3f);
            var handle = UIKit.Panel(go.transform, "handle", new Vector2(22f, 22f), Vector2.zero, UIKit.Paper, 11f);
            handle.sprite = RuntimeAssets.CircleSprite;
            handle.type = Image.Type.Simple;
            var slider = go.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0.35f;
            slider.maxValue = 2.2f;
            slider.value = InputReader.MouseSensitivity;
            slider.onValueChanged.AddListener(v =>
            {
                InputReader.MouseSensitivity = v;
                PlayerPrefs.SetFloat("Liangshan.Sensitivity", v);
                PlayerPrefs.Save();
            });
            return slider;
        }

        public void Show()
        {
            _root.SetActive(true);
            _group.alpha = 0f;
            _group.blocksRaycasts = true;
            _group.interactable = true;
            if (_slider != null) _slider.value = InputReader.MouseSensitivity;
            StartCoroutine(FadeIn());
        }

        IEnumerator FadeIn()
        {
            float t = 0f;
            while (t < 0.18f)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Clamp01(t / 0.18f);
                yield return null;
            }
            _group.alpha = 1f;
        }

        public void Hide()
        {
            _group.blocksRaycasts = false;
            _group.interactable = false;
            _root.SetActive(false);
        }
    }

    public sealed class EndView : MonoBehaviour
    {
        Game _game;
        GameObject _root;
        CanvasGroup _group;
        Text _title;
        Text _sub;
        Text _stats;
        Text _quote;
        Image _seal;
        bool _victory;

        public void Initialize(Game game, Transform parent)
        {
            _game = game;
            var full = UIKit.Full(parent, "End Screen");
            _root = full.gameObject;
            _group = _root.AddComponent<CanvasGroup>();
            _root.SetActive(false);
            var dim = full.gameObject.AddComponent<Image>();
            dim.color = new Color(0.012f, 0.015f, 0.018f, 0.86f);
            dim.raycastTarget = true;

            var panel = UIKit.Panel(full, "end panel", new Vector2(980f, 760f), Vector2.zero, new Color(0.035f, 0.042f, 0.042f, 0.96f), 8f, true);
            UIKit.Brush(panel.transform, new Vector2(840f, 170f), new Vector2(0f, 248f), new Color(0.7f, 0.62f, 0.48f, 0.09f), -2f);
            _seal = UIKit.Seal(panel.transform, "替天\n行道", new Vector2(92f, 92f), Vector2.zero, 21);
            UIKit.SetRect(_seal.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(92f, 92f));
            _title = UIKit.Label(panel.transform, "title", "替 天 行 道", 68, UIKit.Paper, TextAnchor.MiddleCenter, FontStyle.Bold, 900f, 100f);
            UIKit.SetRect(_title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -172f), new Vector2(900f, 100f));
            _sub = UIKit.Label(panel.transform, "sub", "", 22, UIKit.Gold, TextAnchor.MiddleCenter, FontStyle.Normal, 800f, 36f);
            UIKit.SetRect(_sub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -248f), new Vector2(800f, 36f));
            _quote = UIKit.Label(panel.transform, "quote", "", 19, new Color(0.6f, 0.64f, 0.6f), TextAnchor.MiddleCenter, FontStyle.Italic, 760f, 60f);
            UIKit.SetRect(_quote.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(760f, 60f));
            _stats = UIKit.Label(panel.transform, "stats", "", 24, UIKit.Paper, TextAnchor.MiddleCenter, FontStyle.Normal, 860f, 90f);
            UIKit.SetRect(_stats.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -380f), new Vector2(860f, 90f));

            UIKit.Button(panel.transform, "again", "再 战 梁 山", new Vector2(330f, 70f), new Vector2(-184f, -252f), UIKit.Vermilion,
                new Color(1f, 0.93f, 0.82f), () => _game.RestartRun(), 24, true);
            UIKit.Button(panel.transform, "menu", "返 回 主 菜 单", new Vector2(330f, 70f), new Vector2(184f, -252f),
                new Color(0.1f, 0.115f, 0.115f, 0.95f), UIKit.Paper, () => _game.QuitToMenu(), 22);
        }

        public void Show(bool victory, int score, int waves, float seconds, int kills)
        {
            _victory = victory;
            _root.SetActive(true);
            _group.alpha = 0f;
            _group.blocksRaycasts = true;
            _group.interactable = true;
            _title.text = victory ? "替 天 行 道" : "壮 志 未 酬";
            _title.color = victory ? UIKit.Paper : new Color(0.78f, 0.72f, 0.63f);
            _seal.color = victory ? UIKit.Vermilion : new Color(0.32f, 0.3f, 0.28f);
            _sub.text = victory ? "高俅伏诛，梁山旗展" : "风雪埋骨，来日再战";
            _quote.text = victory
                ? "「他日若遂凌云志，敢笑黄巢不丈夫。」"
                : "「路见不平一声吼，该出手时就出手。」";
            int s = Mathf.Max(0, Mathf.FloorToInt(seconds));
            _stats.text = "战功  " + score.ToString("0000") + "          波次  " + waves + " / 5\n击退  " + kills + " 人          用时  " + (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
            StartCoroutine(FadeIn());
        }

        IEnumerator FadeIn()
        {
            float t = 0f;
            while (t < 0.4f)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Clamp01(t / 0.4f);
                yield return null;
            }
            _group.alpha = 1f;
        }

        public void Hide()
        {
            _group.blocksRaycasts = false;
            _group.interactable = false;
            _root.SetActive(false);
        }
    }

    public sealed class CodexView : MonoBehaviour
    {
        Game _game;
        GameObject _root;
        CanvasGroup _group;
        Text _detailName;
        Text _detailStar;
        Text _detailNick;
        Text _detailNote;
        Image _detailSeal;
        Text _detailSealText;
        RectTransform _gridContent;
        ScrollRect _scroll;
        readonly List<Image> _cells = new List<Image>();
        int _selected = 5;

        public bool IsOpen => _root != null && _root.activeSelf;

        public void Initialize(Game game, Transform parent)
        {
            _game = game;
            var full = UIKit.Full(parent, "Codex");
            _root = full.gameObject;
            _group = _root.AddComponent<CanvasGroup>();
            _root.SetActive(false);
            var dim = full.gameObject.AddComponent<Image>();
            dim.color = new Color(0.012f, 0.015f, 0.018f, 0.92f);
            dim.raycastTarget = true;

            var panel = UIKit.Panel(full, "codex panel", new Vector2(1740f, 930f), Vector2.zero, new Color(0.04f, 0.046f, 0.046f, 0.98f), 8f, true);
            var title = UIKit.Label(panel.transform, "title", "一 百 单 八 将", 44, UIKit.Paper, TextAnchor.MiddleLeft, FontStyle.Bold, 600f, 56f);
            UIKit.SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(46f, -30f), new Vector2(600f, 56f));
            var sub = UIKit.Label(panel.transform, "sub", "天罡三十六  ·  地煞七十二  ·  梁山泊英雄排座次", 16, UIKit.Gold, TextAnchor.MiddleLeft, FontStyle.Normal, 800f, 26f);
            UIKit.SetRect(sub.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -82f), new Vector2(800f, 26f));
            UIKit.Button(panel.transform, "close", "关 闭  ×", new Vector2(160f, 44f), Vector2.zero,
                new Color(0.1f, 0.11f, 0.11f, 0.95f), UIKit.PaperDim, () => Hide(), 18)
                .GetComponent<RectTransform>().anchoredPosition = new Vector2(800f, 390f);
            var closeRect = panel.transform.Find("close").GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-34f, -30f);

            // Scrollable grid.
            var viewport = UIKit.Panel(panel.transform, "viewport", new Vector2(1160f, 744f), Vector2.zero, new Color(0f, 0f, 0f, 0.18f), 4f, true);
            UIKit.SetRect(viewport.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(38f, -128f), new Vector2(1160f, 744f));
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = UIKit.Empty(viewport.transform, "content");
            _gridContent = content.GetComponent<RectTransform>();
            _gridContent.anchorMin = new Vector2(0f, 1f);
            _gridContent.anchorMax = new Vector2(1f, 1f);
            _gridContent.pivot = new Vector2(0.5f, 1f);
            _gridContent.anchoredPosition = Vector2.zero;
            _gridContent.sizeDelta = new Vector2(0f, 0f);
            var grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(184f, 56f);
            grid.spacing = new Vector2(6f, 6f);
            grid.padding = new RectOffset(6, 6, 6, 6);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _scroll = viewport.gameObject.AddComponent<ScrollRect>();
            _scroll.content = _gridContent;
            _scroll.viewport = viewport.rectTransform;
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.scrollSensitivity = 36f;
            _scroll.inertia = true;

            for (int i = 0; i < Heroes108.All.Count; i++)
            {
                var entry = Heroes108.All[i];
                var cell = UIKit.Panel(content.transform, "cell " + entry.Rank, grid.cellSize, Vector2.zero,
                    new Color(0.07f, 0.08f, 0.08f, 0.96f), 3f, true);
                var button = cell.gameObject.AddComponent<Button>();
                int index = i;
                button.onClick.AddListener(() => Select(index));
                var rank = UIKit.Label(cell.transform, "rank", entry.Rank.ToString("00"), 13, entry.Heavenly ? UIKit.Gold : new Color(0.55f, 0.58f, 0.55f), TextAnchor.MiddleLeft, FontStyle.Normal, 34f, 24f);
                UIKit.SetRect(rank.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(9f, 12f), new Vector2(34f, 24f));
                var name = UIKit.Label(cell.transform, "name", entry.Name, 19, UIKit.Paper, TextAnchor.MiddleLeft, FontStyle.Bold, 78f, 26f);
                UIKit.SetRect(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(42f, 11f), new Vector2(82f, 26f));
                var nick = UIKit.Label(cell.transform, "nick", entry.Nickname, 11, new Color(0.6f, 0.58f, 0.5f), TextAnchor.MiddleLeft, FontStyle.Normal, 130f, 20f);
                UIKit.SetRect(nick.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(42f, -13f), new Vector2(132f, 20f));
                _cells.Add(cell);
            }

            // Right detail.
            var detail = UIKit.Panel(panel.transform, "detail", new Vector2(470f, 744f), Vector2.zero, new Color(0.055f, 0.062f, 0.06f, 0.95f), 4f, false);
            UIKit.SetRect(detail.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-36f, -128f), new Vector2(470f, 744f));
            _detailSeal = UIKit.Seal(detail.transform, "林", new Vector2(118f, 118f), Vector2.zero, 52);
            UIKit.SetRect(_detailSeal.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(118f, 118f));
            _detailSealText = _detailSeal.transform.Find("seal text").GetComponent<Text>();
            _detailName = UIKit.Label(detail.transform, "name", "", 56, UIKit.Paper, TextAnchor.MiddleCenter, FontStyle.Bold, 420f, 70f);
            UIKit.SetRect(_detailName.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -184f), new Vector2(420f, 70f));
            _detailStar = UIKit.Label(detail.transform, "star", "", 17, UIKit.Gold, TextAnchor.MiddleCenter, FontStyle.Normal, 420f, 28f);
            UIKit.SetRect(_detailStar.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -248f), new Vector2(420f, 28f));
            _detailNick = UIKit.Label(detail.transform, "nick", "", 30, new Color(0.76f, 0.68f, 0.54f), TextAnchor.MiddleCenter, FontStyle.Normal, 420f, 42f);
            UIKit.SetRect(_detailNick.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -286f), new Vector2(420f, 42f));
            UIKit.Divider(detail.transform, new Vector2(330f, 1f), new Vector2(0f, 252f), new Color(0.5f, 0.42f, 0.3f, 0.65f));
            _detailNote = UIKit.Label(detail.transform, "note", "", 18, new Color(0.68f, 0.66f, 0.58f), TextAnchor.UpperCenter, FontStyle.Normal, 380f, 220f);
            UIKit.SetRect(_detailNote.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -382f), new Vector2(380f, 220f));
            var hint = UIKit.Label(detail.transform, "hint", "梁山座次  ·  忠义堂石碣", 13, new Color(0.45f, 0.43f, 0.38f), TextAnchor.MiddleCenter, FontStyle.Normal, 380f, 24f);
            UIKit.SetRect(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(380f, 24f));
        }

        public void Show()
        {
            _root.SetActive(true);
            _group.alpha = 0f;
            _group.blocksRaycasts = true;
            _group.interactable = true;
            Select(_selected);
            StartCoroutine(FadeIn());
        }

        IEnumerator FadeIn()
        {
            float t = 0f;
            while (t < 0.22f)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Clamp01(t / 0.22f);
                yield return null;
            }
            _group.alpha = 1f;
        }

        public void Hide()
        {
            _group.blocksRaycasts = false;
            _group.interactable = false;
            _root.SetActive(false);
        }

        void Select(int index)
        {
            _selected = Mathf.Clamp(index, 0, Heroes108.All.Count - 1);
            var entry = Heroes108.All[_selected];
            _detailName.text = entry.Name;
            _detailStar.text = entry.Star + "  ·  第 " + entry.Rank + " 位";
            _detailNick.text = "「" + entry.Nickname + "」";
            _detailSealText.text = entry.Name.Substring(0, 1);
            _detailSeal.color = entry.Heavenly ? UIKit.Vermilion : new Color(0.36f, 0.32f, 0.3f);
            _detailNote.text = entry.Heavenly
                ? "上应天罡，星落梁山。\n" + entry.Star + "，位列忠义堂第 " + entry.Rank + " 把交椅。\n\n" + NoteFor(entry)
                : "下应地煞，聚义水泊。\n" + entry.Star + "，位列忠义堂第 " + entry.Rank + " 把交椅。\n\n" + NoteFor(entry);
            for (int i = 0; i < _cells.Count; i++)
            {
                bool selected = i == _selected;
                var cell = _cells[i];
                cell.color = selected ? new Color(0.16f, 0.13f, 0.09f, 1f) : new Color(0.07f, 0.08f, 0.08f, 0.96f);
                var border = cell.transform.Find("selected border");
                if (border != null) border.gameObject.SetActive(selected);
            }
            if (_cells.Count > 0 && _scroll != null)
            {
                float row = _selected / 6f;
                float viewHeight = 744f;
                float contentHeight = Mathf.Max(0f, 18f * 62f + 12f);
                float y = Mathf.Clamp01(1f - (row * 62f) / Mathf.Max(1f, contentHeight - viewHeight));
                _scroll.verticalNormalizedPosition = y;
            }
        }

        static string NoteFor(Hero108Entry entry)
        {
            if (entry.Rank == 6) return "东京八十万禁军枪棒教头。风雪山神庙，雪夜上梁山。";
            if (entry.Rank == 13) return "拳打镇关西，倒拔垂杨柳。酒肉穿肠，佛在心头。";
            if (entry.Rank == 14) return "景阳冈打虎，狮子楼除奸。行者双刀，快意恩仇。";
            if (entry.Rank == 1) return "及时雨宋江，仗义疏财，坐镇忠义堂。";
            if (entry.Rank == 2) return "玉麒麟卢俊义，棍棒天下无双。";
            if (entry.Rank == 3) return "智多星吴用，羽扇纶巾，运筹帷幄。";
            return "替天行道，聚义梁山。";
        }
    }
}
