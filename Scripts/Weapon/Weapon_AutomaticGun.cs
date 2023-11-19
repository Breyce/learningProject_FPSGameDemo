using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundClips
{
    public AudioClip shootSound;
    public AudioClip shootSound_Silencer;
    public AudioClip reloadSoundAmmoLeft;
    public AudioClip reloadSoundAmmoRunOut;
}
public class Weapon_AutomaticGun : Weapon
{
    [Header("Shooting")]
    public Transform ShootPoint;//射线打出的位置
    public Transform BulletShootPoint;//子弹打出的位置
    public Transform CasingBulletPoint;//弹壳抛出的位置

    [Header("Practical System")]
    public ParticleSystem muzzleFlash; // 枪口火焰
    public ParticleSystem muzzleSpark; // 枪口火星
    public Light muzzleFlashLight; // 枪口灯光 
    public float lightDuration; // 灯光持续时间
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;

    [Header("Gun feature")]
    public float range;
    public float fireRate;
    public int bulletMag; //弹匣子弹数
    private int currentBullet; // 当前所剩子弹
    private int bulletLeft; //备弹
    private float SpreadFactor; //射击的偏移量
    private float originRate; //原始射速
    private float fireTimer; //计时器控制射速
    private float bulletForce; //子弹发射的力

    [Header("Sound Sources")]
    private AudioSource mainAudioSource;
    public SoundClips GunSound;

    private void Start()
    {
        //获取组件
        mainAudioSource = GetComponent<AudioSource>();

        //初始化参数
        range = 300f;
        SpreadFactor = 1;
        bulletLeft = 5 * bulletMag;
        currentBullet = bulletMag;
    }

    private void Update()
    {
        //控制射速
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }
        if (Input.GetMouseButton(0))
        {
            //开枪射击
            GunFire();
        }
    }

    public override void GunFire()
    {
        if (fireTimer < fireRate || currentBullet <= 0) return;

        // 开启灯光携程
        StartCoroutine(MuzzleFlashLight());

        // 播放射击音效和火光和粒子
        mainAudioSource.clip = GunSound.shootSound;
        mainAudioSource.Play();
        muzzleFlash.Emit(1);
        muzzleSpark.Emit(Random.Range(minSparkEmission, maxSparkEmission));


        RaycastHit hit;
        Vector3 shootDirect = ShootPoint.forward;

        shootDirect = shootDirect + ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
        if (Physics.Raycast(ShootPoint.position, shootDirect, out hit, range))
        {
            Debug.Log(hit.transform.gameObject.name + "打到了");
        }

        currentBullet--;

        //重置计时器
        fireTimer = 0;
    }

    public IEnumerator MuzzleFlashLight(){

        //点亮灯光
        muzzleFlashLight.enabled = true;

        yield return new WaitForSeconds(lightDuration);

        //熄灭灯光
        muzzleFlashLight.enabled = false;
    }
    public override void AimIn()
    {
    }

    public override void AimOut()
    {
    }

    public override void DoReloadAnimation()
    {
    }

    public override void ExpaningCrossUpdate(float expanDegree)
    {
    }

    public override void Reload()
    {
    }
}
