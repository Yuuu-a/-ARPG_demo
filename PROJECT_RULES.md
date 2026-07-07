# ARPG Demo 项目规则

# ARPG Demo 项目上下文

## 项目目标

这是一个 Unity 2022.3 的第三人称 ARPG Demo。

当前项目目标是实现一个基础 ARPG 战斗角色，包含：

1. 第三人称移动
2. 摄像机跟随
3. Dash / 闪避
4. 普攻四段连招
5. 武器拔刀 / 收刀
6. Animation Rigging 控制手部贴合刀柄
7. 基础战斗命中检测
8. 后续可扩展敌人 AI、受击、伤害、UI、技能等模块

---

## 当前已实现内容

### 1. 第三人称移动

已基于 Unity Starter Assets Third Person Controller 实现第三人称角色移动。

当前移动逻辑以代码控制为主，普通移动不依赖 Root Motion。

### 2. 摄像机跟随

已实现第三人称摄像机跟随。

摄像机已经可以跟随角色，并用于角色移动方向计算。

### 3. Dash / 闪避

已实现 Dash 功能。

当前 Dash 使用 Root Motion Dash 动画，Dash 动画允许角色产生前向位移。

Dash 相关规则：

- Dash 动画可以保留 Root Motion 位移
- Dash 动画的 Root Transform Position XZ 可以不烘焙
- Dash 进入时需要快速响应输入
- Dash 期间通常禁止普通移动输入覆盖 Dash 位移

### 4. Animation Rigging

已使用 Animation Rigging 实现 Idle 状态下手部贴合刀柄。

当前思路：

- 手部 IK Target 设置在刀柄附近
- Idle 时 Rig 权重为 1
- 攻击、挥砍、拔刀等动作中，Rig 权重需要根据动画阶段降低或关闭
- 后续可以通过动画事件、脚本或 Animator 参数控制 Rig 权重

---

## 当前正在实现的内容

### 普攻连招系统

当前准备实现 ARPG 普攻四段连招。

目标效果：

1. 按攻击键进入 Attack01
2. 攻击期间播放攻击动画
3. 在指定连招窗口内再次按攻击，可以缓存下一段攻击
4. Attack01 可以衔接 Attack02
5. Attack02 可以衔接 Attack03
6. Attack03 可以衔接 Attack04
7. 没有继续输入时，攻击结束后回到 Idle / Move
8. 攻击期间根据需要限制普通移动、Dash 或转向

---

## 当前 Animator 初步结构

当前 Animator 中已有：

- Idle Walk Run Blend
- Dash
- Attack-a01
- Attack-aEnd01

end01是收招动画

```text
Idle Walk Run Blend -> Attack-a01 -> Attack-aEnd01 -> Idle Walk Run Blend
```

---

## 当前武器与 IK 控制脚本

已新增 `PlayerWeaponController.cs`，用于角色攻击动画中的武器挂点切换和右手 IK 权重控制。

当前实现规则：

1. 脚本只负责读取 Animator 参数和切换武器父节点，不自动修改 Animator Controller、Prefab、Scene、Animation Clip 或 Animation Event。
2. 脚本挂在 Player 根对象上，引用通过 Inspector 手动拖拽。
3. 武器支持三个挂点：
   - `weaponHipSocket`：臀部 / 刀鞘挂点
   - `weaponLeftHandSocket`：左手挂点
   - `weaponRightHandSocket`：右手挂点
4. 当前只控制右手 IK：
   - Animator Float 参数名：`RightHandIKWeight`
   - 每帧在 `LateUpdate` 中读取该参数
   - 将读取到的值赋给 `rightHandRig.weight`
5. 当前不需要左手 Rig，也不读取 `LeftHandIKWeight`。
6. 动画事件可手动调用：
   - `AttachWeaponToLeftHand()`：武器切到左手
   - `AttachWeaponToRightHand()`：武器切到右手
   - `AttachWeaponToHip()`：武器挂回臀部 / 刀鞘
   - `AttachWeaponToHand()`：兼容旧事件名，当前等价于切到右手
7. 每次切换父节点后，都会重置：
   - `weapon.localPosition = Vector3.zero`
   - `weapon.localRotation = Quaternion.identity`
   - `weapon.localScale = Vector3.one`
