using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Wall : MonoSingleton<Wall>
{
    [System.Serializable]
    public class PaintVertex
    {
        public Vector3 vertexPos;
        public bool painted;
        public PaintVertex(Vector3 vertexPos, bool painted)
        {
            this.vertexPos = vertexPos;
            this.painted = painted;
        }
        public PaintVertex() { }
    }

    public static Wall instance;
    public static event Action<float> OnPaintVertex; //her vertex boyandýðýnda ui ý uyarýr
    public static event Action OnPainted; //%100 boyanýnca
    Camera cam;

    [Header("Paint Options")]
    bool isRun;
    MeshRenderer meshRenderer; //boyanacak yüzey
    public Texture2D brush; //fýrça görseli
    public Vector2Int textureArea; //kordinatlar, !! yüzde hesaplamak için sonradan calass içerisinde kullan
    Texture2D texture;
    [Header("Percent Dataset")]
    private List<PaintVertex> verticesData; //vertekslerin datasý
    private float paintedPercent; //boyanan yüzde
    public GameObject visualPrefab;
    List<GameObject> visualList = new List<GameObject>();

    private void Awake()
    {
        instance = this;
        meshRenderer = GetComponent<MeshRenderer>();
        cam = Camera.main.GetComponent<Camera>();
    }
    private void Start()
    {
        CreateVerticesData();
        ResetWall();
        isRun = false;
    }
    private void Update()
    {
        if (Input.GetMouseButton(0) && isRun)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit)) //sadece duvarda çalýþtýr (performans)
            {
                SetVerticesPosition(hit.point);
                Paint(hit.textureCoord);
            }
        }
    }

    public void CreateVerticesData()
    {
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        verticesData = new List<PaintVertex>();
        //Matrix 4x4 localToWorld = transform.localToWorldMatrix; //PATLIYOR, sorunu tespit et !!!
        foreach (var vertex in vertices)
        {
            verticesData.Add(new PaintVertex(transform.TransformPoint(vertex), false)); //Get Global position, bool
            //verticesData.Add(new PaintVertex(localToWorld.MultiplyPoint3x4(vertex), false)); //PATLIYOR, sorunu tespit et !!!
        }
        paintedPercent = 0;
    }

    public void SetVerticesPosition(Vector3 brushPosition)
    {
        foreach (var vertex in verticesData)
        {
            if (!vertex.painted && 0.5f >= Vector3.Distance(vertex.vertexPos, brushPosition))
            {
                AddVisualObjects(vertex.vertexPos);

                paintedPercent += 100f / verticesData.Count;
                AudioManager.Instance.PlayMonoSound(2);
                vertex.painted = true;

                OnPaintVertex.Invoke(paintedPercent);

                if (paintedPercent >= 100)
                {
                    isRun = false;
                    AudioManager.Instance.PlayMonoSound(4);
                    UiManager.Instance.eventList.onWin.gameEvent.Invoke();
                }
            }
        }
    }
    public void AddVisualObjects(Vector3 position)
    {
        GameObject obj = PoolSystem.Instance.GetObject<GameObject>(visualPrefab);
        obj.transform.position = position;
        visualList.Add(obj);
    }

    public void ActivePaint() { isRun = true; }

    //birden fazla class olabilir static ile çalýþtýrma evente abone et

    public void ResetWall()
    {
        ClearVisualObjects();
        //material - texture
        texture = new Texture2D(textureArea.x, textureArea.y, TextureFormat.ARGB32, false); //create
        meshRenderer.material.mainTexture = texture; //add
        //vertex - percent
        paintedPercent = 0;
        foreach (var vertex in verticesData)
        {
            vertex.painted = false;
        }
    }

    public void ClearVisualObjects()
    {
        foreach (var obj in visualList)
        {
           PoolSystem.Instance.HideObject<GameObject>(visualPrefab, obj);
        }
        visualList.Clear();
    }
    private void Paint(Vector2 cordinate)
    {
        cordinate.x *= texture.width; //0 - 1024
        cordinate.y *= texture.height;
        Color32[] textureC32 = texture.GetPixels32();
        Color32[] brushC32 = brush.GetPixels32();

        Vector2Int halfbrush = new Vector2Int(brush.width / 2, brush.height / 2);

        for (int x = 0; x < brush.width; x++)
        {
            int xPos = x - halfbrush.x + (int)cordinate.x;
            if (xPos < 0 || xPos >= texture.width) 
                continue;
            for (int y = 0; y < brush.height; y++)
            {
                int yPos = y - halfbrush.y + (int)cordinate.y;
                if (yPos < 0 ||yPos >= texture.height) 
                    continue; 
                if (brushC32[x + (y * brush.width)].a > 0f)
                {
                    int tPos = xPos + (texture.width * yPos);
                    textureC32[tPos] = brushC32[x + (y * brush.width)];
                }
            }
        }

        texture.SetPixels32(textureC32);
        texture.Apply();
    }
}