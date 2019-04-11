using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum gunType
{
    Pistol = 1,
    AssaultRifle = 2,
    BattleRifle = 3,
    Shotgun = 4,
    Rocket = 5,
    Grenade = 6,
    Random = 7,
}
public class GunScript : MonoBehaviour
{

    //public string typeofGun;


    public bool hasPistol, hasAssault, hasBattle, hasShotgun, hasRocket, hasGrenade, hasRandom;
    public bool canShoot = true;
    public float shootspeed;
    public float shotcooldown;
    public float betweenShotTime;
    public int ammoCount;
    public int shotcount;
    public double bulletDamage;
    public float explosionTime;
    public float strayFactor;
    [Header("Audio")]
    public List<AudioClip> audioList;
    public AudioClip currentAudio;
    public AudioSource audioLocation;
    [Header("Gun Model")]
    public List<GameObject> gunMeshes;
    public GameObject currentGunModel;
    public gunType currentGun;
    public gunType previousGun;

    public List<GameObject> gunSFX;
    public GameObject currentSFX;
    [Header("Bullet Info")]
    public List<GameObject> projectileList;
    public GameObject currentProjectile;
    public List<GameObject> bulletHoleList;
    public GameObject currentBulletHole;




    // Use this for initialization
    void Start()
    {
        hasPistol = true;
        hasAssault = true;
        previousGun = gunType.Pistol;
        currentGun = gunType.AssaultRifle;
        GetAssaultRifle();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.G) && currentGun == gunType.Grenade)
        {
            StartCoroutine(Shoot());
        }
        if (Input.GetButton("Fire1") && canShoot && currentGun != gunType.Grenade)
        {
            StartCoroutine(Shoot());

        }
        if (Input.GetKeyDown(KeyCode.Q) && canShoot)
        {
            var LastUsed = currentGun;
            currentGun = previousGun;
            previousGun = LastUsed;
            PickUpGun();
        }
    }

    IEnumerator Shoot()
    {

        canShoot = false;

        for (int i = 0; i < shotcount; i++)
        {
            currentSFX.SetActive(true);
            Vector3 shootDirection = this.GetComponentInChildren<Camera>().transform.forward;
            shootDirection.x = Random.Range(-strayFactor, strayFactor);
            shootDirection.y = Random.Range(-strayFactor, strayFactor);
            shootDirection.z = Random.Range(-strayFactor, strayFactor);
            var bullet = (GameObject)Instantiate(
                currentProjectile,
                this.GetComponentInChildren<Camera>().transform.position,
                this.GetComponentInChildren<Camera>().transform.rotation
            );
            //StartCoroutine(PlaySFX());
            Physics.IgnoreCollision(bullet.GetComponent<Collider>(), GetComponent<Collider>());
            bullet.transform.Rotate(shootDirection.x, shootDirection.y, shootDirection.z);
            bullet.GetComponent<Rigidbody>().AddForce(shootspeed * bullet.transform.forward * 100);
            Destroy(bullet, 10.0f);
            yield return new WaitForSeconds(shotcooldown);
        }
        audioLocation.PlayOneShot(currentAudio);
        //AudioSource.PlayClipAtPoint(currentAudio, this.GetComponentInChildren<Camera>().transform.position);
        currentSFX.SetActive(false);
        yield return new WaitForSeconds(betweenShotTime);
        canShoot = true;

    }
    IEnumerator PlaySFX()
    {

        if (canShoot)
        {

        }
        yield return new WaitForSeconds(0.1f);

    }

    public void PickUpGun()
    {
        switch (currentGun)
        {
            case gunType.Pistol:
                hasPistol = true;
                GetPistol();
                break;
            case gunType.AssaultRifle:
                hasAssault = true;
                GetAssaultRifle();
                break;
            case gunType.BattleRifle:
                hasBattle = true;
                GetBattleRifle();
                break;
            case gunType.Shotgun:
                hasShotgun = true;
                GetShotgun();
                break;
            case gunType.Grenade:
                hasGrenade = true;
                GetGrenade();
                break;
            case gunType.Random:
                hasRandom = true;
                GetRandom();
                break;
        }
    }
    void DisableGuns()
    {
        foreach (GameObject obj in gunMeshes)
        {
            obj.SetActive(false);
        }
    }

    void GetPistol()
    {
        if (hasPistol)
        {
            DisableGuns();
            gunMeshes[0].SetActive(true);
            currentGunModel = gunMeshes[0];
            currentAudio = audioList[0];
            currentProjectile = projectileList[0];
            currentBulletHole = bulletHoleList[0];
            currentSFX = gunSFX[0];
            strayFactor = 0;
            bulletDamage = 20.0;
            currentProjectile.GetComponent<GunDebug>().bulletDamage = bulletDamage;
            currentProjectile.GetComponent<GunDebug>().bulletDecal = currentBulletHole;
            shootspeed = 200f;
            shotcount = 1;
            shotcooldown = 0.3f;
            betweenShotTime = 0.00001f;
            ammoCount = 12;
            Debug.Log("Picked Up Pistol");
        }
        else
        {
            Debug.Log("Don't Have Pistol");
        }
    }
    void GetAssaultRifle()
    {
        if (hasAssault)
        {
            DisableGuns();
            gunMeshes[1].SetActive(true);
            currentGunModel = gunMeshes[1];
            currentAudio = audioList[1];
            currentProjectile = projectileList[1];
            currentBulletHole = bulletHoleList[1];
            currentSFX = gunSFX[1];
            strayFactor = 2.5f;
            bulletDamage = 12.5;
            currentProjectile.GetComponent<GunDebug>().bulletDamage = bulletDamage;
            currentProjectile.GetComponent<GunDebug>().bulletDecal = currentBulletHole;
            shootspeed = 250f;
            shotcount = 1;
            shotcooldown = 0.03f;
            betweenShotTime = 0.05f;
            ammoCount = 60;
            Debug.Log("Picked Up AR");
        }
        else
        {
            Debug.Log("Don't Have AR");
        }
    }

    void GetBattleRifle()
    {
        if (hasBattle)
        {
            DisableGuns();
            gunMeshes[2].SetActive(true);
            currentGunModel = gunMeshes[2];
            currentAudio = audioList[2];
            currentProjectile = projectileList[2];
            currentBulletHole = bulletHoleList[2];
            currentSFX = gunSFX[2];
            strayFactor = 0.5f;
            shootspeed = 150f;
            bulletDamage = 11.25;
            currentProjectile.GetComponent<GunDebug>().bulletDamage = bulletDamage;
            currentProjectile.GetComponent<GunDebug>().bulletDecal = currentBulletHole;
            shotcount = 3;
            shotcooldown = 0.02f;
            betweenShotTime = 0.5f;
            ammoCount = 30;
            Debug.Log("Picked Up BR");
        }
        else
        {
            Debug.Log("Don't Have BR");
        }
    }
    void GetShotgun()
    {
        if (hasShotgun)
        {
            DisableGuns();
            gunMeshes[3].SetActive(true);
            currentGunModel = gunMeshes[3];
            currentAudio = audioList[3];
            currentProjectile = projectileList[3];
            currentBulletHole = bulletHoleList[3];
            currentSFX = gunSFX[3];
            strayFactor = 6.5f;
            shootspeed = 150f;
            bulletDamage = 15.50;
            currentProjectile.GetComponent<GunDebug>().bulletDamage = bulletDamage;
            currentProjectile.GetComponent<GunDebug>().bulletDecal = currentBulletHole;
            shotcount = 14;
            shotcooldown = 0f;
            betweenShotTime = 0.7f;
            ammoCount = 12;
            Debug.Log("Picked Up SG");
        }
        else
        {
            Debug.Log("Don't Have SG");
        }
    }
    void GetGrenade()
    {
        if (hasGrenade)
        {
            DisableGuns();
            gunMeshes[4].SetActive(true);
            currentGunModel = gunMeshes[4];
            currentAudio = audioList[4];
            currentProjectile = projectileList[4];
            currentBulletHole = bulletHoleList[4];
            currentSFX = gunSFX[4];
            explosionTime = 1.7f;
            strayFactor = 0f;
            shootspeed = 10f;
            bulletDamage = 25.00;
            currentProjectile.GetComponent<GunDebug>().bulletDamage = bulletDamage;
            currentProjectile.GetComponent<GunDebug>().bulletDecal = currentBulletHole;
            shotcount = 1;
            shotcooldown = 0f;
            betweenShotTime = 1f;
            ammoCount = 2;
            Debug.Log("Picked Up Grenade");
        }
        else
        {
            Debug.Log("Don't Have Grenade");
        }
    }
    void GetRandom()
    {
        if (hasRandom)
        {
            DisableGuns();
            gunMeshes[Random.Range(0, gunMeshes.Count)].SetActive(true);
            currentGunModel = gunMeshes[Random.Range(0, gunMeshes.Count)];
            currentAudio = audioList[Random.Range(0, audioList.Count)];
            currentProjectile = projectileList[Random.Range(0, projectileList.Count)];
            currentBulletHole = bulletHoleList[Random.Range(0, bulletHoleList.Count)];
            currentSFX = gunSFX[Random.Range(0, gunSFX.Count)];
            explosionTime = Random.Range(0, 0.5f);
            strayFactor = Random.Range(0, 12f);
            shootspeed = Random.Range(50f, 150f);
            bulletDamage = Random.Range(0f, 25f);
            currentProjectile.GetComponent<GunDebug>().bulletDamage = bulletDamage;
            currentProjectile.GetComponent<GunDebug>().bulletDecal = currentBulletHole;
            shotcount = Random.Range(0, 20);
            shotcooldown = Random.Range(0, 0.2f);
            betweenShotTime = Random.Range(0.2f, 0.7f);
            ammoCount = Random.Range(4, 120);
            Debug.Log("Picked Up Random");
        }
        else
        {
            Debug.Log("Don't Have Random");
        }
    }
}
