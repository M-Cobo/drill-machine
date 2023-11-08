using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class TextPopUp : MonoBehaviour
{
    public static TextPopUp Create(Vector3 position, string text, Color color, float scale = 1.0f, float delay = 0.0f)
    {
        GameObject popupObj = Instantiate(
            Resources.Load("Prefabs/UI/Text - PopUp"), 
            position, 
            Quaternion.identity
        ) as GameObject;

        TextPopUp textPopUp = popupObj.GetComponent<TextPopUp>();
        textPopUp.Setup(text, color, scale, delay);

        return textPopUp;
    }

    [Header("In Animation")]
    [SerializeField] Vector3 inOffset = Vector3.one;
    [SerializeField] Vector3 inScale = Vector3.one;
    [SerializeField] float inDuration = 0.5f;
    [SerializeField] float inDelay = 0.0f;
    [SerializeField] Ease inEase = Ease.Linear;

    [Header("Out Animation")]
    [SerializeField] Vector3 outOffset = Vector3.one;
    [SerializeField] Vector3 outScale = Vector3.one;
    [SerializeField] float outDuration = 0.5f;
    [SerializeField] float outDelay = 0.0f;
    [SerializeField] Ease outEase = Ease.Linear;

    private TextMeshPro textMesh;

    private void Awake() {
        textMesh = this.GetComponent<TextMeshPro>();
    }

    private void Setup(string text, Color color, float scale = 1.0f, float delay = 0.0f) {
        textMesh.color = color;
        textMesh.SetText(text);
        transform.localScale = (Vector3.one * scale);

        if(delay > 0.0f) {
            outDelay = delay;
        }

        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        Sequence inSequence = DOTween.Sequence();

        inSequence.Append(transform.DOMove(transform.position + inOffset, inDuration))
        .Join(transform.DOScale(inScale, inDuration))
        .Join(textMesh.DOFade(0.0f, inDuration).From())
        .SetDelay(inDelay).SetEase(inEase);

        yield return inSequence.WaitForKill();

        Sequence outSequence = DOTween.Sequence();

        outSequence.Append(transform.DOMove(transform.position + outOffset, outDuration))
        .Join(transform.DOScale(outScale, outDuration))
        .Join(textMesh.DOFade(0.0f, outDuration))
        .SetDelay(outDelay).SetEase(outEase);

        yield return outSequence.WaitForKill();

        Destroy(this.gameObject);
    }
}
