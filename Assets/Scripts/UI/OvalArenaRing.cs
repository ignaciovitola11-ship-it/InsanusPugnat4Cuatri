using UnityEngine;

namespace GladiusAI
{
    // Anillo que tapa las esquinas del piso rectangular y levanta un muro bajo siguiendo la elipse del
    // ArenaBounds: la arena se ve ovalada sin tocar el piso original. Se arma solo al abrir la escena.
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class OvalArenaRing : MonoBehaviour
    {
        [SerializeField] private ArenaBounds bounds;
        [SerializeField] private float wallHeight = 0.8f;
        [SerializeField] private float outerScale = 1.6f;
        [SerializeField] private int segments = 64;

        private Mesh mesh;

        private void OnEnable() => Build();
        private void OnValidate() => Build();

        private void OnDisable()
        {
            if (mesh != null) DestroyImmediate(mesh);
        }

        private void Build()
        {
            if (bounds == null || segments < 8) return;

            if (mesh == null)
            {
                mesh = new Mesh { name = "OvalArenaRing", hideFlags = HideFlags.HideAndDontSave };
                GetComponent<MeshFilter>().sharedMesh = mesh;
            }

            transform.SetPositionAndRotation(bounds.Center + Vector3.up * 0.03f, Quaternion.identity);
            transform.localScale = Vector3.one;

            float rx = bounds.Size.x / 2f;
            float rz = bounds.Size.z / 2f;
            var vertices = new Vector3[(segments + 1) * 3];
            var triangles = new int[segments * 12];

            for (int i = 0; i <= segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                float c = Mathf.Cos(a), s = Mathf.Sin(a);
                vertices[i] = new Vector3(c * rx, 0f, s * rz);
                vertices[segments + 1 + i] = new Vector3(c * rx * outerScale, 0f, s * rz * outerScale);
                vertices[2 * (segments + 1) + i] = new Vector3(c * rx, wallHeight, s * rz);
            }

            int t = 0;
            for (int i = 0; i < segments; i++)
            {
                int inner = i, outer = segments + 1 + i, top = 2 * (segments + 1) + i;

                // Anillo plano (de la elipse hacia afuera).
                triangles[t++] = inner; triangles[t++] = outer + 1; triangles[t++] = outer;
                triangles[t++] = inner; triangles[t++] = inner + 1; triangles[t++] = outer + 1;
                // Muro vertical sobre el borde interior.
                triangles[t++] = inner; triangles[t++] = top; triangles[t++] = top + 1;
                triangles[t++] = inner; triangles[t++] = top + 1; triangles[t++] = inner + 1;
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}
