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
    public AudioClip reloadShotgunOpen;
    public AudioClip reloadShotgunInsert;
    public AudioClip reloadShotgunClose;
}


public class Weapon_AutomaticGun : Weapon
{
    private GameObject player;
    private PlayerMovement componentPlayer;
    private Animator anim;

    [Header("Amming")]
    private Camera mainCamera;
    public Camera gunCamera;
    private Vector3 sniperFiflePosition; // 枪械初始位置
    public Vector3 sniperFifleOnPosition; // 枪械开启瞄准镜之后的位置

    [Header("Shooting")]
    public Transform ShootPoint;//射线打出的位置
    public Transform BulletShootPoint;//子弹打出的位置
    public Transform CasingBulletPoint;//弹壳抛出的位置
    public Transform bulletPrefab;
    public Transform casingPrefab;

    [Header("Practical System")]
    public ParticleSystem muzzleFlash; // 枪口火焰
    public ParticleSystem muzzleSpark; // 枪口火星
    public Light muzzleFlashLight; // 枪口灯光 
    public float lightDuration; // 灯光持续时间
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;

    [Header("Gun feature")]
    public bool isSilence; // 枪械是否装载消音器
    public float range;
    public float fireRate;
    public int shotgunFragment;
    public int bulletMag; //弹匣子弹数
    public int bulletLeft; //备弹
    private int currentBullet; // 当前所剩子弹
    private float SpreadFactor; //射击的偏移量
    private float originRate; //原始射速
    private float fireTimer; //计时器控制射速
    private float bulletForce; //子弹发射的力
    private bool gunShoot;
    private bool isReload;
    private bool isAmming;

    [Header("Sound Sources")]
    private AudioSource mainAudioSource;
    public SoundClips GunSound;

    [Header("UI")]
    public Image[] crossQuarterImges; //准心
    public Text ammTextUI;
    public Text shootModeTextUI;
    public float currentExpanedDegree; //当前准心开合度
    private float crossExpanedDegree; // 每帧准心的开合度
    private float maxCrossDegree;

    [Header("Key Binds")]
    private KeyCode reloadBulletKey;
    private KeyCode shootModeChange;
    private KeyCode inspectGun;
    
    /* 枚举定义全自动和半自动类型 */
    public enum ShootMode
    {
        AutoRife, SemiGun
    }
    public ShootMode shootMode;
    private string shootModeName;

    public PlayerMovement.MovementState state;

    [Header("Sniper features")]
    public Material scopeRenderMaterial;
    public Color fadeColor;
    public Color defaultColor;


    private void Start()
    {
        //按键绑定
        reloadBulletKey = KeyCode.R;
        shootModeChange = KeyCode.C;
        inspectGun = KeyCode.E;

        //获取组件
        mainAudioSource = GetComponent<AudioSource>();

        player = GameObject.Find("Player");
        componentPlayer = player .GetComponent<PlayerMovement>();

        anim = GetComponent<Animator>();
        mainCamera = Camera.main;

        //初始化参数
        range = 300f;
        maxCrossDegree = 150f;
        crossExpanedDegree = 30f;
        SpreadFactor = 0.1f;// 瞄准时要确保SpreadFactor能够更稳定

        bulletForce = 100f;
        bulletLeft = 5 * bulletMag;
        currentBullet = bulletMag;

        shootMode = ShootMode.AutoRife;
        shootModeName = "全自动模式";

        sniperFiflePosition = transform.localPosition;
        // 渲染UI
        UpdateAmmoUI();
    }

