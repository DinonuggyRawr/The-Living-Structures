#if UNITY_EDITOR
using System.IO;
using TLS.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TLS.Editor
{
    public static class TLSPhase1SceneBuilder
    {
        private const string ScenePath = "Assets/TheLivingStructures/Scenes/Phase1_1_Academy.unity";

        [MenuItem("Tools/The Living Structures/Build Phase 1.1 Demo Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("TLS 3D Academy Prototype");

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(11, 20, 36, 255);
            camera.fieldOfView = 48f;
            cameraObject.transform.position = new Vector3(0, 12.3f, -21f);
            cameraObject.transform.LookAt(new Vector3(0, 1.3f, 1));
            cameraObject.transform.SetParent(root.transform, true);

            GameObject lamp = new GameObject("Main light", typeof(Light));
            lamp.GetComponent<Light>().type = LightType.Directional;
            lamp.GetComponent<Light>().intensity = 1.55f;
            lamp.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            lamp.transform.SetParent(root.transform, true);
            Material ground = MakeMaterial(new Color32(18, 39, 58, 255));
            Material cyan = MakeMaterial(new Color32(34, 128, 160, 255));
            Material violet = MakeMaterial(new Color32(97, 73, 154, 255));
            Material slate = MakeMaterial(new Color32(32, 54, 75, 255));
            Cube("Academy Platform", root.transform, new Vector3(0, -0.2f, 1f), new Vector3(26, 0.35f, 20), ground);
            Cube("Central Walkway", root.transform, new Vector3(0, 0.03f, -5.1f), new Vector3(3.0f, 0.12f, 7.4f), slate);

            BuildLab(root.transform, "STACK LAB / ACTIVE", new Vector3(0, 0, 2.0f), cyan, true);
            BuildLab(root.transform, "LINKED LISTS / LATER", new Vector3(-8f, 0, 3f), violet, false);
            BuildLab(root.transform, "RECURSION / LATER", new Vector3(8f, 0, 3f), violet, false);
            BuildLab(root.transform, "TREES / LATER", new Vector3(-8f, 0, 9f), slate, false);
            BuildLab(root.transform, "GRAPHS / LATER", new Vector3(8f, 0, 9f), slate, false);
            for (int i = -3; i <= 3; i++)
                Cube("Walkway marker " + i, root.transform, new Vector3(i * 0.38f, 0.11f, -6.6f),
                    new Vector3(0.16f, 0.08f, 0.55f), cyan);

            GameObject app = new GameObject("TLS Phase 1.1 App");
            app.AddComponent<TLSAcademyApp>();
            Directory.CreateDirectory("Assets/TheLivingStructures/Scenes");
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            // Add only this scene. Never wipe unrelated scenes from Build Settings.
            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!buildScenes.Exists(item => item.path == ScenePath))
                buildScenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("TLS Phase 1.1 scene created at " + ScenePath + ". Press Play to enter the academy.");
            EditorUtility.DisplayDialog("The Living Structures", "Phase 1.1 scene created. Press Play to test the 3D hub and Stack Laboratory.\n\nRun EditMode tests before treating this as accepted.", "OK");
        }

        private static void BuildLab(Transform parent, string title, Vector3 position, Material material, bool open)
        {
            GameObject building = new GameObject(title);
            building.transform.SetParent(parent, true);
            building.transform.position = position;
            Cube("Foundation", building.transform, new Vector3(0, 0.7f, 0), new Vector3(4.3f, 1.4f, 3f), material);
            Cube("Entrance", building.transform, new Vector3(0, 1.5f, -1.56f), new Vector3(2.1f, 2.6f, 0.15f),
                MakeMaterial(open ? new Color32(56, 200, 244, 255) : new Color32(68, 79, 96, 255)));
            Cube("Upper building", building.transform, new Vector3(0, 3.1f, 0.45f), new Vector3(3.7f, 3.4f, 2.5f), material);
            Cube("Roof strip", building.transform, new Vector3(0, 5.05f, 0.4f), new Vector3(4f, 0.17f, 2.8f),
                MakeMaterial(open ? new Color32(56, 200, 244, 255) : new Color32(167, 139, 250, 255)));
        }

        private static GameObject Cube(string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localScale = scale;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            return obj;
        }

        private static Material MakeMaterial(Color color)
        {
            const string directory = "Assets/TheLivingStructures/Generated";
            if (!AssetDatabase.IsValidFolder(directory))
                AssetDatabase.CreateFolder("Assets/TheLivingStructures", "Generated");
            string path = directory + "/Mat_" + ColorUtility.ToHtmlStringRGBA(color) + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            // Prefer the shader that corresponds to the active rendering pipeline.
            bool srp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
            Shader shader = srp ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) throw new System.InvalidOperationException("No supported scene material shader was found.");
            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
#endif
