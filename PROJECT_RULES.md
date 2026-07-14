# ARPG Demo 项目规则与当前进展

## 架构与职责规则

- 每个脚本应尽量只承担一项明确职责，不要把输入、UI、数据存储和业务逻辑直接写在同一个脚本中。
- 输入层只负责接收和转发输入；UI 层只负责界面展示和用户交互；数据层只负责数据结构、查询和持久化；业务层负责游戏规则和状态变更。
- 跨系统通信优先使用事件、接口或管理器公开方法，不要直接访问其他类的内部字段或可变容器。
- 需要暴露状态时，优先提供只读属性、查询方法或有边界的操作方法，避免外部任意修改内部数据。
- 修改代码前，先说明涉及脚本的职责、依赖关系和跨系统通信方式，再实施修改。

## 项目定位

这是一个基于 Unity 2022.3 的第三人称 ARPG Demo。当前目标是先做出一个可操作、可连招、可命中怪物、可播放受击和死亡反馈的基础战斗角色，再逐步接入敌人 AI、移动寻路、攻击、技能、UI 等模块。

项目目前以 Unity Starter Assets Third Person Controller 为底座，角色素材主要使用 `anbi` 相关模型、贴图和动画资源。

## 当前已完成

### 第三人称基础

- 已接入 Starter Assets 的第三人称移动与相机跟随。
- 普通移动主要由代码控制，不依赖 Root Motion。
- 摄像机方向已用于角色移动、Dash 和攻击朝向计算。
- Jump 目前在 `StarterAssetsInputs` 中被有意禁用。

### 输入层

- `StarterAssetsInputs.cs` 已扩展 `Dash` 和 `Attack` 输入状态。
- Input System 里已有 `Dash` 与 `Attack` action。
- Dash / Attack 脚本都支持从 `StarterAssetsInputs` 读取输入，也兼容 `PlayerInput.actions` 查询。

### Dash / 闪避

- `PlayerDash.cs` 负责 Dash 玩法逻辑。
- Dash 有冷却时间 `dashCooldown`。
- Dash 开始时会根据移动输入和摄像机方向决定闪避方向；没有移动输入时使用角色当前 forward。
- Dash 开始时会把角色朝向转到 Dash 方向。
- Dash Trigger 通过 `ConsumeDashTrigger()` 消费，避免 Animator 每帧重复触发。
- Dash 结束由动画事件调用 `OnDashAnimationEnd()`。
- Dash 期间不允许再次 Dash。
- 攻击期间不允许 Dash，并会清掉当前 Dash 输入。

### 普攻 / 四段连招

- `PlayerAttack.cs` 已实现基础四段普攻状态机。
- 最大连招段数为 4。
- 非攻击状态下按 Attack 会进入第一段攻击。
- 攻击开始时会根据移动输入和摄像机方向修正角色朝向。
- 攻击期间如果处于连招窗口，并且当前段数小于 4，再按 Attack 会进入下一段。
- 连招窗口由动画事件调用：
  - `OpenComboWindow()`
  - `CloseComboWindow()`
- 攻击结束由动画事件调用 `OnAttackAnimationEnd()`。
- `attackLockTimeout` 是兜底保护：如果动画事件没有正确结束攻击，会在超时后自动退出攻击状态。
- 攻击期间不允许 Dash；Dash 期间不允许开始攻击。
- 攻击收招阶段支持移动取消：
  - Animator 需要有 Bool 参数 `MoveCancel`。
  - 收招状态需要配置到 `Idle Walk Run Blend` 的过渡，条件为 `MoveCancel == true`。
  - `PlayerAttack` 会在攻击阶段持续检测移动输入并缓存取消意图。
  - 只有 Animator 当前状态真正进入收招状态时，才会设置 `MoveCancel = true` 并解除攻击锁。
  - 起手和挥砍阶段不会被移动打断。
- 当前识别为收招的 Animator State 名称：
  - `Attcak-aEnd01`（注意当前 Animator 中第一段收招拼写为 Attcak）
  - `Attack-aEnd02`
  - `Attack-aEnd03`
  - `Attack-aEnd04`

