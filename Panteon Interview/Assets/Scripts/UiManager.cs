using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
public class UiManager : MonoBehaviour
{
    public static UiManager instance;

    [Header("Player List")]
    public GameObject playeListPanel;
    public TMP_Text[] playeListTexts;
    [Header("Stage Panel")]
    public GameObject stagePanel;
    public TMP_Text titleText;
    public TMP_Text buttonText;
    [Header("")]
    public TMP_Text countdownText;
    public Image info;
    [Header("Paint Properies")]
    public TMP_Text paintedPercent;

    public void Awake()
    {
        instance = this;

        Cursor.visible = true;
        foreach (var text in playeListTexts) { text.GetComponent<TMP_Text>(); }

        titleText.GetComponent<TMP_Text>();
        buttonText.GetComponent<TMP_Text>();

        countdownText.GetComponent<TMP_Text>();
        paintedPercent.GetComponent<TMP_Text>();

        info.GetComponent<Image>();
        info.gameObject.SetActive(true);
    }

    private void Start()
    {
        GameSystem.playerSort += ReadPlayerList;
        CharacterMovement.OnFinish += OnFinish;
        Wall.OnPaintVertex += WritePaintedPercent;
        Wall.OnPainted += Win;
    }

    public void StandMenu()
    {
        playeListPanel.SetActive(false);
        stagePanel.SetActive(true);
        countdownText.gameObject.SetActive(false);
        paintedPercent.gameObject.SetActive(false);
        titleText.text = "Welcome";
        buttonText.text = "Play";
    }

    public void PlayGame()
    {
        GameSystem.ReadyGame();
        Wall.ResetPaintableWall();
        StartCoroutine(Countdown());
        StartCoroutine(EnableInfo());
        Cursor.visible = false;
    }

    public void OnFinish()
    {
        Cursor.visible = true;
        playeListPanel.SetActive(false);
        paintedPercent.gameObject.SetActive(true);
        paintedPercent.text = "0%";
    }

    public IEnumerator EnableInfo()
    {
        info.color = Color.white;
        info.GetComponent<RectTransform>().localPosition = new Vector2(250, -250);
        LeanTween.moveLocal(info.gameObject, new Vector3(200, -250, 0), 1);
        yield return new WaitForSeconds(1);
        LeanTween.moveLocal(info.gameObject, new Vector3(300, -250, 0), 2);
        yield return new WaitForSeconds(2);
        LeanTween.value(gameObject, 0.1f, 1, 0.5f).setOnUpdate((value) => { info.color = Color.Lerp(info.color, new Color(255, 255, 255, 0), value); });
    }

    public void WritePaintedPercent(float value)
    {
        paintedPercent.text = (int)value + "%";
    }

    private IEnumerator Countdown()
    {
        playeListPanel.SetActive(true);
        stagePanel.SetActive(false);
        countdownText.gameObject.SetActive(true);
        countdownText.color = Color.white;

        for (int i = 3; i > 0; i--)
        {
            countdownText.transform.localScale = new Vector3(0, 0, 0);
            LeanTween.scale(countdownText.gameObject, new Vector3(1f, 1f, 1f), 0.5f);

            countdownText.gameObject.SetActive(true);
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        countdownText.transform.localScale = new Vector3(0, 0, 0);
        LeanTween.scale(countdownText.gameObject, new Vector3(1f, 1f, 1f), 0.5f);

        countdownText.gameObject.SetActive(true);
        countdownText.text = "Run";
        yield return new WaitForSeconds(1);
        LeanTween.value(gameObject, 0.1f, 1, 0.5f).setOnUpdate((value) => { countdownText.color = Color.Lerp(countdownText.color, new Color(255,255,255,0), value); });
        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(false);
    }

    public void Win() => SetTryAgainPanel("YOU WIN");
    public void SetTryAgainPanel(string explanation)
    {
        Cursor.visible = true;
        paintedPercent.gameObject.SetActive(false);
        playeListPanel.SetActive(false);
        stagePanel.SetActive(true);
        countdownText.gameObject.SetActive(false);
        paintedPercent.gameObject.SetActive(false);
        titleText.text = explanation;
        buttonText.text = "TRY AGAIN";
    }
    public void ReadPlayerList(string[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            playeListTexts[i].text = i + ". " + list[i];
        }
    }
    public void SetItem(int index) => CharacterMovement.SetItem(index);
}
