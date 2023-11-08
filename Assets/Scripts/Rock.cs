using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using Doozy.Engine;

public class Rock : MonoBehaviour
{
    // Public Fields
    [HideInInspector] public Mineral mineral = null;

#region Private Fields
    
    int resistance = 0;

    Transform rockModel;
    MeshRenderer meshRenderer;

    GameObject healthCanvas;

    Image healthCounter_bg;
    Image healthCounter_arrow;
    TextMeshProUGUI healthCounter_tmp;

    BoolVariable vibrationState;

#endregion

#region Properties

    Material[] _Material { get { return mineral.materials; } }

    ParticleSystem _Particle { get { return mineral.breakParticle; } }

    public int _Resistance { get { return resistance; } }

#endregion

    void Awake() 
    {
        vibrationState = Resources.Load("Prefabs/Vibration State") as BoolVariable;
        rockModel = transform.parent.GetChild(0);
        
        healthCanvas = Instantiate(Resources.Load("Prefabs/UI/Rock Health - Canvas"), Vector3.zero, Quaternion.identity) as GameObject;
        healthCanvas.transform.SetParent(transform.parent);

        healthCounter_arrow = healthCanvas.transform.GetChild(0).Find("Arrow").gameObject.GetComponent<Image>();
        healthCounter_bg = healthCanvas.transform.GetChild(0).Find("Background").gameObject.GetComponent<Image>();
        healthCounter_tmp = healthCanvas.transform.GetChild(0).Find("Text (TMP)").gameObject.GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        this.name = mineral.name;

        if(!mineral.isWall) {
            rockModel.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(-180, 180), 0);
        }
        
        resistance = (int)mineral.resistance;

        healthCanvas.transform.localPosition = mineral.name != "PowerUp" ? new Vector3(0, 2f, 0.75f) : new Vector3(0, 3f, 0.75f);

        meshRenderer = rockModel.gameObject.GetComponent<MeshRenderer>();
        meshRenderer.materials = _Material;

        if(mineral.name == "Obsidian") {
            healthCounter_tmp.rectTransform.localRotation = Quaternion.Euler(0, 0, 90);
            healthCounter_tmp.SetText("8");
        } else {
            healthCounter_tmp.SetText(resistance.ToString());
        }

        healthCounter_bg.color = healthCounter_arrow.color = mineral.canvasColor;
    }

    public void Damage(int damage, float hitRate, bool instantDestroy = false, bool playerHit = true)
    {
        if(instantDestroy) { Break(playerHit); return; }

        resistance -= damage;

        UpdateUI();
        PlayAnimations(hitRate);

        if(resistance <= 0) { Break(playerHit); }
        else if(mineral.name != "Obsidian") {
            TextPopUp.Create(
                new Vector3(transform.position.x, transform.position.y + 2.25f, transform.position.z),
                damage.ToString(),
                Color.cyan
            );
        }
    }

    void UpdateUI()
    {
        if(mineral.name == "Obsidian") {
            healthCounter_tmp.SetText("8");
        } else {
            healthCounter_tmp.SetText(resistance.ToString());
        }
    }

    void PlayAnimations(float duration)
    {
        foreach (Material mat in meshRenderer.materials)
        {
            mat.DOGradientColor(mineral.tweenGradient, "_Color", duration * mineral.tweenDuration);
        }
        
        rockModel.DOPunchScale(
            rockModel.localScale * mineral.punchScale,
            duration * mineral.tweenDuration, 
            0, 
            mineral.tweenElasticity
        );

        healthCanvas.transform.DOPunchScale(
            healthCanvas.transform.localScale * mineral.punchScale,
            duration * mineral.tweenDuration,
            0,
            mineral.tweenElasticity
        );

        healthCanvas.transform.DOShakePosition(
            duration * mineral.tweenDuration,
            Vector3.up * 0.2f,
            0
        );
    }

    void Break(bool playerHit = true)
    {
        if(playerHit)
        {
            if(mineral.name == "PowerUp") {
                GameEventMessage.SendEvent("ActivatePowerUp");
                Instantiate(
                    Resources.Load("Prefabs/Particles/GroundSlamPurple"), 
                    new Vector3(transform.position.x, transform.position.y + 1.25f, transform.position.z), 
                    Quaternion.identity
                );
            } 
            else if(!mineral.isWall) {
                GameEventMessage.SendEvent("IncreaseFever", this.gameObject);
            }
        }

        if(mineral.isWall) {
            Instantiate(
                Resources.Load("Prefabs/Particles/ConfettiCannon"), 
                new Vector3(transform.position.x - 4.5f, transform.position.y + 1.5f, transform.position.z), 
                Quaternion.Euler(-90, 0, 0)
            );

            Instantiate(
                Resources.Load("Prefabs/Particles/ConfettiCannon"), 
                new Vector3(transform.position.x + 4.5f, transform.position.y + 1.5f, transform.position.z), 
                Quaternion.Euler(-90, 0, 0)
            );
        }

        if(mineral.name != "Obsidian") {
            TextPopUp.Create(
                new Vector3(transform.position.x, transform.position.y + 2.25f, transform.position.z),
                Mathf.RoundToInt(mineral.resistance).ToString(),
                Color.green,
                1.5f
            );

            var pointsManager = GameObject.FindObjectOfType<PointsManager>();
            if(pointsManager != null) {
                pointsManager.AddMinerals(Mathf.RoundToInt(mineral.resistance));
            }
        }
        
        if(vibrationState.value) { Handheld.Vibrate(); }

        Instantiate(_Particle, rockModel.position + new Vector3(0, 0.75f, 0), Quaternion.identity);
        Destroy(transform.parent.gameObject);
    }
}
