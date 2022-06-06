using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ai : MonoBehaviour
{
    public enum State
    {
        Stand,
        Runing
    }

    private Renderer mesh;
    private NavMeshAgent agent;
    private Animator animator;
    private Rigidbody rb;

    public State state;
    public Material mat;
    public ParticleSystem bounceVfx;

    private IEnumerator corountine;
    private Vector3 destination;


    public void CrateAvatar(float x, float y)
    {
        mesh = transform.GetChild(0).GetComponent<Renderer>();
        mat = Instantiate(mat);
        mat.mainTextureOffset = new Vector2(0.7f + (x / 10), (y / 20));
        mesh.material = mat;

        PoolSystem.Instance.InitPool(bounceVfx, 3);

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        Ready();
    }

    public void Ready()
    {
        rb.isKinematic = false;
        agent.enabled = false;
        rb.velocity = Vector3.zero;

        animator.SetBool("Fall", false);
        animator.SetInteger("Case", 0);
        state = State.Stand;
    }

    public void Go(Vector3 targetPoint)
    {
        destination = targetPoint;
        SetActivePhysic(false);
        agent.destination = targetPoint;
        animator.SetInteger("Case", 1);
        state = State.Runing;
    }

    private void Update()
    {
        if (state != State.Runing) { return; }
        Collider[] rangeChecks = Physics.OverlapCapsule(transform.position, transform.position + new Vector3(0,2.5f,0), 0.7f);
        foreach (var collision in rangeChecks)
        {
            switch (collision.gameObject.tag)
            {
                case "Destroyer":
                    if (corountine != null)
                    {
                        StopCoroutine(corountine);
                        corountine = null;
                    }
                    rb.velocity = Vector3.zero;
                    GameSystem.Reposition(gameObject);
                    AudioManager.Play3DSound(1, transform.position);
                    SetActivePhysic(false);
                    break;
                case "Bounce":
                    if (agent.enabled)
                    {
                        var bVfx = PoolSystem.Instance.GetInstance<ParticleSystem>(bounceVfx);
                        bVfx.time = 0.0f;
                        bVfx.Play();
                        bVfx.transform.position = transform.position + new Vector3(0.71f, 0, 0);
                        AudioManager.Play3DSound(0, transform.position);
                    }
                    SetActivePhysic(true);
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + new Vector3(0, 1.5f, 0), collision.transform.position, out hit))
                    {
                        rb.AddExplosionForce(20000, hit.point, 1);
                    }
                    CheckGround();
                    break;
                case "RotateObstacle":
                    Vector3 direction = transform.position - collision.transform.position;
                    float angle = Vector3.Angle(direction, transform.right) - 90;
                    if (Mathf.Abs(angle) > 10)
                    {
                        if (agent.enabled)
                        { 
                            SetActivePhysic(true);
                            CheckGround();
                        }

                        if (collision.transform.rotation.y >= 0) //mor ve turkuaz silindir
                            rb.AddForce(transform.right * 30, ForceMode.Acceleration);
                        else //sarý silindir
                            rb.AddForce(transform.right * -30, ForceMode.Acceleration);
                    }
                    break;
                case "Player":
                    SetActivePhysic(true);
                    CheckGround();
                    break;
                case "Ai":
                    if (collision.gameObject == gameObject) { return; }
                    if (agent.enabled)
                    {
                        var aVfx = PoolSystem.Instance.GetInstance<ParticleSystem>(bounceVfx);
                        aVfx.time = 0.0f;
                        aVfx.Play();
                        aVfx.transform.position = transform.position;
                        AudioManager.Play3DSound(0, transform.position);
                    }
                    SetActivePhysic(true);
                    CheckGround();
                    RaycastHit aiHit;
                    if (Physics.Raycast(transform.position + new Vector3(0,1,0), collision.transform.position + new Vector3(0, 1, 0), out aiHit)) //sadece duvarda çalýþtýr (performans)
                    {
                        rb.AddExplosionForce(1000, aiHit.point, 1);                        
                    }
                    break;
                case "Finish":
                    collision.gameObject.SetActive(false);
                    GameSystem.Lose();
                    break;
                default:
                    break;
            }
        }
    }

    public void SetActivePhysic(bool value)
    {
        animator.SetBool("Fall", value);
        rb.isKinematic = !value;
        agent.enabled = !value;
        if (agent.enabled == true)
        {
            transform.position += new Vector3(0,0.05f,0);
            agent.SetDestination(destination);
            state = State.Runing;
        }
    }


    public void CheckGround()
    {
        if (corountine != null)
        {
            StopCoroutine(corountine);
            corountine = null;
        }
        corountine = ReCheckGround();
        StartCoroutine(corountine);
    }

    IEnumerator ReCheckGround()
    {
        yield return new WaitForSeconds(1);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 0.2f)) //zemine basýyor ise
        {
            SetActivePhysic(false);
        }
        else //zemine basmýyor ise biraz daha bekle
        {
            CheckGround();
        }
    }
}
