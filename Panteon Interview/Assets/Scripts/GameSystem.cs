using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public class GameSystem : MonoSingleton<GameSystem>
{
    bool run;

    public GameObject FinishObstacle;
    public GameData data;

    List<aiData> aiDataset;
    NavMeshSurface surface;

    private void Awake()
    {
        PoolSystem.Create(transform);

        aiDataset = new List<aiData>();
        if (data.aiPrefabs.Length > 0)
        {
            if (data.aiCount % 2 != 0) { data.aiCount += 1; Debug.LogError("Admin: Ai count odd number !"); }

            playerList = new string[data.aiCount + 1];
            for (int z = 0; z < data.aiCount / 2; z++)
            {
                for (int x = 0; x < 2; x++)
                {
                    Ai newAi = Instantiate(data.aiPrefabs[x]).GetComponent<Ai>();
                    aiDataset.Add(new aiData(newAi, GetName()));
                    newAi.transform.position = data.spawnPoint + new Vector3(x * 3, 0, z * 3);
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
        UiManager.Instance.eventList.onStand.gameEvent.Invoke();
        StartCoroutine(RunLongUpdate());
    }

    IEnumerator RunLongUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
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
            if (aiDataset[a].ai.transform.position.x < CharacterMovement.Instance.transform.position.x)
            {
                no++;
            }
            playerList[no] = aiDataset[a].name;
        }
        no = 0;
        for (int a = 0; a < aiDataset.Count; a++)
        {
            if (CharacterMovement.Instance.transform.position.x < aiDataset[a].ai.transform.position.x)
            {
                no++;
            }
        }
        playerList[no] = "You";
        UiManager.Instance.ReadPlayerList(playerList);
    }

    public string GetName()
    {
        string name = data.names[UnityEngine.Random.Range(0, data.names.Length)];
        foreach (var ai in aiDataset)
        {
            if (ai.name == name)
            {
                return GetName();
            }
        }
        return name;
    }
    public void ReadyGame()
    {
        Time.timeScale = 1;
        FinishObstacle.SetActive(true);
        CheckPlayerList();
        for (int z = 0; z < data.aiCount / 2; z++)
        {
            for (int x = 0; x < 2; x++)
            {
                aiDataset[(z * 2) + x].ai.transform.position = data.spawnPoint + new Vector3(x * 2, 0, z * 2);
                aiDataset[(z * 2) + x].ai.gameObject.SetActive(true);
                aiDataset[(z * 2) + x].ai.Ready();
            }
        }
        CharacterMovement.Instance.Ready();
        StartCoroutine(Go());
    }

    public IEnumerator Go()
    {
        yield return new WaitForSeconds(3);
        run = true;
        Time.timeScale = 1;
        AudioManager.Instance.PlayMonoSound(3);
        CharacterMovement.Instance.Run();
        foreach (var aiData in aiDataset)
        {
            aiData.ai.Go(data.finishPoint);
        }
    }

    public void Reposition(GameObject character)
    {
        character.transform.position = data.spawnPoint;
    }

    public void OnFinish()
    {
        run = false;
        Time.timeScale = 1;
        HideAllEnemies();
        LeanTween.move(CharacterMovement.Instance.gameObject, data.finishPoint, 1);
        CharacterMovement.Instance.transform.rotation = Quaternion.Euler(0,90,0);

        Camera.main.GetComponent<Cinemachine.CinemachineBrain>().enabled = false;
        LeanTween.move(Camera.main.gameObject, data.paintCamPos, 1);
        LeanTween.rotate(Camera.main.gameObject, data.paintCamRot, 1);
    }

    public void HideAllEnemies()
    {
        run = false;
        foreach (var aiData in aiDataset)
        {
            aiData.ai.gameObject.SetActive(false);
        }
    }
    public static void SetCamera(bool value) { Camera.main.GetComponent<Cinemachine.CinemachineBrain>().enabled = value; }

    public void LoseGame()
    {
        HideAllEnemies();
        UiManager.Instance.eventList.onLose.gameEvent.Invoke();
        AudioManager.Instance.PlayMonoSound(5);
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