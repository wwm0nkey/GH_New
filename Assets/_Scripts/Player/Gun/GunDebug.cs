using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunDebug : MonoBehaviour
{
    public double bulletDamage;
    private GunScript gunScript;
    public GameObject bulletDecal;
    public GameObject hitEffect;
    // Use this for initialization
    void Awake()
    {
        gunScript = GameObject.FindGameObjectWithTag("Player").GetComponent<GunScript>();
        if (this.tag == "Grenade")
        {
            Debug.Log("I am a Grenade");
            StartCoroutine(AirTime());
        }
    }

    // Update is called once per frame
    void Update()
    {



    }

    IEnumerator Detonate()
    {
        yield return new WaitForSeconds(gunScript.explosionTime);
        // Instantiate()
        BlowUp();
    }

    IEnumerator AirTime()
    {
        yield return new WaitForSeconds(3f);
        BlowUp();
    }

    public void BlowUp()
    {
        Instantiate(bulletDecal, transform.position, transform.rotation);
        Collider[] collidersRad1 = Physics.OverlapSphere(transform.position, 2.5f);
        foreach (Collider nearbyObj in collidersRad1)
        {
            if (nearbyObj.transform.tag == "Enemy")
            {
                nearbyObj.GetComponent<SimpleAI>().TakeDamage(bulletDamage * 4);
            }
            Rigidbody rb = nearbyObj.GetComponent<Rigidbody>();
            if (rb != null)
            {

                rb.AddExplosionForce(700f, transform.position, 4f, 1f);
            }
        }
        Collider[] collidersRad2 = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider nearbyObj in collidersRad2)
        {
            {
                if (nearbyObj.transform.tag == "Enemy")
                {
                    nearbyObj.GetComponent<SimpleAI>().TakeDamage(bulletDamage * 2);
                }
                Rigidbody rb = nearbyObj.GetComponent<Rigidbody>();
                if (rb != null)

                    rb.AddExplosionForce(700f, transform.position, 7f, 0.5f);
            }
        }
        Collider[] collidersRad3 = Physics.OverlapSphere(transform.position, 12);
        foreach (Collider nearbyObj in collidersRad3)
        {
            if (nearbyObj.transform.tag == "Enemy")
            {
                nearbyObj.GetComponent<SimpleAI>().TakeDamage(bulletDamage);
            }
            Rigidbody rb = nearbyObj.GetComponent<Rigidbody>();
            if (rb != null)
            {

                rb.AddExplosionForce(700f, transform.position, 10f, 0.2f);
            }
        }
        Debug.Log("Boom");
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (this.tag == "Grenade")
        {
            StartCoroutine(Detonate());
            this.GetComponent<SphereCollider>().material.bounciness = 0;
            this.GetComponent<Rigidbody>().velocity = this.GetComponent<Rigidbody>().velocity / 1.5f;
            //Use for Plasma Grenades
            // this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
            // this.GetComponent<Rigidbody>().freezeRotation = true;
            // this.GetComponent<SphereCollider>().enabled = false;

            return;
        }
        if (collision.transform.tag == "Grenade")
        {
            collision.transform.GetComponent<GunDebug>().BlowUp();
        }
        if (collision.transform.tag == "Ammo")
        {
            return;
        }
        if (collision.transform.tag == "Enemy")
        {
            var aiObject = collision.gameObject.GetComponent<SimpleAI>();
            aiObject.TakeDamage(bulletDamage);
            // if (collision.transform.GetComponent<Rigidbody>() != null)
            // {
            //     collision.transform.GetComponent<Rigidbody>().AddExplosionForce(100f, transform.position, 1f, 2f);
            //     //collision.transform.GetComponent<Rigidbody>().AddForceAtPosition(collision.transform.position - transform.position, transform.position);
            // }
        }
        Debug.Log("Collided With " + collision.transform.tag.ToString());
        foreach (ContactPoint contact in collision.contacts)
        {
            var bDecal = Instantiate(bulletDecal, contact.point, Quaternion.FromToRotation(Vector3.up, contact.normal));
            Instantiate(hitEffect, transform.position, transform.rotation);
            bDecal.transform.parent = collision.transform;
            Destroy(bDecal, 20f);
            Debug.DrawRay(contact.point, contact.normal, Color.red, 1f);
            // if (bulletDamage == 20)
            // {
            //     Debug.DrawRay(contact.point, contact.normal, Color.magenta, 500f);
            // }
            // if (bulletDamage == 9.5)
            // {
            //     Debug.DrawRay(contact.point, contact.normal, Color.red, 500f);
            // }
            // if (bulletDamage == 11.25)
            // {
            //     Debug.DrawRay(contact.point, contact.normal, Color.blue, 500f);
            // }
            // if (bulletDamage == 11.25)
            // {
            //     Debug.DrawRay(contact.point, contact.normal, Color.green, 500f);
            // }
        }
        if (this.tag != "Grenade")
        {
            Destroy(this.gameObject);
        }
    }
    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Enemy")
        {
            var aiObject = other.gameObject.GetComponent<SimpleAI>();
            aiObject.TakeDamage(bulletDamage);
        }
    }
}
