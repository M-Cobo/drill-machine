using UnityEngine;
using Doozy.Engine;
using System.Collections;
using Doozy.Engine.Progress;

public class FeverProgress : MonoBehaviour
{
    [SerializeField] int maxEnergy = 50;
    [SerializeField] float feverDuration = 10.0f;
    
    [SerializeField] bool onFever = false;

    Progressor feverProgressor;

    private void Awake() 
    {
        feverProgressor = this.GetComponent<Progressor>();
        feverProgressor.SetMax(maxEnergy + 1);
        feverProgressor.SetValue(0);
    }

    private void Start() { StartCoroutine(DecreaseEnergy()); }

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
        if (message.EventName != "IncreaseFever") return;

        if (message.Source == null) return;

        if (onFever) return;

        switch (message.Source.name)
        {
            case "Stone":
                IncreaseEnergy(2);
            break;

            case "Iron":
                IncreaseEnergy(4);
            break;

            case "Gold":
                IncreaseEnergy(6);
            break;

            case "Diamond":
                IncreaseEnergy(8);
            break;
        }
    }

    private void IncreaseEnergy(int energy)
    {
        int totalEnergy = Mathf.RoundToInt(feverProgressor.Value + energy);
        totalEnergy = Mathf.Clamp(totalEnergy, 0, maxEnergy);

        feverProgressor.AnimationDuration = 0.5f;
        feverProgressor.SetValue(totalEnergy);
    }

    public void CheckEnergy(float progress)
    {
        if(progress >= maxEnergy && !onFever) {
            onFever = true;
            GameEventMessage.SendEvent("ActivateFever");
            feverProgressor.AnimationDuration = feverDuration;
            feverProgressor.SetValue(0);
        }

        if(progress <= 0 && onFever) {
            onFever = false;
            GameEventMessage.SendEvent("DeactivateFever");
        }
    }

    IEnumerator DecreaseEnergy()
    {
        yield return new WaitUntil(() => !onFever && feverProgressor.Value > 0);

        yield return new WaitForSeconds(1.0f);

        feverProgressor.AnimationDuration = 0.5f;
        feverProgressor.SetValue(feverProgressor.Value - 1);
    }
}
