using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public static event Action OnFinish;
    public static CharacterMovement instance;
    public enum State
    {
        Stand,
        Runing,
        Painting
    }

    public State state;

    public float forwardSpeed = 2f;
    public float sensitivity = 1f;
    private float lastFrameFingerPositionZ;
    private float moveFactorZ;
    public float bounceForce = 50000;

    private Animator animator;
    private Rigidbody rb;

    public ParticleSystem bounceVfx, macigVfx;
    public GameObject[] items;

    //Save Load
    const string filename = "savedata";
    SaveData saveData;

    private void Awake()
    {
        instance = this;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        PoolSystem.Instance.InitPool(bounceVfx, 3);

        LoadGame();
    }

    public void LoadGame()
    {
        saveData = jsonSaveLoad.Load<SaveData>(filename);
        if (saveData == null)// eðer hiç save yapýlmamýþsa
        {
            saveData = new SaveData(0);

            Debug.Log("Jarvis: Save Folder Created");
        }
        SetItemObject(saveData.itemNo);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V) && Application.isEditor)
        {
            transform.position = new Vector3(190, 0.15f, 0);
        }
        switch (state)
        {
            case State.Stand:
                break;
            case State.Runing:
                if (Input.GetMouseButtonDown(0))
                {
                    lastFrameFingerPositionZ = Input.mousePosition.x;
                }
                else if (Input.GetMouseButton(0))
                {
                    moveFactorZ = Input.mousePosition.x - lastFrameFingerPositionZ;
                    lastFrameFingerPositionZ = Input.mousePosition.x;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    moveFactorZ = 0;
                }

                //rb.MovePosition( new Vector3( //Dont Smooth
                //        transform.position.x + forwardSpeed * Time.deltaTime,
                //        rb.velocity.y, Mathf.Lerp(transform.position.z,
                //        transform.position.z - moveFactorZ * swerveSpeed,
                //        Time.deltaTime)));

                rb.AddForce(transform.right * moveFactorZ * Time.deltaTime * sensitivity * 300, ForceMode.Acceleration);

                transform.rotation = Quaternion.Euler(0, 90, 0);
                //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 90 + moveFactorZ, 0), 2f * Time.deltaTime);

                if (rb.velocity.x < 8)
                {
                    rb.AddForce(transform.forward * forwardSpeed * Time.deltaTime * 300, ForceMode.Acceleration);
                }
                break;
            case State.Painting:
                animator.SetBool("Mouse0", System.Convert.ToBoolean(Input.GetAxis("Fire1")));
                break;
            default:
                break;
        }
    }
    public void Ready()
    {
        animator.Rebind();
        state = State.Stand;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        animator.SetInteger("Case", 0);
        macigVfx.gameObject.SetActive(false);
        transform.position = new Vector3(-10, 0.15f, 0);
        GameSystem.SetCamera();
    }
    public void Run()
    {
        state = State.Runing;
        rb.isKinematic = false;
        animator.SetInteger("Case", 1);
    }

    public void StartPaint()
    {
        state = State.Painting;
        AudioManager.PlaySound(3);
        macigVfx.gameObject.SetActive(true);
        animator.SetInteger("Case", 2);
        rb.velocity = Vector3.zero;
        OnFinish.Invoke();
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Destroyer":
                rb.velocity = Vector3.zero;
                GameSystem.Reposition(gameObject);
                AudioManager.PlaySound(1);
                break;
            case "Bounce":
                //rb.velocity = Vector3.zero;
                rb.AddExplosionForce(bounceForce, collision.contacts[0].point, 1);
                AudioManager.PlaySound(0);

                var vfx = PoolSystem.Instance.GetInstance<ParticleSystem>(bounceVfx);
                vfx.time = 0.0f;
                vfx.Play();
                vfx.transform.position = transform.position;
                break;
            case "Ai":
                rb.velocity = Vector3.zero;
                rb.AddExplosionForce(bounceForce / 10, collision.contacts[0].point, 1);
                AudioManager.PlaySound(0);

                var aVfx = PoolSystem.Instance.GetInstance<ParticleSystem>(bounceVfx);
                aVfx.time = 0.0f;
                aVfx.Play();
                aVfx.transform.position = transform.position;
                break;
            case "Finish":
                collision.gameObject.SetActive(false);
                StartPaint();
                break;
            default:
                break;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "RotateObstacle")
        {
            //Vector3 direction = transform.position - collision.transform.position;
            //float angle = Vector3.Angle(direction, transform.right) - 90;
            //angle = Mathf.Clamp(angle, -1, 1);

            //rb.AddForce(transform.right * -angle * 10, ForceMode.Acceleration);

            if (collision.transform.rotation.y >= 0) //mor ve turkuaz silindir
                rb.AddForce(transform.right * 30, ForceMode.Acceleration);
            else //sarý silindir
                rb.AddForce(transform.right * -30, ForceMode.Acceleration);
        }
    }
    public static void SetItem(int index) => instance.SetItemObject(index);
    public void SetItemObject(int index)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (i == index) 
                items[i].SetActive(true); 
            else 
                items[i].SetActive(false);
        }
        saveData.itemNo = index;
        jsonSaveLoad.Save(filename, saveData);
    }
}

[System.Serializable]
public class SaveData
{
    public int itemNo;
    public SaveData(int itemNo)
    {
        this.itemNo = itemNo;
    }
    public SaveData()
    {

    }
}