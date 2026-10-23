using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace GladiusAI.EditorTools
{
    // Herramienta de un solo uso: vuelve ovalada la arena en Arena 1 y Arena 2 (limite elipse + anillo).
    // Borrar este archivo despues de usarla.
    public static class AddOvalArena
    {
        private const string MaterialPath = "Assets/Art/Materiales/M-AnilloArena.mat";

        private static readonly string[] Scenes =
        {
            "Assets/Scenes/ArenaDeCombate1.unity",
            "Assets/Scenes/ArenaDeCombate2.unity",
        };

        [MenuItem("Tools/Insanus Pugnat/Arena ovalada (Arena 1 y 2)")]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string original = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            try
            {
                foreach (string path in Scenes)
                    Apply(path);
            }
            finally
            {
                if (!string.IsNullOrEmpty(original))
                    EditorSceneManager.OpenScene(original);
            }
        }

        private static void Apply(string path)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var bounds = Object.FindFirstObjectByType<ArenaBounds>(FindObjectsInactive.Include);
            if (bounds == null)
            {
                Debug.LogError($"[{scene.name}] no hay ArenaBounds.");
                return;
            }

            if (Object.FindFirstObjectByType<OvalArenaRing>(FindObjectsInactive.Include) != null)
            {
                Debug.Log($"[{scene.name}] ya tiene el anillo ovalado.");
                return;
            }

            var material = GetOrCreateMaterial();

            var so = new SerializedObject(bounds);
            so.FindProperty("elliptical").boolValue = true;
            so.ApplyModifiedProperties();

            var go = new GameObject("AnilloArena", typeof(MeshFilter), typeof(MeshRenderer), typeof(OvalArenaRing));
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            var ringSo = new SerializedObject(go.GetComponent<OvalArenaRing>());
            ringSo.FindProperty("bounds").objectReferenceValue = bounds;
            ringSo.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[{scene.name}] arena ovalada lista (limite elipse + anillo).");
        }

        private static Material GetOrCreateMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null) return material;

            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.55f, 0.43f, 0.32f);
            material.SetFloat("_Cull", 0f);
            material.SetFloat("_Smoothness", 0f);
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }
    }
}
