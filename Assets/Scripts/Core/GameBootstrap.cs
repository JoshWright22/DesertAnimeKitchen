using UnityEngine;

namespace DessertFactory
{
    public class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Leave empty to use the built in placeholder content")]
        [SerializeField] GameContent content;
        [Tooltip("Only used by the placeholder content, one of each kind of building")]
        [SerializeField] Building[] placeholderPrefabs;
        [Tooltip("Optional hand made setup placed for free at the start")]
        [SerializeField] FactoryLayout startingLayout;

        [SerializeField] DesertMap map;
        [SerializeField] Factory factory;
        [SerializeField] BuildController builder;
        [SerializeField] Hud hud;
        [SerializeField] Gacha gacha;
        [SerializeField] Camera cam;

        [SerializeField] int mapWidth = 128;
        [SerializeField] int mapHeight = 128;
        [SerializeField] int seed = 1234;
        [SerializeField] bool scatterDeposits = true;

        void Start()
        {
            if (content == null)
                content = GameContent.CreateDefault(placeholderPrefabs);

            map.Generate(mapWidth, mapHeight, content.deposits, seed, scatterDeposits);
            gacha.Init(content);
            if (startingLayout != null)
                startingLayout.Apply(map, factory);

            // start over the middle of the map
            var worldSize = map.WorldSize;
            cam.transform.position = new Vector3(worldSize.x / 2f, worldSize.y / 2f, -10f);
            cam.orthographicSize = Mathf.Min(12f, worldSize.y / 2f + 1f);

            builder.Init(content);
            hud.Init(factory, content, builder);
        }
    }
}
