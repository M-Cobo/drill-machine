using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class MineralObj {
    public Mineral source;
    [Range(0.0f, 1.0f)] public float percent = 0.0f;
}

[Serializable]
public class CollectableObj {
    public GameObject source;
    [Range(0.0f, 1.0f)] public float percent = 0.0f;
}
    
[Serializable]
public class WallMineral {
    public Mineral mineral;
    [Range(0, 100)] public int priority;
}

public class MapManager : MonoBehaviour
{
    [SerializeField] MapSettings[] mapsByLevel = null;
    [SerializeField] MapSettings[] lastMaps = null;
    
    [HideInInspector] public MapSettings mapSetting = null;

    [Header("Road")]
    [SerializeField] GameObject[] initialSegments = new GameObject[0];
    [SerializeField] GameObject[] roadSegments = new GameObject[0];
    [SerializeField] GameObject[] rockTypes = new GameObject[0];

    [Header("Forest")]
    [SerializeField] GameObject lake = null;
    [SerializeField] List<GameObject> forestSegments = new List<GameObject>();
    [SerializeField] List<GameObject> endSegments = new List<GameObject>();

    // Private Fields
    GameObject path = null;
    List<Transform> SP_Minerals = new List<Transform>();
    List<Transform> SP_Walls = new List<Transform>();
    System.Random random = new System.Random();

    private void Awake() 
    {
        mapSetting = GetMapSettings();
        GenerateMap();
    }

    private void GenerateMap()
    {
        if(path != null) { Destroy(path); }
        path = new GameObject("Path");

        SpawnRoad();
        SpawnForest();
        SpawnMinerals();
        SpawnWalls();
        SpawnPowerUp();
        SpawnCollectables();
    }

    private void SpawnRoad()
    {
        // Initialize Vars & Spawn an Initial Segment

        Transform spawnPoint = null;
        GameObject newInstance = null;

        GameObject lastSegment = Instantiate(
            initialSegments[UnityEngine.Random.Range(0, initialSegments.Length)],
            transform.position,
            transform.rotation
        );

        lastSegment.transform.SetParent(path.transform);


        // Spawn initial ground road

        spawnPoint = GetSpawnPoint("Road_SP", lastSegment);

        lastSegment = Instantiate(
            Array.Find(roadSegments, segment => segment.name == "Ground_Init"),
            spawnPoint.position, 
            spawnPoint.rotation
        );

        lastSegment.transform.SetParent(path.transform);


        // Spawn the road
        
        for (int i = 0; i < mapSetting.mapLength; i++)
        {
            spawnPoint = GetSpawnPoint("Road_SP", lastSegment);

            newInstance = Instantiate(
                Array.Find(roadSegments, segment => segment.name == "Ground_Path"),
                spawnPoint.position, 
                spawnPoint.rotation
            );

            newInstance.transform.SetParent(path.transform);

            Transform minerals_sps = GetSpawnPoint("Minerals_SP", newInstance);
            int rnd_sp = UnityEngine.Random.Range(0, minerals_sps.childCount);
            Transform mineral_sp = minerals_sps.GetChild(rnd_sp);

            foreach (Transform sp in mineral_sp) { SP_Minerals.Add(sp); }

            lastSegment = newInstance;
        }


        // Spawn ending ground road

        spawnPoint = GetSpawnPoint("Road_SP", lastSegment);

        newInstance = Instantiate(
            Array.Find(roadSegments, segment => segment.name == "Ground_End"),
            spawnPoint.position, 
            spawnPoint.rotation
        );

        newInstance.transform.SetParent(path.transform);

        Transform walls_sp = GetSpawnPoint("Walls_SP", newInstance);
        foreach (Transform sp in walls_sp) { SP_Walls.Add(sp); }
    }

    private void SpawnMinerals()
    {
        float mineralsQuantity = Mathf.Abs(mapSetting.mapLength * mapSetting.mineralsPerSegment);                                                                                                                                                                                                                                                                                                                                                                                                                  

        foreach (MineralObj mineral in mapSetting.minerals)
        {
            float length = Mathf.Abs(mineralsQuantity * mineral.percent);
            
            for (int i = 0; i < length; i++)                                                
            {
                int rnd = UnityEngine.Random.Range(0, SP_Minerals.Count);
                Transform spawnPoint = SP_Minerals[rnd];
                SP_Minerals.RemoveAt(rnd);

                GameObject newInstnace = Instantiate(
                    rockTypes[UnityEngine.Random.Range(0, rockTypes.Length)],
                    spawnPoint.position,
                    Quaternion.identity
                );

                newInstnace.transform.GetChild(1).GetComponent<Rock>().mineral = mineral.source;

                newInstnace.transform.SetParent(path.transform);
            }
        }
    }

