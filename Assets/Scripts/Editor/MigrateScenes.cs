using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GladiusAI.EditorTools
{
    // Herramienta de un solo uso: carga la tabla del GladiatorFactory nuevo en las escenas que lo tienen
    // y agrega el limite de arena (ArenaBounds) a Arena 1 y Arena 2. Borrar este archivo despues de usarla.
    public static class MigrateScenes
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Gladiator.prefab";
        private const string EnemyPrefabPath = "Assets/Prefabs/GladiatorEnemy.prefab";

        private static readonly string[] FactoryScenes =
        {
            "Assets/Scenes/ArenaDeCombate1.unity",
            "Assets/Scenes/ArenaDeCombate2.unity",
            "Assets/Scenes/AI.unity",
            "Assets/Scenes/EscenaDePrueba.unity",
        };

        private static readonly string[] ArenaScenes =
        {
            "Assets/Scenes/ArenaDeCombate1.unity",
            "Assets/Scenes/ArenaDeCombate2.unity",
        };

        [MenuItem("Tools/Insanus Pugnat/Migrar escenas (factory + limite de arena)")]
        public static void Run()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("Sali del Play mode antes de correr la migracion.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (LoadPrefab(PlayerPrefabPath) == null || LoadPrefab(EnemyPrefabPath) == null)
            {
                Debug.LogError($"No pude cargar los prefabs ({PlayerPrefabPath} / {EnemyPrefabPath}). No se cambio nada.");
                return;
            }

            string originalScene = SceneManager.GetActiveScene().path;

            try
            {
                foreach (string path in FactoryScenes)
                    MigrateScene(path);
            }
            finally
            {
                if (!string.IsNullOrEmpty(originalScene))
                    EditorSceneManager.OpenScene(originalScene);
            }

            Debug.Log("Migracion terminada. Revisa los mensajes de arriba (uno por escena).");
        }

        private static GladiatorNPC LoadPrefab(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return asset != null ? asset.GetComponent<GladiatorNPC>() : null;
        }

        private static void MigrateScene(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Debug.LogWarning($"[{path}] no existe, se saltea.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            bool factoryChanged = MigrateFactory(scene.name);
            bool boundsAdded = System.Array.IndexOf(ArenaScenes, path) >= 0 && AddArenaBounds(scene.name);

            if (factoryChanged || boundsAdded)
            {
                if (!EditorSceneManager.SaveScene(scene))
                    Debug.LogError($"[{scene.name}] no se pudo guardar.");
            }
        }

        private static bool MigrateFactory(string sceneName)
        {
            var factory = Object.FindFirstObjectByType<GladiatorFactory>(FindObjectsInactive.Include);
            if (factory == null)
            {
                Debug.LogWarning($"[{sceneName}] no tiene GladiatorFactory.");
                return false;
            }

            // Los prefabs se cargan DESPUES de abrir la escena: al abrir una escena en modo Single Unity
            // descarta los assets que nadie referencia, y una referencia cargada antes quedaba en blanco.
            var playerPrefab = LoadPrefab(PlayerPrefabPath);
            var enemyPrefab = LoadPrefab(EnemyPrefabPath);
            if (playerPrefab == null || enemyPrefab == null)
            {
                Debug.LogError($"[{sceneName}] no pude cargar los prefabs, no se toca el factory.");
                return false;
            }

            var so = new SerializedObject(factory);
            var rows = so.FindProperty("prefabs");

            if (rows.arraySize == 2 &&
                rows.GetArrayElementAtIndex(0).FindPropertyRelative("prefab").objectReferenceValue != null &&
                rows.GetArrayElementAtIndex(1).FindPropertyRelative("prefab").objectReferenceValue != null)
            {
                Debug.Log($"[{sceneName}] el factory ya tenia su tabla, no se toca.");
                return false;
            }

            rows.arraySize = 2;
            SetRow(rows.GetArrayElementAtIndex(0), GladiatorKind.Player, playerPrefab);
            SetRow(rows.GetArrayElementAtIndex(1), GladiatorKind.Enemy, enemyPrefab);
            so.ApplyModifiedProperties();

            var saved = new SerializedObject(factory).FindProperty("prefabs");
            bool stuck = saved.arraySize == 2 &&
                saved.GetArrayElementAtIndex(0).FindPropertyRelative("prefab").objectReferenceValue != null &&
                saved.GetArrayElementAtIndex(1).FindPropertyRelative("prefab").objectReferenceValue != null;
            if (!stuck)
            {
                Debug.LogError($"[{sceneName}] la tabla del factory NO quedo con los prefabs asignados.");
                return false;
            }

            Debug.Log($"[{sceneName}] tabla del factory cargada (Player y Enemy).");
            return true;
        }

        private static void SetRow(SerializedProperty row, GladiatorKind kind, GladiatorNPC prefab)
        {
            row.FindPropertyRelative("kind").enumValueIndex = (int)kind;
            row.FindPropertyRelative("prefab").objectReferenceValue = prefab;
        }

        private static bool AddArenaBounds(string sceneName)
        {
            if (Object.FindFirstObjectByType<ArenaBounds>(FindObjectsInactive.Include) != null)
            {
                Debug.Log($"[{sceneName}] ya tiene ArenaBounds, no se agrega otro.");
                return false;
            }

            var floor = GameObject.Find("Suelo");
            var floorRenderer = floor != null ? floor.GetComponent<Renderer>() : null;
            if (floorRenderer == null)
            {
                Debug.LogWarning($"[{sceneName}] no encontre el objeto 'Suelo' con Renderer: no se agrego el limite.");
                return false;
            }

            Bounds area = floorRenderer.bounds;

            var go = new GameObject("ArenaBounds");
            go.transform.position = area.center;
            var arena = go.AddComponent<ArenaBounds>();

            var so = new SerializedObject(arena);
            so.FindProperty("size").vector3Value = new Vector3(area.size.x, 0f, area.size.z);
            so.ApplyModifiedProperties();

            Debug.Log($"[{sceneName}] limite de arena agregado: centro {area.center}, tamano {area.size.x:F1} x {area.size.z:F1}.");
            return true;
        }
    }
}
