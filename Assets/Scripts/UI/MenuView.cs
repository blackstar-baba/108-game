using UnityEngine;
using UnityEngine.UI;

namespace Liangshan
{
    public sealed class MenuView : MonoBehaviour
    {
        Game _game;
        CanvasGroup _group;
        GameObject _root;
        Text _heroName;
        Text _heroNick;
        Text _heroStar;
        Text _heroRole;
        Text _heroQuote;
        Text _heroSkill;
        HeroCard[] _cards;
        int _selected;

        public void Initialize(Game game, Transform parent)
        {
            _game = game;
            var full = UIKit.Full(parent, "Main Menu");
            _root = full.gameObject;
            _group = _root.AddComponent<CanvasGroup>();
            _root.SetActive(false);

            // Dusk vignette and paper shadow.
            UIKit.Vignette(full, 0.62f);
            var shade = UIKit.Full(full, "bottom shade");
            var shadeImage = shade.gameObject.AddComponent<Image>();
            shadeImage.sprite = RuntimeAssets.GradientDownSprite;
            shadeImage.type = Image.Type.Simple;
            shadeImage.color = new Color(0.035f, 0.04f, 0.04f, 0.94f);
            shadeImage.raycastTarget = false;
            shade.anchorMin = new Vector2(0f, 0f);
            shade.anchorMax = new Vector2(1f, 0f);
            shade.pivot = new Vector2(0.5f, 0f);
            shade.sizeDelta = new Vector2(0f, 620f);
            shade.anchoredPosition = Vector2.zero;

            var top = UIKit.Full(full, "top shade");
            var topImage = top.gameObject.AddComponent<Image>();
            topImage.sprite = RuntimeAssets.GradientUpSprite;
            topImage.type = Image.Type.Simple;
            topImage.color = new Color(0.02f, 0.025f, 0.025f, 0.82f);
            topImage.raycastTarget = false;
            top.anchorMin = new Vector2(0f, 1f);
            top.anchorMax = new Vector2(1f, 1f);
            top.pivot = new Vector2(0.5f, 1f);
            top.sizeDelta = new Vector2(0f, 420f);

            // Left composition: a faded sun, brush stroke, title.
            var sun = UIKit.Panel(full, "red sun", new Vector2(490f, 490f), new Vector2(0f, 0f), new Color(0.52f, 0.16f, 0.12f, 0.20f), 200f);
            sun.sprite = RuntimeAssets.CircleSprite;
            sun.type = Image.Type.Simple;
            sun.rectTransform.anchorMin = new Vector2(0f, 1f);
            sun.rectTransform.anchorMax = new Vector2(0f, 1f);
            sun.rectTransform.pivot = new Vector2(0f, 1f);
            sun.rectTransform.anchoredPosition = new Vector2(-90f, 70f);

            var stroke = UIKit.Brush(full, new Vector2(890f, 250f), new Vector2(0f, 0f), new Color(0.92f, 0.86f, 0.72f, 0.10f), -3.5f);
            stroke.rectTransform.anchorMin = new Vector2(0f, 1f);
            stroke.rectTransform.anchorMax = new Vector2(0f, 1f);
            stroke.rectTransform.pivot = new Vector2(0f, 1f);
            stroke.rectTransform.anchoredPosition = new Vector2(36f, -62f);

            var title = UIKit.LabelAnchored(full, "title", "水泊梁山", 124, UIKit.Paper,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(104f, -86f), new Vector2(900f, 170f),
                TextAnchor.UpperLeft, FontStyle.Bold);

            var sub = UIKit.LabelAnchored(full, "subtitle", "一 百 单 八 将", 42, UIKit.Gold,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(112f, -248f), new Vector2(700f, 60f));

            UIKit.Seal(full, "替天\n行道", new Vector2(94f, 94f), new Vector2(0f, 1f), 22);
            var seal = full.Find("seal");
            if (seal != null)
            {
                var sealRect = seal.GetComponent<RectTransform>();
                sealRect.anchorMin = new Vector2(0f, 1f);
                sealRect.anchorMax = new Vector2(0f, 1f);
                sealRect.pivot = new Vector2(0f, 1f);
                sealRect.anchoredPosition = new Vector2(842f, -248f);
            }

            UIKit.LabelAnchored(full, "lore", "宣和年间，天下汹汹。\n一百单八位好汉聚义梁山，替天行道。",
                21, new Color(0.82f, 0.78f, 0.69f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(112f, -330f), new Vector2(650f, 90f));
            UIKit.LabelAnchored(full, "english", "THE WATER MARGIN  ·  OUTLAWS OF LIANGSHAN", 14,
                new Color(0.65f, 0.56f, 0.44f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(114f, -418f), new Vector2(720f, 30f));

            // Selected hero detail panel on the right.
            var detail = UIKit.PanelAnchored(full, "hero detail", new Vector2(300f, 500f), new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f), new Vector2(-92f, 34f), new Color(0.055f, 0.06f, 0.06f, 0.78f), 10f, false);
            var accent = UIKit.Divider(detail.transform, new Vector2(4f, 440f), new Vector2(18f, 0f), UIKit.Vermilion);
            _heroStar = UIKit.Label(detail.transform, "star", "", 15, UIKit.Gold, TextAnchor.UpperLeft, FontStyle.Normal, 240f, 26f);
            UIKit.SetRect(_heroStar.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -34f), new Vector2(230f, 26f));
            _heroName = UIKit.Label(detail.transform, "name", "", 54, UIKit.Paper, TextAnchor.UpperLeft, FontStyle.Bold, 230f, 74f);
            UIKit.SetRect(_heroName.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(38f, -64f), new Vector2(230f, 74f));
            _heroNick = UIKit.Label(detail.transform, "nick", "", 24, UIKit.Gold, TextAnchor.UpperLeft, FontStyle.Normal, 230f, 34f);
            UIKit.SetRect(_heroNick.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -142f), new Vector2(230f, 34f));
            _heroRole = UIKit.Label(detail.transform, "role", "", 16, new Color(0.76f, 0.72f, 0.64f), TextAnchor.UpperLeft, FontStyle.Normal, 230f, 56f);
            UIKit.SetRect(_heroRole.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -184f), new Vector2(230f, 56f));
            UIKit.Divider(detail.transform, new Vector2(220f, 1f), new Vector2(40f, 84f), new Color(0.5f, 0.42f, 0.31f, 0.7f));
            _heroSkill = UIKit.Label(detail.transform, "skill", "", 16, new Color(0.85f, 0.8f, 0.7f), TextAnchor.UpperLeft, FontStyle.Normal, 230f, 96f);
            UIKit.SetRect(_heroSkill.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -266f), new Vector2(230f, 96f));
            _heroQuote = UIKit.Label(detail.transform, "quote", "", 17, new Color(0.62f, 0.7f, 0.68f), TextAnchor.LowerLeft, FontStyle.Italic, 230f, 120f);
            UIKit.SetRect(_heroQuote.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 34f), new Vector2(230f, 120f));

            // Bottom hero selection strip.
            var cardsRoot = UIKit.Empty(full, "hero cards");
            var cardsRect = cardsRoot.GetComponent<RectTransform>();
            cardsRect.anchorMin = new Vector2(0.5f, 0f);
            cardsRect.anchorMax = new Vector2(0.5f, 0f);
            cardsRect.pivot = new Vector2(0.5f, 0f);
            cardsRect.anchoredPosition = new Vector2(0f, 40f);
            cardsRect.sizeDelta = new Vector2(1160f, 310f);

            _cards = new HeroCard[HeroDatabase.Playable.Count];
            for (int i = 0; i < HeroDatabase.Playable.Count; i++)
            {
                var hero = HeroDatabase.Playable[i];
                var card = new HeroCard();
                card.Build(cardsRoot.transform, hero, i, () => _game.SelectHero(i));
                _cards[i] = card;
            }

            UIKit.LabelAnchored(full, "select hint", "A / D  选择好汉", 15, new Color(0.66f, 0.6f, 0.5f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(500f, 24f), TextAnchor.MiddleCenter);

            // Start and codex controls.
            UIKit.Button(full, "start", "踏 入 梁 山", new Vector2(320f, 86f), new Vector2(0f, 0f),
                UIKit.Vermilion, new Color(1f, 0.92f, 0.8f), () => _game.StartRun(), 30, true)
                .GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0f);
            var startRect = full.Find("start").GetComponent<RectTransform>();
            startRect.anchorMin = new Vector2(1f, 0f);
            startRect.anchorMax = new Vector2(1f, 0f);
            startRect.pivot = new Vector2(1f, 0f);
            startRect.anchoredPosition = new Vector2(-92f, 46f);

            UIKit.Button(full, "codex", "一百单八将 · 图鉴", new Vector2(320f, 52f), Vector2.zero,
                new Color(0.09f, 0.1f, 0.1f, 0.9f), UIKit.PaperDim, () => _game.UI.ShowCodex(), 19)
                .GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0f);
            var codexRect = full.Find("codex").GetComponent<RectTransform>();
            codexRect.anchorMin = new Vector2(1f, 0f);
            codexRect.anchorMax = new Vector2(1f, 0f);
            codexRect.pivot = new Vector2(1f, 0f);
            codexRect.anchoredPosition = new Vector2(-92f, 144f);

            UIKit.LabelAnchored(full, "controls", "WASD 移动   鼠标 视角   左键 攻击   右键 重击   Space 闪避   E 技能   R 怒气   Esc 暂停",
                14, new Color(0.62f, 0.58f, 0.5f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(104f, 42f), new Vector2(900f, 28f));
            UIKit.LabelAnchored(full, "version", "v1.0  ·  梁山水泊", 13, new Color(0.42f, 0.4f, 0.35f),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-34f, -28f), new Vector2(240f, 24f), TextAnchor.MiddleRight);
        }

        public void Open(int selected)
        {
            _root.SetActive(true);
            _group.alpha = 0f;
            _group.blocksRaycasts = true;
            _group.interactable = true;
            _selected = selected;
            Refresh();
            StartCoroutine(FadeIn());
        }

        System.Collections.IEnumerator FadeIn()
        {
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Clamp01(t / 0.5f);
                yield return null;
            }
            _group.alpha = 1f;
        }

        public void Close()
        {
            _group.blocksRaycasts = false;
            _group.interactable = false;
            _root.SetActive(false);
        }

        public void Select(int index)
        {
            _selected = Mathf.Clamp(index, 0, HeroDatabase.Playable.Count - 1);
            Refresh();
        }

        public void Refresh()
        {
            if (_cards == null) return;
            for (int i = 0; i < _cards.Length; i++)
                _cards[i].SetSelected(i == _selected);
            HeroDefinition hero = HeroDatabase.Playable[_selected];
            if (_heroName != null) _heroName.text = hero.Name;
            if (_heroNick != null) _heroNick.text = hero.Nickname;
            if (_heroStar != null) _heroStar.text = hero.Star + "  ·  第 " + GameConfig.ChineseNumber(hero.Seat) + " 位";
            if (_heroRole != null) _heroRole.text = hero.Role + "\n" + hero.WeaponName;
            if (_heroSkill != null) _heroSkill.text = "技能 · " + hero.SkillName + "\n" + hero.SkillDesc;
            if (_heroQuote != null) _heroQuote.text = "「" + hero.Quote + "」";
        }
    }

    public sealed class HeroCard
    {
        public GameObject Root;
        Image _border;
        Image _background;
        Image _accent;
        CanvasGroup _group;
        Text _name;
        Text _nick;
        Text _star;
        Text _role;
        Text _quote;
        RectTransform _rect;
        int _index;

        public void Build(Transform parent, HeroDefinition hero, int index, System.Action onClick)
        {
            _index = index;
            var go = UIKit.Empty(parent, "hero card " + hero.Name);
            _rect = go.GetComponent<RectTransform>();
            _rect.sizeDelta = new Vector2(340f, 292f);
            _rect.anchorMin = new Vector2(0.5f, 0f);
            _rect.anchorMax = new Vector2(0.5f, 0f);
            _rect.pivot = new Vector2(0.5f, 0f);
            _rect.anchoredPosition = new Vector2((index - 1) * 374f, 0f);
            Root = go;
            _group = go.AddComponent<CanvasGroup>();

            _border = UIKit.Panel(go.transform, "border", new Vector2(344f, 296f), new Vector2(0f, 2f), new Color(0.32f, 0.3f, 0.26f, 0.65f), 12f);
            _background = UIKit.Panel(go.transform, "background", new Vector2(338f, 290f), Vector2.zero, new Color(0.06f, 0.07f, 0.07f, 0.92f), 10f, true);
            _background.gameObject.AddComponent<Button>().onClick.AddListener(() => onClick());
            _background.raycastTarget = true;

            _accent = UIKit.Divider(go.transform, new Vector2(48f, 4f), new Vector2(0f, 128f), hero.Trim);
            _accent.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _accent.rectTransform.anchoredPosition = new Vector2(0f, 128f);

            var seal = UIKit.Seal(go.transform, hero.Name.Substring(0, 1), new Vector2(58f, 58f), Vector2.zero, 28);
            UIKit.SetRect(seal.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -24f), new Vector2(58f, 58f));
            seal.color = hero.Primary;

            _star = UIKit.Label(go.transform, "star", hero.Star, 14, new Color(0.72f, 0.66f, 0.54f), TextAnchor.UpperLeft, FontStyle.Normal, 260f, 22f);
            UIKit.SetRect(_star.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100f, -26f), new Vector2(220f, 22f));

            _name = UIKit.Label(go.transform, "name", hero.Name, 38, UIKit.Paper, TextAnchor.UpperLeft, FontStyle.Bold, 230f, 52f);
            UIKit.SetRect(_name.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(98f, -48f), new Vector2(220f, 52f));

            _nick = UIKit.Label(go.transform, "nick", hero.Nickname, 20, hero.Trim, TextAnchor.UpperLeft, FontStyle.Normal, 220f, 28f);
            UIKit.SetRect(_nick.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100f, -100f), new Vector2(220f, 28f));

            _role = UIKit.Label(go.transform, "role", hero.Role, 14, new Color(0.68f, 0.65f, 0.58f), TextAnchor.UpperLeft, FontStyle.Normal, 290f, 34f);
            UIKit.SetRect(_role.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -138f), new Vector2(292f, 34f));

            float[] values = { hero.Damage / 34f, hero.MoveSpeed / 7f, hero.MaxHealth / 240f };
            string[] labels = { "力", "速", "韧" };
            for (int i = 0; i < 3; i++)
            {
                var lab = UIKit.Label(go.transform, "stat label " + i, labels[i], 13, new Color(0.6f, 0.57f, 0.5f), TextAnchor.MiddleLeft, FontStyle.Normal, 22f, 20f);
                UIKit.SetRect(lab.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -178f - i * 22f), new Vector2(22f, 20f));
                var bar = Bar.Create(go.transform, new Vector2(248f, 7f), Vector2.zero, new Color(0.16f, 0.16f, 0.15f, 0.9f), hero.Trim);
                UIKit.SetRect(bar.Background.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(56f, -185f - i * 22f), new Vector2(248f, 7f));
                bar.Set(Mathf.Clamp01(values[i]));
            }

            _quote = UIKit.Label(go.transform, "quote", "「" + hero.Quote + "」", 13, new Color(0.56f, 0.62f, 0.58f), TextAnchor.UpperLeft, FontStyle.Italic, 288f, 48f);
            UIKit.SetRect(_quote.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -250f), new Vector2(288f, 48f));
        }

        public void SetSelected(bool selected)
        {
            _border.color = selected ? UIKit.Gold : new Color(0.3f, 0.28f, 0.24f, 0.55f);
            _background.color = selected ? new Color(0.105f, 0.11f, 0.105f, 0.96f) : new Color(0.055f, 0.06f, 0.06f, 0.86f);
            _accent.color = selected ? UIKit.Gold : new Color(0.45f, 0.38f, 0.28f, 0.8f);
            _group.alpha = selected ? 1f : 0.78f;
            _rect.anchoredPosition = new Vector2((_index - 1) * 374f, selected ? 16f : 0f);
            _rect.localScale = Vector3.one * (selected ? 1.02f : 0.98f);
        }
    }
}
