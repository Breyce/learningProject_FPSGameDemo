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