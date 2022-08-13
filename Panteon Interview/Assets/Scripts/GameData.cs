using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "My Assets/Game Data")]
public class GameData : ScriptableObject
{
    [Space(20)]
    public Vector3 spawnPoint;
    public Vector3 finishPoint;
    [Space(20)]
    public Vector3 paintCamPos;
    public Vector3 paintCamRot;
    [Space(20)]
    public int aiCount = 10;
    public GameObject[] aiPrefabs;
    public string[] names;
}
