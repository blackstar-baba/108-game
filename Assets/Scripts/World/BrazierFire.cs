using UnityEngine;

namespace Liangshan
{
    public sealed class BrazierFire : MonoBehaviour
    {
        public Vector3 FirePoint = new Vector3(0f, 1.35f, 0f);
        public Light Light;

        void Start()
        {
            CreateFire();
            CreateSmoke();
        }

        void CreateFire()
        {
            var go = new GameObject("Fire");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = FirePoint;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.65f);
            main.startRotation = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
            main.startColor = new ParticleSystem.MinMaxGradient(GameConfig.Hex("FFC06A"), GameConfig.Hex("E4552D"));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 80;
            main.gravityModifier = -0.12f;

            var emission = ps.emission;
            emission.rateOverTime = 28f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 16f;
            shape.radius = 0.22f;
            shape.position = Vector3.zero;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var color = ps.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 0.6f), new GradientColorKey(new Color(0.4f, 0.25f, 0.2f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0.7f, 0.6f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1.1f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = RuntimeAssets.NewParticleMaterial(RuntimeAssets.GlowTexture, true, new Color(1f, 0.8f, 0.55f, 1f));
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 2;
            ps.Play();
        }

        void CreateSmoke()
        {
            var go = new GameObject("Smoke");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = FirePoint + Vector3.up * 0.4f;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 1.15f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.16f, 0.17f, 0.17f, 0.22f), new Color(0.25f, 0.24f, 0.22f, 0.1f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 40;
            main.gravityModifier = -0.03f;

            var emission = ps.emission;
            emission.rateOverTime = 5f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 22f;
            shape.radius = 0.2f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.4f, 1f, 1.5f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = RuntimeAssets.NewParticleMaterial(RuntimeAssets.SoftCircle, false, new Color(0.18f, 0.18f, 0.17f, 0.5f));
            renderer.sortingOrder = 1;
            ps.Play();
        }
    }
}
