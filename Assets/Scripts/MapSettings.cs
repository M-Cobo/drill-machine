using UnityEngine;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "Map Setting")]
public class MapSettings : ScriptableObject
{
    public int mapLength = 10;
    public int3 renderDistance = new int3(3, 3, 3);

    [Header("Minerals")]
    [Range(0, 5)] public float mineralsPerSegment = 2;
    public MineralObj[] minerals = new MineralObj[0];

    [Header("Walls")]
    [Range(-1, 3)] public int wallsToSpawn = 2;
    public GameObject wallPrefab = null;
    public WallMineral[] wallMinerals = new WallMineral[0];


    [Header("Collectables")]
    [Range(0, 10)] public float collectablesPerSegment = 2;
    public CollectableObj[] collectables = new CollectableObj[0];

    [Header("Power-Up")]
    public int powerupsToSpawn = 2;
    public Mineral powerupMineral = null;
    public GameObject powerupPrefab = null;
}
