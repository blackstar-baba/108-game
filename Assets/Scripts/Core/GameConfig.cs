using UnityEngine;

namespace Liangshan
{
    public enum GameState
    {
        Boot,
        Menu,
        Playing,
        Boon,
        Paused,
        Victory,
        Defeat
    }

    public enum Team
    {
        Player,
        Enemy,
        Neutral
    }

    /// <summary>Shared tuning and the game's limited palette.</summary>
    public static class GameConfig
    {
        public const string Version = "1.0 · 聚义";
        public const int TotalWaves = 5;

        public const float ArenaRadius = 21.5f;
        public const float PlayerBoundary = 25.5f;
        public const float IslandRadius = 33f;
        public const float WaterHeight = -7.8f;
        public const float PlayerSpawnZ = 7.5f;

        public static readonly Vector3 MenuFocus = new Vector3(0f, 1.1f, 1.5f);
        public static readonly Vector3 HallPosition = new Vector3(0f, 0f, 27.5f);

        public static readonly Color Paper = Hex("E7D9BC");
        public static readonly Color PaperLight = Hex("F2E8D2");
        public static readonly Color PaperDark = Hex("C9B593");
        public static readonly Color Ink = Hex("171A1C");
        public static readonly Color InkSoft = Hex("2D3235");
        public static readonly Color InkBlue = Hex("293B45");
        public static readonly Color Vermilion = Hex("A63327");
        public static readonly Color VermilionBright = Hex("C64A33");
        public static readonly Color Gold = Hex("C9A45C");
        public static readonly Color Jade = Hex("4D6B5E");
        public static readonly Color Mist = Hex("C8B39B");
        public static readonly Color Bone = Hex("D8CDB7");
        public static readonly Color Night = Hex("131B23");

        public static readonly Color HeroLin = Hex("2D5866");
        public static readonly Color HeroLinCloth = Hex("D9D3C4");
        public static readonly Color HeroWu = Hex("7E2B2B");
        public static readonly Color HeroWuCloth = Hex("2B2B30");
        public static readonly Color HeroLu = Hex("8C6540");
        public static readonly Color HeroLuCloth = Hex("4A3A2E");

        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString("#" + hex, out Color c))
                return c;
            return Color.magenta;
        }

        public static Color Alpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v;
        }

        public static float AngleTo(Vector3 from, Vector3 to)
        {
            Vector3 a = Flat(from);
            Vector3 b = Flat(to);
            if (a.sqrMagnitude < 0.0001f || b.sqrMagnitude < 0.0001f)
                return 0f;
            return Vector3.Angle(a, b);
        }

        public static string ChineseNumber(int value)
        {
            string[] digits = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            if (value <= 0) return "零";
            if (value < 10) return digits[value];
            if (value < 20) return value == 10 ? "十" : "十" + digits[value - 10];
            if (value < 100)
            {
                int tens = value / 10;
                int ones = value % 10;
                return digits[tens] + "十" + (ones > 0 ? digits[ones] : "");
            }
            return value.ToString();
        }
    }
}
