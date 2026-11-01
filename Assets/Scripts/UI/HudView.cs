using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Liangshan
{
    public sealed class HudView : MonoBehaviour
    {
        Game _game;
        GameObject _root;
        CanvasGroup _group;
        PlayerController _player;

        Text _name;
        Text _title;
        Text _portrait;
        Image _portraitRing;
        Bar _healthBar;
        Bar _ultBar;
        Text _healthText;
        Text _waveText;
        Text _waveSub;
        Text _enemyText;
        Text _scoreText;
        Text _timeText;
        Text _comboText;
        CanvasGroup _comboGroup;
        float _comboPulse;
        Text _skillText;
        Text _skillName;
        Image _skillFill;
        Image _skillChip;
        Text _ultText;
        Image _ultChip;
        Text _dodgeText;
        Text _toast;
        CanvasGroup _toastGroup;
        Text _bossName;
        Text _bossTitle;
        Bar _bossBar;
        GameObject _bossGroup;
        Text _bossMove;
        CanvasGroup _bossMoveGroup;
        RectTransform _bossRect;
        Vector2 _bossBase;
        float _bossShake;
        Image _damageFlash;
        float _flashAlpha;
        GameObject _banner;
        CanvasGroup _bannerGroup;
        Text _bannerTitle;
        Text _bannerSub;
        Text _boonText;
        float _toastTimer;

        public void Initialize(Game game, Transform parent)
        {
            _game = game;
            var full = UIKit.Full(parent, "HUD");
            _root = full.gameObject;
            _group = _root.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
            _root.SetActive(false);

            // Full screen damage vignette.
            var flash = UIKit.Full(full, "damage flash");
            _damageFlash = flash.gameObject.AddComponent<Image>();
            _damageFlash.sprite = RuntimeAssets.SpriteFrom(RuntimeAssets.VignetteTexture);
            _damageFlash.color = new Color(0.62f, 0.06f, 0.04f, 0f);
            _damageFlash.raycastTarget = false;

            // Player panel.
            var panel = UIKit.PanelAnchored(full, "player panel", new Vector2(500f, 164f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(44f, -32f), new Color(0.045f, 0.05f, 0.05f, 0.72f), 10f);
            var portraitBg = UIKit.Panel(panel.transform, "portrait bg", new Vector2(92f, 92f), Vector2.zero, new Color(0.05f, 0.06f, 0.06f, 1f), 46f);
            portraitBg.sprite = RuntimeAssets.CircleSprite;
            portraitBg.type = Image.Type.Simple;
            UIKit.SetRect(portraitBg.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -18f), new Vector2(92f, 92f));
            _portraitRing = UIKit.Panel(panel.transform, "portrait ring", new Vector2(98f, 98f), Vector2.zero, UIKit.Gold, 49f);
            _portraitRing.sprite = RuntimeAssets.CircleSprite;
            _portraitRing.type = Image.Type.Simple;
            _portraitRing.raycastTarget = false;
            UIKit.SetRect(_portraitRing.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(19f, -15f), new Vector2(98f, 98f));
            _portrait = UIKit.Label(panel.transform, "portrait letter", "林", 46, UIKit.Paper, TextAnchor.MiddleCenter, FontStyle.Bold, 92f, 92f);
            UIKit.SetRect(_portrait.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -18f), new Vector2(92f, 92f));

            _name = UIKit.Label(panel.transform, "name", "林冲", 30, UIKit.Paper, TextAnchor.UpperLeft, FontStyle.Bold, 340f, 38f);
            UIKit.SetRect(_name.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(136f, -16f), new Vector2(340f, 38f));
            _title = UIKit.Label(panel.transform, "title", "天雄星 · 豹子头", 15, UIKit.Gold, TextAnchor.UpperLeft, FontStyle.Normal, 340f, 24f);
            UIKit.SetRect(_title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(138f, -56f), new Vector2(340f, 24f));

            _healthBar = Bar.Create(panel.transform, new Vector2(330f, 15f), Vector2.zero, new Color(0.18f, 0.12f, 0.1f, 0.95f), new Color(0.69f, 0.24f, 0.19f));
            UIKit.SetRect(_healthBar.Background.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(138f, -84f), new Vector2(330f, 15f));
            _healthText = UIKit.Label(panel.transform, "health text", "200 / 200", 12, new Color(0.94f, 0.87f, 0.76f), TextAnchor.MiddleRight, FontStyle.Normal, 330f, 15f);
            UIKit.SetRect(_healthText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(138f, -84f), new Vector2(330f, 15f));

            _ultBar = Bar.Create(panel.transform, new Vector2(330f, 8f), Vector2.zero, new Color(0.16f, 0.14f, 0.1f, 0.95f), UIKit.Gold);
            UIKit.SetRect(_ultBar.Background.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(138f, -105f), new Vector2(330f, 8f));
            var ultLabel = UIKit.Label(panel.transform, "ult label", "怒 气", 11, UIKit.Gold, TextAnchor.MiddleLeft, FontStyle.Normal, 60f, 20f);
            UIKit.SetRect(ultLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(138f, -120f), new Vector2(60f, 20f));
            _boonText = UIKit.Label(panel.transform, "boons", "", 12, new Color(0.66f, 0.72f, 0.67f), TextAnchor.MiddleLeft, FontStyle.Normal, 280f, 20f);
            UIKit.SetRect(_boonText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(202f, -120f), new Vector2(280f, 20f));

            // Wave header.
            _waveText = UIKit.LabelAnchored(full, "wave", "第 一 波", 34, UIKit.Paper, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -34f), new Vector2(600f, 48f), TextAnchor.MiddleCenter, FontStyle.Bold);
            _waveSub = UIKit.LabelAnchored(full, "wave sub", "守住聚义厅 · 击退官兵", 15, new Color(0.76f, 0.7f, 0.58f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(600f, 24f), TextAnchor.MiddleCenter);
            _enemyText = UIKit.LabelAnchored(full, "enemies", "余敌 8", 18, UIKit.Gold, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -108f), new Vector2(400f, 28f), TextAnchor.MiddleCenter);

            // Score and clock.
            _scoreText = UIKit.LabelAnchored(full, "score", "战功  0000", 25, UIKit.Paper, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-46f, -38f), new Vector2(360f, 34f), TextAnchor.MiddleRight);
            _timeText = UIKit.LabelAnchored(full, "time", "00:00", 16, new Color(0.7f, 0.66f, 0.58f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-46f, -76f), new Vector2(360f, 24f), TextAnchor.MiddleRight);

            // Boss bar.
            _bossGroup = UIKit.Empty(full, "boss group");
            _bossRect = _bossGroup.GetComponent<RectTransform>();
            UIKit.SetRect(_bossRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(700f, 82f));
            _bossBase = _bossRect.anchoredPosition;
            var bossPanel = UIKit.Panel(_bossGroup.transform, "boss plate", new Vector2(700f, 78f), Vector2.zero, new Color(0.07f, 0.055f, 0.05f, 0.9f), 8f, false);
            _bossName = UIKit.Label(_bossGroup.transform, "boss name", "高俅", 25, UIKit.Gold, TextAnchor.MiddleCenter, FontStyle.Bold, 300f, 30f);
            UIKit.SetRect(_bossName.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -7f), new Vector2(300f, 30f));
            _bossTitle = UIKit.Label(_bossGroup.transform, "boss title", "殿前太尉 · 祸乱朝纲", 12, new Color(0.7f, 0.62f, 0.5f), TextAnchor.MiddleCenter, FontStyle.Normal, 400f, 20f);
            UIKit.SetRect(_bossTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(400f, 20f));
            _bossBar = Bar.Create(_bossGroup.transform, new Vector2(650f, 12f), Vector2.zero, new Color(0.12f, 0.08f, 0.07f, 1f), new Color(0.72f, 0.22f, 0.15f));
            UIKit.SetRect(_bossBar.Background.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 9f), new Vector2(650f, 12f));
            _bossMove = UIKit.Label(_bossGroup.transform, "boss move", "", 19, UIKit.Vermilion, TextAnchor.MiddleCenter, FontStyle.Bold, 400f, 28f);
            UIKit.SetRect(_bossMove.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, -24f), new Vector2(400f, 28f));
            _bossMoveGroup = _bossMove.gameObject.AddComponent<CanvasGroup>();
            _bossMoveGroup.alpha = 0f;
            _bossGroup.SetActive(false);

            // Combo.
            var comboGo = UIKit.Empty(full, "combo");
            var comboRect = comboGo.GetComponent<RectTransform>();
            UIKit.SetRect(comboRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 284f), new Vector2(600f, 110f));
            _comboGroup = comboGo.AddComponent<CanvasGroup>();
            _comboGroup.alpha = 0f;
            _comboText = UIKit.Label(comboGo.transform, "combo text", "连斩  0", 48, UIKit.Gold, TextAnchor.MiddleCenter, FontStyle.Bold, 600f, 74f);
            _comboText.rectTransform.anchorMin = Vector2.zero;
            _comboText.rectTransform.anchorMax = Vector2.one;
            _comboText.rectTransform.offsetMin = Vector2.zero;
            _comboText.rectTransform.offsetMax = new Vector2(0f, 24f);

            // Ability chips.
            _skillChip = BuildChip(full, "skill chip", "E", "技能", -196f, out _skillText, out _skillName, out _skillFill);
            _ultChip = BuildChip(full, "ult chip", "R", "怒气", 0f, out _ultText, out _, out _);
            BuildDodgeChip(full, 196f);

            // Banner.
            _banner = UIKit.Empty(full, "wave banner").gameObject;
            var bannerRect = _banner.GetComponent<RectTransform>();
            UIKit.SetRect(bannerRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(1100f, 200f));
            var brush = UIKit.Brush(_banner.transform, new Vector2(850f, 170f), Vector2.zero, new Color(0.03f, 0.04f, 0.04f, 0.82f), -1.5f);
            _bannerTitle = UIKit.Label(_banner.transform, "banner title", "", 58, UIKit.Paper, TextAnchor.MiddleCenter, FontStyle.Bold, 1080f, 90f);
            _bannerTitle.rectTransform.anchorMin = Vector2.zero;
            _bannerTitle.rectTransform.anchorMax = Vector2.one;
            _bannerTitle.rectTransform.offsetMin = new Vector2(0f, 30f);
            _bannerTitle.rectTransform.offsetMax = new Vector2(0f, -10f);
            _bannerSub = UIKit.Label(_banner.transform, "banner sub", "", 18, UIKit.Gold, TextAnchor.MiddleCenter, FontStyle.Normal, 1080f, 30f);
            _bannerSub.rectTransform.anchorMin = Vector2.zero;
            _bannerSub.rectTransform.anchorMax = Vector2.one;
            _bannerSub.rectTransform.offsetMin = new Vector2(0f, 8f);
            _bannerSub.rectTransform.offsetMax = new Vector2(0f, -110f);
            _bannerGroup = _banner.AddComponent<CanvasGroup>();
            _bannerGroup.alpha = 0f;

            // Toast.
            _toast = UIKit.LabelAnchored(full, "toast", "", 28, UIKit.Paper, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 214f), new Vector2(900f, 46f), TextAnchor.MiddleCenter, FontStyle.Bold);
            _toastGroup = _toast.gameObject.AddComponent<CanvasGroup>();
            _toastGroup.alpha = 0f;

            UIKit.LabelAnchored(full, "objective", "守住聚义厅 · 替天行道", 15, new Color(0.72f, 0.67f, 0.56f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(46f, 36f), new Vector2(600f, 26f));
            UIKit.LabelAnchored(full, "hint", "WASD 移动   鼠标 视角   左键 连击   右键 重击   Space 闪避   E 技能   R 怒气   Esc 暂停",
                13, new Color(0.55f, 0.52f, 0.45f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-46f, 36f), new Vector2(980f, 24f), TextAnchor.MiddleRight);
        }

        Image BuildChip(Transform parent, string name, string key, string label, float x, out Text cooldown, out Text title, out Image fill)
        {
            var chip = UIKit.PanelAnchored(parent, name, new Vector2(174f, 66f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(x, 54f), new Color(0.05f, 0.055f, 0.055f, 0.86f), 8f);
            var seal = UIKit.Panel(chip.transform, "key", new Vector2(40f, 40f), new Vector2(-58f, 0f), new Color(0.12f, 0.13f, 0.13f, 1f), 5f);
            var keyText = UIKit.Label(seal.transform, "key text", key, 20, UIKit.Gold, TextAnchor.MiddleCenter, FontStyle.Bold, 40f, 40f);
            keyText.rectTransform.anchorMin = Vector2.zero;
            keyText.rectTransform.anchorMax = Vector2.one;
            keyText.rectTransform.offsetMin = Vector2.zero;
            keyText.rectTransform.offsetMax = Vector2.zero;
            title = UIKit.Label(chip.transform, "chip label", label, 15, new Color(0.76f, 0.72f, 0.63f), TextAnchor.MiddleLeft, FontStyle.Normal, 110f, 24f);
            UIKit.SetRect(title.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(72f, 8f), new Vector2(100f, 24f));
            cooldown = UIKit.Label(chip.transform, "chip cd", "", 13, new Color(0.58f, 0.56f, 0.5f), TextAnchor.MiddleLeft, FontStyle.Normal, 104f, 20f);
            UIKit.SetRect(cooldown.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(72f, -12f), new Vector2(100f, 20f));
            fill = UIKit.Panel(chip.transform, "ready", new Vector2(4f, 48f), new Vector2(81f, 0f), UIKit.Gold, 2f);
            fill.raycastTarget = false;
            return chip;
        }

        void BuildDodgeChip(Transform parent, float x)
        {
            var chip = UIKit.PanelAnchored(parent, "dodge chip", new Vector2(154f, 66f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(x, 54f), new Color(0.05f, 0.055f, 0.055f, 0.86f), 8f);
            var seal = UIKit.Panel(chip.transform, "key", new Vector2(40f, 40f), new Vector2(-46f, 0f), new Color(0.12f, 0.13f, 0.13f, 1f), 5f);
            var keyText = UIKit.Label(seal.transform, "key text", "⇧", 18, UIKit.Gold, TextAnchor.MiddleCenter, FontStyle.Bold, 40f, 40f);
            keyText.rectTransform.anchorMin = Vector2.zero;
            keyText.rectTransform.anchorMax = Vector2.one;
            keyText.rectTransform.offsetMin = Vector2.zero;
            keyText.rectTransform.offsetMax = Vector2.zero;
            var title = UIKit.Label(chip.transform, "chip label", "闪避", 15, new Color(0.76f, 0.72f, 0.63f), TextAnchor.MiddleLeft, FontStyle.Normal, 90f, 24f);
            UIKit.SetRect(title.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(54f, 8f), new Vector2(90f, 24f));
            _dodgeText = UIKit.Label(chip.transform, "chip cd", "SPACE", 13, new Color(0.58f, 0.56f, 0.5f), TextAnchor.MiddleLeft, FontStyle.Normal, 90f, 20f);
            UIKit.SetRect(_dodgeText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(54f, -12f), new Vector2(90f, 20f));
        }

        public void Show()
        {
            _root.SetActive(true);
            _group.blocksRaycasts = false;
            _group.interactable = false;
            StopAllCoroutines();
            StartCoroutine(FadeGroup(1f, 0.35f));
            if (_toastTimer <= 0f) _toastGroup.alpha = 0f;
        }

        public void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(FadeGroup(0f, 0.2f, () => _root.SetActive(false)));
        }

        IEnumerator FadeGroup(float target, float duration, System.Action done = null)
        {
            float start = _group.alpha;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            _group.alpha = target;
            done?.Invoke();
        }

        public void BindPlayer(PlayerController player)
        {
            _player = player;
            if (player == null || player.Hero == null) return;
            HeroDefinition hero = player.Hero;
            _portrait.text = hero.Name.Substring(0, 1);
            _portraitRing.color = hero.Trim;
            _name.text = hero.Name;
            _title.text = hero.Star + " · " + hero.Nickname;
            _boonText.text = "";
            _skillName.text = hero.SkillName;
            _healthBar.Set(1f);
            _ultBar.Set(0f);
        }

        public void SetUltimate(float value01) => _ultBar?.Set(value01);

        public void PulseCombo(int combo)
        {
            if (_comboGroup == null) return;
            _comboGroup.alpha = 1f;
            _comboText.text = "连斩  " + combo;
            _comboPulse = 1f;
            _comboText.rectTransform.localScale = Vector3.one * 1.25f;
        }

        public void ShowSkillName(string name)
        {
            ShowToast("技能 · " + name);
        }

        public void ShowUltimateName(string name)
        {
            ShowToast("怒气爆发 · " + name);
            _comboGroup.alpha = 0f;
        }

        void ShowToast(string text)
        {
            _toast.text = text;
            _toastTimer = 1.8f;
            _toastGroup.alpha = 1f;
        }

        public void ShowBossMove(string text)
        {
            _bossMove.text = text;
            _bossMoveGroup.alpha = 1f;
        }

        public void ShowBossPhase(string text)
        {
            ShowToast(text);
            _bossBar.Fill.color = new Color(0.86f, 0.3f, 0.18f);
            _bossShake = 0.7f;
        }

        public void ShakeBossBar()
        {
            _bossShake = Mathf.Max(_bossShake, 0.4f);
        }

        public void SetBoss(EnemyController boss)
        {
            if (boss == null)
            {
                _bossGroup.SetActive(false);
                return;
            }
            _bossGroup.SetActive(true);
            _bossName.text = boss.Definition.Name;
            _bossTitle.text = boss.Definition.Title;
            _bossBar.Set(1f);
        }

        public void UpdateBoss(EnemyController boss)
        {
            if (boss == null) return;
            _bossBar.Set(boss.Health01);
        }

        public void SetWave(int wave, int total)
        {
            _waveText.text = "第 " + GameConfig.ChineseNumber(wave) + " 波";
            _waveSub.text = wave == total ? "高俅亲至 · 死战不退" : "守住聚义厅 · 击退官兵";
        }

        public void SetEnemies(int alive)
        {
            _enemyText.text = "余敌 " + Mathf.Max(0, alive);
        }

        public void SetScore(int score)
        {
            _scoreText.text = "战功  " + score.ToString("0000");
        }

        public void SetTime(float seconds)
        {
            int s = Mathf.Max(0, Mathf.FloorToInt(seconds));
            _timeText.text = (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
        }

        public void SetBoons(string text)
        {
            _boonText.text = text;
        }

        public void FlashInk(float strength)
        {
            _flashAlpha = Mathf.Clamp01(Mathf.Max(_flashAlpha, strength));
        }

        public void ShowWaveBanner(string title, string subtitle, float duration = 2.4f)
        {
            StopCoroutine(nameof(BannerRoutine));
            StartCoroutine(BannerRoutine(title, subtitle, duration));
        }

        IEnumerator BannerRoutine(string title, string subtitle, float duration)
        {
            _bannerTitle.text = title;
            _bannerSub.text = subtitle;
            _bannerGroup.alpha = 0f;
            float t = 0f;
            while (t < 0.4f)
            {
                t += Time.unscaledDeltaTime;
                _bannerGroup.alpha = Mathf.Clamp01(t / 0.4f);
                yield return null;
            }
            _bannerGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(duration);
            t = 0f;
            while (t < 0.45f)
            {
                t += Time.unscaledDeltaTime;
                _bannerGroup.alpha = 1f - Mathf.Clamp01(t / 0.45f);
                yield return null;
            }
            _bannerGroup.alpha = 0f;
        }

        void Update()
        {
            if (_player == null) return;
            if (_player.Combat != null)
            {
                _healthBar.Set(_player.Combat.Health01);
                _healthText.text = Mathf.CeilToInt(_player.Combat.Health) + " / " + Mathf.CeilToInt(_player.Combat.MaxHealth);
                _healthBar.Fill.color = Color.Lerp(new Color(0.78f, 0.2f, 0.14f), new Color(0.72f, 0.26f, 0.18f), _player.Combat.Health01);
            }
            _ultBar.Set(_player.UltimateMeter / 100f);
            _skillText.text = _player.SkillCooldownRemaining > 0.05f
                ? "冷却 " + _player.SkillCooldownRemaining.ToString("0.0") + "s"
                : "可释放";
            _skillFill.color = _player.SkillCooldownRemaining > 0.05f ? new Color(0.35f, 0.33f, 0.28f) : UIKit.Gold;
            _skillFill.rectTransform.sizeDelta = new Vector2(4f, _player.SkillCooldownRemaining > 0.05f ? 20f : 48f);
            _ultText.text = _player.UltimateMeter >= 99.5f ? "可爆发" : Mathf.FloorToInt(_player.UltimateMeter) + "%";
            _ultText.color = _player.UltimateMeter >= 99.5f ? UIKit.Gold : new Color(0.58f, 0.56f, 0.5f);
            _ultChip.color = _player.UltimateMeter >= 99.5f ? new Color(0.14f, 0.11f, 0.08f, 0.95f) : new Color(0.05f, 0.055f, 0.055f, 0.86f);
            _dodgeText.text = _player.DodgeCooldownRemaining > 0.05f
                ? _player.DodgeCooldownRemaining.ToString("0.0") + "s"
                : "可用";

            if (_player.Combo > 0)
            {
                _comboGroup.alpha = Mathf.Min(1f, _comboGroup.alpha + Time.deltaTime * 8f);
                if (_comboPulse > 0f)
                {
                    _comboPulse = Mathf.Max(0f, _comboPulse - Time.deltaTime * 4f);
                    _comboText.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, 1.18f, _comboPulse);
                }
            }
            else
            {
                _comboGroup.alpha = Mathf.MoveTowards(_comboGroup.alpha, 0f, Time.deltaTime * 3f);
            }

            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.deltaTime;
                _toastGroup.alpha = Mathf.Clamp01(_toastTimer / 0.4f);
            }
            else
            {
                _toastGroup.alpha = Mathf.MoveTowards(_toastGroup.alpha, 0f, Time.deltaTime * 3f);
            }

            if (_bossMoveGroup != null && _bossMoveGroup.alpha > 0f)
                _bossMoveGroup.alpha = Mathf.MoveTowards(_bossMoveGroup.alpha, 0f, Time.deltaTime * 1.2f);

            _flashAlpha = Mathf.MoveTowards(_flashAlpha, 0f, Time.deltaTime * 1.6f);
            _damageFlash.color = new Color(0.62f, 0.06f, 0.04f, _flashAlpha * 0.55f);

            if (_bossGroup.activeSelf && _bossShake > 0f)
            {
                _bossShake = Mathf.Max(0f, _bossShake - Time.deltaTime * 3f);
                _bossRect.anchoredPosition = _bossBase + new Vector2(Mathf.Sin(Time.unscaledTime * 80f) * _bossShake * 9f, 0f);
            }
            else if (_bossGroup.activeSelf)
            {
                _bossRect.anchoredPosition = Vector2.Lerp(_bossRect.anchoredPosition, _bossBase, Time.deltaTime * 10f);
            }
        }
    }
}
