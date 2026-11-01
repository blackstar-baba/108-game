using System.Collections.Generic;
using UnityEngine;

namespace Liangshan
{
    /// <summary>Runtime-created shaders, materials, sprites, fonts and noise textures.</summary>
    public static class RuntimeAssets
    {
        static bool _initialized;
        static readonly Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();
        static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        public static Shader ToonShader { get; private set; }
        public static Shader SkyShader { get; private set; }
        public static Shader WaterShader { get; private set; }
        public static Shader ParticleShader { get; private set; }
        public static Material SkyMaterial { get; private set; }
        public static Material WaterMaterial { get; private set; }
        public static Material ParticleMaterial { get; private set; }
        public static Material ParticleAdditiveMaterial { get; private set; }
        public static Font TitleFont { get; private set; }
        public static Font BodyFont { get; private set; }
        public static Font NumberFont { get; private set; }

        public static Texture2D SoftCircle { get; private set; }
        public static Texture2D InkSplat { get; private set; }
        public static Texture2D InkSplatB { get; private set; }
        public static Texture2D PaperTexture { get; private set; }
        public static Texture2D VignetteTexture { get; private set; }
        public static Texture2D StreakTexture { get; private set; }
        public static Texture2D GlowTexture { get; private set; }
        public static Texture2D White { get; private set; }

        public static Sprite RoundedSprite { get; private set; }
        public static Sprite RoundedThinSprite { get; private set; }
        public static Sprite CircleSprite { get; private set; }
        public static Sprite BrushSprite { get; private set; }
        public static Sprite SquareSprite { get; private set; }
        public static Sprite DiamondSprite { get; private set; }
        public static Sprite GradientUpSprite { get; private set; }
        public static Sprite GradientDownSprite { get; private set; }
        public static Sprite SplatSprite { get; private set; }
        public static Sprite SplatSpriteB { get; private set; }
        public static Sprite SealSprite { get; private set; }

        public static void Initialize()
        {
            if (_initialized)
                return;
            _initialized = true;

            ToonShader = LoadShader("Shaders/InkToon", "Universal Render Pipeline/Lit");
            SkyShader = LoadShader("Shaders/InkSky", "Skybox/Procedural");
            WaterShader = LoadShader("Shaders/InkWater", "Universal Render Pipeline/Unlit");
            ParticleShader = LoadShader("Shaders/InkParticle", "Sprites/Default");

            BuildTextures();
            BuildSprites();
            BuildFonts();
            BuildMaterials();
        }

        static Shader LoadShader(string path, string fallback)
        {
            Shader shader = Resources.Load<Shader>(path);
            if (shader == null)
            {
                shader = Shader.Find(fallback);
                Debug.LogWarning("[Liangshan] Missing shader " + path + ", using " + fallback);
            }
            return shader;
        }

        static void BuildMaterials()
        {
            SkyMaterial = new Material(SkyShader) { name = "Ink Sky" };
            SkyMaterial.SetVector("_SunDir", new Vector4(-0.48f, 0.16f, -0.86f, 0f));
            SkyMaterial.SetColor("_SunColor", GameConfig.Hex("FFC27A"));
            SkyMaterial.SetColor("_HorizonColor", GameConfig.Hex("E9A26F"));
            SkyMaterial.SetColor("_ZenithColor", GameConfig.Hex("34465C"));
            SkyMaterial.SetColor("_CloudColor", GameConfig.Hex("D6A98D"));
            SkyMaterial.SetColor("_MountainFar", GameConfig.Hex("55616F"));
            SkyMaterial.SetColor("_MountainNear", GameConfig.Hex("2A333C"));
            SkyMaterial.SetColor("_HazeColor", GameConfig.Hex("C39778"));
            SkyMaterial.SetFloat("_CloudStrength", 0.46f);
            SkyMaterial.SetFloat("_MountainStrength", 1f);
            SkyMaterial.SetFloat("_Exposure", 1.02f);

            WaterMaterial = new Material(WaterShader) { name = "Ink Water" };
            WaterMaterial.SetColor("_DeepColor", GameConfig.Hex("16282A"));
            WaterMaterial.SetColor("_ShallowColor", GameConfig.Hex("385550"));
            WaterMaterial.SetColor("_SkyTint", GameConfig.Hex("C8A17C"));
            WaterMaterial.SetColor("_FoamColor", GameConfig.Hex("E8DCC0"));
            WaterMaterial.SetColor("_SunColor", GameConfig.Hex("FFD08A"));
            WaterMaterial.SetFloat("_WaveScale", 1f);
            WaterMaterial.SetFloat("_WaveStrength", 0.065f);
            WaterMaterial.SetFloat("_Opacity", 0.94f);
            WaterMaterial.SetFloat("_FoamWidth", 0.9f);

            ParticleMaterial = new Material(ParticleShader) { name = "Ink Particles" };
            if (ParticleMaterial.HasProperty("_MainTex"))
                ParticleMaterial.SetTexture("_MainTex", SoftCircle);
            ParticleMaterial.SetColor("_Color", Color.white);

            ParticleAdditiveMaterial = new Material(ParticleShader) { name = "Ink Sparks" };
            if (ParticleAdditiveMaterial.HasProperty("_MainTex"))
                ParticleAdditiveMaterial.SetTexture("_MainTex", GlowTexture);
            ParticleAdditiveMaterial.SetColor("_Color", Color.white);
        }

