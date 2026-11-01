using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Liangshan
{
    public sealed class WorldRefs
    {
        public GameObject Root;
        public Transform Environment;
        public Transform Props;
        public Transform Characters;
        public Transform Effects;
        public Transform Sun;
        public Transform Fill;
        public Volume PostVolume;
        public Vector3 ArenaCenter = Vector3.zero;
        public Vector3 MenuStage = new Vector3(0f, 0.15f, 14f);
        public Vector3 PlayerSpawn = new Vector3(0f, 0f, 7.5f);
        public Vector3 HallCenter = new Vector3(0f, 0f, 27.5f);
        public readonly List<Vector3> EnemySpawnPoints = new List<Vector3>();
        public readonly List<Vector3> BrazierPositions = new List<Vector3>();
        public readonly List<Light> Fires = new List<Light>();
        public readonly List<Transform> MenuHeroSpots = new List<Transform>();
        public readonly List<Transform> AwayPoints = new List<Transform>();

        public Vector3 RandomSpawn()
        {
            if (EnemySpawnPoints.Count == 0)
                return new Vector3(0f, 0f, -28f);
            return EnemySpawnPoints[Random.Range(0, EnemySpawnPoints.Count)];
        }
    }
}