### Animator 参数同步

- `PlayerAnimationController.cs` 作为动画参数统一同步脚本。
- 它同步 Starter Assets 原有移动参数：
  - `Speed`
  - `MotionSpeed`
  - `Grounded`
- 它同步 Dash 参数：
  - `Dash`
  - `IsDashing`
- 它同步攻击参数：
  - `Attack`
  - `IsAttacking`
  - `ComboIndex`
  - `CanCombo`
- 攻击收招移动取消使用 Animator Bool 参数：
  - `MoveCancel`
- 注意：目前 `PlayerAttack` 已经直接写入 Attack Trigger，`ConsumeAttackTrigger()` 保留主要是为了兼容 `PlayerAnimationController` 现有调用。

### 武器挂点与右手 IK

- `PlayerWeaponController.cs` 用于动画过程中切换武器父节点、控制右手 IK 权重、转发武器命中窗口事件。
- 支持三个武器挂点：
  - `weaponHipSocket`
  - `weaponLeftHandSocket`
  - `weaponRightHandSocket`
- 支持武器挂点动画事件：
  - `AttachWeaponToLeftHand()`
  - `AttachWeaponToRightHand()`
  - `AttachWeaponToHip()`
  - `AttachWeaponToHand()`，旧事件名保留，目前等价于右手持刀。
- 每次切换挂点后都会重置武器本地 transform：
  - `localPosition = Vector3.zero`
  - `localRotation = Quaternion.identity`
  - `localScale = Vector3.one`
- 当前只控制右手 Rig：
  - Animator Float 参数：`RightHandIKWeight`
  - 每帧在 `LateUpdate` 中读取并写入 `rightHandRig.weight`
- 当前不需要左手 Rig，也不读取 `LeftHandIKWeight`。

### 武器命中检测

- 已采用“武器下方挂载碰撞盒”的方案。
- `WeaponHitbox.cs` 挂在武器子物体上，使用 Trigger Collider 做检测，不产生实际物理阻挡。
- `WeaponHitbox` 要求同物体存在：
  - `Collider`，`Is Trigger = true`
  - `Rigidbody`，`Is Kinematic = true`，`Use Gravity = false`
- `WeaponHitbox.targetLayers` 用来指定可命中的目标层，目前建议只勾选 `Enemy`。
- 武器 Hitbox 自身 Layer 建议使用 `PlayerWeapon` 或 `PlayerHitbox`，不要设置成 `Enemy`。
- 怪物实际 Collider 所在 GameObject 的 Layer 需要是 `Enemy`。
- `Project Settings > Physics > Layer Collision Matrix` 中需要确保 `PlayerWeapon` 和 `Enemy` 允许触发检测。
- 每个命中窗口内，同一个 `EnemyDamageReceiver` 只会被命中一次，避免同一刀多帧重复扣血。
- 下一次 `BeginWeaponHit()` 会清空已命中列表，允许新一段攻击再次命中同一个怪物。
- 如果只调用 `BeginWeaponHit()` 而忘记调用 `EndWeaponHit()`，命中盒会一直保持开启，新碰到的怪物仍可能被命中。

### 怪物血量 / 受击 / 死亡

- 已新增怪物基础脚本：
  - `EnemyHealth.cs`
  - `EnemyDamageReceiver.cs`
  - `EnemyHitReaction.cs`
  - `HitInfo.cs`
- 当前已经验证可以实现：
  - 武器命中怪物
  - 怪物掉血
  - 怪物受击动画
  - 怪物死亡动画
- `EnemyHealth` 只负责血量、生死状态和事件派发。
- `EnemyDamageReceiver` 是怪物对外受击入口，武器命中后调用它。
- `EnemyHitReaction` 监听 `EnemyHealth.OnDamaged` 和 `EnemyHealth.OnDied`，负责写 Animator 参数。
- 怪物死亡参数当前使用：
  - Trigger：`Dead`
- 怪物受击参数当前使用：
  - Trigger：`Hit`
- `EnemyHitReaction.HandleDied()` 当前使用：
  - `animator.ResetTrigger(DeadHash)`
  - `animator.SetTrigger(DeadHash)`

