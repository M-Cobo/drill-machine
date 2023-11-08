using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using UnityEngine.Playables;
using Doozy.Engine;
using Doozy.Engine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] GameObject timelineDirector = null;
    [SerializeField] GameObject auraCharge = null;

    List<GameObject> walls;

    bool isCompleted = false;

    public void LevelCompleted() 
    {
        isCompleted = true;

        PlayerPrefs.SetInt("CurrentLevel", (PlayerPrefs.GetInt("CurrentLevel") + 1));

        StartCoroutine(FinalRewardCompleted());
    }

    IEnumerator FinalRewardCompleted()
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Wall").Length <= 0);

        yield return new WaitForSeconds(2.0f);

        UIPopup popup = UIPopup.GetPopup("FullLevelCompleted");
        popup.Show();

        GameEventMessage.SendEvent("GameOver");
    }

    public void LevelFinished()
    {
        if(!isCompleted) 
        {
            TinySauce.OnGameFinished(
                levelNumber: PlayerPrefs.GetInt("CurrentLevel").ToString(), 
                PlayerPrefs.GetInt("CollectedMinerals")
            );

            return; 
        }

        StartCoroutine(Finish());
    }

    IEnumerator Finish()
    {
        yield return new WaitForEndOfFrame();

        SetTimeline();
    }

    void SetTimeline()
    {
        Transform spawnPoint = GameObject.FindGameObjectWithTag("Timeline_SP").transform;

        timelineDirector.transform.position = spawnPoint.position;
        timelineDirector.SetActive(true);
    }

    public void SetCar()
    {
        Transform floatings = GameObject.FindGameObjectWithTag("CarFloatings").transform;
        floatings.gameObject.SetActive(true);

        foreach (Transform floating in floatings)
        {
            floating.gameObject.SetActive(true);
            floating.DOScale(Vector3.zero, 1f).From();
        }

        PlayableDirector playableDirector = GameObject.FindGameObjectWithTag("CarWaterPipe").GetComponent<PlayableDirector>();
        playableDirector.Play();
    }

    public void SetWater(GameObject water)
    {
        walls = GameObject.FindGameObjectsWithTag("Wall").ToList();
        walls = walls.OrderBy(x => x.transform.position.z).ToList();

        Transform sp_Wall;

        if(walls.Count > 0) {
            sp_Wall = walls[0].transform;
            water.transform.position = new Vector3(sp_Wall.position.x, water.transform.position.y, sp_Wall.position.z);
        } else {
            sp_Wall = GameObject.FindGameObjectWithTag("Water_SP").transform;
            water.transform.position = new Vector3(sp_Wall.position.x, water.transform.position.y, sp_Wall.position.z);
        }
    }

    public void ForestGrowth() 
    {
        List<GameObject> sp_aura = GameObject.FindGameObjectsWithTag("AuraCharge_SP").ToList();
        sp_aura = sp_aura.OrderBy(x => x.transform.position.z).ToList();

        List<Foliage> forest = GameObject.FindObjectsOfType<Foliage>().ToList();
        forest = forest.OrderBy(x => x.transform.position.z).ToList();

        float floraToGrow;

        switch (walls.Count)
        {
            case 3:
                floraToGrow = 2;
            break;

            case 2:
                floraToGrow = 4;
            break;

            case 1:
                floraToGrow = 6;
            break;

            default:
                floraToGrow = 13;
            break;
        }

        StartCoroutine(PlayAura(floraToGrow, sp_aura));

        floraToGrow = Mathf.Abs(forest.Count * (floraToGrow).ReMap(0.0f, 13.0f, 0.0f, 1.0f));

        StartCoroutine(StartGrowing(floraToGrow, forest)); 
    }

    IEnumerator PlayAura(float nAura, List<GameObject> spawnPoints)
    {
        for (int i = 0; i < nAura + 2; i++)
        {
            Instantiate(auraCharge, spawnPoints[i].transform.position, Quaternion.Euler(-90, 0, 0));

            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator StartGrowing(float nFlora, List<Foliage> flora)
    {
        var pointsManager = GameObject.FindObjectOfType<PointsManager>();

        for (int i = 0; i < nFlora; i++)
        {
            flora[i].Growth();
            pointsManager.AddMinerals(1);

            yield return new WaitForSeconds(0.075f);
        }

        GameEventMessage.SendEvent("ShowLevelCompletedUI");
        
        TinySauce.OnGameFinished(
            levelNumber: PlayerPrefs.GetInt("CurrentLevel").ToString(),
            true,
            PlayerPrefs.GetInt("CollectedMinerals")
        );
    }
}
