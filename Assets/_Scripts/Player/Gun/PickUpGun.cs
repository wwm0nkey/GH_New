using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpGun : MonoBehaviour
{

    public GunScript _gs;
    public bool triggerEntered = false;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && triggerEntered == true)
        {
            // _gs.previousGun = _gs.currentGun;
            var newgun = Instantiate(_gs.currentGunModel, this.transform.position, this.transform.rotation);
            switch (_gs.currentGun)
            {
                case gunType.Shotgun:
                    _gs.hasShotgun = false;
                    break;
                case gunType.Pistol:
                    _gs.hasPistol = false;
                    break;
                case gunType.AssaultRifle:
                    _gs.hasAssault = false;
                    break;
                case gunType.BattleRifle:
                    _gs.hasBattle = false;
                    break;
                case gunType.Grenade:
                    _gs.hasGrenade = false;
                    break;
                case gunType.Random:
                    _gs.hasRandom = false;
                    break;
            }
            _gs.currentGun = (gunType)System.Enum.Parse(typeof(gunType), this.tag.ToString());
            _gs.PickUpGun();
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            _gs = other.GetComponent<GunScript>();
            Debug.Log("Can PickUp Gun");
            triggerEntered = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Exited Range");
            triggerEntered = false;
        }
    }

    private void OnCollisionEnter(Collision other)
    {

    }
}