### 怪物 AI / NavMesh 寻路

- 已新增 `EnemyAI.cs`。
- 怪物需要挂 `NavMeshAgent`，地图需要通过 NavMesh Surface 烘焙可行走区域。
- `EnemyAI` 会自动查找 Tag 为 `Player` 的目标，也可以在 Inspector 手动指定 `target`。
- 玩家进入 `chaseRange` 后，怪物开始追击。
- 玩家离开 `loseTargetRange` 后，怪物停止追击并保持 Idle。
- 玩家进入 `attackRange` 后，怪物停止移动、面向玩家，并按 `attackCooldown` 触发攻击。
- 怪物死亡时，`EnemyAI` 会停止移动、把 `Speed` 写为 0，并禁用自身和 `NavMeshAgent`。
- 怪物 Animator 参数当前使用：
  - Trigger：`Attack`
  - Trigger：`Hit`
  - Trigger：`Dead`
  - Float：`Speed`
- `EnemyAI` 只负责追击、攻击触发和 `Speed` 参数；受击、死亡 Trigger 仍由 `EnemyHitReaction` 负责。

### 背包 / 物品数据

- 已新增基础背包、物品配置、物品详情展示和本地存档功能。
- `ItemDataManager` 是场景中的物品配置入口，在 `Awake()` 中读取 `Resources/Config/WeaponConfig.Json`，并按物品 `id` 建立查询字典。
- `PackageItem` 当前包含：
  - `id`
  - `type`
  - `name`
  - `description`
  - `imagePath`
- 当前物品类型：`Weapon`、`Food`、`Prop`；只有 `Food` 和 `Prop` 可以堆叠，`Weapon` 每件独占一个格子。
- `ItemVisualDatabaseSO` / `ItemVisualSO` 用于按物品 ID 配置图标和模型 Prefab；图标未配置时，回退到 `PackageItem.imagePath` 对应的 `Resources` 路径。
- 物品配置 ID 必须大于 0 且保持唯一；视觉配置中的 `itemId` 必须与 JSON 配置一致。
- `InventoryManager` 是静态背包数据入口，提供：
  - `Load()` / `Save()`
  - `AddItem()` / `RemoveItem()`
  - `GetItemCount()` / `GetSlot()`
  - `Clear()`
- 背包存档使用 `JsonUtility` 写入 `Application.persistentDataPath/InventorySave.json`。
- 背包修改后会触发 `InventoryChanged`，但不会自动保存；需要在合适的业务节点显式调用 `InventoryManager.Save()`。
- `PackageUI` 监听 `InventoryChanged` 并动态刷新格子，按 `itemId`、`slotIndex` 排序显示；选中格子后显示图标、名称和描述。
- 背包格子 Prefab 固定从 `Resources/Prefabs/UIPanel/PackageItem` 加载。
- 当前可在 Play Mode 使用 `GMcmd/背包` 菜单添加 10 个随机武器或清空背包，用于测试配置、显示和存档链路。

### 角色基础数据 / 存档

- `PlayerSaveData` 只保存角色 ID、等级、当前经验和当前生命，不重复保存基础属性、成长值或最终属性。
- `PlayerSaveManager` 使用 `Application.persistentDataPath/PlayerSave.json` 保存角色数据，默认在游戏启动时读取、退出时保存，也提供公开的 `Save()` / `Load()` 方法。
- 加载时由 `CharacterConfigRegistry` 在 `Resources/Config` 下按角色 ID 查询 `CharacterBaseConfig`。
- 恢复顺序为：恢复角色配置、清空运行时装备/Buff 加成、通过 `PlayerProgression.RestoreProgress()` 恢复等级与经验、通过 `PlayerStats.RestoreHealth()` 恢复当前生命。
- 存档已预留 `equippedItems`，只记录装备槽位与物品实例 ID；装备属性仍应由未来的装备系统读取配置后统一提交给 `PlayerStats.SetEquipmentModifiers()`。
- `GameScene` 的玩家对象已挂载 `PlayerSaveManager`，并绑定同对象上的 `PlayerStats` 与 `PlayerProgression`。

