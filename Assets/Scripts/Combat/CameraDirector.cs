using UnityEngine;

namespace Liangshan
{
    public sealed class CameraDirector : MonoBehaviour
    {
        public Camera Camera;
        public Transform Target;
        public float Distance = 6.6f;
        public float MenuDistance = 7.6f;

        Game _game;
        CameraShake _shake;
        float _yaw = 180f;
        float _pitch = 14f;
        float _cinematic;
        float _cinematicZoom = 1f;
        Vector3 _velocity;
        float _menuSway;
        bool _menuMode = true;
        public float Sensitivity = 1f;

        public void Initialize(Game game, Camera camera)
        {
            _game = game;
            Camera = camera;
            _shake = new CameraShake();
            transform.SetParent(null);
            transform.position = new Vector3(7.4f, 3.55f, 3.1f);
            transform.LookAt(new Vector3(0f, 1.45f, 14.2f));
        }

        public void SetTarget(Transform target, bool menu)
        {
            Target = target;
            _menuMode = menu;
            if (!menu)
            {
                _yaw = target != null ? target.eulerAngles.y : 180f;
                _pitch = 15f;
                _velocity = Vector3.zero;
            }
        }

        public void SnapBehind()
        {
            if (Target == null) return;
            _yaw = Target.eulerAngles.y;
            _pitch = 15f;
            Vector3 focus = Target.position + Vector3.up * 1.45f;
            Quaternion orbit = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.position = focus + orbit * (Vector3.back * Distance);
            transform.rotation = Quaternion.LookRotation(focus - transform.position, Vector3.up);
            _velocity = Vector3.zero;
        }

        public void AddLook(Vector2 delta)
        {
            if (_menuMode) return;
            _yaw += delta.x * 2.6f * Sensitivity;
            _pitch -= delta.y * 2.0f * Sensitivity;
            _pitch = Mathf.Clamp(_pitch, -8f, 52f);
        }

        public void Shake(float amount)
        {
            _shake.Add(amount);
        }

        public void Cinematic(float duration, float zoom)
        {
            _cinematic = Mathf.Max(_cinematic, duration);
            _cinematicZoom = zoom;
        }

        void LateUpdate()
        {
            float dt = Time.unscaledDeltaTime;
            if (_shake == null) return;
            _shake.Update(dt);
            if (_cinematic > 0f)
                _cinematic -= dt;

            if (_menuMode)
            {
                _menuSway += dt * 0.22f;
                Vector3 pos = new Vector3(7.4f + Mathf.Sin(_menuSway) * 0.22f, 3.55f + Mathf.Sin(_menuSway * 0.7f) * 0.08f, 3.1f + Mathf.Cos(_menuSway * 0.8f) * 0.14f);
                transform.position = Vector3.SmoothDamp(transform.position, pos, ref _velocity, 0.8f, Mathf.Infinity, dt);
                Quaternion rot = Quaternion.LookRotation(new Vector3(0.25f, 1.42f, 14.2f) - transform.position, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1f - Mathf.Exp(-2.5f * dt));
                if (Camera != null) Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, 38f, 1f - Mathf.Exp(-2f * dt));
            }
            else if (Target != null)
            {
                float zoom = _cinematic > 0f ? _cinematicZoom : 1f;
                float distance = Distance * zoom;
                Vector3 focus = Target.position + Vector3.up * 1.45f;
                Quaternion orbit = Quaternion.Euler(_pitch, _yaw, 0f);
                Vector3 desired = focus + orbit * (Vector3.back * distance);
                if (desired.y < 1.2f) desired.y = 1.2f;
                float smooth = 1f - Mathf.Exp(-9f * dt);
                transform.position = Vector3.Lerp(transform.position, desired, smooth);
                Quaternion look = Quaternion.LookRotation(focus - transform.position, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, 1f - Mathf.Exp(-13f * dt));
                if (Camera != null) Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, 50f, 1f - Mathf.Exp(-3f * dt));
            }

            if (_shake.Trauma > 0f)
            {
                transform.position += _shake.Offset;
                transform.rotation *= _shake.RotationOffset;
            }
        }
    }
}
