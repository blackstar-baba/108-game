using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Liangshan
{
    /// <summary>Isolated turntable used to validate the procedural rig and palettes.</summary>
    public static class CharacterTest
    {
        public static void Build(Transform parent)
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = GameConfig.Hex("9AA6B0");
            RenderSettings.ambientEquatorColor = GameConfig.Hex("8C8170");
            RenderSettings.ambientGroundColor = GameConfig.Hex("3A3632");
            RenderSettings.ambientIntensity = 0.8f;
            RenderSettings.fog = false;

            var cameraGo = new GameObject("Test Camera");
            cameraGo.transform.SetParent(parent, false);
            cameraGo.tag = "MainCamera";
            var cam = cameraGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = GameConfig.Hex("5C646B");
            cam.fieldOfView = 36f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 60f;
            cam.allowHDR = true;
            cameraGo.AddComponent<AudioListener>();
            var data = cameraGo.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = false;
            data.renderShadows = true;
            GameBootstrap.SetMainCamera(cam);

            var sunGo = new GameObject("Test Key");
            sunGo.transform.SetParent(parent, false);
            sunGo.transform.rotation = Quaternion.Euler(34f, 28f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = GameConfig.Hex("FFE2C0");
            sun.intensity = 1.8f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.72f;
            sunGo.AddComponent<UniversalAdditionalLightData>();

            var fillGo = new GameObject("Test Fill");
            fillGo.transform.SetParent(parent, false);
            fillGo.transform.rotation = Quaternion.Euler(18f, 208f, 0f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = GameConfig.Hex("A9C4D6");
            fill.intensity = 0.85f;
            fill.shadows = LightShadows.None;
            fillGo.AddComponent<UniversalAdditionalLightData>();

            var ground = WorldFactory.Primitive("Test ground", parent, PrimitiveType.Plane, Vector3.zero,
                new Vector3(14f, 1f, 14f), null, RuntimeAssets.Toon(GameConfig.Hex("6A6A66"), 0f), false, false);
            ground.GetComponent<MeshRenderer>().receiveShadows = true;

            for (int i = 0; i < HeroDatabase.Playable.Count; i++)
            {
                var hero = HeroDatabase.Playable[i];
                float x = (i - 1) * 1.5f;
                CharacterFactory.BuildHero(parent, hero, new Vector3(x, 0.02f, 0f), Quaternion.Euler(0f, 180f, 0f));
            }
            var test = parent.gameObject.AddComponent<CharacterTestRunner>();
            test.StartCoroutine(test.Capture("/tmp/chartest"));
        }
    }

    public sealed class CharacterTestRunner : MonoBehaviour
    {
        public IEnumerator Capture(string directory)
        {
            Directory.CreateDirectory(directory);
            yield return new WaitForSeconds(1.4f);
            Camera cam = GameBootstrap.MainCamera;
            Vector3[] pos =
            {
                new Vector3(0f, 1.10f, -4.7f),
                new Vector3(-2.6f, 1.45f, -3.6f),
                new Vector3(3.0f, 1.1f, -3.2f),
                new Vector3(0f, 1.2f, 4.3f),
                new Vector3(0f, 1.15f, -2.8f)
            };
            Vector3[] look =
            {
                new Vector3(0f, 1.05f, 0f),
                new Vector3(0f, 1.0f, 0f),
                new Vector3(0f, 1.0f, 0f),
                new Vector3(0f, 1.0f, 0f),
                new Vector3(0f, 1.2f, 0f)
            };
            for (int i = 0; i < pos.Length; i++)
            {
                cam.transform.position = pos[i];
                cam.transform.rotation = Quaternion.LookRotation(look[i] - pos[i], Vector3.up);
                yield return new WaitForSeconds(0.4f);
                string path = Path.Combine(directory, "hero-" + i + ".png");
                ScreenCapture.CaptureScreenshot(path);
                yield return new WaitForSeconds(0.4f);
            }
            yield return new WaitForSeconds(0.3f);
            Application.Quit();
        }
    }
}
