using System.Collections.Generic;
using UnityEngine;

namespace Liangshan
{
    /// <summary>Small procedural mesh toolkit used for the whole island.</summary>
    public sealed class MeshBuilder
    {
        readonly List<Vector3> _vertices = new List<Vector3>(256);
        readonly List<int> _triangles = new List<int>(512);
        readonly List<Color> _colors = new List<Color>(256);
        readonly List<Vector2> _uvs = new List<Vector2>(256);
        bool _flat;

        public MeshBuilder(bool flatShading = false)
        {
            _flat = flatShading;
        }

        public int Vertex(Vector3 position, Color color, Vector2 uv)
        {
            _vertices.Add(position);
            _colors.Add(color);
            _uvs.Add(uv);
            return _vertices.Count - 1;
        }

        public void Triangle(int a, int b, int c)
        {
            _triangles.Add(a);
            _triangles.Add(b);
            _triangles.Add(c);
        }

        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
        {
            if (_flat)
            {
                FlatQuad(a, b, c, d, color);
                return;
            }

            int ia = Vertex(a, color, new Vector2(0, 0));
            int ib = Vertex(b, color, new Vector2(0, 1));
            int ic = Vertex(c, color, new Vector2(1, 1));
            int id = Vertex(d, color, new Vector2(1, 0));
            Triangle(ia, ib, ic);
            Triangle(ia, ic, id);
        }

        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color ca, Color cb, Color cc, Color cd)
        {
            if (_flat)
            {
                FlatQuad(a, b, c, d, ca);
                return;
            }

            int ia = Vertex(a, ca, new Vector2(0, 0));
            int ib = Vertex(b, cb, new Vector2(0, 1));
            int ic = Vertex(c, cc, new Vector2(1, 1));
            int id = Vertex(d, cd, new Vector2(1, 0));
            Triangle(ia, ib, ic);
            Triangle(ia, ic, id);
        }

        void FlatQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
        {
            FlatTriangle(a, b, c, color);
            FlatTriangle(a, c, d, color);
        }

