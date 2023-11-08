using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// The terrain deformer which modifies the terrain
    /// </summary>
    public class TerrainDeformer : MonoBehaviour
    {
        /// <summary>
        /// Does the left mouse button add or remove terrain
        /// </summary>
        [Header("Terrain Deforming Settings")]
        [SerializeField] private bool leftClickAddsTerrain = true;
        [SerializeField] bool constantDeform = false;

        /// <summary>
        /// How fast the terrain is deformed
        /// </summary>
        [SerializeField] private float deformSpeed = 0.1f;

        /// <summary>
        /// How far the deformation can reach
        /// </summary>
        [SerializeField] private float deformRange = 3f;

        /// <summary>
        /// The world the will be deformed
        /// </summary>
        [Header("Player Settings")]
        [SerializeField] private World world;

        [Header("Custome Variables")]
        [SerializeField] private Transform drill = null;

        Transform vehicleModel;

        private void Awake() {
            vehicleModel = this.transform.Find("Vehicle");

            if(world == null) {
                world = FindObjectOfType<World>();
            }
        }

        private void Update()
        {
            if(!constantDeform) { return; }

            if (deformSpeed <= 0)
            {
                Debug.LogWarning("Deform Speed must be positive!");
                return;
            }

            if (deformRange <= 0)
            {
                Debug.LogWarning("Deform Range must be positive");
                return;
            }

            RaycastToTerrain(!leftClickAddsTerrain);
        }

        /// <summary>
        /// Tests if the player is in the way of deforming and edits the terrain if the player is not.
        /// </summary>
        /// <param name="addTerrain">Should terrain be added or removed</param>
        private void RaycastToTerrain(bool addTerrain)
        {
            if(vehicleModel != null) {
                EditTerrain(drill.position, addTerrain, deformSpeed, deformRange * vehicleModel.localScale.x);
            } else {
                EditTerrain(drill.position, addTerrain, deformSpeed, deformRange);
            }
        }

        /// <summary>
        /// Deforms the terrain in a spherical region around the point
        /// </summary>
        /// <param name="point">The point to modify the terrain around</param>
        /// <param name="addTerrain">Should terrain be added or removed</param>
        /// <param name="deformSpeed">How fast the terrain should be deformed</param>
        /// <param name="range">How far the deformation can reach</param>
        public void EditTerrain(Vector3 point, bool addTerrain, float deformSpeed, float range)
        {
            int buildModifier = addTerrain ? 1 : -1;

            int hitX = Mathf.RoundToInt(point.x);
            int hitY = Mathf.RoundToInt(point.y);
            int hitZ = Mathf.RoundToInt(point.z);

            int intRange = Mathf.CeilToInt(range);

            for (int x = -intRange; x <= intRange; x++)
            {
                for (int y = -intRange; y <= intRange; y++)
                {
                    for (int z = -intRange; z <= intRange; z++)
                    {
                        int offsetX = hitX - x;
                        int offsetY = hitY - y;
                        int offsetZ = hitZ - z;

                        var offsetPoint = new int3(offsetX, offsetY, offsetZ);
                        float distance = math.distance(offsetPoint, point);
                        if (distance > range)
                        {
                            continue;
                        }

                        float modificationAmount = deformSpeed / distance * buildModifier;

                        float oldDensity = world.GetDensity(offsetPoint);
                        float newDensity = Mathf.Clamp(oldDensity - modificationAmount, -1, 1);

                        world.SetDensity(newDensity, offsetPoint);
                    }
                }
            }
        }
    }
}