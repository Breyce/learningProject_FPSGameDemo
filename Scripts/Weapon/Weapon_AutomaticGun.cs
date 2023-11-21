using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Weapon_AutomaticGun;
using Random = UnityEngine.Random;



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
    private PlayerMovement player;
    private Animator anim;

    [Header("Amming")]
    private Camera mainCamera;
    public Camera gunCamera;
    private Vector3 sniperFiflePosition; // ǹе��ʼλ��
    public Vector3 sniperFifleOnPosition; // ǹе������׼��֮���λ��

    [Header("Shooting")]
    public Transform ShootPoint;//���ߴ����λ��
    public Transform BulletShootPoint;//�ӵ������λ��
    public Transform CasingBulletPoint;//�����׳���λ��
    public Transform bulletPrefab;
    public Transform casingPrefab;

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
    private bool gunShoot;
    private bool isReload;
    private bool isAmming;

    [Header("Sound Sources")]
    private AudioSource mainAudioSource;
    public SoundClips GunSound;

    [Header("UI")]
    public Image[] crossQuarterImges; //׼��
    public Text ammTextUI;
    public Text shootModeTextUI;
    public float currentExpanedDegree; //��ǰ׼�Ŀ��϶�
    private float crossExpanedDegree; // ÿ֡׼�ĵĿ��϶�
    private float maxCrossDegree;

    [Header("Key Binds")]
    private KeyCode reloadBulletKey;
    private KeyCode shootModeChange;
    private KeyCode inspectGun;
    
    /* ö�ٶ���ȫ�Զ��Ͱ��Զ����� */
    public enum ShootMode
    {
        AutoRife, SemiGun
    }
    public ShootMode shootMode;
    private string shootModeName;

    public PlayerMovement.MovementState state;



    private void Start()
    {
        //������
        reloadBulletKey = KeyCode.R;
        shootModeChange = KeyCode.C;
        inspectGun = KeyCode.E;

        //��ȡ���
        mainAudioSource = GetComponent<AudioSource>();
        player = GetComponentInParent<PlayerMovement>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;

        //��ʼ������
        range = 300f;
        maxCrossDegree = 150f;
        crossExpanedDegree = 30f;
        SpreadFactor = 0.1f;// ��׼ʱҪȷ��SpreadFactor�ܹ����ȶ�

        bulletForce = 100f;
        bulletLeft = 5 * bulletMag;
        currentBullet = bulletMag;

        shootMode = ShootMode.AutoRife;
        shootModeName = "ȫ�Զ�ģʽ";

        sniperFiflePosition = transform.localPosition;
        // ��ȾUI
        UpdateAmmoUI();
    }

    private void Update()
    {
        //״̬�����
        StateHandler();

        switch (shootMode)
        {
            case ShootMode.AutoRife: // ȫ�Զ�
                gunShoot = Input.GetMouseButton(0);
                fireRate = 0.1f;
                break;
            case ShootMode.SemiGun: //���Զ�
                gunShoot = Input.GetMouseButtonDown(0);
                fireRate = 0.2f;
                break;
        }

        if (gunShoot)
        {
            //��ǹ���
            GunFire();
        }

        //����׼��
        state = player.state;
        if(state == PlayerMovement.MovementState.walking 
            && state != PlayerMovement.MovementState.sprinting 
            && state != PlayerMovement.MovementState.crouching
            && player.thRB.velocity.sqrMagnitude > 0.9f )
        {
            //�ƶ�ʱ��׼�Ŀ��϶�
            ExpandingCrossUpdate(crossExpanedDegree);
        }
        else if(state != PlayerMovement.MovementState.walking
            && state == PlayerMovement.MovementState.sprinting
            && state != PlayerMovement.MovementState.crouching
            && player.thRB.velocity.sqrMagnitude > 0.9f)
        {
            //����ʱ��׼�Ŀ��϶�
            ExpandingCrossUpdate(crossExpanedDegree * 2);
        }
        else
        {
            //����վ�����¶�ʱ������׼�Ŀ��϶�
            ExpandingCrossUpdate(0);
        }

        //ʵ������ƶ�����
        anim.SetBool("isRun", player.isRun);
        anim.SetBool("isWalk", player.isWalk);

        //ʵ�ֲ鿴����
        if (Input.GetKeyDown(inspectGun))
        {
            anim.SetTrigger("Inspect");
        }

        //ʵ�ֻ�������
        bool reloadAmmoLeft = anim.GetCurrentAnimatorStateInfo(0).IsName("reloadAmmoLeft");
        bool reloadOutOfAmmo = anim.GetCurrentAnimatorStateInfo(0).IsName("reloadOutOfAmmo");

        if (reloadOutOfAmmo || reloadAmmoLeft)
        {
            isReload = true;
        }
        else
        {
            isReload = false;
        }

        if (Input.GetKey(reloadBulletKey) 
            && currentBullet < bulletMag 
            && !isReload 
            && !anim.GetCurrentAnimatorStateInfo(0).IsName("inspectWeapon"))
        {
            DoReloadAnimation();
        }


        //ʵ����׼������
        //��׼������ľ��Ȳ�ͬ
        SpreadFactor = (isAmming) ? 0.01f : 0.1f;

        if(Input.GetMouseButton(1) && !isReload && !player.isRun)
        {
            //��׼��1. ׼����ʧ�� 2. ��Ұ��ǰ
            isAmming = true;

            //��׼��������Ч
            anim.SetBool("isAim", isAmming);

            //��Ұ��ǰ
            transform.localPosition = sniperFifleOnPosition;
        }
        else
        {
            //�˳���׼��1. ׼�ǳ��֣� 2. ��Ұ�ظ�
            isAmming = false;

            //��׼��������Ч
            anim.SetBool("isAim", isAmming);

            //��Ұ��ǰ
            transform.localPosition = sniperFiflePosition;
        }

        //��������
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

    }

    /* ��ǹ���ƺ���
     * GunFire()����ǹ����
     * MuzzleFlashLight()�����ƿ�ǹ����
     * AimIn()����׼�����߼�
     * AimOut()����׼�˳��߼�
     */
    public override void GunFire()
    {
        if (fireTimer < fireRate 
            || player.isRun
            || currentBullet <= 0 
            || anim.GetCurrentAnimatorStateInfo(0).IsName("takeOutWapon")
            || anim.GetCurrentAnimatorStateInfo(0).IsName("inspectWeapon")
            || isReload) return;

        // �����ƹ�׼��Я��
        StartCoroutine(MuzzleFlashLight());
        StartCoroutine(Shoot_Cross());

        // ���������Ч�ͻ�������
        mainAudioSource.clip = GunSound.shootSound;
        mainAudioSource.Play();
        muzzleFlash.Emit(1);
        muzzleSpark.Emit(Random.Range(minSparkEmission, maxSparkEmission));


        RaycastHit hit;
        Vector3 shootDirect = BulletShootPoint.forward;

        shootDirect = shootDirect + BulletShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
        if (Physics.Raycast(BulletShootPoint.position, shootDirect, out hit, range))
        {
            Transform bullet = Instantiate(bulletPrefab, BulletShootPoint.transform.position, BulletShootPoint.transform.rotation);
            
            //���ӵ���Ԥ�Ƶķ�����һ���ٶ�
            //bullet.transform.LookAt(hit.point);
            bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + shootDirect) * bulletForce;
            Debug.Log(hit.transform.gameObject.name + "����");
        }

        //�����������
        if (!isAmming)
        {
            anim.CrossFadeInFixedTime("fire", 0.1f);
        }
        else
        {
            //������׼״̬�µĿ����
            anim.Play("aimFire", 0, 0);
        }

        //ʵ���������׳� 
        Instantiate(casingPrefab, CasingBulletPoint.transform.position, CasingBulletPoint.transform.rotation);

        currentBullet--;

        //���ü�ʱ��
        fireTimer = 0;

        //�����ӵ�UI
        UpdateAmmoUI();
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
        float currentVelocity = 0.1f;
        //׼����ʧ
        for (int i = 0; i < crossQuarterImges.Length; i++)
        {
            crossQuarterImges[i].gameObject.SetActive(false);
        }

        //�ı���Ұ����Ұ���
        mainCamera.fieldOfView = Mathf.SmoothDamp(30, 60, ref currentVelocity, 0.3f);

        //��������
        AudioManager.instance.PlayNoNeedStopSound(0);
    }

    public override void AimOut()
    {
        float currentVelocity = 0.1f;
        //׼�ǳ���
        for (int i = 0; i < crossQuarterImges.Length; i++)
        {
            crossQuarterImges[i].gameObject.SetActive(true);
        }

        //�ı���Ұ����Ұ��Զ
        mainCamera.fieldOfView = Mathf.SmoothDamp(60, 30, ref currentVelocity, 0.3f);

    }

    /**
     * �������ƺ���
     * DoReloadAnimation()�����Ų�ͬ�Ļ�������
     * Reload()����װ��ҩ�߼����ڶ������е���
     */
    public override void DoReloadAnimation()
    {
        if(currentBullet > 0 && bulletLeft > 0)
        {
            anim.Play("reloadAmmoLeft");
            Reload();
            mainAudioSource.clip = GunSound.reloadSoundAmmoLeft;
            mainAudioSource.Play();
        }
        else if (currentBullet == 0 && bulletLeft > 0)
        {
            anim.Play("reloadOutOfAmmo");
            Reload();
            mainAudioSource.clip = GunSound.reloadSoundAmmoRunOut;
            mainAudioSource.Play();
        }
    }

    public override void Reload()
    {
        if (bulletLeft <= 0) return;

        //���㱸��������
        int bulletChange = bulletMag - currentBullet;

        if (bulletLeft > bulletChange)
        {
            //���±�������
            bulletLeft -= bulletChange;

            //��䵱ǰ�ӵ�
            currentBullet = bulletMag;
        }
        else if (bulletLeft <= bulletChange)
        {
            //���µ�ǰ�ӵ�����
            currentBullet += bulletLeft;

            //���±�������
            bulletLeft = 0;
        }

        UpdateAmmoUI();
    }

    /*
     * UI���ƺ���
     */
    public void UpdateAmmoUI()
    {
        ammTextUI.text = "BULLET: " + currentBullet + " / " + bulletLeft;
        shootModeTextUI.text = shootModeName;
    }

    /*
     * ׼�Ŀ��ƺ���
     * ExpandingCrossUpdate(float expanDegree)�����ڵ���׼��
     * ExpendCross(float add)�����ڿ���׼�Ŀ���
     * Shoot_Cross()��Я�̣�����׼�Ŀ��϶ȣ�һִ֡��5�Σ�ֻ�������ʱ˲������׼��
     */
    public override void ExpandingCrossUpdate(float expanDegree)
    {
        if(currentExpanedDegree < expanDegree - 5)
        {
            ExpendCross(150 * Time.deltaTime);
        }
        else if(currentExpanedDegree > expanDegree + 5)
        {
            ExpendCross(-300 * Time.deltaTime);
        }
    }
    
    public void ExpendCross(float add)
    {
        crossQuarterImges[0].transform.localPosition += new Vector3(-add,0,0);  //Left
        crossQuarterImges[1].transform.localPosition += new Vector3(add, 0,0);  //Right
        crossQuarterImges[2].transform.localPosition += new Vector3(0,add,0);  //Top
        crossQuarterImges[3].transform.localPosition += new Vector3(0,-add,0);  // Bottom

        //���浱ǰ׼�Ŀ��϶�
        currentExpanedDegree += add;
        currentExpanedDegree = Mathf.Clamp(currentExpanedDegree, 0, maxCrossDegree);
    }

    public IEnumerator Shoot_Cross()
    {
        yield return null;
        for (int i = 0; i < 5; i++)
        {
            ExpendCross(Time.deltaTime * 500);
        }

        for (int i = 0; i < 5; i++)
        {
            ExpendCross(Time.deltaTime * -500);
        }
    }

    /* 
     * ȫ�Զ����Զ��л�״̬��
     * StateHandler():�������ģʽ״̬
     */
    public void StateHandler()
    {
        if (Input.GetKeyDown(shootModeChange))
        {
            if (shootMode == ShootMode.AutoRife)
            {
                shootMode = ShootMode.SemiGun;
                shootModeName = "���Զ�ģʽ";
                UpdateAmmoUI();
            }
            else
            {
                shootMode = ShootMode.AutoRife;
                shootModeName = "ȫ�Զ�ģʽ";
                UpdateAmmoUI();
            }
        }
    }
}