        public void FlatTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            int ia = Vertex(a, color, Vector2.zero);
            int ib = Vertex(b, color, Vector2.up);
            int ic = Vertex(c, color, Vector2.one);
            Triangle(ia, ib, ic);
        }

        public void Box(Vector3 center, Vector3 size, Color color)
        {
            Vector3 h = size * 0.5f;
            Vector3 p000 = center + new Vector3(-h.x, -h.y, -h.z);
            Vector3 p001 = center + new Vector3(-h.x, -h.y, h.z);
            Vector3 p010 = center + new Vector3(-h.x, h.y, -h.z);
            Vector3 p011 = center + new Vector3(-h.x, h.y, h.z);
            Vector3 p100 = center + new Vector3(h.x, -h.y, -h.z);
            Vector3 p101 = center + new Vector3(h.x, -h.y, h.z);
            Vector3 p110 = center + new Vector3(h.x, h.y, -h.z);
            Vector3 p111 = center + new Vector3(h.x, h.y, h.z);

            Quad(p001, p101, p111, p011, color); // +Z
            Quad(p100, p000, p010, p110, color); // -Z
            Quad(p101, p100, p110, p111, color); // +X
            Quad(p000, p001, p011, p010, color); // -X
            Quad(p011, p111, p110, p010, color); // +Y
            Quad(p000, p100, p101, p001, color); // -Y
        }

        public void Disc(Vector3 center, float radius, int segments, Color inner, Color outer, float y = 0f)
        {
            for (int i = 0; i < segments; i++)
            {
                float a0 = i / (float)segments * Mathf.PI * 2f;
                float a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                Vector3 p0 = center + new Vector3(Mathf.Cos(a0) * radius, y, Mathf.Sin(a0) * radius);
                Vector3 p1 = center + new Vector3(Mathf.Cos(a1) * radius, y, Mathf.Sin(a1) * radius);
                int ic = Vertex(center, inner, new Vector2(0.5f, 0.5f));
                int i1 = Vertex(p1, outer, new Vector2(0.5f + Mathf.Cos(a1) * 0.5f, 0.5f + Mathf.Sin(a1) * 0.5f));
                int i0 = Vertex(p0, outer, new Vector2(0.5f + Mathf.Cos(a0) * 0.5f, 0.5f + Mathf.Sin(a0) * 0.5f));
                Triangle(ic, i1, i0);
            }
        }

        public void Ring(Vector3 center, float innerRadius, float outerRadius, int segments, Color innerColor, Color outerColor, float y = 0f)
        {
            for (int i = 0; i < segments; i++)
            {
                float a0 = i / (float)segments * Mathf.PI * 2f;
                float a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                Vector3 inner0 = center + new Vector3(Mathf.Cos(a0) * innerRadius, y, Mathf.Sin(a0) * innerRadius);
                Vector3 inner1 = center + new Vector3(Mathf.Cos(a1) * innerRadius, y, Mathf.Sin(a1) * innerRadius);
                Vector3 outer0 = center + new Vector3(Mathf.Cos(a0) * outerRadius, y, Mathf.Sin(a0) * outerRadius);
                Vector3 outer1 = center + new Vector3(Mathf.Cos(a1) * outerRadius, y, Mathf.Sin(a1) * outerRadius);
                Quad(inner0, inner1, outer1, outer0, innerColor, innerColor, outerColor, outerColor);
            }
        }

        public void Cone(Vector3 baseCenter, float radius, float height, int segments, Color baseColor, Color topColor, bool cap = true)
        {
            Vector3 apex = baseCenter + Vector3.up * height;
            for (int i = 0; i < segments; i++)
            {
                float a0 = i / (float)segments * Mathf.PI * 2f;
                float a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                Vector3 p0 = baseCenter + new Vector3(Mathf.Cos(a0) * radius, 0f, Mathf.Sin(a0) * radius);
                Vector3 p1 = baseCenter + new Vector3(Mathf.Cos(a1) * radius, 0f, Mathf.Sin(a1) * radius);

                if (_flat)
                {
                    FlatTriangle(p0, apex, p1, baseColor);
                }
                else
                {
                    int i0 = Vertex(p0, baseColor, new Vector2(a0 / (Mathf.PI * 2f), 0f));
                    int ia = Vertex(apex, topColor, new Vector2(0.5f, 1f));
                    int i1 = Vertex(p1, baseColor, new Vector2(a1 / (Mathf.PI * 2f), 0f));
                    Triangle(i0, ia, i1);
                }

                if (cap)
                {
                    Vector3 center = baseCenter;
                    if (_flat)
                    {
                        FlatTriangle(center, p1, p0, baseColor * 0.65f);
                    }
                    else
                    {
                        int ic = Vertex(center, baseColor * 0.65f, new Vector2(0.5f, 0.5f));
                        int i1 = Vertex(p1, baseColor * 0.65f, Vector2.zero);
                        int i0 = Vertex(p0, baseColor * 0.65f, Vector2.zero);
                        Triangle(ic, i1, i0);
                    }
                }
            }
        }

        public void Rock(Vector3 center, float radius, float height, int seed, Color color)
        {
            var random = new System.Random(seed);
            int sides = 7;
            var ring = new Vector3[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = i / (float)sides * Mathf.PI * 2f;
                float r = radius * (0.72f + (float)random.NextDouble() * 0.55f);
                ring[i] = center + new Vector3(Mathf.Cos(angle) * r, (float)random.NextDouble() * height * 0.18f, Mathf.Sin(angle) * r);
            }

            Vector3 top = center + new Vector3(
                ((float)random.NextDouble() - 0.5f) * radius * 0.35f,
                height,
                ((float)random.NextDouble() - 0.5f) * radius * 0.35f);
            Vector3 bottom = center + Vector3.down * height * 0.28f;

            Color dark = color * 0.72f;
            for (int i = 0; i < sides; i++)
            {
                int n = (i + 1) % sides;
                FlatTriangle(ring[i], top, ring[n], color);
                FlatTriangle(ring[n], bottom, ring[i], dark);
            }
        }

        /// <summary>A curved gable roof with upturned eaves, built as a closed slab.</summary>
        public void GableRoof(Vector3 center, float width, float depth, float height, float overhang, float upturn, int segmentsX, int segmentsZ, Color tileColor, Color darkTile)
        {
            float halfW = width * 0.5f + overhang;
            float halfD = depth * 0.5f + overhang;
            float thickness = 0.22f;

            float Profile(float x, float z)
            {
                float ax = Mathf.Abs(x) / halfW;
                float az = Mathf.Abs(z) / halfD;
                float slope = 1f - Mathf.Pow(az, 1.45f);
                float corner = Mathf.Pow(ax, 6f) * Mathf.Pow(az, 4f);
                return height * slope + upturn * corner + upturn * 0.35f * Mathf.Pow(az, 8f);
            }

            for (int ix = 0; ix < segmentsX; ix++)
            {
                for (int iz = 0; iz < segmentsZ; iz++)
                {
                    float x0 = Mathf.Lerp(-halfW, halfW, ix / (float)segmentsX);
                    float x1 = Mathf.Lerp(-halfW, halfW, (ix + 1) / (float)segmentsX);
                    float z0 = Mathf.Lerp(-halfD, halfD, iz / (float)segmentsZ);
                    float z1 = Mathf.Lerp(-halfD, halfD, (iz + 1) / (float)segmentsZ);

                    float y00 = Profile(x0, z0);
                    float y01 = Profile(x0, z1);
                    float y10 = Profile(x1, z0);
                    float y11 = Profile(x1, z1);

                    Vector3 p00 = center + new Vector3(x0, y00, z0);
                    Vector3 p01 = center + new Vector3(x0, y01, z1);
                    Vector3 p10 = center + new Vector3(x1, y10, z0);
                    Vector3 p11 = center + new Vector3(x1, y11, z1);
                    Vector3 d00 = p00 - Vector3.up * thickness;
                    Vector3 d01 = p01 - Vector3.up * thickness;
                    Vector3 d10 = p10 - Vector3.up * thickness;
                    Vector3 d11 = p11 - Vector3.up * thickness;

                    Color c0 = ((ix + iz) % 3 == 0) ? darkTile : tileColor;
                    Color c1 = ((ix + iz + 1) % 3 == 0) ? darkTile : tileColor;

                    Quad(p00, p01, p11, p10, c0, c0, c1, c1);
                    Quad(d00, d10, d11, d01, darkTile * 0.55f);
                }
            }

            // eave fascia
            float yFrontL = Profile(-halfW, -halfD);
            float yFrontR = Profile(halfW, -halfD);
            Quad(center + new Vector3(-halfW, yFrontL, -halfD),
                center + new Vector3(-halfW, yFrontL - thickness, -halfD),
                center + new Vector3(halfW, yFrontR - thickness, -halfD),
                center + new Vector3(halfW, yFrontR, -halfD), darkTile * 0.7f);
        }

        /// <summary>Cloth banner in local XY plane, waving along Z, with both faces.</summary>
        public void Banner(float width, float height, int segmentsX, int segmentsY, Color top, Color bottom, float wave)
        {
            for (int ix = 0; ix < segmentsX; ix++)
            {
                for (int iy = 0; iy < segmentsY; iy++)
                {
                    float u0 = ix / (float)segmentsX;
                    float u1 = (ix + 1) / (float)segmentsX;
                    float v0 = iy / (float)segmentsY;
                    float v1 = (iy + 1) / (float)segmentsY;
                    float x0 = Mathf.Lerp(-width * 0.5f, width * 0.5f, u0);
                    float x1 = Mathf.Lerp(-width * 0.5f, width * 0.5f, u1);
                    float y0 = Mathf.Lerp(-height, 0f, v0);
                    float y1 = Mathf.Lerp(-height, 0f, v1);
                    float z00 = Mathf.Sin(u0 * 7f) * wave;
                    float z10 = Mathf.Sin(u1 * 7f) * wave;
                    float z01 = Mathf.Sin(u0 * 7f + 1.2f) * wave;
                    float z11 = Mathf.Sin(u1 * 7f + 1.2f) * wave;

                    Color c0 = Color.Lerp(bottom, top, v0);
                    Color c1 = Color.Lerp(bottom, top, v1);

                    Vector3 a = new Vector3(x0, y0, z00);
                    Vector3 b = new Vector3(x0, y1, z01);
                    Vector3 c = new Vector3(x1, y1, z11);
                    Vector3 d = new Vector3(x1, y0, z10);
                    Quad(a, b, c, d, c0, c1, c1, c0);
                    Quad(a, d, c, b, c0, c0, c1, c1); // reverse face
                }
            }
        }

        public Mesh Build(string name, bool recalculateNormals = true)
        {
            var mesh = new Mesh { name = name };
            if (_vertices.Count > 65000)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(_vertices);
            mesh.SetColors(_colors);
            mesh.SetUVs(0, _uvs);
            mesh.SetTriangles(_triangles, 0);
            if (recalculateNormals)
                mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
