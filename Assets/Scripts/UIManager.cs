using Doozy.Engine.Progress;
using Doozy.Engine.UI;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] UIToggle vibrationToggle = null;
    [SerializeField] BoolVariable boolVariable = null;

    [SerializeField] Progressor levelProgressor = null;
    [SerializeField] TextMeshProUGUI barLevelText = null;
    [SerializeField] TextMeshProUGUI endLevelText = null;

    [SerializeField] TextMeshProUGUI goPointsText = null;
    [SerializeField] Progressor goPointsProgressor = null;

    [SerializeField] Progressor pointsProgressor = null;
    [SerializeField] TextMeshProUGUI pointsText = null;
    [SerializeField] RectTransform pointsTranform = null;

    [SerializeField] TextMeshProUGUI mineralsCollectedText = null;

    private PointsManager pointsManager = null;
    private Transform carSphere = null;
    private Transform finishLine = null;

    private void Start() 
    {
        pointsManager = this.GetComponent<PointsManager>();
        mineralsCollectedText.SetText(PlayerPrefs.GetInt("CollectedMinerals", 0).ToString());

        string sLevel = PlayerPrefs.GetInt("CurrentLevel", 0).ToString();
        barLevelText.SetText(sLevel);
        endLevelText.SetText(sLevel);

        carSphere = GameObject.FindGameObjectWithTag("Car").transform;
        finishLine = GameObject.FindGameObjectWithTag("FinishLine").transform;
        levelProgressor.SetMax(Mathf.Abs(carSphere.position.z - finishLine.position.z));

        vibrationToggle.IsOn = boolVariable.value;
    }

    private void Update() 
    {
        if(levelProgressor.Value > 0.5f){
            levelProgressor.SetValue(Mathf.Abs(carSphere.position.z - finishLine.position.z));
        } else {
            levelProgressor.SetValue(0);
        }
    }

    public void StartPlaying()
    {
        TinySauce.OnGameStarted(levelNumber: PlayerPrefs.GetInt("CurrentLevel").ToString());
    }

    public void ShowPopup(string name)
    {
        UIPopup popup = UIPopup.GetPopup(name);
        popup.Show();
    }

    public void UpdatePointsUI(int totalPoints)
    {
        pointsProgressor.SetMax(totalPoints);
        pointsProgressor.SetProgress(1.0f);

        pointsTranform.DOPunchPosition(Vector3.up * 10.0f, 0.4f);
        pointsText.DOColor(Color.green, 0.3f).OnComplete(() => {
            pointsText.DOColor(Color.white, 0.1f);
        });
    }

    public void SetPointsAtGameOver()
    {
        goPointsText.SetText(pointsManager.MineralsCollected.ToString());

        goPointsProgressor.SetMax(pointsManager.MineralsCollected);
        goPointsProgressor.SetProgress(1.0f);

        PlayerPrefs.SetInt(
            "CollectedMinerals",
            PlayerPrefs.GetInt("CollectedMinerals", 0) + pointsManager.MineralsCollected
        );
    }

    public void SetVibration(bool value) { boolVariable.value = value; }
}
