using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MarchingCubes.Examples;

public class TNT : MonoBehaviour
{
    [SerializeField] Transform crateModel = null;
    [SerializeField] float explosionRange = 5.0f;
    [SerializeField] float deformSpeed = 25.0f;

    Transform crateParent;
    TerrainDeformer terrainDeformer;
    BoolVariable vibrationState;

    void Awake() {
        vibrationState = Resources.Load("Prefabs/Vibration State") as BoolVariable;
        crateParent = this.transform.parent;
        terrainDeformer = this.GetComponent<TerrainDeformer>();
    }

    public void Explode() {
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        Tween tween = crateParent.DOJump(new Vector3(crateParent.position.x, crateParent.position.y, crateParent.position.z + 4f), 2, 1, 0.25f);

        yield return tween.WaitForKill();

        Instantiate(
            Resources.Load("Prefabs/Particles/NukeYellow"),
            crateModel.position,
            Quaternion.identity
        );

        if(vibrationState.value) { Handheld.Vibrate(); }

        terrainDeformer.EditTerrain(crateModel.position, false, deformSpeed, explosionRange);

        Collider[] colliders = Physics.OverlapSphere(crateModel.position, explosionRange, 1 << 19);

        foreach (var col in colliders) {
            Rock rock = col.gameObject.GetComponent<Rock>();
            if(rock != null) {
                rock.Damage(100, 0.1f, true, false);
            }
        }

        Destroy(crateParent.gameObject);
    }
}
