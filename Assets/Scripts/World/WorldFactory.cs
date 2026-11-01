using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Liangshan
{
    public static class WorldFactory
    {
        static readonly List<GameObject> Temp = new List<GameObject>();

        public static GameObject Empty(string name, Transform parent, Vector3 position, Quaternion? rotation = null, Vector3? scale = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation ?? Quaternion.identity;
            go.transform.localScale = scale ?? Vector3.one;
            return go;
        }

        public static GameObject MeshObject(string name, Transform parent, Mesh mesh, Material material,
            Vector3 position, Quaternion? rotation = null, Vector3? scale = null, bool castShadows = true, bool receiveShadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation ?? Quaternion.identity;
            go.transform.localScale = scale ?? Vector3.one;
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = receiveShadows;
            return go;
        }

        public static GameObject Primitive(string name, Transform parent, PrimitiveType type, Vector3 position,
            Vector3? scale = null, Quaternion? rotation = null, Material material = null, bool collider = false,
            bool castShadows = true)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation ?? Quaternion.identity;
            go.transform.localScale = scale ?? Vector3.one;
            if (!collider)
            {
                var col = go.GetComponent<Collider>();
                if (col != null)
                    Object.Destroy(col);
            }
            var renderer = go.GetComponent<MeshRenderer>();
            if (material != null)
                renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            return go;
        }

        public static TextMesh Text(string name, Transform parent, string content, Vector3 position, Quaternion rotation,
            float characterSize, Color color, Font font = null, TextAnchor anchor = TextAnchor.MiddleCenter,
            TextAlignment alignment = TextAlignment.Center, int sortingOrder = 0)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            var text = go.AddComponent<TextMesh>();
            text.font = font != null ? font : RuntimeAssets.BodyFont;
            text.fontSize = 72;
            text.characterSize = characterSize;
            text.anchor = anchor;
            text.alignment = alignment;
            text.lineSpacing = 0.92f;
            text.tabSize = 4f;
            text.richText = false;
            text.color = color;
            text.text = content;
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = text.font.material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.sortingOrder = sortingOrder;
            }
            return text;
        }

        public static GameObject Panel(Transform parent, string name, Vector3 position, Quaternion rotation, Vector2 size, Material material, float thickness = 0.04f)
        {
            var go = Primitive(name, parent, PrimitiveType.Cube, position, new Vector3(size.x, size.y, thickness), rotation, material);
            return go;
        }

        public static void AddCollider(GameObject go, bool convex = false)
        {
            var meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                var collider = go.AddComponent<MeshCollider>();
                collider.sharedMesh = meshFilter.sharedMesh;
                collider.convex = convex;
            }
        }

        public static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }
    }
}
