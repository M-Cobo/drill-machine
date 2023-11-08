using UnityEngine;

public class RollingBall : MonoBehaviour
{
    [SerializeField] float speed = 50f;
    [SerializeField] float timeLimit = 5f;
    [HideInInspector] public bool roll;

    Rigidbody rigidBody;
    GameObject ballParent;

    float timer;

    private void Awake() {
        rigidBody = this.GetComponent<Rigidbody>();
        ballParent = transform.parent.gameObject;
    }

    private void FixedUpdate() 
    {
        if(roll != true) { rigidBody.velocity = Vector3.zero; return; }

        rigidBody.AddForce(Vector3.forward * speed, ForceMode.Acceleration);
		rigidBody.AddForce(Vector3.down * 20, ForceMode.Acceleration);

        timer += Time.deltaTime;

        if(timer >= timeLimit) 
        {
            Instantiate(
                Resources.Load("Prefabs/Particles/Break_Ball_PS"),
                transform.position,
                Quaternion.identity
            );

            Destroy(ballParent);
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Rock"))
        {
            other.gameObject.GetComponent<Rock>().Damage(100, 0.1f, true, false);
        }
    }
}
