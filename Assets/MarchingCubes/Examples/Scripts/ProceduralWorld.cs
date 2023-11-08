using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// A procedurally generated world
    /// </summary>
    public class ProceduralWorld : World
    {
        /// <summary>
        /// The procedural terrain generation settings
        /// </summary>
        [SerializeField] private ProceduralTerrainSettings proceduralTerrainSettings = new ProceduralTerrainSettings(1, 16, 50, -40);

        /// <summary>
        /// The "radius" of the chunks the player sees
        /// </summary>
        private int3 renderDistance = new int3(3, 3, 3);

        /// <summary>
        /// The viewer which the terrain is generated around
        /// </summary>
        [SerializeField] private Transform spawnpoint = null;

        /// <summary>
        /// The point where terrain was last generated around
        /// </summary>
        private Vector3 _startPos;

        /// <summary>
        /// The pooled chunks that can be moved to a new position
        /// </summary>
        private Queue<ProceduralChunk> _availableChunks;

        /// <summary>
        /// The procedural terrain generation settings
        /// </summary>
        public ProceduralTerrainSettings ProceduralTerrainSettings => proceduralTerrainSettings;

        private void Awake()
        {
            _availableChunks = new Queue<ProceduralChunk>();
            renderDistance = GameObject.FindObjectOfType<MapManager>().mapSetting.renderDistance;
        }

        protected override void Start()
        {
            base.Start();

            transform.position = GameObject.FindGameObjectWithTag("Finish").transform.position;
            GenerateNewTerrain(spawnpoint.position);
        }

        /// <summary>
        /// Generates new procedural terrain
        /// </summary>
        /// <param name="sp_Pos">The position to generate the terrain around</param>
        private void GenerateNewTerrain(Vector3 sp_Pos)
        {
            int3 playerCoordinate = WorldPositionToCoordinate(sp_Pos);

            _availableChunks.Clear();

            foreach (var chunk in Chunks.Values.ToList())
            {
                int xOffset = Mathf.Abs(chunk.Coordinate.x - playerCoordinate.x);
                int yOffset = Mathf.Abs(chunk.Coordinate.y - playerCoordinate.y);
                int zOffset = Mathf.Abs(chunk.Coordinate.z - playerCoordinate.z);

                if (xOffset > renderDistance.x || yOffset > renderDistance.y || zOffset > renderDistance.z)
                {
                    // This conversion always succeeds because we are always adding only ProceduralChunks to Chunks.
                    _availableChunks.Enqueue(chunk as ProceduralChunk);
                    Chunks.Remove(chunk.Coordinate);
                }
            }

            for (int x = -renderDistance.x; x <= renderDistance.x; x++)
            {
                for (int y = -renderDistance.y; y <= renderDistance.y; y++)
                {
                    for (int z = -renderDistance.z; z <= 0; z++)
                    {
                        int3 chunkCoordinate = playerCoordinate + new int3(x, y, z);

                        // Make sure that there is a chunk at that coordinate
                        bool chunkExists = GetOrCreateChunk(chunkCoordinate, out Chunk chunk);
                        if (!chunkExists)
                        {
                            Chunks.Add(chunkCoordinate, chunk);
                        }
                    }
                }
            }

            _startPos = sp_Pos;
        }
        
        /// <summary>
        /// Ensures that a chunk exists at a coordinate. It either moves (from availableChunks) or instantiates a chunk
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <param name="chunk">The chunk at that position</param>
        /// <returns>Does a chunk already exist there</returns>
        private bool GetOrCreateChunk(int3 chunkCoordinate, out Chunk chunk)
        {
            if (Chunks.TryGetValue(chunkCoordinate, out Chunk existingChunk))
            {
                chunk = existingChunk;
                return true;
            }

            if (_availableChunks.Count > 0)
            {
                ProceduralChunk proceduralChunk = _availableChunks.Dequeue();
                proceduralChunk.SetCoordinate(chunkCoordinate);
                chunk = proceduralChunk;
                return false;
            }

            chunk = CreateChunk(chunkCoordinate);
            return false;
        }

        /// <summary>
        /// Instantiates a chunk at a coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <returns>The created chunk</returns>
        private ProceduralChunk CreateChunk(int3 chunkCoordinate)
        {
            ProceduralChunk chunk = Instantiate(ChunkPrefab, (chunkCoordinate * ChunkSize).ToVectorInt(), Quaternion.identity).GetComponent<ProceduralChunk>();
            chunk.World = this;
            chunk.Initialize(ChunkSize, Isolevel, chunkCoordinate);
            chunk.transform.SetParent(transform);

            return chunk;
        }
    }
}