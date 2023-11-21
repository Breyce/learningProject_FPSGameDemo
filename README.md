# FPSGameDemo

## Development Day 1: 2023.11.15

1. 灯光模式一般选择混合模式`Mixed`：

   ​	而三种模式的区别为：（截取自Unity手册）

   - Baked：Unity 在运行时之前预先计算__烘焙 (Baked)__ 光源产生的光照，而不会将这些光源包括在任何运行时光照计算中。
   - Realtime：Unity 在运行时每帧计算并更新__实时 (Realtime)__ 光源的光照。Unity 不会为实时光源进行任何预先计算。
   - Mixed：Unity 为混合 (Mixed) 光源预先执行一些计算，另一些计算则会在运行时执行。

2. `FixedUpdate()`：

   ​	本机玩家循环中的更新阶段。每隔Time.fixedDeltaTime被调用一次。Time.fixedDeltaTime默认是0.02s，可以通过Edit->ProjectSettings->Time来设置。实际情况中，游戏的帧率是在不断变化的，也就是说如果用帧率来更新会导致更新频率不一，如果使用`FixedUpdate`可以实现较为匀速的调用和更新。

   ​	这是本机玩家循环中的更新阶段的 C# 表示形式。它只能用于在本机标识更新阶段。

3. `MonoBehaviour.FixedUpdate`：

   ​	用于物理计算且独立于帧率的 MonoBehaviour.FixedUpdate消息。

   ​	MonoBehaviour.FixedUpdate 具有物理系统的频率；每个固定帧率帧调用该函数。在 FixedUpdate 之后，进行 Physics 系统计算。调用之间的默认时间为 0.02 秒（50 次调用/秒）。

4. 移动等角色操作可以使用`CharacterController`：

   1. `CharacterController.Move(movDirection * Speed * Time.deltaTime)`；

5. 相机操作，例如视角移动等可以使用`Cinemachine`：不过这个需要额外安装新的Unity包。

6. `CollisionFLags`：

   `CollisionFlags` 是 `CharacterController.Move(direction)` 返回的位掩码。其概述您的角色与其他任何对象的碰撞位置。指示碰撞的方向：None、Sides、Above 和 Below。`CharacterController.Move `不使用重力

   - `CollisionFLags.Above`
   - `CollisionFLags.Below`
   - `CollisionFLags.None`
   - `CollisionFLags.Sides`

7. `Physics.OverlapSphere`：用一个球体判断是否和指定范围有交叠。



## Development Day 2: 2023.11.19

​	今天发现了使用一个全局的`AudioManager`的局限性，当不同的枪械需要不同的填装音效时，就会出现非常庞杂的音频列表，并且同时有很多用不到的音效存在在场景中，这是我不希望看到的。因此我选择用内部类来处理这个问题，内部类可以让音效在一个脚本当中也得到相对比较独立的封装，代码很干净，用起来很方便。最主要的，它可以另我去定制每一个绑定上这个脚本的游戏枪械实体的音效等信息。这无疑是非常方便的事情。

1. `Camera.Depth`：多个摄像机的时候，会渲染在一个镜头内。摄像机在摄像机渲染顺序中的深度，其中深度较低的摄像机在深度较高的摄像机之前渲染。

2. 使用两个摄像机，分别渲染场景内不同的内容，就可以在玩家视角内避免让玩家看到穿模问题。

3. 在Unity当中，父物体的缩放参数发生变化时子物体也会发生同样的变化，因此在实际当中，如果想要控制父物体变化而子物体不变，那么就需要对子物体进行反向放大。

   我在代码中一开始使用的是以下代码：

   ```c#
   public class KeepChildScale : MonoBehaviour
   {
       private Vector3 initialScale;
   
       void Start()
       {
           // 保存子物体的初始缩放
           initialScale = transform.localScale;
       }
   
       void Update()
       {
           // 获取父物体的缩放
           Vector3 parentScale = transform.parent.localScale;
   
           // 保持子物体相对缩放不变
           transform.localScale = new Vector3(
               initialScale.x / parentScale.x,
               initialScale.y / parentScale.y,
               initialScale.z / parentScale.z
           );
       }
   }
   ```

   后来发现会在运行时出现卡顿，所以将代码改了一下，融入了一份游戏实体的脚本——即父物体身上的脚本——当中。定义一个`GameObject`类型的变量，在Unity当中拖拽赋值，之后在代码中如此使用（因为这个小项目当中只有`y`方向的缩放发生变化，所以写出来如下）：

   ```C#
   Gun.transform.localScale = new Vector3(
       transform.localScale.x / transform.localScale.x,
       startYScale / transform.localScale.y, //startYScale记录的是枪械模型初始的缩放值。
       transform.localScale.z / transform.localScale.z
   );
   ```

4. `muzzleFlash.Emit(int number)`：Emit函数用于立刻发射`number`个粒子。

5. 如果有内部类希望在GUI当中显示其参数信息等，可以使用`[System.Serializable]`来操作：

   ```	C#
   [System.Serializable]
   public class SoundClips
   {
       public AudioClip shootSound;
       public AudioClip shootSound_Silencer;
       public AudioClip reloadSoundAmmoLeft;
       public AudioClip reloadSoundAmmoRunOut;
   }
   ```

   类似这种音效，可以用以下方式调用（记得要现在脚本绑定的物体上添加AudioSource组件）：

   ```C#
   public class Weapon_AutomaticGun : Weapon
   {
       private AudioSource mainAudioSource;
       public SoundClips GunSound;
       
       private void Start()
       {
           //获取组件
           mainAudioSource = GetComponent<AudioSource>();
       }
       
       public override void GunFire()
       {
           // 播放射击音效
           mainAudioSource.clip = GunSound.shootSound;
           mainAudioSource.Play();
       }
   }
   ```




## Development Day 3 & 4: 2023.11.20 / 21

1. 子弹打到准心，这件事情说难也难，说简单也简单。我想着有这么几个实现办法：将子弹往镜头中心发出的射线碰到的地方（target）飞过去，但是这个就有一个问题，就是在近距离的时候有可能是达不到目标所在位置的，因为飞过去的路径上可能被别的实体挡住。毕竟shootPoint和target之间的连线可能会有别的物体。有可能被挡下。这个方法PASS。我用的方法是把shootPoint和Camera的中心放在一条直线上，或者不在一条直线上，但是不断地调整位置，另其最终实现效果在一个位置上，简单点的方法就是把shootPoint和Camera放在一个父物体下，然后再去编辑其他的位置问题。

2. 可以在动画状态机当中添加脚本来给动画添加额外的代码操作。脚本继承的是`StateMachineBehaviour`类

   `StateMachineBehaviour`是一个可添加到状态机状态的组件。它是一个基类，所有状态脚本都派生自这个类。

   `animator.ResetTrigger(signal)`：可以重置触发信号，避免重复执行同一段动画。

   ```C#
   using System.Collections;
   using System.Collections.Generic;
   using UnityEngine;
   
   public class FSMClearSignals : StateMachineBehaviour
   {
       // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
       //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
       //{
       //    
       //}
   
       // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
       //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
       //{
       //    
       //}
   
       // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
       //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
       //{
       //    
       //}
   
       // OnStateMove is called right after Animator.OnAnimatorMove()
       //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
       //{
       //    // Implement code that processes and affects root motion
       //}
   
       // OnStateIK is called right after Animator.OnAnimatorIK()
       //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
       //{
       //    // Implement code that sets up animation IK (inverse kinematics)
       //}
   }
   
   ```

3. `Mathf.SmoothDamp()`：用于实现平滑移动目标的功能。