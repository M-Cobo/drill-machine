using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Doozy.Engine;
using Doozy.Engine.Progress;

public class VehicleHealth : MonoBehaviour
{
#region Serializable Fields

    // Health Settings
    [SerializeField] int resistance = 50;
    [SerializeField] float loseHealthRate = 1.0f;
    [SerializeField] float gainHealthRate = 0.5f;

    public int Resistance
    {
        set { resistance = Mathf.Clamp(value, 0, 100); }
        get { return resistance; }
    }

#endregion

#region Private Fields

    // Other Systems
    VehicleController vehicleController;
    VehicleEnhancers vehicleEnhancers;
    Driller driller;

    // UI Health - Fields
    GameObject healthCanvas;
    Progressor healthProgress;
    Image healthCounter_bg;
    Image healthCounter_arrow;
    TextMeshProUGUI healthCounter_tmp;
    Vector3 healthCanvasSize;
    
    // Other Vars
    float timer;

#endregion


    void Awake() 
    {
        // Get Vehicle components
        vehicleEnhancers = this.GetComponent<VehicleEnhancers>();
        vehicleController = this.GetComponent<VehicleController>();
        driller = this.GetComponent<Driller>();

        // Player Vehicle UI
        GameObject ui_vehicle = Instantiate(Resources.Load("Prefabs/UI/Vehicle - Health"), Vector3.zero, Quaternion.identity) as GameObject;
        ui_vehicle.transform.SetParent(transform);
        ui_vehicle.transform.localPosition = new Vector3(0, 1.2f, 0);
        ui_vehicle.transform.localRotation = Quaternion.Euler(Vector3.zero);

        // Health Canvas Setup
        healthCanvas = ui_vehicle.transform.Find("Vehicle Health - Canvas").gameObject.transform.Find("Health Counter - Holder").gameObject;
        healthCanvasSize = healthCanvas.transform.localScale;

        healthProgress = healthCanvas.GetComponent<Progressor>();
        healthProgress.SetMax(Resistance + 1);
        healthProgress.InstantSetValue(Resistance);

        healthCounter_arrow = healthProgress.transform.Find("Arrow").gameObject.GetComponent<Image>();
        healthCounter_bg = healthProgress.transform.Find("Background").gameObject.GetComponent<Image>();
        healthCounter_tmp = healthProgress.transform.Find("Text (TMP)").gameObject.GetComponent<TextMeshProUGUI>();

        healthCounter_tmp.SetText(Resistance.ToString());
    }

    void Update() 
    {
        if(vehicleController.player.GetButton("Accelerate") && driller.rock == null && !vehicleEnhancers.isPowerUp && !vehicleEnhancers.onFever)
        {
            timer += Time.deltaTime;

            if (timer >= loseHealthRate && Resistance > 0) {
                ReduceHealth(1, loseHealthRate / 3);
                timer = 0;
            }
        }

        if(vehicleEnhancers.onFever)
        {
            timer += Time.deltaTime;

            if (timer >= gainHealthRate) {
                IncreaseHealth(1);
                timer = 0;
            }
        }
    }

    public void ReduceHealth(int damage, float animateTime)
    {
        Resistance -= damage;

        float animDuration = animateTime * 0.8f;

        UpdateUI(animDuration);
        PlayUIAnimation(animDuration);

        if(Resistance <= 0 && !vehicleEnhancers.onFever) { GameEventMessage.SendEvent("GameOver"); }
    }

    public void IncreaseHealth(int health)
    {
        Resistance += health;

        UpdateUI(0.5f);
    }

    void UpdateUI(float duration)
    {
        healthCounter_tmp.SetText(Resistance.ToString());

        healthProgress.AnimationDuration = duration;
        healthProgress.SetValue(Resistance);
    }

    public void HideUI() { healthCanvas.transform.DOScale(Vector3.zero, 0.25f); }
    public void ShowUI() { healthCanvas.transform.DOScale(healthCanvasSize, 0.25f); }


    void PlayUIAnimation(float duration)
    {
        healthCanvas.transform.DOPunchScale(
            healthCanvas.transform.localScale * 0.1f,
            duration,
            0,
            1.0f
        );

        healthCanvas.transform.DOShakePosition(
            duration,
            Vector3.up * 0.2f,
            0
        );
    }

    public void EngineBroke()
    {
        vehicleController.controllable = false;
    }
}