### NPC 交互 / 对话

- 已新增统一交互接口 `IInteractable`，交互对象需要实现：
  - `Interact()`
  - `GetInteractText()`
  - `GetTransform()`
- `PlayerInteract` 每帧响应旧输入系统的 `E` 键，通过半径 2 米的 `Physics.OverlapSphere` 查找交互对象；范围内有多个对象时选择距离最近的对象。
- `PlayerInteractUI` 直接显示最近交互对象返回的提示文本，不通过 `UIManager` 动态创建。
- `NpcInteract` 实现 `IInteractable`，每个 NPC 需要配置 `dialogueFile` 和 `interactText`。
- NPC 交互后通过 `UIManager` 打开 `DialoguePanel`，并把 `TextAsset` 传给 `StartDialogue()`。
- 对话 TXT 按换行拆分，行首尾空白和空行会被忽略；当前每个有效行代表一句对话。
- 对话开始后显示第一句，后续使用旧输入系统鼠标左键逐句推进；最后一句之后自动关闭面板。
- `DialoguePanel` 会记录打开帧，避免触发交互或打开面板的同一次点击直接跳过第一句。
- NPC 没有配置 TXT、TXT 没有有效内容或对话文本引用未绑定时，需要保留明确的 Console 警告。

### UI 面板管理

- `UIManager` 当前管理 `PackagePanel`、`CharacterPanel`、`AbilityPanel`、`JingYanPanel`、`EquipmentPanel` 和 `DialoguePanel`，对应 Prefab 位于 `Resources/Prefabs/UIPanel/`。
- C 键打开 `CharacterPanel`；其中 `AbilityButton` 与 `SelectedButton` 进入 `AbilityPanel`，`EquipmentButton` 进入 `EquipmentPanel`。
- C 键可关闭当前打开的角色相关面板；B 键背包与角色相关面板互斥，切换时会先关闭另一组面板。
- 面板 Prefab 根节点必须挂载 `BasePanel` 或其子类，同名面板不允许重复打开。
- `BasePanel` 打开时进入 UI 模式：解锁并显示鼠标、清空玩家移动/视角/冲刺/Dash/Attack 输入，并停用玩家 `PlayerInput`。
- `BasePanel` 关闭时恢复玩家输入、重新锁定鼠标，并从 `UIManager.panelDic` 移除面板。
- Input System 中的 `Package` 与 `Character` action 会在 UI 模式下单独保持可用，以支持 B/C 键关闭对应面板；新增面板热键时需要同步检查这套输入启停逻辑。

## 当前 Animator 结构

基础移动：

```text
Idle Walk Run Blend
```

Dash：

```text
Idle Walk Run Blend -> Dash -> Idle Walk Run Blend
```

普攻连招目标：

```text
Idle Walk Run Blend
  -> Attack01
  -> Attack02
  -> Attack03
  -> Attack04
  -> Idle Walk Run Blend
```

如果使用收招动画，可以保持：

```text
Attack01 -> Attack-aEnd01 -> Idle Walk Run Blend
```

收招移动取消建议结构：

```text
Attcak-aEnd01 -> Idle Walk Run Blend
Attack-aEnd02 -> Idle Walk Run Blend
Attack-aEnd03 -> Idle Walk Run Blend
Attack-aEnd04 -> Idle Walk Run Blend

Condition: MoveCancel == true
Has Exit Time: Off
Transition Duration: 0.05 ~ 0.1
```

怪物 Animator 当前建议：

```text
Any State -> Hit
Any State -> Death
```

受击过渡：

```text
Condition: Hit Trigger
Has Exit Time: Off
Transition Duration: 0 或很短
```

死亡过渡：

```text
Condition: Dead Trigger
Has Exit Time: Off
Transition Duration: 0 或很短
```

死亡状态一般不再返回 Idle。

## Unity 场景 / Prefab 需要挂载

### Player / Animator 所在对象

播放攻击动画的 Animator 所在 GameObject 上需要挂：

- `PlayerAttack`
- `PlayerDash`
- `PlayerAnimationController`
- `PlayerWeaponController`

