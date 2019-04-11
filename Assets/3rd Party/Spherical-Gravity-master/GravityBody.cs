using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{

    public GravityAttractor planet;
    public Rigidbody rigidbody;
    public bool canAttract = false;
    public bool lowGravity = true;
    //public Text GravityOnOff;
    void Awake()
    {
        //planet = GameObject.FindGameObjectWithTag("Planet").GetComponent<GravityAttractor>();
        rigidbody = GetComponent<Rigidbody>();

        // Disable rigidbody gravity and rotation as this is simulated in GravityAttractor scrip
        canAttract = false;

        //rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            canAttract = true;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            canAttract = false;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (lowGravity)
            {
                lowGravity = false;
                // GravityOnOff.text = "Low Gravity: Off";
            }
            else if (!lowGravity)
            {
                lowGravity = true;
                // GravityOnOff.text = "Low Gravity: On";
            }
        }
        if (Input.GetButtonDown("Fire2"))
        {
            canAttract = true;
            StartCoroutine(GetPlanet());
        }
        if (Input.GetButtonUp("Fire2"))
        {
            canAttract = false;
        }
    }

    void FixedUpdate()
    {
        // Allow this body to be influenced by planet's gravity
        if (planet != null && canAttract == true && planet.name == transform.name + " Hook")
        {
            if (lowGravity == true)
            {
                rigidbody.useGravity = false;
            }
            else
            {
                rigidbody.useGravity = true;
            }
            planet.Attract(rigidbody);
        }
        if (canAttract == false)
        {
            rigidbody.useGravity = true;
        }
    }

    IEnumerator GetPlanet()
    {
        yield return new WaitForSeconds(0.02f);
        //planet = GameObject.FindGameObjectWithTag("Planet").GetComponent<GravityAttractor>();
    }
}




