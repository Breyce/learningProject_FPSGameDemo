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
    public Transform ShootPoint;//���ߴ����λ��
    public Transform BulletShootPoint;//�ӵ������λ��
    public Transform CasingBulletPoint;//�����׳���λ��

    [Header("Practical System")]
    public ParticleSystem muzzleFlash; // ǹ�ڻ���
    public ParticleSystem muzzleSpark; // ǹ�ڻ���
    public Light muzzleFlashLight; // ǹ�ڵƹ� 
    public float lightDuration; // �ƹ����ʱ��
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;

    [Header("Gun feature")]
    public float range;
    public float fireRate;
    public int bulletMag; //��ϻ�ӵ���
    private int currentBullet; // ��ǰ��ʣ�ӵ�
    private int bulletLeft; //����
    private float SpreadFactor; //�����ƫ����
    private float originRate; //ԭʼ����
    private float fireTimer; //��ʱ����������
    private float bulletForce; //�ӵ��������

    [Header("Sound Sources")]
    private AudioSource mainAudioSource;
    public SoundClips GunSound;

    private void Start()
    {
        //��ȡ���
        mainAudioSource = GetComponent<AudioSource>();

        //��ʼ������
        range = 300f;
        SpreadFactor = 1;
        bulletLeft = 5 * bulletMag;
        currentBullet = bulletMag;
    }

    private void Update()
    {
        //��������
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }
        if (Input.GetMouseButton(0))
        {
            //��ǹ���
            GunFire();
        }
    }

    public override void GunFire()
    {
        if (fireTimer < fireRate || currentBullet <= 0) return;

        // �����ƹ�Я��
        StartCoroutine(MuzzleFlashLight());

        // ���������Ч�ͻ�������
        mainAudioSource.clip = GunSound.shootSound;
        mainAudioSource.Play();
        muzzleFlash.Emit(1);
        muzzleSpark.Emit(Random.Range(minSparkEmission, maxSparkEmission));


        RaycastHit hit;
        Vector3 shootDirect = ShootPoint.forward;

        shootDirect = shootDirect + ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
        if (Physics.Raycast(ShootPoint.position, shootDirect, out hit, range))
        {
            Debug.Log(hit.transform.gameObject.name + "����");
        }

        currentBullet--;

        //���ü�ʱ��
        fireTimer = 0;
    }

    public IEnumerator MuzzleFlashLight(){

        //�����ƹ�
        muzzleFlashLight.enabled = true;

        yield return new WaitForSeconds(lightDuration);

        //Ϩ��ƹ�
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