    private void Update()
    {
        //状态机检测
        StateHandler();

        switch (shootMode)
        {
            case ShootMode.AutoRife: // 全自动
                gunShoot = Input.GetMouseButton(0);
                fireRate = 0.1f;
                break;
            case ShootMode.SemiGun: //半自动
                gunShoot = Input.GetMouseButtonDown(0);
                fireRate = 0.2f;
                break;
        }

        if (gunShoot)
        {
            //开枪射击
            GunFire();
        }

        //控制准心
        state = componentPlayer.state;
        if(state == PlayerMovement.MovementState.walking 
            && state != PlayerMovement.MovementState.sprinting 
            && state != PlayerMovement.MovementState.crouching
            && componentPlayer.thRB.velocity.sqrMagnitude > 0.9f )
        {
            //移动时的准心开合度
            ExpandingCrossUpdate(crossExpanedDegree);
        }
        else if(state != PlayerMovement.MovementState.walking
            && state == PlayerMovement.MovementState.sprinting
            && state != PlayerMovement.MovementState.crouching
            && componentPlayer.thRB.velocity.sqrMagnitude > 0.9f)
        {
            //奔跑时的准心开合度
            ExpandingCrossUpdate(crossExpanedDegree * 2);
        }
        else
        {
            //正常站立和下蹲时不调整准心开合度
            ExpandingCrossUpdate(0);
        }

        //实现玩家移动动画
        anim.SetBool("isRun", componentPlayer.isRun);
        anim.SetBool("isWalk", componentPlayer.isWalk);

        //实现查看操作
        if (Input.GetKeyDown(inspectGun))
        {
            anim.SetTrigger("Inspect");
        }

        //实现换弹操作
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        if (
            info.IsName("reloadAmmoLeft") || 
            info.IsName("reloadOutOfAmmo") ||
            info.IsName("reload_open") ||
            info.IsName("reload_insert") ||
            info.IsName("reload_close") 
            )
        {
            isReload = true;
        }
        else
        {
            isReload = false;
        }

        if(info.IsName("reload_insert") && currentBullet == bulletMag)
        {
            anim.Play("reload_close");
            isReload = false;
        }

        if (Input.GetKey(reloadBulletKey) 
            && currentBullet < bulletMag 
            && !isReload 
            && !anim.GetCurrentAnimatorStateInfo(0).IsName("inspectWeapon"))
        {
            DoReloadAnimation();
        }


        //实现瞄准操作：
        //瞄准和射击的精度不同
        SpreadFactor = (isAmming) ? 0.01f : 0.1f;

        if(Input.GetMouseButton(1) && !isReload && !componentPlayer.isRun)
        {
            //瞄准：1. 准星消失； 2. 视野靠前
            isAmming = true;

            //瞄准动画和音效
            anim.SetBool("isAim", isAmming);

            //视野靠前
            transform.localPosition = sniperFifleOnPosition;
        }
        else
        {
            //退出瞄准：1. 准星出现； 2. 视野回复
            isAmming = false;

            //瞄准动画和音效
            anim.SetBool("isAim", isAmming);

            //视野靠前
            transform.localPosition = sniperFiflePosition;
        }

        //控制射速
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

    }

    /* 开枪控制函数
     * GunFire()：开枪函数
     * MuzzleFlashLight()：控制开枪动画
     * AimIn()：瞄准进入逻辑
     * AimOut()：瞄准退出逻辑
     */
    public override void GunFire()
    {
        if (fireTimer < fireRate 
            || componentPlayer.isRun
            || currentBullet <= 0 
            || anim.GetCurrentAnimatorStateInfo(0).IsName("takeOutWapon")
            || anim.GetCurrentAnimatorStateInfo(0).IsName("inspectWeapon")
            || isReload) return;

        // 开启灯光准心携程
        StartCoroutine(MuzzleFlashLight());
        StartCoroutine(Shoot_Cross());

        // 播放射击音效和火光和粒子
        mainAudioSource.clip = isSilence ? GunSound.shootSound_Silencer : GunSound.shootSound; //切换射击音效
        mainAudioSource.Play();
        muzzleFlash.Emit(1);
        muzzleSpark.Emit(Random.Range(minSparkEmission, maxSparkEmission));

        for (int i = 0; i < shotgunFragment; i++)
        {
            RaycastHit hit;
            Vector3 shootDirect = BulletShootPoint.forward;

            shootDirect = shootDirect + BulletShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
            if (Physics.Raycast(BulletShootPoint.position, shootDirect, out hit, range))
            {
                Transform bullet;
                if (gameObject.name == "3")
                {
                    //霰弹枪特殊处理下将子弹限制位置设定到hit.point
                    bullet = Instantiate(bulletPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                }
                else
                {
                    bullet = Instantiate(bulletPrefab, BulletShootPoint.transform.position, BulletShootPoint.transform.rotation);
                }
                
                //给子弹代预制的方向上一个速度
                //bullet.transform.LookAt(hit.point);
                bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + shootDirect) * bulletForce;
                Debug.Log(hit.transform.gameObject.name + "打到了");
            }
        }


        //播放射击动画
        if (!isAmming)
        {
            anim.CrossFadeInFixedTime("fire", 0.1f);
        }
        else
        {
            //播放瞄准状态下的开火动画
            anim.Play("aimFire", 0, 0);
        }

        //实例化弹壳抛出 
        Instantiate(casingPrefab, CasingBulletPoint.transform.position, CasingBulletPoint.transform.rotation);

        currentBullet--;

        //重置计时器
        fireTimer = 0;

        //更新子弹UI
        UpdateAmmoUI();
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
        float currentVelocity = 0.1f;

        //准星消失
        Debug.Log("准星消失");
        for (int i = 0; i < crossQuarterImges.Length; i++)
        {
            crossQuarterImges[i].gameObject.SetActive(false);
        }

        //如果是狙击枪
        if(gameObject.name == "4")
        {
            scopeRenderMaterial.color = defaultColor;
            gunCamera.fieldOfView = 30;
        }

        //改变视野，视野变近
        mainCamera.fieldOfView = Mathf.SmoothDamp(30, 60, ref currentVelocity, 0.3f);

        //播放声音
        AudioManager.instance.PlayNoNeedStopSound(0);
    }

