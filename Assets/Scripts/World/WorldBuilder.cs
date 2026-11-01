using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Liangshan
{
    public static class WorldBuilder
    {
        public static WorldRefs Build()
        {
            var refs = new WorldRefs();
            refs.Root = new GameObject("Liangshan");
            refs.Environment = WorldFactory.Empty("Environment", refs.Root.transform, Vector3.zero).transform;
            refs.Props = WorldFactory.Empty("Props", refs.Root.transform, Vector3.zero).transform;
            refs.Characters = WorldFactory.Empty("Characters", refs.Root.transform, Vector3.zero).transform;
            refs.Effects = WorldFactory.Empty("Effects", refs.Root.transform, Vector3.zero).transform;

            ConfigureLighting(refs);
            BuildWater(refs);
            BuildIsland(refs);
            BuildArena(refs);
            BuildHall(refs);
            BuildGate(refs);
            BuildPalisade(refs);
            BuildArenaProps(refs);
            BuildMenuStage(refs);
            BuildPostProcessing(refs);

            WorldProps.BuildTrees(refs);
            WorldProps.BuildDistantScenery(refs);

            WorldProps.BuildWatchtower(refs, new Vector3(-24.5f, 0f, -21.5f), 20f, 0);
            WorldProps.BuildWatchtower(refs, new Vector3(24.5f, 0f, -21.5f), -20f, 1);

            refs.EnemySpawnPoints.Add(new Vector3(0f, 0f, -31.8f));
            refs.EnemySpawnPoints.Add(new Vector3(-18f, 0f, -23f));
            refs.EnemySpawnPoints.Add(new Vector3(18f, 0f, -23f));

            // Invisible floor so the player's CharacterController can stand and walk.
            var floor = WorldFactory.Empty("Island Floor Collider", refs.Environment, new Vector3(0f, -0.55f, 0f));
            var floorBox = floor.AddComponent<BoxCollider>();
            floorBox.size = new Vector3(58f, 1.1f, 58f);

            return refs;
        }

        static void ConfigureLighting(WorldRefs refs)
        {
            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(refs.Environment);
            sunGo.transform.rotation = Quaternion.Euler(11f, 212f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = GameConfig.Hex("FFC28F");
            sun.intensity = 0.76f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.86f;
            sun.shadowBias = 0.04f;
            sun.shadowNormalBias = 0.32f;
            sun.shadowNearPlane = 0.2f;
            sunGo.AddComponent<UniversalAdditionalLightData>();
            refs.Sun = sunGo.transform;

            var fillGo = new GameObject("Cool Fill");
            fillGo.transform.SetParent(refs.Environment);
            fillGo.transform.rotation = Quaternion.Euler(28f, 28f, 0f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = GameConfig.Hex("89A7C0");
            fill.intensity = 0.16f;
            fill.shadows = LightShadows.None;
            fillGo.AddComponent<UniversalAdditionalLightData>();
            refs.Fill = fillGo.transform;

            RenderSettings.sun = sun;
            RenderSettings.skybox = RuntimeAssets.SkyMaterial;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = GameConfig.Hex("5A7088");
            RenderSettings.ambientEquatorColor = GameConfig.Hex("96775C");
            RenderSettings.ambientGroundColor = GameConfig.Hex("241F1A");
            RenderSettings.ambientIntensity = 0.24f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.0105f;
            RenderSettings.fogColor = GameConfig.Hex("A58069");
            RenderSettings.reflectionIntensity = 0.35f;
            RenderSettings.haloStrength = 0.3f;
            QualitySettings.shadowDistance = 72f;
            QualitySettings.shadowResolution = UnityEngine.ShadowResolution.High;
            QualitySettings.shadowCascades = 4;
            QualitySettings.shadowCascade4Split = new Vector3(0.04f, 0.13f, 0.32f);
            QualitySettings.antiAliasing = 4;
            DynamicGI.UpdateEnvironment();
        }

        static void BuildWater(WorldRefs refs)
        {
            var water = WorldFactory.Primitive("Water", refs.Environment, PrimitiveType.Plane,
                new Vector3(0f, GameConfig.WaterHeight, 0f), new Vector3(90f, 1f, 90f), null, RuntimeAssets.WaterMaterial, false, false);
            water.GetComponent<MeshRenderer>().receiveShadows = false;
        }

        static void BuildIsland(WorldRefs refs)
        {
            const int segments = 72;
            var ground = new MeshBuilder(false);
            Color dirt = GameConfig.Hex("4B3F31");
            Color dirtDark = GameConfig.Hex("3A3228");
            Color moss = GameConfig.Hex("39422F");
            Color mossDark = GameConfig.Hex("2C3527");
            float[] radii = { 0f, 4f, 9f, 14f, 19f, 23f, 26f, 29f, 31.5f, 33f };
            for (int i = 0; i < radii.Length - 1; i++)
            {
                Color a = (i % 2 == 0) ? dirt : dirtDark;
                Color b = (i % 2 == 0) ? dirtDark : moss;
                if (i >= 5)
                {
                    a = (i % 2 == 0) ? moss : mossDark;
                    b = (i % 2 == 0) ? mossDark : moss;
                }
                ground.Ring(Vector3.zero, radii[i], radii[i + 1], segments, a, b, 0f);
            }
            WorldFactory.MeshObject("island ground", refs.Environment, ground.Build("island ground"), RuntimeAssets.VertexToon(0.004f), Vector3.zero);

            // Moss and bare-earth patches.
            var patches = new MeshBuilder(false);
            var random = new System.Random(773);
            for (int i = 0; i < 22; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float radius = 22f + (float)random.NextDouble() * 9f;
                Vector3 center = new Vector3(Mathf.Cos(angle) * radius, 0.012f, Mathf.Sin(angle) * radius);
                Color c = (i % 3 == 0) ? GameConfig.Hex("2F382A") : GameConfig.Hex("3B352A");
                patches.Disc(center, 0.8f + (float)random.NextDouble() * 2.2f, 9, c, c, 0f);
            }
            WorldFactory.MeshObject("ground patches", refs.Environment, patches.Build("patches"), RuntimeAssets.VertexToon(0f), Vector3.zero, null, null, false, true);

            // Cliffs: three rings of wet rock with irregular silhouettes.
            var cliff = new MeshBuilder(true);
            Color rockTop = GameConfig.Hex("3F3A32");
            Color rockMid = GameConfig.Hex("2C2A26");
            Color rockBottom = GameConfig.Hex("202020");
            for (int i = 0; i < segments; i++)
            {
                float a0 = i / (float)segments * Mathf.PI * 2f;
                float a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                float jitter0 = (Mathf.PerlinNoise(i * 0.31f, 1.7f) - 0.5f) * 3.2f;
                float jitter1 = (Mathf.PerlinNoise((i + 1) * 0.31f, 1.7f) - 0.5f) * 3.2f;
                Vector3 top0 = new Vector3(Mathf.Cos(a0) * 33f, 0f, Mathf.Sin(a0) * 33f);
                Vector3 top1 = new Vector3(Mathf.Cos(a1) * 33f, 0f, Mathf.Sin(a1) * 33f);
                Vector3 mid0 = new Vector3(Mathf.Cos(a0) * (35.2f + jitter0 * 0.3f), -3.4f, Mathf.Sin(a0) * (35.2f + jitter0 * 0.3f));
                Vector3 mid1 = new Vector3(Mathf.Cos(a1) * (35.2f + jitter1 * 0.3f), -3.4f, Mathf.Sin(a1) * (35.2f + jitter1 * 0.3f));
                Vector3 bot0 = new Vector3(Mathf.Cos(a0) * (37.5f + jitter0), GameConfig.WaterHeight - 1.6f, Mathf.Sin(a0) * (37.5f + jitter0));
                Vector3 bot1 = new Vector3(Mathf.Cos(a1) * (37.5f + jitter1), GameConfig.WaterHeight - 1.6f, Mathf.Sin(a1) * (37.5f + jitter1));

                Color c0 = Color.Lerp(rockTop, rockMid, Mathf.PerlinNoise(i * 0.2f, 4.1f));
                Color c1 = Color.Lerp(rockMid, rockBottom, Mathf.PerlinNoise(i * 0.17f, 7.3f));
                cliff.Quad(top0, top1, mid1, mid0, c0);
                cliff.Quad(mid0, mid1, bot1, bot0, c1);
            }
            WorldFactory.MeshObject("cliffs", refs.Environment, cliff.Build("cliffs"), RuntimeAssets.VertexToon(0.006f, 0.2f), Vector3.zero, null, null, true, true);

            // Stone path from the gate to the arena and from the arena up to the hall.
            var path = new MeshBuilder(true);
            BuildPath(path, -31f, -22.4f, 4.6f, GameConfig.Hex("565146"));
            BuildPath(path, 22.4f, 21.2f, 4.6f, GameConfig.Hex("565146"));
            WorldFactory.MeshObject("stone path", refs.Environment, path.Build("stone path"), RuntimeAssets.VertexToon(0.003f), Vector3.zero);
        }

        static void BuildPath(MeshBuilder builder, float z0, float z1, float width, Color color)
        {
            int count = Mathf.Max(4, Mathf.RoundToInt(Mathf.Abs(z1 - z0) / 1.25f));
            for (int i = 0; i < count; i++)
            {
                float t0 = i / (float)count;
                float t1 = (i + 1) / (float)count;
                float a = Mathf.Lerp(z0, z1, t0);
                float b = Mathf.Lerp(z0, z1, t1);
                Color c = (i % 2 == 0) ? color : color * 0.9f;
                builder.Box(new Vector3(0f, 0.055f, (a + b) * 0.5f), new Vector3(width, 0.09f, Mathf.Abs(b - a) - 0.06f), c);
            }
        }

        static void BuildArena(WorldRefs refs)
        {
            var arena = new MeshBuilder(false);
            Color stoneA = GameConfig.Hex("565349");
            Color stoneB = GameConfig.Hex("625D52");
            Color stoneC = GameConfig.Hex("4A4840");
            Color seam = GameConfig.Hex("35342E");

            arena.Disc(Vector3.zero, 4.8f, 72, stoneB, stoneA, 0.06f);
            float[] radii = { 4.8f, 7.8f, 11.2f, 14.2f, 17.2f, 20.2f, 21.6f };
            for (int i = 0; i < radii.Length - 1; i++)
            {
                Color a = (i % 3 == 0) ? stoneA : (i % 3 == 1 ? stoneB : stoneC);
                Color b = (i % 3 == 1) ? stoneB : (i % 3 == 2 ? stoneC : stoneA);
                arena.Ring(Vector3.zero, radii[i], radii[i + 1], 72, a, b, 0.06f);
                arena.Ring(Vector3.zero, radii[i] - 0.045f, radii[i] + 0.045f, 72, seam, seam, 0.075f);
            }
            foreach (float r in radii)
                arena.Ring(Vector3.zero, r - 0.05f, r + 0.05f, 72, seam, seam, 0.08f);
            arena.Ring(Vector3.zero, 21.5f, 22.2f, 72, GameConfig.Hex("36332B"), GameConfig.Hex("282620"), 0.02f);
            arena.Ring(Vector3.zero, 17.9f, 18.05f, 72, GameConfig.Vermilion * 0.8f, GameConfig.Vermilion * 0.8f, 0.095f);

            // Twelve radial brush lines.
            for (int i = 0; i < 12; i++)
            {
                float angle = i / 12f * Mathf.PI * 2f;
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Vector3 side = new Vector3(-dir.z, 0f, dir.x) * 0.045f;
                Vector3 a = dir * 5.2f + Vector3.up * 0.085f;
                Vector3 b = dir * 21.2f + Vector3.up * 0.085f;
                arena.Quad(a - side, a + side, b + side, b - side, seam * 0.85f);
            }

            WorldFactory.MeshObject("arena stone", refs.Environment, arena.Build("arena stone"), RuntimeAssets.VertexToon(0.003f), Vector3.zero, null, null, true, true);

            var yi = WorldFactory.Text("Arena 義", refs.Environment, "義", new Vector3(0f, 0.13f, 0f),
                Quaternion.Euler(90f, 180f, 0f), 5.6f, GameConfig.Vermilion.Alpha(0.62f), RuntimeAssets.TitleFont,
                TextAnchor.MiddleCenter, TextAlignment.Center, 0);
            yi.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        static void BuildHall(WorldRefs refs)
        {
            var root = WorldFactory.Empty("忠义堂", refs.Environment, new Vector3(0f, 0f, 27.5f));
            Material stone = RuntimeAssets.Toon(GameConfig.Hex("6B675D"), 0.006f);
            Material stoneDark = RuntimeAssets.Toon(GameConfig.Hex("504D45"), 0.004f);
            Material red = RuntimeAssets.Toon(GameConfig.Hex("7E2E27"), 0.012f);
            Material wood = RuntimeAssets.Toon(GameConfig.Hex("3E2C20"), 0.008f);
            Material darkWood = RuntimeAssets.Toon(GameConfig.Hex("2A1E17"), 0.006f);
            Material plaster = RuntimeAssets.Toon(GameConfig.Hex("B7AC94"), 0.004f);
            Material roofMat = RuntimeAssets.Toon(GameConfig.Hex("2A2E31"), 0.008f);
            Material gold = RuntimeAssets.Toon(GameConfig.Hex("C7A15B"), 0.004f);

            WorldFactory.Primitive("foundation", root.transform, PrimitiveType.Cube, new Vector3(0f, 0.35f, 0f),
                new Vector3(22f, 0.7f, 14f), null, stone);
            WorldFactory.Primitive("floor edge", root.transform, PrimitiveType.Cube, new Vector3(0f, 0.72f, 0f),
                new Vector3(21f, 0.06f, 13f), null, stoneDark);

            // Front steps
            for (int i = 0; i < 3; i++)
            {
                WorldFactory.Primitive("step", root.transform, PrimitiveType.Cube,
                    new Vector3(0f, 0.12f + i * 0.23f, -7.6f - i * 0.75f),
                    new Vector3(10f + i * 2.4f, 0.24f, 0.85f), null, stone);
            }

            // Pillars
            float[] xs = { -7.2f, -2.5f, 2.5f, 7.2f };
            float[] zs = { -4.6f, 4.4f };
            foreach (float x in xs)
            {
                foreach (float z in zs)
                {
                    WorldFactory.Primitive("pillar base", root.transform, PrimitiveType.Cylinder,
                        new Vector3(x, 0.85f, z), new Vector3(0.55f, 0.2f, 0.55f), null, stoneDark);
                    WorldFactory.Primitive("pillar", root.transform, PrimitiveType.Cylinder,
                        new Vector3(x, 4f, z), new Vector3(0.34f, 3.2f, 0.34f), null, red);
                }
            }

            // Back and side walls
            WorldFactory.Primitive("back wall", root.transform, PrimitiveType.Cube, new Vector3(0f, 3.7f, 4.75f),
                new Vector3(15.2f, 6.2f, 0.35f), null, plaster);
            WorldFactory.Primitive("side wall L", root.transform, PrimitiveType.Cube, new Vector3(-7.55f, 3.7f, 0f),
                new Vector3(0.35f, 6.2f, 9.2f), null, plaster);
            WorldFactory.Primitive("side wall R", root.transform, PrimitiveType.Cube, new Vector3(7.55f, 3.7f, 0f),
                new Vector3(0.35f, 6.2f, 9.2f), null, plaster);

            // Lattice doors, deliberately simple but layered.
            for (int i = 0; i < 4; i++)
            {
                float x = -5.4f + i * 3.6f;
                WorldFactory.Primitive("door frame", root.transform, PrimitiveType.Cube, new Vector3(x, 3.1f, -4.62f),
                    new Vector3(2.7f, 4.7f, 0.22f), null, darkWood);
                WorldFactory.Primitive("door paper", root.transform, PrimitiveType.Cube, new Vector3(x, 3.1f, -4.74f),
                    new Vector3(2.25f, 4.25f, 0.06f), null, RuntimeAssets.Toon(GameConfig.Hex("BFAF8D"), 0f));
                for (int row = 0; row < 4; row++)
                {
                    WorldFactory.Primitive("lattice", root.transform, PrimitiveType.Cube,
                        new Vector3(x, 1.55f + row * 1.05f, -4.79f), new Vector3(2.3f, 0.06f, 0.06f), null, darkWood);
                }
                WorldFactory.Primitive("lattice", root.transform, PrimitiveType.Cube, new Vector3(x, 3.1f, -4.79f),
                    new Vector3(0.06f, 4.2f, 0.06f), null, darkWood);
            }

            // Lintel beams
            WorldFactory.Primitive("lintel front", root.transform, PrimitiveType.Cube, new Vector3(0f, 6.55f, -4.6f),
                new Vector3(16.2f, 0.42f, 0.45f), null, darkWood);
            WorldFactory.Primitive("lintel back", root.transform, PrimitiveType.Cube, new Vector3(0f, 6.55f, 4.5f),
                new Vector3(16.2f, 0.42f, 0.45f), null, darkWood);

            // Two roofs, the upper one set back.
            var roofBuilder = new MeshBuilder(true);
            Color tile = GameConfig.Hex("303437");
            Color tileDark = GameConfig.Hex("25292C");
            roofBuilder.GableRoof(new Vector3(0f, 7.0f, 0f), 19f, 12.8f, 3.1f, 1.7f, 1.15f, 14, 12, tile, tileDark);
            roofBuilder.GableRoof(new Vector3(0f, 10.0f, 0f), 13.6f, 9.0f, 2.4f, 1.1f, 1.0f, 10, 8, tile * 1.04f, tileDark);
            WorldFactory.MeshObject("roof", root.transform, roofBuilder.Build("hall roof"), roofMat, Vector3.zero, null, null, true, true);

            WorldFactory.Primitive("ridge", root.transform, PrimitiveType.Cylinder, new Vector3(0f, 12.52f, 0f),
                new Vector3(0.22f, 7.2f, 0.22f), Quaternion.Euler(0f, 0f, 90f), darkWood);
            WorldFactory.Primitive("ridge ornament L", root.transform, PrimitiveType.Sphere, new Vector3(-7.3f, 12.75f, 0f),
                Vector3.one * 0.38f, null, gold);
            WorldFactory.Primitive("ridge ornament R", root.transform, PrimitiveType.Sphere, new Vector3(7.3f, 12.75f, 0f),
                Vector3.one * 0.38f, null, gold);

            // Plaque and couplets.
            WorldFactory.Primitive("plaque board", root.transform, PrimitiveType.Cube, new Vector3(0f, 6.35f, -7.15f),
                new Vector3(5.2f, 1.35f, 0.18f), null, darkWood);
            WorldFactory.Primitive("plaque trim", root.transform, PrimitiveType.Cube, new Vector3(0f, 6.35f, -7.06f),
                new Vector3(5.5f, 1.6f, 0.06f), null, gold);
            WorldFactory.Text("忠义堂", root.transform, "忠义堂", new Vector3(0f, 6.35f, -7.17f),
                Quaternion.identity, 0.92f, GameConfig.Gold, RuntimeAssets.TitleFont, TextAnchor.MiddleCenter, TextAlignment.Center, 1);

            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -2.5f : 2.5f;
                string text = i == 0 ? "替天行道" : "保境安民";
                WorldFactory.Primitive("couplet board", root.transform, PrimitiveType.Cube, new Vector3(x, 3.9f, -5.29f),
                    new Vector3(0.75f, 3.9f, 0.12f), null, darkWood);
                WorldFactory.Text("couplet", root.transform, WorldProps.Verticalize(text), new Vector3(x, 3.9f, -5.23f),
                    Quaternion.identity, 0.44f, GameConfig.Gold, RuntimeAssets.TitleFont, TextAnchor.MiddleCenter, TextAlignment.Center, 1);
            }

            // Red lanterns under the lower eave.
            for (int i = 0; i < 4; i++)
            {
                float x = -5.6f + i * 3.7f;
                WorldFactory.Primitive("lantern cord", root.transform, PrimitiveType.Cylinder, new Vector3(x, 6.1f, -5.4f),
                    new Vector3(0.02f, 0.5f, 0.02f), null, darkWood);
                var lantern = WorldFactory.Primitive("lantern", root.transform, PrimitiveType.Sphere, new Vector3(x, 5.4f, -5.4f),
                    new Vector3(0.34f, 0.46f, 0.34f), null, RuntimeAssets.Toon(GameConfig.Hex("B84132"), GameConfig.Hex("5B1712"), 0f));
                WorldFactory.Primitive("tassel", root.transform, PrimitiveType.Cube, new Vector3(x, 4.9f, -5.4f),
                    new Vector3(0.06f, 0.55f, 0.08f), null, gold);
            }

            var hallLightGo = new GameObject("Hall glow");
            hallLightGo.transform.SetParent(root.transform, false);
            hallLightGo.transform.localPosition = new Vector3(0f, 4.2f, -1.5f);
            var hallLight = hallLightGo.AddComponent<Light>();
            hallLight.type = LightType.Point;
            hallLight.color = GameConfig.Hex("FFA24E");
            hallLight.intensity = 2.2f;
            hallLight.range = 13f;
            hallLight.shadows = LightShadows.None;
            hallLightGo.AddComponent<FlickerLight>().baseIntensity = 2.2f;

            // Ceremonial drums at the steps.
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -9.5f : 9.5f;
                WorldFactory.Primitive("drum stand", root.transform, PrimitiveType.Cube, new Vector3(x, 0.8f, -8.2f),
                    new Vector3(1.3f, 1.4f, 1.1f), null, darkWood);
                WorldFactory.Primitive("drum", root.transform, PrimitiveType.Cylinder, new Vector3(x, 1.55f, -8.2f),
                    new Vector3(0.72f, 0.55f, 0.72f), Quaternion.Euler(90f, 0f, 0f), RuntimeAssets.Toon(GameConfig.Hex("8D3228"), 0.006f));
            }
        }

        static void BuildGate(WorldRefs refs)
        {
            var root = WorldFactory.Empty("梁山泊 gate", refs.Environment, new Vector3(0f, 0f, -30f));
            Material wood = RuntimeAssets.Toon(GameConfig.Hex("412D20"), 0.008f);
            Material darkWood = RuntimeAssets.Toon(GameConfig.Hex("2C1F17"), 0.006f);
            Material roofMat = RuntimeAssets.Toon(GameConfig.Hex("2B2F32"), 0.006f);
            Material gold = RuntimeAssets.Toon(GameConfig.Gold, 0.004f);
            Material stone = RuntimeAssets.Toon(GameConfig.Hex("666157"), 0.005f);

            WorldFactory.Primitive("gate base L", root.transform, PrimitiveType.Cube, new Vector3(-4.9f, 0.3f, 0f),
                new Vector3(2.2f, 0.6f, 2.4f), null, stone);
            WorldFactory.Primitive("gate base R", root.transform, PrimitiveType.Cube, new Vector3(4.9f, 0.3f, 0f),
                new Vector3(2.2f, 0.6f, 2.4f), null, stone);
            WorldFactory.Primitive("gate pillar L", root.transform, PrimitiveType.Cylinder, new Vector3(-3.9f, 4f, 0f),
                new Vector3(0.4f, 3.4f, 0.4f), null, wood);
            WorldFactory.Primitive("gate pillar R", root.transform, PrimitiveType.Cylinder, new Vector3(3.9f, 4f, 0f),
                new Vector3(0.4f, 3.4f, 0.4f), null, wood);
            WorldFactory.Primitive("gate lintel", root.transform, PrimitiveType.Cube, new Vector3(0f, 7.2f, 0f),
                new Vector3(10.6f, 0.5f, 0.7f), null, darkWood);
            WorldFactory.Primitive("gate brace L", root.transform, PrimitiveType.Cube, new Vector3(-3.1f, 6.7f, 0f),
                new Vector3(2.4f, 0.25f, 0.45f), Quaternion.Euler(0f, 0f, -22f), darkWood);
            WorldFactory.Primitive("gate brace R", root.transform, PrimitiveType.Cube, new Vector3(3.1f, 6.7f, 0f),
                new Vector3(2.4f, 0.25f, 0.45f), Quaternion.Euler(0f, 0f, 22f), darkWood);

            var roof = new MeshBuilder(true);
            roof.GableRoof(new Vector3(0f, 7.5f, 0f), 11.6f, 4.2f, 2.2f, 1.25f, 1.1f, 8, 5, GameConfig.Hex("2F3336"), GameConfig.Hex("23272A"));
            WorldFactory.MeshObject("gate roof", root.transform, roof.Build("gate roof"), roofMat, Vector3.zero);

            WorldFactory.Primitive("gate plaque", root.transform, PrimitiveType.Cube, new Vector3(0f, 5.7f, 1.0f),
                new Vector3(4.1f, 1.1f, 0.15f), null, darkWood);
            WorldFactory.Text("梁山泊", root.transform, "梁山泊", new Vector3(0f, 5.7f, 1.09f),
                Quaternion.Euler(0f, 180f, 0f), 0.76f, GameConfig.Gold, RuntimeAssets.TitleFont, TextAnchor.MiddleCenter, TextAlignment.Center, 1);

            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -2.2f : 2.2f;
                WorldFactory.Primitive("door", root.transform, PrimitiveType.Cube, new Vector3(x * 1.8f, 2.7f, 0.35f),
                    new Vector3(3.6f, 5.0f, 0.18f), Quaternion.Euler(0f, x < 0 ? 18f : -18f, 0f), wood);
                WorldFactory.Primitive("door stud", root.transform, PrimitiveType.Sphere, new Vector3(x * 1.35f, 3.2f, 0.9f),
                    Vector3.one * 0.14f, null, gold);
            }

            var torchL = WorldProps.BuildBrazier(refs, new Vector3(-6.2f, 0f, -28.8f), 20);
            var torchR = WorldProps.BuildBrazier(refs, new Vector3(6.2f, 0f, -28.8f), 21);
            refs.BrazierPositions.Add(new Vector3(-6.2f, 0f, -28.8f));
            refs.BrazierPositions.Add(new Vector3(6.2f, 0f, -28.8f));
        }

        static void BuildPalisade(WorldRefs refs)
        {
            var builder = new MeshBuilder(true);
            Color wood = GameConfig.Hex("4A3728");
            Color woodDark = GameConfig.Hex("38291D");
            Color woodLight = GameConfig.Hex("5A4532");
            const int count = 132;
            var random = new System.Random(401);
            for (int i = 0; i < count; i++)
            {
                float angle = -Mathf.PI + i / (float)count * Mathf.PI * 2f;
                float degrees = angle * Mathf.Rad2Deg;
                if (Mathf.Abs(Mathf.DeltaAngle(degrees, -90f)) < 8.5f)
                    continue;
                float radius = 30.7f;
                float height = 2.7f + (float)random.NextDouble() * 0.9f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Quaternion rot = Quaternion.Euler(0f, -degrees, 0f);
                Color c = Color.Lerp(wood, woodLight, (float)random.NextDouble() * 0.5f);
                var points = new Vector3[8];
                float localWidth = 0.17f;
                float localDepth = 0.19f;
                Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
                // A slightly tapered log.
                builder.Box(matrix.MultiplyPoint3x4(new Vector3(0f, height * 0.5f, 0f)), new Vector3(0.33f, height, 0.38f), c);
                builder.Cone(matrix.MultiplyPoint3x4(new Vector3(0f, height, 0f)), 0.23f, 0.42f, 4, c, c * 1.1f, false);

                if (i % 4 == 0)
                {
                    builder.Box(matrix.MultiplyPoint3x4(new Vector3(0f, 1.85f, 0f)), new Vector3(0.55f, 3.7f, 0.58f), woodDark);
                }
            }

            // Horizontal rails, broken at the gate.
            for (int i = 0; i < count; i++)
            {
                float angle = -Mathf.PI + i / (float)count * Mathf.PI * 2f;
                float degrees = angle * Mathf.Rad2Deg;
                if (Mathf.Abs(Mathf.DeltaAngle(degrees, -90f)) < 9f)
                    continue;
                float radius = 31.02f;
                float segment = Mathf.PI * 2f * radius / count + 0.05f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Quaternion rot = Quaternion.Euler(0f, -degrees, 0f);
                Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
                builder.Box(matrix.MultiplyPoint3x4(new Vector3(0f, 1.15f, 0f)), new Vector3(0.14f, 0.16f, segment), woodDark);
                builder.Box(matrix.MultiplyPoint3x4(new Vector3(0f, 2.25f, 0f)), new Vector3(0.14f, 0.16f, segment), woodDark);
            }

            WorldFactory.MeshObject("palisade", refs.Environment, builder.Build("palisade"), RuntimeAssets.VertexToon(0.004f), Vector3.zero, null, null, true, true);
        }

        static void BuildArenaProps(WorldRefs refs)
        {
            // Stone posts and lanterns around the duelling ground.
            Material stone = RuntimeAssets.Toon(GameConfig.Hex("6A6458"), 0.006f);
            Material dark = RuntimeAssets.Toon(GameConfig.Hex("3A3732"), 0.004f);
            Material lanternPaper = RuntimeAssets.Toon(GameConfig.Hex("E0A15A"), GameConfig.Hex("5C2C18"), 0f);
            for (int i = 0; i < 8; i++)
            {
                float angle = i / 8f * Mathf.PI * 2f + 0.2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 23.4f, 0f, Mathf.Sin(angle) * 23.4f);
                var post = WorldFactory.Empty("Stone lantern", refs.Props, pos, Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f));
                WorldFactory.Primitive("base", post.transform, PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f),
                    new Vector3(0.85f, 0.4f, 0.85f), null, stone);
                WorldFactory.Primitive("column", post.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.9f, 0f),
                    new Vector3(0.22f, 0.5f, 0.22f), null, stone);
                WorldFactory.Primitive("lamp", post.transform, PrimitiveType.Cube, new Vector3(0f, 1.45f, 0f),
                    new Vector3(0.55f, 0.5f, 0.55f), null, lanternPaper);
                WorldFactory.Primitive("cap", post.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.88f, 0f),
                    new Vector3(0.75f, 0.15f, 0.75f), null, dark);
            }

            // Banners: eight poles, alternating calligraphy.
            string[] texts = { "替天行道", "忠义", "梁山", "聚义", "替天行道", "忠义", "梁山", "聚义" };
            Color[] cloth = { GameConfig.Vermilion, GameConfig.Hex("26343C"), GameConfig.Hex("6F2A22"), GameConfig.Hex("3D503F") };
            for (int i = 0; i < 8; i++)
            {
                float angle = i / 8f * Mathf.PI * 2f + 0.6f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 18.8f, 0f, Mathf.Sin(angle) * 18.8f);
                float facing = -angle * Mathf.Rad2Deg + 90f;
                WorldProps.BuildBanner(refs, pos, facing, texts[i], cloth[i % cloth.Length], GameConfig.PaperLight, 6.8f);
            }

            // Four braziers at the corners of the arena.
            for (int i = 0; i < 4; i++)
            {
                float angle = i / 4f * Mathf.PI * 2f + Mathf.PI * 0.25f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 16.6f, 0f, Mathf.Sin(angle) * 16.6f);
                WorldProps.BuildBrazier(refs, pos, i);
                refs.BrazierPositions.Add(pos);
            }

            // Weapon rack and supply crates near the hall.
            Material wood = RuntimeAssets.Toon(GameConfig.Hex("493626"), 0.005f);
            Material iron = RuntimeAssets.Toon(GameConfig.Hex("66676A"), 0.003f);
            var rack = WorldFactory.Empty("Weapon rack", refs.Props, new Vector3(-12.5f, 0f, 18f), Quaternion.Euler(0f, 30f, 0f));
            WorldFactory.Primitive("bar", rack.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.3f, 0f),
                new Vector3(0.1f, 1.25f, 0.1f), Quaternion.Euler(0f, 0f, 90f), wood);
            for (int i = 0; i < 5; i++)
            {
                WorldFactory.Primitive("spear", rack.transform, PrimitiveType.Cylinder, new Vector3(-1.5f + i * 0.75f, 0.95f, 0.1f),
                    new Vector3(0.035f, 1.7f, 0.035f), Quaternion.Euler(8f, 0f, 3f), wood);
                WorldFactory.Primitive("head", rack.transform, PrimitiveType.Cylinder, new Vector3(-1.5f + i * 0.75f, 2.6f, 0.05f),
                    new Vector3(0.09f, 0.18f, 0.09f), null, iron);
            }
            for (int i = 0; i < 4; i++)
            {
                WorldFactory.Primitive("crate", refs.Props, PrimitiveType.Cube,
                    new Vector3(12.2f + (i % 2) * 0.9f, 0.42f + (i / 2) * 0.8f, 17.5f + (i / 2) * 0.2f),
                    new Vector3(0.8f, 0.8f, 0.8f), Quaternion.Euler(0f, i * 17f, 0f), wood);
            }
            WorldFactory.Primitive("drum", refs.Props, PrimitiveType.Cylinder, new Vector3(9.5f, 1.3f, -16f),
                new Vector3(1.1f, 0.8f, 1.1f), Quaternion.Euler(90f, 0f, 0f), RuntimeAssets.Toon(GameConfig.Hex("8C3428"), 0.006f));
        }

        static void BuildMenuStage(WorldRefs refs)
        {
            Material dark = RuntimeAssets.Toon(GameConfig.Hex("36332B"), 0.006f);
            Material red = RuntimeAssets.Toon(GameConfig.Hex("70302A"), 0.006f);
            var dais = WorldFactory.Empty("聚义台", refs.Props, new Vector3(0f, 0f, 14f));
            WorldFactory.Primitive("dais", dais.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f),
                new Vector3(4.7f, 0.12f, 4.7f), null, dark);
            WorldFactory.Primitive("dais ring", dais.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.19f, 0f),
                new Vector3(4.25f, 0.035f, 4.25f), null, red);

            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 2.75f;
                var spot = WorldFactory.Empty("Menu hero spot", refs.Props, new Vector3(x, 0.22f, 14f), Quaternion.Euler(0f, 180f, 0f));
                refs.MenuHeroSpots.Add(spot.transform);
            }
        }

        static void BuildPostProcessing(WorldRefs refs)
        {
            var volumeGo = new GameObject("Post FX");
            volumeGo.transform.SetParent(refs.Environment);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Liangshan Post";
            volume.profile = profile;

            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.Neutral);
            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(1.0f);
            bloom.intensity.Override(0.48f);
            bloom.scatter.Override(0.65f);
            bloom.tint.Override(GameConfig.Hex("FFD7AE"));
            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(-0.04f);
            color.contrast.Override(12f);
            color.saturation.Override(10f);
            color.colorFilter.Override(GameConfig.Hex("FFF0DA"));
            var white = profile.Add<WhiteBalance>(true);
            white.temperature.Override(-2f);
            white.tint.Override(-2f);
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.22f);
            vignette.smoothness.Override(0.48f);
            vignette.color.Override(GameConfig.Hex("1A1714"));
            var grain = profile.Add<FilmGrain>(true);
            grain.type.Override(FilmGrainLookup.Medium1);
            grain.intensity.Override(0.09f);
            grain.response.Override(0.8f);
            refs.PostVolume = volume;
        }
    }
}
