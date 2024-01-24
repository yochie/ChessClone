using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnPopup : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI label;

    [SerializeField]
    GameObject popup;

    [SerializeField]
    GameObject background;

    [SerializeField]
    float slideDurationSeconds;

    [SerializeField]
    float stillDurationSeconds;

    [SerializeField]
    Transform startPosition;

    [SerializeField]
    Transform endPosition;

    [SerializeField]
    AudioClip wooshSound;

    public void TriggerPopup(bool yourTurn, bool afterCheckingMove)
    {
        this.StopAllCoroutines();
        if(yourTurn)
            StartCoroutine(this.PopupCoroutine("Your turn", Color.green, afterCheckingMove));
        else
            StartCoroutine(this.PopupCoroutine("Opponent turn", Color.red, afterCheckingMove));
    }

    public IEnumerator PopupCoroutine(string labelText, Color labelColor, bool afterCheckingMove)
    {
        //this.background.SetActive(true);
        this.popup.SetActive(true);
        if (afterCheckingMove)
            this.label.text = string.Format("{0}\n{1}", labelText, "Checked!");
        else
            this.label.text = labelText;
        this.label.color = labelColor;
        float elapsedSeconds = 0f;
        Vector3 screenCenterPosition = Vector3.zero;
        this.popup.transform.localPosition = this.startPosition.localPosition;

        if (this.wooshSound != null)
            AudioManager.Singleton.PlaySoundEffect(this.wooshSound);
        while (elapsedSeconds < slideDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            this.popup.transform.localPosition = Vector3.Lerp(this.popup.transform.localPosition, screenCenterPosition, elapsedSeconds / slideDurationSeconds);
            yield return null;
        }

        yield return new WaitForSeconds(this.stillDurationSeconds);

        if (this.wooshSound != null)
            AudioManager.Singleton.PlaySoundEffect(this.wooshSound);
        elapsedSeconds = 0f;
        while (elapsedSeconds < slideDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            this.popup.transform.localPosition = Vector3.Lerp(this.popup.transform.localPosition, this.endPosition.localPosition, elapsedSeconds / slideDurationSeconds);
            yield return null;
        }

        //this.background.SetActive(false);
        this.popup.transform.localPosition = this.startPosition.position;
        this.popup.SetActive(false);
    }
}
