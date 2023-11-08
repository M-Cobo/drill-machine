using UnityEngine;

public class Driller : MonoBehaviour
{
    [SerializeField] Transform drill = null;
    [SerializeField] Transform drillModel = null;

    [SerializeField] int damage = 1;
    [SerializeField] float hitRate = 0.2f;
    [SerializeField] float drillDistance = 1.0f;
	[SerializeField, Range(-180.0f, 180.0f)] float rotateDrill = 90.0f;

    [HideInInspector] public Rock rock = null;

    VehicleHealth vehicleHealth = null;
    VehicleEnhancers vehicleEnhancers = null;

    BoolVariable vibrationState;

    float timer;
	int layer_mask;
	float rotateTarget;

    void Awake() 
    {
        vibrationState = Resources.Load("Prefabs/Vibration State") as BoolVariable;
        vehicleHealth = this.GetComponent<VehicleHealth>();
        vehicleEnhancers = this.GetComponent<VehicleEnhancers>();
        layer_mask = LayerMask.GetMask("Rock");
    }

    void Update() 
    {
        if(vehicleHealth.Resistance <= 0) { return; }

		rotateTarget = Mathf.Lerp(rotateTarget, rotateDrill, Time.deltaTime * 4f);

		drillModel.localRotation = Quaternion.Slerp(
			drillModel.localRotation,
			Quaternion.Euler(new Vector3(drillModel.localRotation.x, drillModel.localRotation.y, drillModel.localEulerAngles.z + rotateTarget)), 
			Time.deltaTime * 2.0f
		);
    }

    void FixedUpdate() 
    {
        if(!vehicleEnhancers.isPowerUp) {
            Drill();
        }
    }

    void OnTriggerStay(Collider other) 
    {
        if(vehicleEnhancers.isPowerUp && other.CompareTag("Rock")) {
            other.gameObject.GetComponent<Rock>().Damage(100, 0.1f, true, false);
        }

        if(other.CompareTag("Wall") && vehicleHealth.Resistance > 0) {
            Rock wall = other.gameObject.GetComponent<Rock>();
            int wallResistance = wall._Resistance;
            if(wallResistance < vehicleHealth.Resistance) {
                wall.Damage(wallResistance, 0.1f, true);
            } else {
                wall.Damage(vehicleHealth.Resistance, 0.25f);
            }
            vehicleHealth.ReduceHealth(wallResistance, hitRate);
        }
    }

    void Drill()
    {
        RaycastHit hit;

        bool hitRock = Physics.Raycast(drill.position, drill.forward, out hit, drillDistance, layer_mask);

        if(hitRock && rock == null) {
            rock = hit.transform.gameObject.GetComponent<Rock>();
            if(vibrationState.value) { Handheld.Vibrate(); }
        }
        else if(!hitRock && rock != null) {
            rock = null;
        }

        if(hitRock && rock != null && vehicleHealth.Resistance > 0)
        {
            timer += Time.deltaTime;

            if(timer >= hitRate && !vehicleEnhancers.onFever) 
            {
                rock.Damage(damage, hitRate);
                vehicleHealth.ReduceHealth(damage, hitRate);

                timer = 0f;
            } 
            else if(vehicleEnhancers.onFever)
            {
                vehicleHealth.ReduceHealth(rock._Resistance / 2, hitRate);
                rock.Damage(rock._Resistance, hitRate, true);
            }

        } else { timer = 0f; }
    }

    void OnDrawGizmosSelected() 
    {
        Gizmos.DrawRay(drill.position, drill.forward * drillDistance);
    }
}
