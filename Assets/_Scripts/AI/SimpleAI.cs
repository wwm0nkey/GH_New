// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.AI;

// public class SimpleAI : MonoBehaviour
// {

//     public GameObject aiBody;
//     public GameObject player;
//     public AudioSource audioSource;
//     public AudioClip painSFX;
//     public Material aiMat;
//     public double health;
//     NavMeshAgent agent;
//     public Vector3 RandomNavmeshLocation(float radius)
//     {
//         Vector3 randomDirection = Random.insideUnitSphere * radius;
//         randomDirection += transform.position;
//         NavMeshHit hit;
//         Vector3 finalPosition = Vector3.zero;
//         if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
//         {
//             finalPosition = hit.position;
//         }
//         return finalPosition;
//     }
//     // Use this for initialization
//     void Start()
//     {
//         player = GameObject.FindGameObjectWithTag("Player");
//         audioSource = GetComponent<AudioSource>();
//         agent = GetComponent<NavMeshAgent>();
//         agent.SetDestination(RandomNavmeshLocation(100f));
//     }

//     // Update is called once per frame
//     void Update()
//     {

//         if (!agent.pathPending)
//         {
//             if (agent.remainingDistance <= agent.stoppingDistance)
//             {
//                 if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
//                 {
//                     agent.SetDestination(RandomNavmeshLocation(400f));
//                 }
//             }
//         }
//     }

//     public void TakeDamage(double amount)
//     {

//         health = health - amount;
//         if (health <= 75)
//         {
//             this.GetComponent<Renderer>().material.color = Color.yellow;

//         }
//         if (health <= 25)
//         {
//             this.GetComponent<Renderer>().material.color = Color.red;
//         }
//         if (health <= 0)
//         {
//             AudioSource.PlayClipAtPoint(painSFX, this.transform.position);
//             player.GetComponent<GrapplingHook>().grappling_line.SetActive(false);
//             Destroy(this.gameObject);

//         }
//     }
// }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleAI : MonoBehaviour
{

    public GameObject aiBody;
    public GameObject player;
    public AudioSource audioSource;
    public AudioClip painSFX;
    public Material aiMat;
    public double health;

    public bool firstCheck = false;
    public bool secondCheck = false;
    public bool thirdCheck = false;
    public bool isTakingDamage = false;
    public bool isDead = false;
    NavMeshAgent agent;
    public Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }
    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(RandomNavmeshLocation(100f));
    }

    // Update is called once per frame
    void Update()
    {
        // if (isDead)
        // {
        //     this.gameObject.AddComponent<Rigidbody>().velocity = Vector3.ClampMagnitude(this.gameObject.AddComponent<Rigidbody>().velocity, 20f);
        // }
        if (!isDead)
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        agent.SetDestination(RandomNavmeshLocation(400f));
                    }
                }
            }
        }
    }

    public void TakeDamage(double amount)
    {

        health = health - amount;
        if (health <= 75 && !firstCheck)
        {
            audioSource.PlayOneShot(painSFX);
            this.GetComponent<Renderer>().material.color = Color.yellow;
            //StartCoroutine(FallBack());
            firstCheck = true;


        }
        if (health <= 25 && !secondCheck)
        {

            audioSource.PlayOneShot(painSFX);
            this.GetComponent<Renderer>().material.color = Color.red;
            //StartCoroutine(FallBack());
            secondCheck = true;

        }
        if (health <= 0 && !thirdCheck)
        {
            thirdCheck = true;
            isDead = true;
            this.GetComponent<NavMeshAgent>().enabled = false;
            ///this.gameObject.GetComponent<Rigidbody>().AddExplosionForce(20f, transform.position, 2, 20f);
            StartCoroutine(Die());

        }


    }

    IEnumerator FallBack()
    {
        this.gameObject.AddComponent<Rigidbody>();
        yield return new WaitForSeconds(0.2f);
        Destroy(this.gameObject.GetComponent<Rigidbody>());
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(0.1f);
        if (this.gameObject.GetComponent<Rigidbody>() == null)
        {
            this.gameObject.AddComponent<Rigidbody>();
        }
        yield return new WaitForSeconds(5f);
        if (this.GetComponentInChildren<GravityBody>() != null)
        {
            player.GetComponent<GrapplingHook>().BreakTether();
        }
        AudioSource.PlayClipAtPoint(painSFX, this.transform.position);
        Destroy(this.gameObject);
    }
}