        public static Material Toon(Color color, float outline = 0f, bool vertexColor = false, bool twoSided = false)
        {
            Color shadow = Color.Lerp(color * 0.42f, GameConfig.InkBlue, 0.35f);
            return Toon(color, shadow, outline, vertexColor, twoSided);
        }

        public static Material Toon(Color color, Color shadow, float outline = 0f, bool vertexColor = false, bool twoSided = false)
        {
            string key = ColorUtility.ToHtmlStringRGBA(color) + "|" + ColorUtility.ToHtmlStringRGBA(shadow)
                       + "|" + outline.ToString("F3") + "|" + vertexColor + "|" + twoSided;
            if (MaterialCache.TryGetValue(key, out Material cached))
                return cached;

            var mat = new Material(ToonShader) { name = "Ink Toon " + key.GetHashCode() };
            mat.SetColor("_BaseColor", color);
            mat.SetColor("_ShadowColor", shadow);
            mat.SetColor("_RimColor", GameConfig.Hex("FFD9A0"));
            mat.SetFloat("_RimPower", 3.2f);
            mat.SetFloat("_RimStrength", 0.40f);
            mat.SetFloat("_LightStrength", 0.88f);
            mat.SetFloat("_AmbientStrength", 0.40f);
            mat.SetFloat("_RampSoft", 0.12f);
            mat.SetFloat("_VertexColor", vertexColor ? 1f : 0f);
            mat.SetColor("_OutlineColor", new Color(0.035f, 0.04f, 0.045f, 1f));
            mat.SetFloat("_OutlineWidth", outline);
            mat.SetFloat("_Cull", twoSided ? 0f : 2f);
            mat.enableInstancing = true;
            MaterialCache[key] = mat;
            return mat;
        }

        public static Material VertexToon(float outline = 0f, float rim = 0.4f)
        {
            string key = "vertex" + outline;
            if (MaterialCache.TryGetValue(key, out Material cached))
                return cached;
            var mat = new Material(ToonShader) { name = "Ink Ground" };
            mat.SetColor("_BaseColor", Color.white);
            mat.SetColor("_ShadowColor", new Color(0.44f, 0.47f, 0.50f, 1f));
            mat.SetColor("_RimColor", GameConfig.Hex("FFD9A0"));
            mat.SetFloat("_RimPower", 4f);
            mat.SetFloat("_RimStrength", rim);
            mat.SetFloat("_LightStrength", 0.84f);
            mat.SetFloat("_AmbientStrength", 0.44f);
            mat.SetFloat("_RampSoft", 0.18f);
            mat.SetFloat("_VertexColor", 1f);
            mat.SetFloat("_OutlineWidth", outline);
            mat.SetFloat("_Cull", 2f);
            MaterialCache[key] = mat;
            return mat;
        }

        public static Material Unlit(Color color, bool twoSided = false)
        {
            string key = "unlit" + ColorUtility.ToHtmlStringRGBA(color) + twoSided;
            if (MaterialCache.TryGetValue(key, out Material cached))
                return cached;
            Shader shader = ParticleShader != null ? ParticleShader : ToonShader;
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var mat = new Material(shader) { name = "Ink Unlit" };
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);
            if (mat.HasProperty("_Cull"))
                mat.SetFloat("_Cull", twoSided ? 0f : 2f);
            MaterialCache[key] = mat;
            return mat;
        }

        public static Material NewParticleMaterial(Texture2D texture, bool additive = false, Color? tint = null)
        {
            var mat = new Material(ParticleShader) { name = additive ? "Ink Spark" : "Ink Particle" };
            if (mat.HasProperty("_MainTex"))
                mat.SetTexture("_MainTex", texture != null ? texture : SoftCircle);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", tint ?? Color.white);
            if (mat.HasProperty("_SrcBlend"))
            {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", additive ? (int)UnityEngine.Rendering.BlendMode.One : (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
            }
            return mat;
        }