重要规则：

- Unity Animation Event 只会调用播放该动画的 Animator 所在 GameObject 上的脚本方法。
- 所以 `BeginWeaponHit()`、`EndWeaponHit()`、`OpenComboWindow()`、`CloseComboWindow()`、`OnAttackAnimationEnd()` 所在脚本必须挂在 Animator 同一个 GameObject 上。

`PlayerWeaponController` 需要引用：

- `animator`
- `weapon`
- `weaponHitbox`
- `weaponHipSocket`
- `weaponLeftHandSocket`
- `weaponRightHandSocket`
- `rightHandRig`

### 武器子物体

建议结构：

```text
Weapon
└─ WeaponHitbox
```

`WeaponHitbox` 子物体上挂：

- `BoxCollider` 或 `CapsuleCollider`
  - `Is Trigger = true`
- `Rigidbody`
  - `Is Kinematic = true`
  - `Use Gravity = false`
- `WeaponHitbox.cs`

`WeaponHitbox` 组件设置：

- `playerAttack`：拖玩家上的 `PlayerAttack`
- `attacker`：拖玩家根对象
- `targetLayers`：勾选 `Enemy`
- `comboDamages`：四段攻击伤害，例如 `10, 12, 15, 20`

### 怪物对象

怪物上当前需要挂：

- `EnemyHealth`
- `EnemyDamageReceiver`
- `EnemyHitReaction`
- `EnemyAI`
- `NavMeshAgent`
- `Collider`
- `Animator`

怪物 Collider 所在对象 Layer 建议为：

```text
Enemy
```

怪物 Animator 参数：

- Trigger：`Attack`
- Trigger：`Hit`
- Trigger：`Dead`
- Float：`Speed`

### UI / 背包对象

- 场景中需要存在 `Canvas` 和一个可用的 `UIManager`；建议显式绑定 `uiRoot`，未绑定时会自动查找 Canvas。
- 场景中需要存在一个 `ItemDataManager`，并为其绑定 `ItemVisualDatabaseSO`（允许暂时为空，但模型查询和视觉数据库图标回退将不可用）。
- `PackagePanel.prefab` 根节点挂 `PackageUI`，并保留脚本当前查找的节点名称：
  - `Content`
  - `Weapon`
  - `Weapon (1)`
  - `WeaponName`
  - `Description`
  - `Button`
- `PackageItem.prefab` 使用 `PackageSlotUI`，并保留：`weapon`、`Level`、`Selected`、`UnSelected` 节点。
- `DialoguePanel.prefab` 根节点挂 `DialoguePanel`，并绑定 `dialogueText`。

### 玩家 / NPC 交互对象

- 玩家需要挂 `PlayerInteract`；场景提示 UI 需要挂 `PlayerInteractUI`，并绑定玩家、提示容器和提示文字。
- NPC 需要挂 `NpcInteract` 和可被 `Physics.OverlapSphere` 检测到的 Collider。
- NPC 的 `dialogueFile` 使用 TXT `TextAsset`，`interactText` 用于玩家靠近时的交互提示。

## 动画事件要求

### Dash 动画

Dash 动画末尾调用：

```text
OnDashAnimationEnd
```

### 每段攻击动画

每段攻击动画都需要按动画节奏放置命中窗口事件：

```text
刀开始有伤害：BeginWeaponHit
刀结束有伤害：EndWeaponHit
```

连招窗口事件：

```text
允许输入下一段：OpenComboWindow
关闭连招输入：CloseComboWindow
```

攻击状态结束事件：

```text
OnAttackAnimationEnd
```

一个常见顺序：

```text
Attack01
  BeginWeaponHit
  OpenComboWindow
  CloseComboWindow
  EndWeaponHit
  OnAttackAnimationEnd
```

也可以根据动画手感调整顺序，但每个 `BeginWeaponHit` 必须配一个 `EndWeaponHit`。

### 武器挂点事件

拔刀、持刀、收刀时按动画节奏调用：

- `AttachWeaponToRightHand()`
- `AttachWeaponToLeftHand()`
- `AttachWeaponToHip()`

## 当前调试开关

