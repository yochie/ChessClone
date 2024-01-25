using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingPopup : MonoBehaviour
{

    [SerializeField]
    GameObject popup;

    [SerializeField]
    CanvasGroup popupCanvasGroup;

    [SerializeField]
    GameObject background;

    [SerializeField]
    float animationDurationSeconds;

    [SerializeField]
    AnimationCurve fadeCurve;

    [SerializeField]
    AnimationCurve growthCurve;

    internal void Display()
    {
        this.popup.gameObject.SetActive(true);
    }

    public void FadeOut()
    {
        StartCoroutine(this.PopupCoroutine());
    }

    public float GetFadeOutDuration()
    {
        return this.animationDurationSeconds;
    }

    public IEnumerator PopupCoroutine()
    {
        float elapsedSeconds = 0f;
        Vector3 startingScale = this.popup.transform.localScale;
        while (elapsedSeconds < this.animationDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            this.popupCanvasGroup.alpha = this.fadeCurve.Evaluate(elapsedSeconds / this.animationDurationSeconds);
            float growth = this.growthCurve.Evaluate(elapsedSeconds / this.animationDurationSeconds);
            this.popup.transform.localScale = startingScale * growth;
            yield return null;
        }

        this.popup.transform.localScale = startingScale;
        this.background.SetActive(false);
        this.popup.SetActive(false);
    }
}
