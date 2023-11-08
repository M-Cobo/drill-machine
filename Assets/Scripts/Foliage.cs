using UnityEngine;
using DG.Tweening;

public class Foliage : MonoBehaviour
{
    Vector3 initScale;

    void Start()
    {
        initScale = this.transform.localScale;
        this.transform.localScale = Vector3.zero;
    }

    public void Growth()
    {
        this.transform.DOScale(initScale, 2.0f).SetEase(Ease.OutSine);

        TextPopUp.Create(
            position: new Vector3(transform.position.x, transform.position.y + 2.0f, transform.position.z - 1.0f),
            text: ("+1"),
            color: Color.cyan,
            scale: 2,
            delay: 0.5f
        );
    }
}
