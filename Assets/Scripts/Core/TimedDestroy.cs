using UnityEngine;

namespace Liangshan
{
    public sealed class TimedDestroy : MonoBehaviour
    {
        public float lifetime = 1f;

        void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}
