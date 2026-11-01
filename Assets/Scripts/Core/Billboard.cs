using UnityEngine;

namespace Liangshan
{
    public sealed class Billboard : MonoBehaviour
    {
        public bool yawOnly = true;

        void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;
            if (yawOnly)
            {
                Vector3 forward = camera.transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
            }
            else
            {
                transform.rotation = camera.transform.rotation;
            }
        }
    }
}