    private void SpawnWalls()
    {
        int x = 4;

        if(mapSetting.wallsToSpawn > 0) { 
            x = mapSetting.wallsToSpawn; 
        }
        else if(mapSetting.wallsToSpawn == -1) { 
            x = UnityEngine.Random.Range(0, 4); 
        }
        else if(mapSetting.wallsToSpawn == 0) { return; }

        for (int i = x; i > 0; i--)
        {
            GameObject newInstnace = Instantiate(
                mapSetting.wallPrefab,
                SP_Walls[i - 1].position,
                Quaternion.identity
            );

            newInstnace.transform.GetChild(1).GetComponent<Rock>().mineral = GetWallWithMaxProb();
            newInstnace.transform.SetParent(path.transform);
        }
    }

    private void SpawnForest()
    {
        Transform lakeSP = GameObject.FindGameObjectWithTag("Lake_SP").transform;
        GameObject newInstance = Instantiate(lake, lakeSP.position, lakeSP.rotation);
        newInstance.transform.SetParent(path.transform);

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("RoadSides_SP");
        forestSegments = forestSegments.Shuffle();
        int segmentID = 0;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform spawnPoint = spawnPoints[i].transform;
            newInstance = Instantiate(forestSegments[segmentID], spawnPoint.position, spawnPoint.rotation);
            newInstance.transform.SetParent(path.transform);
            segmentID++;

            if((i + 1) % forestSegments.Count == 0) {
                forestSegments = forestSegments.Shuffle();
                segmentID = 0;
            }
        }

        spawnPoints = GameObject.FindGameObjectsWithTag("DRoadSides_SP");
        endSegments = endSegments.Shuffle();
        segmentID = 0;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform spawnPoint = spawnPoints[i].transform;
            newInstance = Instantiate(endSegments[segmentID], spawnPoint.position, spawnPoint.rotation);
            newInstance.transform.SetParent(path.transform);
            segmentID++;

            if((i + 1) % forestSegments.Count == 0) {
                endSegments = endSegments.Shuffle();
                segmentID = 0;
            }
        }
    }
    
    private void SpawnPowerUp()
    {
        for (int i = 0; i < mapSetting.powerupsToSpawn; i++)
        {
            int rnd = UnityEngine.Random.Range(0, SP_Minerals.Count);
            Transform spawnPoint = SP_Minerals[rnd];
            SP_Minerals.RemoveAt(rnd);

            GameObject newInstnace = Instantiate(
                mapSetting.powerupPrefab,
                spawnPoint.position,
                Quaternion.identity
            );

            newInstnace.transform.GetChild(1).GetComponent<Rock>().mineral = mapSetting.powerupMineral;

            newInstnace.transform.SetParent(path.transform);
        }
    }

    private void SpawnCollectables()
    {
        List<GameObject> spawnPoints = GameObject.FindGameObjectsWithTag("Collectables_SP").ToList();

        float collectablesQuantity = Mathf.Abs(mapSetting.mapLength * mapSetting.collectablesPerSegment);

        foreach (CollectableObj collectable in mapSetting.collectables)
        {
            float length = Mathf.Abs(collectablesQuantity * collectable.percent);
            
            for (int i = 0; i < length; i++)                                                
            {
                int rnd = UnityEngine.Random.Range(0, spawnPoints.Count);
                Transform spawnPoint = spawnPoints[rnd].transform;
                spawnPoints.RemoveAt(rnd);

                GameObject newInstnace = Instantiate(
                    collectable.source,
                    spawnPoint.position,
                    Quaternion.identity
                );

                newInstnace.transform.SetParent(path.transform);
            }
        }
    }

    Transform GetSpawnPoint(string name, GameObject source)
    {
        Transform spawnPoint = source.transform.Find(name);
        if(spawnPoint == null) 
        {
            Debug.LogError(string.Format("(Transform)SpawnPoint wasn't found in the Path Segment: {0}", source.name), source); 
        }
        return spawnPoint;
    }

    Mineral GetWallWithMaxProb()
    {
        int totalWeight = mapSetting.wallMinerals.Sum(t => t.priority);
        int randomNumber = random.Next(0, totalWeight);

        Mineral xMineral = null;
        foreach (WallMineral wall in mapSetting.wallMinerals)
        {
            if(randomNumber < wall.priority){
                xMineral = wall.mineral;
                break;
            }
            randomNumber -= wall.priority;
        }

        return xMineral;
    }

    MapSettings GetMapSettings() 
    {
        int xLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
        MapSettings xSettings = null;

        if(xLevel <= 18) {
            xSettings = mapsByLevel[xLevel];
        }
        else if(xLevel > 18) {
            xSettings = lastMaps[UnityEngine.Random.Range(0, lastMaps.Length)];
        }

        return xSettings;
    }
}
