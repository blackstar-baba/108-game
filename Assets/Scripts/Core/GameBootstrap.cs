using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Liangshan
{
    public static class GameBootstrap
    {
        public static WorldRefs World { get; private set; }
        public static Camera MainCamera { get; private set; }
        public static bool IsPreview { get; private set; }

        public static void SetMainCamera(Camera camera) { MainCamera = camera; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;

            // The project boots from an empty scene; clear any template leftovers.
            Scene scene = SceneManager.GetActiveScene();
            foreach (GameObject go in scene.GetRootGameObjects())
                UnityEngine.Object.Destroy(go);

            RuntimeAssets.Initialize();

            string[] args = Environment.GetCommandLineArgs();
            var root = new GameObject("Liangshan 108");
            if (HasArg(args, "-chartest"))
            {
                CharacterTest.Build(root.transform);
                return;
            }
            World = WorldBuilder.Build();
            World.Root.transform.SetParent(root.transform, false);

            SetupCamera(root.transform);
            SetupInputAndCursor();

            if (HasArg(args, "-worldpreview"))
            {
                IsPreview = true;
                SpawnPreviewCast();
                var preview = root.AddComponent<WorldPreview>();
                preview.StartCoroutine(preview.Capture(GetArg(args, "-shotdir", "/tmp/liangshan-shots")));
            }
            else
            {
                var game = root.AddComponent<Game>();
                game.Initialize(World, MainCamera);
            }
        }

        static void SpawnPreviewCast()
        {
            if (World == null || World.MenuHeroSpots == null) return;
            for (int i = 0; i < HeroDatabase.Playable.Count && i < World.MenuHeroSpots.Count; i++)
            {
                var hero = HeroDatabase.Playable[i];
                var spot = World.MenuHeroSpots[i];
                var rig = CharacterFactory.BuildHero(spot, hero, Vector3.zero, Quaternion.identity);
                rig.transform.localPosition = Vector3.zero;
                rig.transform.localRotation = Quaternion.Euler(0f, i == 0 ? 6f : (i == 2 ? -6f : 0f), 0f);
            }
            for (int i = 0; i < 5; i++)
            {
                float angle = -0.8f + i * 0.4f;
                var pos = new Vector3(Mathf.Cos(angle) * 8.5f, 0f, -2f + Mathf.Sin(angle) * 6f);
                var def = EnemyDefinition.Get((EnemyType)(i % 4));
                var rig = CharacterFactory.BuildEnemy(World.Characters, def, pos, Quaternion.Euler(0f, 180f + i * 18f, 0f));
                rig.transform.localScale *= 0.95f;
            }
        }

        static void SetupCamera(Transform parent)
        {
            var cameraGo = new GameObject("Main Camera");
            cameraGo.transform.SetParent(parent, false);
            cameraGo.tag = "MainCamera";
            MainCamera = cameraGo.AddComponent<Camera>();
            MainCamera.clearFlags = CameraClearFlags.Skybox;
            MainCamera.backgroundColor = GameConfig.Night;
            MainCamera.fieldOfView = 54f;
            MainCamera.nearClipPlane = 0.08f;
            MainCamera.farClipPlane = 650f;
            MainCamera.allowHDR = true;
            MainCamera.allowMSAA = true;
            MainCamera.depth = 0;
            cameraGo.AddComponent<AudioListener>();
            var data = cameraGo.AddComponent<UniversalAdditionalCameraData>();
            string[] args = Environment.GetCommandLineArgs();
            data.renderPostProcessing = !HasArg(args, "-nopost");
            data.renderShadows = true;
            data.antialiasing = AntialiasingMode.None;
            data.antialiasingQuality = AntialiasingQuality.High;
            data.requiresDepthOption = CameraOverrideOption.On;
            data.requiresColorOption = CameraOverrideOption.On;
            if (HasArg(args, "-debugcolor"))
            {
                MainCamera.clearFlags = CameraClearFlags.SolidColor;
                MainCamera.backgroundColor = new Color(0.15f, 0.6f, 0.9f, 1f);
                RenderSettings.skybox = null;
            }
            cameraGo.transform.position = new Vector3(7.2f, 3.6f, 3.2f);
            cameraGo.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 1.3f, 14f) - cameraGo.transform.position, Vector3.up);
            Debug.Log("[Liangshan] Skybox=" + (RenderSettings.skybox != null ? RenderSettings.skybox.shader.name : "null") + " post=" + data.renderPostProcessing);
        }

        static void SetupInputAndCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            InputReader.Enabled = false;
            InputReader.MouseSensitivity = PlayerPrefs.GetFloat("Liangshan.Sensitivity", 1f);
        }

        static bool HasArg(string[] args, string arg)
        {
            for (int i = 0; i < args.Length; i++)
                if (string.Equals(args[i], arg, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        static string GetArg(string[] args, string arg, string fallback)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], arg, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            return fallback;
        }
    }

    /// <summary>Developer capture mode used to render the stills in the README.</summary>
    public sealed class WorldPreview : MonoBehaviour
    {
        public IEnumerator Capture(string directory)
        {
            Directory.CreateDirectory(directory);
            yield return new WaitForSeconds(1.2f);

            Camera cam = GameBootstrap.MainCamera;
            Vector3[] positions =
            {
                new Vector3(7.2f, 3.6f, 3.2f),
                new Vector3(-7.6f, 4.4f, 4.6f),
                new Vector3(0f, 26f, -4f),
                new Vector3(0f, 3.0f, 20.5f),
                new Vector3(0f, 2.5f, 0f),
                new Vector3(0f, 13f, -46f),
                new Vector3(0f, 3.0f, -18f),
                new Vector3(4.2f, 1.55f, 10.2f),
                new Vector3(-3.8f, 1.75f, 9.6f),
                new Vector3(0f, 2.0f, 6.3f)
            };
            Vector3[] targets =
            {
                new Vector3(0f, 1.3f, 14f),
                new Vector3(0f, 1.5f, 14f),
                new Vector3(0f, 0f, 10f),
                new Vector3(0f, 2.2f, -18f),
                new Vector3(0f, 36f, 2f),
                new Vector3(0f, 8f, 24f),
                new Vector3(0f, 4.2f, -30f),
                new Vector3(0f, 1.15f, 14.2f),
                new Vector3(0f, 1.25f, 14.1f),
                new Vector3(0f, 1.15f, 14.2f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                cam.transform.position = positions[i];
                cam.transform.rotation = i >= 7
                    ? Quaternion.LookRotation(new Vector3((i == 7 ? -1.8f : (i == 8 ? 1.8f : 0f)), 1.2f, 14.1f) - positions[i], Vector3.up)
                    : (i == 4 ? Quaternion.Euler(-55f, 0f, 0f)
                    : Quaternion.LookRotation(targets[i] - positions[i], Vector3.up));
                yield return new WaitForSeconds(0.45f);
                string path = Path.Combine(directory, "world-" + i + ".png");
                ScreenCapture.CaptureScreenshot(path);
                yield return new WaitForSeconds(0.45f);
                Debug.Log("[Liangshan] captured " + path);
            }

            Debug.Log("[Liangshan] preview complete");
            yield return new WaitForSeconds(0.3f);
            Application.Quit();
        }
    }
}