    public override void AimOut()
    {
        float currentVelocity = 0.1f;

        //准星出现
        Debug.Log("准星出现");
        for (int i = 0; i < crossQuarterImges.Length; i++)
        {
            crossQuarterImges[i].gameObject.SetActive(true);
        }

        //如果是狙击枪
        if (gameObject.name == "4")
        {
            scopeRenderMaterial.color = fadeColor;
            gunCamera.fieldOfView = 50;
        }

        //改变视野，视野变远
        mainCamera.fieldOfView = Mathf.SmoothDamp(60, 30, ref currentVelocity, 0.3f);

    }

    /**
     * 换弹控制函数
     * DoReloadAnimation()：播放不同的换弹动画
     * Reload()：填装弹药逻辑，在动画当中调用
     * ShotgunReload(): 霰弹枪添加子弹。
     */
    public override void DoReloadAnimation()
    {
        if(gameObject.name != "3" && gameObject.name != "4")
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
        else
        {
            if (currentBullet == bulletMag) return;
            anim.SetTrigger("shotgun_reload");
        }
    }

    public override void Reload()
    {
        if (bulletLeft <= 0) return;

        //计算备弹减少量
        int bulletChange = bulletMag - currentBullet;

        if (bulletLeft > bulletChange)
        {
            //更新备弹数量
            bulletLeft -= bulletChange;

            //填充当前子弹
            currentBullet = bulletMag;
        }
        else if (bulletLeft <= bulletChange)
        {
            //更新当前子弹数量
            currentBullet += bulletLeft;

            //更新备弹数量
            bulletLeft = 0;
        }

        UpdateAmmoUI();
    }

    public void ShotgunReload()
    {
        if((currentBullet < bulletMag) && bulletLeft > 0)
        {
            currentBullet++;
            bulletLeft--;
            UpdateAmmoUI();
            anim.Play("reload_insert");
        }
        else
        {
            anim.Play("reload_close");
            return;
        }

        if(bulletLeft <= 0) return;
    }

    public void SniperReload()
    {

    }
    /*
     * UI控制函数
     */
    public void UpdateAmmoUI()
    {
        ammTextUI.text = "BULLET: " + currentBullet + " / " + bulletLeft;
        shootModeTextUI.text = shootModeName;
    }

    /*
     * 准心控制函数
     * ExpandingCrossUpdate(float expanDegree)：用于调整准心
     * ExpendCross(float add)：用于控制准心开合
     * Shoot_Cross()：携程，调用准心开合度，一帧执行5次，只负责射击时瞬间增大准心
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

        //保存当前准心开合度
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
     * 全自动半自动切换状态机
     * StateHandler():处理射击模式状态
     */
    public void StateHandler()
    {
        if (Input.GetKeyDown(shootModeChange))
        {
            if (shootMode == ShootMode.AutoRife)
            {
                shootMode = ShootMode.SemiGun;
                shootModeName = "半自动模式";
                UpdateAmmoUI();
            }
            else
            {
                shootMode = ShootMode.AutoRife;
                shootModeName = "全自动模式";
                UpdateAmmoUI();
            }
        }
    }
}