        static void BuildFonts()
        {
            string[] installed = Font.GetOSInstalledFontNames();
            TitleFont = CreateFont(installed, new[] { "Songti SC", "STSong", "Songti TC", "Hiragino Mincho ProN", "Hiragino Sans GB" }, 72);
            BodyFont = CreateFont(installed, new[] { "Songti SC", "STSong", "Hiragino Sans GB", "Songti TC", "Arial Unicode MS" }, 48);
            NumberFont = CreateFont(installed, new[] { "Palatino", "Optima", "Times New Roman", "Songti SC" }, 48);
            if (TitleFont == null) TitleFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (BodyFont == null) BodyFont = TitleFont;
            if (NumberFont == null) NumberFont = BodyFont;
        }

        static Font CreateFont(string[] installed, string[] candidates, int size)
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                string candidate = candidates[i];
                bool found = false;
                for (int j = 0; j < installed.Length; j++)
                {
                    if (installed[j] == candidate)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    continue;
                try
                {
                    Font font = Font.CreateDynamicFontFromOSFont(candidate, size);
                    if (font != null)
                        return font;
                }
                catch
                {
                    // try next font
                }
            }

            // Fall back to asking the OS directly; this also covers localized font names.
            for (int i = 0; i < candidates.Length; i++)
            {
                try
                {
                    Font font = Font.CreateDynamicFontFromOSFont(candidates[i], size);
                    if (font != null && !string.IsNullOrEmpty(font.name))
                        return font;
                }
                catch
                {
                }
            }
            return null;
        }

        static void BuildTextures()
        {
            White = Texture2D.whiteTexture;
            SoftCircle = MakeRadialTexture(128, 1.0f, 0.0f);
            GlowTexture = MakeRadialTexture(128, 2.4f, 0.0f);
            StreakTexture = MakeStreakTexture(256, 32);
            InkSplat = MakeSplatTexture(256, 9127);
            InkSplatB = MakeSplatTexture(256, 4471);
            PaperTexture = MakePaperTexture(512);
            VignetteTexture = MakeVignetteTexture(256);
        }

        static void BuildSprites()
        {
            RoundedSprite = MakeRoundedSprite(64, 14f, 18);
            RoundedThinSprite = MakeRoundedSprite(64, 8f, 10);
            CircleSprite = MakeCircleSprite(128);
            BrushSprite = MakeBrushSprite(384, 28);
            SquareSprite = MakeWhiteSprite();
            DiamondSprite = MakeDiamondSprite(64);
            GradientUpSprite = MakeGradientSprite(32, true);
            GradientDownSprite = MakeGradientSprite(32, false);
            SplatSprite = MakeTextureSprite(InkSplat, "ink splat");
            SplatSpriteB = MakeTextureSprite(InkSplatB, "ink splat b");
            SealSprite = MakeSealSprite(96);
        }

        public static Material SkyboxMaterial => SkyMaterial;

        public static Sprite SpriteFrom(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite MakeTextureSprite(Texture2D texture, string name)
        {
            if (SpriteCache.TryGetValue(name, out Sprite cached))
                return cached;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = name;
            SpriteCache[name] = sprite;
            return sprite;
        }

        static Sprite MakeWhiteSprite()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color32[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
        }

        static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        static Color32 RGBA(float r, float g, float b, float a)
        {
            return new Color32(
                (byte)(Mathf.Clamp01(r) * 255f),
                (byte)(Mathf.Clamp01(g) * 255f),
                (byte)(Mathf.Clamp01(b) * 255f),
                (byte)(Mathf.Clamp01(a) * 255f));
        }

        static Sprite MakeRoundedSprite(int size, float radius, int border)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = Mathf.Abs(x - half + 0.5f);
                    float py = Mathf.Abs(y - half + 0.5f);
                    float dx = Mathf.Max(px - (half - radius), 0f);
                    float dy = Mathf.Max(py - (half - radius), 0f);
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = 1f - SmoothStep(radius - 1.2f, radius + 0.4f, dist);
                    pixels[y * size + x] = RGBA(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
            sprite.name = "rounded " + radius;
            return sprite;
        }

        static Sprite MakeCircleSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - half + 0.5f;
                    float dy = y - half + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) / half;
                    float alpha = 1f - SmoothStep(0.96f, 1f, dist);
                    pixels[y * size + x] = RGBA(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite MakeDiamondSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - half + 0.5f) / half;
                    float dy = Mathf.Abs(y - half + 0.5f) / half;
                    float d = dx + dy;
                    float alpha = 1f - SmoothStep(0.94f, 1f, d);
                    pixels[y * size + x] = RGBA(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite MakeBrushSprite(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            float[] envelope = new float[width];
            for (int x = 0; x < width; x++)
            {
                float u = x / (float)(width - 1);
                float edge = Mathf.Sin(Mathf.Clamp01(u) * Mathf.PI);
                envelope[x] = Mathf.Pow(edge, 0.35f) * (0.65f + 0.35f * Mathf.PerlinNoise(u * 9f, 3.1f));
            }
            for (int y = 0; y < height; y++)
            {
                float v = (y - height * 0.5f) / (height * 0.5f);
                float center = 1f - Mathf.Abs(v);
                for (int x = 0; x < width; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.15f, y * 0.3f);
                    float alpha = envelope[x] * Mathf.Pow(Mathf.Clamp01(center * 1.2f), 1.6f) * (0.6f + n * 0.7f);
                    pixels[y * width + x] = RGBA(1f, 1f, 1f, Mathf.Clamp01(alpha));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite MakeGradientSprite(int size, bool up)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                float v = y / (float)(size - 1);
                float alpha = up ? v : 1f - v;
                for (int x = 0; x < size; x++)
                    pixels[y * size + x] = RGBA(1f, 1f, 1f, alpha);
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite MakeSealSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float edge = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    float rough = Mathf.PerlinNoise(x * 0.15f, y * 0.15f) * 3f;
                    float alpha = edge > (2f + rough) ? 1f : 0f;
                    if (edge > size * 0.36f && edge < size * 0.40f)
                        alpha *= 0.35f;
                    pixels[y * size + x] = RGBA(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static Texture2D MakeRadialTexture(int size, float power, float innerAlpha)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half + 0.5f) / half;
                    float dy = (y - half + 0.5f) / half;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Pow(Mathf.Clamp01(1f - d), power);
                    a = Mathf.Lerp(innerAlpha, 1f, a);
                    pixels[y * size + x] = RGBA(1f, 1f, 1f, Mathf.Clamp01(a));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            return tex;
        }

        static Texture2D MakeStreakTexture(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                float v = 1f - Mathf.Abs((y - height * 0.5f) / (height * 0.5f));
                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)(width - 1);
                    float alpha = Mathf.Pow(v, 2.2f) * SmoothStep(0f, 0.18f, u) * SmoothStep(0f, 0.3f, 1f - u);
                    pixels[y * width + x] = RGBA(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        static Texture2D MakeSplatTexture(int size, int seed)
        {
            var random = new System.Random(seed);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float half = size * 0.5f;
            int lobes = 5 + random.Next(4);
            var lobeAngles = new float[lobes];
            var lobeWeights = new float[lobes];
            for (int i = 0; i < lobes; i++)
            {
                lobeAngles[i] = (float)random.NextDouble() * Mathf.PI * 2f;
                lobeWeights[i] = 0.18f + (float)random.NextDouble() * 0.32f;
            }
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);
                    float wobble = 0f;
                    for (int i = 0; i < lobes; i++)
                        wobble += Mathf.Sin(angle * (2 + i) + lobeAngles[i]) * lobeWeights[i];
                    float radius = 0.66f + wobble * 0.22f;
                    float a = Mathf.Clamp01((radius - dist) * 4.5f);
                    float speck = Mathf.PerlinNoise(dx * 12f + 5f, dy * 12f + 9f);
                    a *= 0.75f + speck * 0.35f;
                    if (dist > radius)
                    {
                        float drop = Mathf.Clamp01((0.16f - Mathf.Abs(dist - radius - 0.08f)) * 8f);
                        float dropAngles = Mathf.Abs(Mathf.Sin(angle * 9f + seed));
                        a = Mathf.Max(a, drop * dropAngles * (speck > 0.55f ? 1f : 0.2f));
                    }
                    pixels[y * size + x] = RGBA(0.06f, 0.065f, 0.07f, Mathf.Clamp01(a));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        static Texture2D MakePaperTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float fiber = Mathf.PerlinNoise(x * 0.09f, y * 0.025f) * 0.5f
                                + Mathf.PerlinNoise(x * 0.4f + 44f, y * 0.05f) * 0.3f;
                    float grain = Mathf.PerlinNoise(x * 0.9f, y * 0.9f) * 0.2f;
                    float v = 0.88f + fiber * 0.1f + grain * 0.06f;
                    pixels[y * size + x] = RGBA(v, v * 0.96f, v * 0.86f, 1f);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        static Texture2D MakeVignetteTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - size * 0.5f) / (size * 0.5f);
                    float dy = (y - size * 0.5f) / (size * 0.5f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Pow(Mathf.Clamp01((d - 0.35f) / 0.9f), 2.3f);
                    pixels[y * size + x] = RGBA(0.03f, 0.035f, 0.04f, a * 0.9f);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }
    }
}
