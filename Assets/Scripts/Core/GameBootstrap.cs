using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace DessertFactory
{
    public class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Leave empty to use the built in placeholder content")]
        [SerializeField] GameContent content;
        [Tooltip("Optional hand made setup placed for free at the start")]
        [SerializeField] FactoryLayout startingLayout;
        [Tooltip("Uses the DesertMap in the scene if there is one, otherwise makes one")]
        [SerializeField] DesertMap map;
        [Tooltip("Falls back to Resources/GameHud")]
        [SerializeField] Hud hudPrefab;

        [SerializeField] int mapWidth = 128;
        [SerializeField] int mapHeight = 128;
        [SerializeField] int seed = 1234;
        [SerializeField] bool scatterDeposits = true;
        [SerializeField] int startingCoins = 300;

        // Lets you just hit play in any scene without setting anything up
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void CreateIfMissing()
        {
            if (FindAnyObjectByType<GameBootstrap>() == null)
                new GameObject("Game").AddComponent<GameBootstrap>();
        }

        void Start()
        {
            if (content == null)
                content = GameContent.CreateDefault();

            if (map == null)
                map = FindAnyObjectByType<DesertMap>();
            if (map == null)
            {
                var gridObject = new GameObject("Grid", typeof(Grid));
                map = gridObject.AddComponent<DesertMap>();
            }
            map.Generate(mapWidth, mapHeight, content.deposits, seed, scatterDeposits);

            var factory = new GameObject("Factory").AddComponent<Factory>();
            factory.transform.SetParent(transform);
            factory.Init(map, new Stockpile(startingCoins));

            if (startingLayout != null)
                startingLayout.Apply(map, factory);

            var cam = SetUpCamera(map);

            if (hudPrefab == null)
                hudPrefab = Resources.Load<Hud>("GameHud");
            var hud = Instantiate(hudPrefab);
            hud.name = hudPrefab.name;
            EnsureEventSystem();

            var builder = gameObject.AddComponent<BuildController>();
            builder.Init(factory, content, cam, hud);
            hud.Init(factory, content, builder);
        }

        static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            DontDestroyOnLoad(go);
        }

        Camera SetUpCamera(DesertMap desert)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.tag = "MainCamera";
            }

            var worldSize = desert.WorldSize;
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.4f, 0.28f);
            cam.transform.position = new Vector3(worldSize.x / 2f, worldSize.y / 2f, -10f);
            cam.orthographicSize = Mathf.Min(12f, worldSize.y / 2f + 1f);

            var controller = cam.GetComponent<CameraController>();
            if (controller == null)
                controller = cam.gameObject.AddComponent<CameraController>();
            controller.Init(worldSize, desert.Grid.cellSize.y);
            return cam;
        }
    }
}