为了排查命中链路，当前脚本里有调试开关：

- `PlayerWeaponController.logWeaponHitWindows`
  - 检查动画事件是否调用到 `BeginWeaponHit` / `EndWeaponHit`
- `WeaponHitbox.logHits`
  - 检查武器是否实际命中怪物
- `EnemyHealth.logDamage`
  - 检查怪物是否掉血
- `EnemyHitReaction.logHits`
  - 检查怪物是否收到受击事件并播放 Hit Trigger

排查顺序：

1. 先看是否打印 `BeginWeaponHit`。
2. 再看是否打印 `WeaponHitbox hit xxx`。
3. 再看是否打印怪物掉血。
4. 最后看 Animator 是否响应 `Hit` / `Dead` Trigger。

## 当前注意事项

- Dash 使用 Root Motion 动画位移是允许的；普通移动仍由代码驱动。
- Dash 动画的 Root Transform Position XZ 可以不烘焙，以保留实际位移。
- 攻击逻辑目前是脚本直接触发 Animator 的 `Attack` Trigger，`PlayerAnimationController` 中的攻击 Trigger 消费路径暂时不会真正触发。
- 如果后续希望所有 Animator 写入都统一到 `PlayerAnimationController`，需要调整 `PlayerAttack.ConsumeAttackTrigger()` 的返回逻辑，并移除 `PlayerAttack` 里直接 `SetTrigger` 的写法。
- `PlayerAttack` 当前在连招窗口内会立刻进入下一段并触发 Animator；Animator 过渡条件需要和 `ComboIndex` / `Attack` / `CanCombo` 保持一致。
- `MoveCancel` 是收招移动取消参数；它只应该用于收招状态到移动状态的过渡，不建议从攻击主体段直接使用。
- `PlayerAttack` 的移动取消逻辑只在当前 Animator State 已经进入收招状态时触发，避免出招阶段持续移动输入过早解锁攻击状态。
- 武器 Hitbox 是检测器，不应该用于真实物理阻挡。
- 怪物当前已经接入基础追击和攻击触发 AI，但尚未实现真正的怪物伤害判定、攻击命中窗口、巡逻和复杂行为树。
- 背包的增删操作只修改内存并发送刷新事件，不会自动落盘；退出游戏或切换流程前必须显式保存。
- `InventoryManager.AddItem()` 在 `ItemDataManager.Instance` 不存在或查不到配置时，会把物品按不可堆叠处理；正常游戏流程应保证物品数据管理器先初始化。
- `PackageUI` 和 `PackageSlotUI` 当前依赖固定的子节点名称，调整 UI Prefab 层级或命名后必须同步更新脚本。
- 对话推进与玩家交互当前仍使用旧输入 API（`Input.GetMouseButtonDown` / `Input.GetKeyDown`），而移动和战斗主要使用 Input System；后续应统一输入方案。
- `BasePanel` 按单面板场景管理玩家输入；如果未来允许背包、对话或其他模态面板同时存在，需要改成面板栈或引用计数，避免关闭一个面板时过早恢复玩家操作。
- Git 状态暂时未检查成功，因为当前仓库触发过 Git dubious ownership 保护；如需查看状态，需要把该路径加入 `safe.directory`。

## 建议下一步

1. 给 `PlayerAttack.EndAttack()` 增加强制关闭武器 Hitbox 的兜底，避免漏配 `EndWeaponHit` 导致判定残留。
2. 给死亡后的怪物关闭 Collider 或禁用 `EnemyDamageReceiver`，避免尸体继续被命中。
3. 给怪物攻击增加命中检测、伤害窗口和对玩家造成伤害的逻辑。
4. 统一背包、交互和对话的输入到 Input System，并补充 UI 模式下的输入冲突处理。
5. 为背包增加容量限制、格子交换/排序和明确的自动保存时机。
6. 为对话增加说话者名称、头像、打字机效果和分支/任务事件接口。
7. 增加简单受击硬直、击退、伤害飘字或血条 UI。
8. 基础追击稳定后，再接入巡逻、攻击前摇/后摇、仇恨范围和更复杂 AI。
