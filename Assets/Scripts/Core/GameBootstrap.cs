using UnityEngine;

namespace DessertFactory
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] GameContent content;
        [SerializeField] int mapWidth = 128;
        [SerializeField] int mapHeight = 128;
        [SerializeField] int seed = 1234;
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

            var map = new GameObject("Desert Map").AddComponent<DesertMap>();
            map.transform.SetParent(transform);
            map.Generate(mapWidth, mapHeight, content.deposits, seed);

            var factory = new GameObject("Factory").AddComponent<Factory>();
            factory.transform.SetParent(transform);
            factory.Init(map, new Stockpile(startingCoins));

            var cam = SetUpCamera(map);

            var hud = gameObject.AddComponent<Hud>();
            var builder = gameObject.AddComponent<BuildController>();
            builder.Init(factory, content, cam, hud);
            hud.Init(factory, content, builder);
        }

        Camera SetUpCamera(DesertMap map)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.tag = "MainCamera";
            }

            cam.orthographic = true;
            cam.orthographicSize = 12f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.4f, 0.28f);
            cam.transform.position = new Vector3(map.Width / 2f, map.Height / 2f, -10f);

            var controller = cam.GetComponent<CameraController>();
            if (controller == null)
                controller = cam.gameObject.AddComponent<CameraController>();
            controller.Init(new Vector2(map.Width, map.Height));
            return cam;
        }
    }
}
