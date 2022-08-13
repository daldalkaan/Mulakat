using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
public class UiManager : MonoSingleton<UiManager>
{
    [System.Serializable]
    public class UIEventList
    {
        public GameEvent onAwake;
        public GameEvent onStand;
        public GameEvent onPlay;
        public GameEvent onFinish;
        public GameEvent onWin;
        public GameEvent onLose;
    }

    public GameObject playeListPanel;
    private TMP_Text[] playeListTexts;

    public GameObject PopupPanel;
    public TMP_Text countdownText;
    public Image tutorial;
    public TMP_Text paintedPercent;
    [Space(20)]
    public UIEventList eventList;


    private void Start()
    {
        playeListTexts = playeListPanel.GetComponentsInChildren<TMP_Text>();
        Wall.OnPaintVertex += WritePaintedPercent;
    }


    public void PlayGame() //using by editör
    {
        GameSystem.Instance.ReadyGame();
        Wall.Instance.ResetWall();
        StartCoroutine(Countdown());
        StartCoroutine(EnableInfo());
        Cursor.visible = false;
    }

    public void SetCursorVisible(bool value) => Cursor.visible = value;

    public IEnumerator EnableInfo()
    {
        tutorial.color = Color.white;
        LeanTween.moveLocalX(tutorial.gameObject, 350, 2).setLoopPingPong();
        yield return new WaitForSeconds(4);
        LeanTween.value(gameObject, 0.1f, 1, 0.5f).setOnUpdate((value) => { tutorial.color = Color.Lerp(tutorial.color, new Color(255, 255, 255, 0), value); });
    }

    public void WritePaintedPercent(float value)
    {
        paintedPercent.text = (int)value + "%";
    }

    private IEnumerator Countdown()
    {
        playeListPanel.SetActive(true);
        PopupPanel.SetActive(false);

        for (int i = 3; i > 0; i--)
        {
            WriteCountdown(i.ToString());
            yield return new WaitForSeconds(1);
        }
        WriteCountdown("RUN");
        yield return new WaitForSeconds(1);
        LeanTween.value(gameObject, 0.1f, 1, 0.5f).setOnUpdate((value) => { countdownText.color = Color.Lerp(countdownText.color, new Color(255,255,255,0), value); });
        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(false);
    }

    public void WriteCountdown(string text)
    {
        countdownText.transform.localScale = new Vector3(0, 0, 0);
        LeanTween.scale(countdownText.gameObject, new Vector3(1f, 1f, 1f), 0.5f);
        countdownText.gameObject.SetActive(true);
        countdownText.text = text;
    }

    public void ReadPlayerList(string[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            playeListTexts[i].text = i + ". " + list[i];
        }
    }

    public void SetItem(int index) => CharacterMovement.Instance.SetItem(index);
}

[System.Serializable]
public class GameEvent
{
    public UnityEvent gameEvent;
}