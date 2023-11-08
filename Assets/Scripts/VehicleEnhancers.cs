using System.Collections;
using UnityEngine;
using DG.Tweening;
using Doozy.Engine;

public class VehicleEnhancers : MonoBehaviour
{

#region Power-Up

    [HideInInspector] public bool isPowerUp = false;
    [SerializeField] float endPowerupTime = 5.0f;
    [SerializeField] float tweenDuration = 0.25f;
    [SerializeField] Ease tweenEase = Ease.Linear;
    [SerializeField] ParticleSystem fireFloorTrail = null;

    float powerupTimer;

#endregion

#region Fever Mode

    public bool onFever = false;

    [SerializeField] ParticleSystem beamDrill = null;
    [SerializeField] ParticleSystem flamethrower = null;

#endregion

    VehicleHealth vehicleHealth;
    BoolVariable vibrationState;

    void Awake() 
    {
        vibrationState = Resources.Load("Prefabs/Vibration State") as BoolVariable;
        vehicleHealth = this.GetComponent<VehicleHealth>();
    }

    private void OnEnable()
    {
        //Start listening for game events
        Message.AddListener<GameEventMessage>(OnMessage);
    }
 
    private void OnDisable()
    {
        //Stop listening for game events
        Message.RemoveListener<GameEventMessage>(OnMessage);
    }
 
    private void OnMessage(GameEventMessage message)
    {
        if (message == null) return;

        switch (message.EventName)
        {
            case "ActivateFever":
                SetFever(true);

                beamDrill.Play();
                flamethrower.Play();
            break;

            case "DeactivateFever":
                SetFever(false);

                beamDrill.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                flamethrower.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            break;
        }
    }

    void Update() 
    {

#region Power-Up

        if(isPowerUp) {
            powerupTimer += Time.deltaTime;

            if(powerupTimer >= endPowerupTime)
            {
                StartCoroutine(DeactivatePowerUp());
                powerupTimer = 0;
            }
        }

#endregion

    }

#region Fever Mode

    void SetFever(bool mode) { onFever = mode; }

#endregion 

#region Power-Up

    public void ActivatePowerUp()
    {
        if(isPowerUp) { return; }

        powerupTimer = 0;
        isPowerUp = true;

        vehicleHealth.HideUI();

        foreach (Transform child in this.transform.parent.transform) {
            child.DOScale(new Vector3(2f, 2f, 2f), tweenDuration).SetEase(tweenEase).SetDelay(1);
        }

        fireFloorTrail.Play();
    }

    IEnumerator DeactivatePowerUp()
    {
        Tween tween = null;

        fireFloorTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        foreach (Transform child in this.transform.parent.transform) {
            tween = child.DOScale(new Vector3(1f, 1f, 1f), tweenDuration).SetEase(tweenEase).SetDelay(1);
        }

        vehicleHealth.ShowUI();

        yield return tween.WaitForKill();

        isPowerUp = false;
    }

#endregion

    void OnTriggerEnter(Collider other)
    {

    #region Health

        if(other.CompareTag("Health"))
        {
            StartCoroutine(HealVehicle(other.transform));
        }

    #endregion

    #region TNT

        if(other.CompareTag("TNT"))
        {
            if(vibrationState.value) { Handheld.Vibrate(); }
            other.GetComponent<TNT>().Explode();
            other.enabled = false;
        }

    #endregion

    #region Rolling Ball

        if(other.CompareTag("RollingBall"))
        {
            if(vibrationState.value) { Handheld.Vibrate(); }
            other.GetComponent<RollingBall>().roll = true;
        }

    #endregion

    #region Finish Line

        if(other.CompareTag("FinishLine"))
        {
            GameEventMessage.SendEvent("LevelCompleted");
            if(vibrationState.value) { Handheld.Vibrate(); }

            Instantiate(
                Resources.Load("Prefabs/Particles/ConfettiCannon"), 
                new Vector3(other.transform.position.x - 4.5f, other.transform.position.y + 1.5f, other.transform.position.z), 
                Quaternion.Euler(-90, 0, 0)
            );

            Instantiate(
                Resources.Load("Prefabs/Particles/ConfettiCannon"), 
                new Vector3(other.transform.position.x + 4.5f, other.transform.position.y + 1.5f, other.transform.position.z),
                Quaternion.Euler(-90, 0, 0)
            );
        }

    #endregion

    }

#region Health

    IEnumerator HealVehicle(Transform collectable)
    {
        vehicleHealth.IncreaseHealth(5);

        Instantiate(
            Resources.Load("Prefabs/Particles/HealNova"),
            new Vector3(collectable.position.x, collectable.position.y + 1.25f, collectable.position.z),
            Quaternion.identity
        );

        GameObject obj = collectable.parent.gameObject;

        Tween tween = obj.transform.DOScale(Vector3.zero, 0.25f);

        yield return tween.WaitForKill();

        Destroy(obj);
    }

#endregion

}
