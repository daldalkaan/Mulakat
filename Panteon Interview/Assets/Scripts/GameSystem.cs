using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public class GameSystem : MonoBehaviour
{
    public static event Action<string[]> playerSort;

    public static GameSystem instance { get; private set; }
    CharacterMovement characterMovement;
    UiManager uiManager;

    bool run;

    public GameObject FinishObstacle;
    public int aiCount = 10;
    [Header("")]
    public Vector3 spawnPoint;
    public Vector3 finishPoint;
    [Header("")]
    public Vector3 paintCamPos;
    public Vector3 paintCamRot;
    [Header("")]
    public GameObject[] aiPrefabs;
    public List<aiData> aiDataset;
    public string[] names;

    NavMeshSurface surface;

    private void Awake()
    {
        instance = this;
        PoolSystem.Create(transform);

        if (aiPrefabs.Length > 0)
        {
            if (aiCount % 2 != 0) { aiCount += 1; Debug.LogError("Admin: Ai count odd number !"); }

            playerList = new string[aiCount + 1];
            for (int z = 0; z < aiCount / 2; z++)
            {
                for (int x = 0; x < 2; x++)
                {
                    Ai newAi = Instantiate(aiPrefabs[x]).GetComponent<Ai>();
                    aiDataset.Add(new aiData(newAi, GetName()));
                    newAi.transform.position = spawnPoint + new Vector3(x * 3, 0, z * 3);
                    newAi.CrateAvatar(z, x);
                    newAi.gameObject.SetActive(false);
                    playerList[(z * 2) + x] = aiDataset[(z * 2) + x].name;
                }
            }
            playerList[playerList.Length - 1] = "You";
        }

        surface = GameObject.FindGameObjectWithTag("NavMesh").GetComponent<NavMeshSurface>();
    }

    private void Start()
    {
        if (CharacterMovement.instance != null)
        {
            characterMovement = CharacterMovement.instance.GetComponent<CharacterMovement>();
            uiManager = UiManager.instance.GetComponent<UiManager>();
            uiManager.StandMenu();
        }
        Wall.OnPainted += Win;
        CharacterMovement.OnFinish += OnFinish;

        StartCoroutine(RunLongUpdate());
    }

    IEnumerator RunLongUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);
            LongUpdate();
        }
    }

    string[] playerList;

    public void LongUpdate()
    {
        surface.BuildNavMesh(); //Bake NavMesh
        if (run) { CheckPlayerList(); } //AI List
    }

    public void CheckPlayerList()
    {
        int no = 0;
        for (int a = 0; a < aiDataset.Count; a++)
        {
            no = 0;
            for (int b = 0; b < aiDataset.Count; b++)
            {
                if (a != b)
                {
                    if (aiDataset[a].ai.transform.position.x < aiDataset[b].ai.transform.position.x)
                    {
                        no++;
                    }
                }
            }
            if (aiDataset[a].ai.transform.position.x < characterMovement.transform.position.x)
            {
                no++;
            }
            playerList[no] = aiDataset[a].name;
        }
        no = 0;
        for (int a = 0; a < aiDataset.Count; a++)
        {
            if (characterMovement.transform.position.x < aiDataset[a].ai.transform.position.x)
            {
                no++;
            }
        }
        playerList[no] = "You";
        playerSort.Invoke(playerList);
    }

    public string GetName()
    {
        string name = names[UnityEngine.Random.Range(0, names.Length)];
        foreach (var ai in aiDataset)
        {
            if (ai.name == name)
            {
                return GetName();
            }
        }
        return name;
    }
    public static void ReadyGame() => instance.Ready();
    public void Ready()
    {
        Time.timeScale = 1;
        PoolSystem.Instance.ResetPool();
        FinishObstacle.SetActive(true);
        CheckPlayerList();
        for (int z = 0; z < aiCount / 2; z++)
        {
            for (int x = 0; x < 2; x++)
            {
                aiDataset[(z * 2) + x].ai.transform.position = spawnPoint + new Vector3(x * 2, 0, z * 2);
                aiDataset[(z * 2) + x].ai.gameObject.SetActive(true);
                aiDataset[(z * 2) + x].ai.Ready();
            }
        }
        characterMovement.Ready();
        StartCoroutine(Go());
    }

    public IEnumerator Go()
    {
        yield return new WaitForSeconds(3);
        run = true;
        Time.timeScale = 1;
        AudioManager.PlaySound(3);
        characterMovement.Run();
        foreach (var aiData in aiDataset)
        {
            aiData.ai.Go(finishPoint);
        }
    }

    public static void Reposition(GameObject character)
    {
        character.transform.position = instance.spawnPoint;
    }

    public void OnFinish()
    {
        run = false;
        Time.timeScale = 1;
        HideAllEnemies();
        LeanTween.move(characterMovement.gameObject, finishPoint, 1);
        characterMovement.transform.rotation = Quaternion.Euler(0,90,0);

        Camera.main.GetComponent<Cinemachine.CinemachineBrain>().enabled = false;
        LeanTween.move(Camera.main.gameObject, paintCamPos, 1);
        LeanTween.rotate(Camera.main.gameObject, paintCamRot, 1);
    }

    public void Win()
    {
        AudioManager.PlaySound(4);
        Debug.Log("Win");
    }

    public void HideAllEnemies()
    {
        run = false;
        foreach (var aiData in aiDataset)
        {
            aiData.ai.gameObject.SetActive(false);
        }
    }
    public static void SetCamera() { Camera.main.GetComponent<Cinemachine.CinemachineBrain>().enabled = true; }

    public static void Lose() => instance.LoseGame();
    public void LoseGame()
    {
        HideAllEnemies();
        uiManager.SetTryAgainPanel("YOU LOSE");
        AudioManager.PlaySound(5);

        Time.timeScale = 0;
    }
}

[System.Serializable]
public class aiData
{
    public Ai ai;
    public string name;
    public aiData(Ai ai, string name)
    {
        this.ai = ai;
        this.name = name;
    }
    public aiData()
    {

    }
}