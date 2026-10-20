---
schema_version: 1
report_status: USER_REVIEWED
report_reviewed_on: 2026-10-20
report_approval_ref: A006
branch: codex/realistic-clouds
base_ref: master
base_commit: d6b1545b527bd8165b8a6c1e8b4817ec695828bc
reviewed_commit: ab989dd380733573dafc7f6a993e80c36f26b735
target_ref: origin/master
target_observed_commit: d6b1545b527bd8165b8a6c1e8b4817ec695828bc
remote_branch_observed_commit: ab989dd380733573dafc7f6a993e80c36f26b735
checked_on: 2026-10-20
timezone: Asia/Shanghai
comparison: base_commit..reviewed_commit
scope: COMMITTED_NET_DIFF
changed_file_count: 20
insertions: 856
deletions: 76
post_base_commit_count: 4
post_base_merge_commit_count: 0
upstream_sync_status: NO_POST_BASE_MAINLINE_UPDATE_OBSERVED
runtime_validation_this_review: NOT_RUN
merge_decision: NOT_AUTHORIZED_BY_THIS_REPORT
---

# 云开发分支变更报告

本报告内容已经用户审阅，见 [A006](#a006)。统计覆盖固定起点到 `reviewed_commit` 的 20 个已提交变更文件，不包含报告自身的后续提交；仅修改本报告的提交归为 `REPORT_ONLY`，可以保留该代码核对点。本地未提交改动见 [本地排除项](#local-only)。`checked_on` 和远端观察值记录核对时的状态，不表示主分支或报告提交后的分支指针永久不变。

导航：[字段约定](#schema) · [模块索引](#modules) · [完整文件索引](#files) · [额外授权索引](#permissions) · [引入依赖](#integration) · [变更条目](#changes) · [提交索引](#commits) · [授权证据](#approvals) · [验证证据](#validation) · [协作检查](#collaboration) · [本地排除项](#local-only) · [待决定事项](#decisions)

读取顺序：先核对头部提交，再按模块或文件定位 `Cxxx`；准备引入时同时读取依赖组、授权和验证记录。报告的候选引入分类不替代用户批准或合并后的验证。

<a id="schema"></a>
## 字段约定

| 字段 | 允许值及含义 |
| --- | --- |
| `report_status` / `report_approval_ref` | `USER_REVIEWED` 表示用户已审阅报告内容，对应记录为 A006；不代表 C004 的全局策略获批，也不代表分支合并或运行验收通过。 |
| `impact_type` | `MODULE_LOCAL` 模块内部；`SHARED_INTEGRATION` 共用接线；`PROJECT_GLOBAL` 项目级运行行为；`DOCUMENTATION` 文档与来源记录；`DEVELOPMENT_WORKFLOW` 开发工具的项目约定。 |
| `requires_extra_approval` | `true` 表示按当前本地约定需要额外明确批准；`false` 表示本条位于已授权任务范围内。不能由此判断验证是否通过。 |
| `approval_status` | `IN_SCOPE` 已有任务范围覆盖；`EXPLICITLY_APPROVED` 找到对应明确要求；`UNCONFIRMED` 额外授权证据尚未确认，不等同于断言从未批准。 |
| `integration_mode` | `INDEPENDENT` 可独立审阅／选择引入的候选；`WITH_DEPENDENCIES` 应按指定依赖组引入；`REVIEW_REQUIRED` 需要单独决定。均不表示已批准合并。 |
| `validation_status` | `NOT_RUN` 本次未验证；`STATIC_CHECKED` 本次核对源码、差异或 Git 行为；`RUNTIME_CHECKED` 对明确版本实际运行验证。本报告各项为 `STATIC_CHECKED`，历史运行记录另列。 |
| `origin` | `THIS_BRANCH` 本开发分支引入；`UPSTREAM_SYNC` 主分支同步；`LOCAL_ONLY` 工作区独有。当前已提交条目均为 `THIS_BRANCH`。 |
| `approval_refs` / `validation_refs` | 指向本报告的 `Axxx` / `Vxxx` / `Hxxx` 证据编号；缺失的原始证据不得用助手结论或提交说明替代。 |
| `depends_on` / `integration_group` | 条目依赖与配套引入组；组内文件不按单文件复制作为默认接入方式。 |

补充规则：路径相对仓库根目录；文内源码链接使用 `../../` 回到根目录。稳定定位采用函数名、配置键、资源属性或节点路径。完整文件索引中的 `A/M` 分别表示新增／修改，增删行数来自固定比较范围。

<a id="modules"></a>
## 模块索引

| 模块 | 条目 | 入口 |
| --- | --- | --- |
| 云场、体积渲染与噪声 | [C001](#c001) | `cloud_band_density()`、`march_cloud_band()`、云材质与三份噪声资源 |
| 布局初始化与共享缓存 | [C002](#c002) | `CloudWeather._enter_tree()`、`_create_weather_map()`、`_create_sunlight_map()` |
| 地表／单位云影与场景接线 | [C003](#c003) | `low_poly.fragment()`、`Camera3D/CloudLayer`、`CloudWeather` 节点 |
| 项目帧率策略 | [C004](#c004) | `application/run/max_fps` |
| 参数说明与来源 | [C005](#c005) | README 顶部“写实云更新”；四个开源参考的固定版本记录 |
| 本地规则文件的 Git 忽略 | [C006](#c006) | `.gitignore` 的 `/AGENTS.md` |

<a id="files"></a>
## 完整文件索引

| 状态 | 文件 | 新增／删除 | 条目 | 稳定定位点 |
| --- | --- | --- | --- | --- |
| M | [.gitignore](../../.gitignore) | 3 / 0 | [C006](#c006) | `/AGENTS.md` |
| M | [project.godot](../../project.godot) | 1 / 0 | [C004](#c004) | `[application] run/max_fps` |
| A | [shaders/CLOUDS_LICENSES.md](../../shaders/CLOUDS_LICENSES.md) | 115 / 0 | [C005](#c005) | 四个来源与参考版本 |
| A | [shaders/CloudWeather.gd](../../shaders/CloudWeather.gd) | 96 / 0 | [C002](#c002) | `_enter_tree()`、`_ready()`、缓存创建与绑定函数 |
| A | [shaders/CloudWeather.gd.uid](../../shaders/CloudWeather.gd.uid) | 1 / 0 | [C002](#c002) | Godot 脚本 UID |
| M | [shaders/README.md](../../shaders/README.md) | 84 / 4 | [C005](#c005) | 顶部写实云说明、当前参数表、缓存限制 |
| M | [shaders/StylePreview.tscn](../../shaders/StylePreview.tscn) | 21 / 10 | [C003](#c003) | `Camera3D/CloudLayer`、`CloudWeather`、内联接收材质 |
| A | [shaders/cloud_base_noise.tres](../../shaders/cloud_base_noise.tres) | 14 / 0 | [C001](#c001) | `NoiseTexture3D`、`FastNoiseLite_base` |
| M | [shaders/cloud_field.gdshaderinc](../../shaders/cloud_field.gdshaderinc) | 258 / 33 | [C001](#c001) | `cloud_band_density()`、`cloud_shadow()`、共享 uniform |
| M | [shaders/cloud_layer.gdshader](../../shaders/cloud_layer.gdshader) | 183 / 11 | [C001](#c001) | `march_cloud_band()`、`fragment()`、`camera_plane_distance` |
| M | [shaders/cloud_layer.tres](../../shaders/cloud_layer.tres) | 8 / 2 | [C001](#c001) | 两份 3D 噪声绑定、`render_priority` |
| M | [shaders/cloud_noise.tres](../../shaders/cloud_noise.tres) | 2 / 2 | [C001](#c001) | `frequency`、`fractal_octaves` |
| A | [shaders/cloud_sunlight.gdshader](../../shaders/cloud_sunlight.gdshader) | 16 / 0 | [C002](#c002) | `fragment()`、太阳透射 RGB 通道 |
| A | [shaders/cloud_sunlight.gdshader.uid](../../shaders/cloud_sunlight.gdshader.uid) | 1 / 0 | [C002](#c002) | Godot 着色器 UID |
| A | [shaders/cloud_volume_noise.tres](../../shaders/cloud_volume_noise.tres) | 17 / 0 | [C001](#c001) | `NoiseTexture3D`、`FastNoiseLite_volume` |
| A | [shaders/cloud_weather.gdshader](../../shaders/cloud_weather.gdshader) | 15 / 0 | [C002](#c002) | `fragment()`、两层云量 RG/BA 通道 |
| A | [shaders/cloud_weather.gdshader.uid](../../shaders/cloud_weather.gdshader.uid) | 1 / 0 | [C002](#c002) | Godot 着色器 UID |
| M | [shaders/low_poly.gdshader](../../shaders/low_poly.gdshader) | 3 / 7 | [C003](#c003) | `fragment()` 的 `cloud_transmission` |
| M | [shaders/low_poly.tres](../../shaders/low_poly.tres) | 7 / 2 | [C003](#c003) | 云场需要的两份 3D 噪声绑定 |
| M | [world/world.tscn](../../world/world.tscn) | 10 / 5 | [C003](#c003) | `Camera3D/CloudLayer`、根节点下的 `CloudWeather` |

合计：20 个文件，856 行新增、76 行删除。当前报告自身及本地排除项不计入上述统计。

<a id="permissions"></a>
## 额外授权索引

| 分类 | 条目 | 状态依据 |
| --- | --- | --- |
| 需要额外授权，独立证据待确认 | [C004](#c004) | `PROJECT_GLOBAL`；`UNCONFIRMED`；[A003](#a003) 只确认后续提交／推送要求。 |
| 任务范围内的云实现 | [C001](#c001)、[C002](#c002) | [A001](#a001) 的云与云影目标；[A002](#a002) 的高度、密度及显示要求。 |
| 任务范围内的必要共用接线 | [C003](#c003) | 只替换云影接收和云节点连接；未扩大为地表风格或玩法重做。 |
| 配套文档 | [C005](#c005) | 云实现的参数、来源与限制记录。 |
| 对应明确要求已确认 | [C006](#c006) | [A004](#a004) 明确要求忽略本地规则，并单独提交、推送忽略规则。 |

本索引按当前本地协作约定分类。它不追溯宣告历史实现“未经任何授权”，也不授权回滚现有设置。

<a id="integration"></a>
## 引入依赖索引

| 组／候选 | 条目 | 引入边界 |
| --- | --- | --- |
| `G001` 云功能配套组 | [C001](#c001)、[C002](#c002)、[C003](#c003)、[C005](#c005) | 云场接口、资源、控制器、接收材质、场景和来源说明配套处理。当前未核对合作者其他开发分支。 |
| 独立候选 | [C006](#c006) | 由 `ab989dd` 单独引入，仅修改 `.gitignore`。仍需确认目标项目是否采用相同的本地规则约定。 |
| 单独决定 | [C004](#c004) | 全局帧率设置在代码上可与云功能区分，但它与云优化同在 `7d3d951` 提交中；整次选择该提交会同时带入 FPS 设置。 |

当前三个云提交不能直接等同于“排除 C004 的 G001”：`7d3d951` 混有云实现、说明和帧率设置。若决定排除 FPS 变化，应另行审阅所需差异，并验证得到的组合，不自动重写已共享历史。

<a id="changes"></a>
## 变更条目

<a id="c001"></a>
### C001 — 云场与可见云渲染

```yaml
module: cloud_rendering
origin: THIS_BRANCH
impact_type: MODULE_LOCAL
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A001, A002]
source_commits: [5731b0a, 799da11, 7d3d951]
integration_mode: WITH_DEPENDENCIES
integration_group: G001
depends_on: [C005]
validation_status: STATIC_CHECKED
validation_refs: [V001, V002, H001, H002, H003]
```

| 起点版本 | 当前版本 |
| --- | --- |
| `cloud_hash()` 按相邻单元生成多瓣轮廓，`cloud_density(vec2)` 返回二维云量。 | 连续多尺度天气场与两份 3D 噪声生成密度，使用 `cloud_band_density(vec3, band, detail)`。 |
| 单个云平面，`cloud_height=7`，没有当前的两个实体高度带。 | 低云 1.5～2.3、中云 4.0～4.7；按本次设计比例，1 场景单位对应 1 km。背景云丝映射高度 9，不参与物理云影。 |
| 云层主要以固定轮廓随 `cloud_velocity` 平移。 | 各层分别漂移并内部演化；上层速度倍率 1.35、风向偏转 0.22 弧度；主风方向沿用草风参数。 |
| 平面着色与透明度。 | 分别积分两层云的可见密度及太阳透光，支持层间遮光，并依据场景深度停止积分。 |
| 无当前的正交视线高度平面修正。 | `fragment()` 沿原视线将起点移到相机世界高度平面，修正近裁剪面造成的横向截断。 |
| 无当前的分段缩放淡出和远景云丝。 | size 8～14 渐显，20～40 恢复厚云；云丝仅在 size 34～40 的背景空白处渐显。显示淡化不改变完整云场的云影。 |

当前关键参数：低／中云密度倍率 `4 / 3.25`，云影柔化偏移 `0.25 / 0.45`，可见步数 `24 / 16`；近景云透明度倍率 `0.38`、凸起表面倍率 `0.08`。凸起识别使用世界高度 `0.18～0.5`，不是按单位类型分类；地形或模型尺寸改变时需复核。

资源变化：新增两份 `96×96×96` 无缝 3D 噪声；原二维噪声频率 `0.004 → 0.009`、倍频数 `2 → 4`。云材质新增资源绑定，透明绘制优先级设为 `-10`。

接口依赖：旧 `cloud_density(vec2)` 已移除；旧版 `low_poly` 仍调用该函数，因此新云场不能与未适配的旧接收着色器混用。配套更新见 [C003](#c003)。当前深度重建及缩放逻辑针对 GL Compatibility 和正交俯视相机，未验证其他投影／渲染模式。

<a id="c002"></a>
### C002 — 布局初始化与两份共享缓存

```yaml
module: cloud_runtime
origin: THIS_BRANCH
impact_type: MODULE_LOCAL
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A001, A002]
source_commits: [5731b0a, 799da11, 7d3d951]
integration_mode: WITH_DEPENDENCIES
integration_group: G001
depends_on: [C001]
validation_status: STATIC_CHECKED
validation_refs: [V001, V002, H002, H003]
```

起点无 `CloudWeather` 脚本或这两份缓存。当前连接如下：

| 生产端 | 输出 | 消费端 |
| --- | --- | --- |
| `CloudWeather._enter_tree()` | 本局 `cloud_weather_origin`；`weather_seed=0` 时随机，非零时可复现布局。使用独立 RNG 实例。 | 云与 `low_poly` 模板；工厂后续生成的地块／单位材质副本。 |
| `cloud_weather.gdshader` + `CloudCoverage` SubViewport | 512×512；RG 为下层云量与云丝包络，BA 为上层；每帧更新。 | 可见云、光照积分及阴影的水平天气场查询。 |
| `cloud_sunlight.gdshader` + `CloudSunlight` SubViewport | 256×256；R/G 为下／上层地面透射，B 为上层向下层透射；每帧更新。 | 地表／单位云影及下层云受光。 |
| `CloudWeather._ready()` | 对预览场景内联接收材质绑定相同原点和缓存。 | 独立美术预览。 |

缓存范围：原点 `(-48,-48)`，跨度 `(128,128)`；范围固定，不随相机移动。范围外回到直接计算，云不会因此消失，但 GPU 开销可能上升。保持固定范围符合 [A002](#a002) 的明确暂缓要求。

参数一致性：初始化从云模板复制覆盖值。以后动态调节云形、风或太阳参数时，需同时同步云材质、接收材质、`_weather_material` 和 `_sunlight_material`；当前无自动传播全部后续参数变化的公共控制接口。复现画面需要同时控制种子、时间和相机。

<a id="c003"></a>
### C003 — 共用材质及场景接线

```yaml
module: cloud_receivers_and_scenes
origin: THIS_BRANCH
impact_type: SHARED_INTEGRATION
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A001, A002]
source_commits: [5731b0a]
integration_mode: WITH_DEPENDENCIES
integration_group: G001
depends_on: [C001, C002]
validation_status: STATIC_CHECKED
validation_refs: [V001, V002, H003]
```

| 定位 | 修改前 | 修改后及影响 |
| --- | --- | --- |
| `low_poly.fragment()` | 用平面高度投影采样旧二维云量。 | 调用 `cloud_shadow(world_position)`，使地块和单位受两层云影影响；云影强度默认值 `0.52 → 0.42`。 |
| `low_poly.tres` | 只有二维云噪声。 | 增加两份三维云噪声，满足共享云场采样需求。 |
| `grass_wind_direction` | 在 `low_poly` 内定义。 | 移到共享 include；名字与默认值 `(0.92,0.38)` 不变。草风速度、强度及分布公式未修改。 |
| `world/world.tscn` | 根节点下的 `CloudLayer`；100×100 PlaneMesh，位置 `(16,7,14)`。 | 改为 `Camera3D/CloudLayer`；2×2 QuadMesh，相机局部位置 `(0,0,-1)`；新增根节点 `CloudWeather`。世界云高由 C001 的参数决定。 |
| `StylePreview.tscn` | 根节点下的平面云层，缺少新控制器与三维噪声绑定。 | 同步相机子节点云层、控制器与内联材质资源。 |

已有消费路径未改 C# 源码：`world/StylizedMaterials.cs::Create()` 复制 `low_poly.tres`，`template/tile/Tile.cs` 与 `template/Piece/Piece.cs` 使用该工厂。此次变化因此会改变它们表面的云影，但没有重写颜色、选中高亮、草风、雾或原有分档光照公式。

协作交集：接收着色器、材质模板、两个场景均属于需与其他模块共同核对的文件。`World.cs` 的相机移动／缩放代码未修改；新增相机子节点仍会使其他相机方案与云显示存在集成依赖。

<a id="c004"></a>
### C004 — 全局 60 FPS 上限

```yaml
module: project_runtime_settings
origin: THIS_BRANCH
impact_type: PROJECT_GLOBAL
requires_extra_approval: true
approval_status: UNCONFIRMED
approval_refs: [A003]
source_commits: [7d3d951]
integration_mode: REVIEW_REQUIRED
integration_group: null
depends_on: []
validation_status: STATIC_CHECKED
validation_refs: [V001, H002, H003]
```

精确差异：`project.godot` 的 `[application]` 新增 `run/max_fps=60`。起点未设置此键；这不代表起点实际渲染帧率一定不受 VSync 等条件限制。

影响：整个项目的默认渲染帧率上限，包括其他场景和模块；隐藏云不会移除此设置。它不是云内部采样预算，也不等同于物理更新频率。

授权核对：[A003](#a003) 明确允许把性能优化与高度等调整提交／推送；本次读取的历史中，没有确认到独立的、实施前针对全局 60 FPS 方案的明确批准。记录为 `UNCONFIRMED`，不从提交说明或助手完成答复反推授权。

待决定：共同版本是否保留该全局策略。该决定不由本报告自动作出；目前代码保持原样，也未执行回滚。

<a id="c005"></a>
### C005 — 参数说明与开源来源记录

```yaml
module: cloud_documentation
origin: THIS_BRANCH
impact_type: DOCUMENTATION
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A001, A002]
source_commits: [5731b0a, 799da11, 7d3d951]
integration_mode: WITH_DEPENDENCIES
integration_group: G001
depends_on: []
validation_status: STATIC_CHECKED
validation_refs: [V001]
```

新增 `CLOUDS_LICENSES.md`，记录 Clay John、MangoButtermilch、Sebastian Lague、Valerie Bettaque 四个参考项目的固定版本、参考范围和 MIT 原文。现有实现没有把它们的插件、天空系统或素材包作为新增项目依赖导入。

README 顶部补充当前两层云、缓存、缩放、云影、默认值及性能条件。其后保留历史草原说明，旧段落中的旧默认值不能作为当前云参数依据；读取顺序以顶部当前更新为准。文档与来源说明随 G001 一起保留。

<a id="c006"></a>
### C006 — 忽略本地 AI 工作约定

```yaml
module: local_development_workflow
origin: THIS_BRANCH
impact_type: DEVELOPMENT_WORKFLOW
requires_extra_approval: false
approval_status: EXPLICITLY_APPROVED
approval_refs: [A004]
source_commits: [ab989dd]
integration_mode: INDEPENDENT
integration_group: null
depends_on: []
validation_status: STATIC_CHECKED
validation_refs: [V001, V003]
```

`.gitignore` 新增注释及根目录匹配 `/AGENTS.md`，共 3 行。它影响该文件的 Git 忽略行为，不修改游戏运行设置；不匹配所有子目录中的同名文件。

提交 `ab989dd` 只包含 `.gitignore`。规则文件实际内容未进入提交；已跟踪的其他文件也不因这条规则被取消跟踪。当前分支报告位于正常项目路径，不属于该忽略项。

<a id="commits"></a>
## 提交索引

| 完整提交 | 主题 | 关联条目 |
| --- | --- | --- |
| `5731b0ae9ee11ac44ef8dffe51d6e2033b782579` | feat: add evolving volumetric clouds and shadows | C001、C002、C003、C005 |
| `799da1183c1ef6b261e801d4d0a81a06ed58cbcd` | feat: add layered clouds and fix orthographic clipping | C001、C002、C005 |
| `7d3d95165284adc4f9d584b13eb461ef0528825a` | perf: cache cloud coverage and tune cloud layers | C001、C002、C004、C005；混有全局 FPS 设置 |
| `ab989dd380733573dafc7f6a993e80c36f26b735` | chore: ignore local AGENTS.md instructions | C006；单独文件提交 |

固定起点为 `d6b1545b527bd8165b8a6c1e8b4817ec695828bc`。在所覆盖代码版本的核对时，远端 `master` 等于该提交，远端云分支等于 `reviewed_commit`；该范围内的 4 次提交没有合并提交。后续 `REPORT_ONLY` 提交会推进分支指针，但不改变上述 20 个文件的统计范围。以后目标分支前进或出现其他路径变化时，更新观察值并核对接入差异，不覆盖固定起点。

<a id="approvals"></a>
## 授权证据索引

本节保留必要的用户原话和来源定位。对话原文位于本地 Codex，访问范围为 `LOCAL_CONVERSATION`，未导出完整对话。其他机器无法读取原文时，只能把摘录作为本报告提供的记录，不能声称已经独立核对原始对话。

<a id="a001"></a>
**A001 — 云与云影的实现范围。** 来源：本地任务 `01a153ed-9fe2-7881-8c0d-2c32be6d3238`，turn `01a15421-364e-7072-a981-60c39e2b8043`，用户消息 `01a15421-3b0a-7713-af4e-a6ecb345f3f7`。摘录：“先按这个思路改一版云和云影，风格往写实方向做，保持现有渲染模式，先不动草原和其他部分”。用于判定云实现及必要云影接线的范围，不授权任意全局设置变化。

<a id="a002"></a>
**A002 — 高度、密度、云影和缓存暂缓边界。** 来源：同一云任务，turn `01a156f1-a443-7530-954b-18259fa7a9ef`，用户消息 `01a156f1-a821-7002-ac47-0f280bc59a02`。原话：“可以，按这个高度方案改一版，密度和云影也一起调整好，保持拉近时不遮挡操作、拉远时有厚云的感觉，缓存范围的问题先不动。”

<a id="a003"></a>
**A003 — 后续性能改动的 Git 操作授权。** 来源：同一云任务，turn `01a156fd-37b0-7732-86b7-98dd1473ac46`，用户消息 `01a156fd-3aad-7ee2-b6be-c3cbe3003a9e`。原话：“好的，把这次性能优化和云层高度、密度、云影的调整一起提交并 push 到之前的开发分支吧”。类型为 `GIT_ACTION`；不将其补写成实施前对 C004 的独立方案批准。

<a id="a004"></a>
**A004 — 本地规则与忽略规则的单独提交。** 来源：本地任务 `01a156ff-c59b-7621-a636-898a0b07d45c`，2026-10-20 用户消息。原话：“把 .gitignore 的修改单独提交并 push 到当前开发分支，AGENTS.md 只保留在本地，不要带上其他未提交的改动。”

<a id="a005"></a>
**A005 — 本报告的保存与审阅安排。** 来源：同一当前任务，2026-10-20 用户消息。原话：“报告随开发分支一起保存和提交，AGENTS.md 继续只保留在本地。把报告维护要求补进 AGENTS.md，并按你说的结构，为当前云开发分支生成第一版变更报告，先给我看一下。”该消息确定先保存并展示报告的步骤；后续审阅结果与 Git 授权见 A006。

<a id="a006"></a>
**A006 — 报告审阅确认与单独提交／推送授权。** 来源：同一当前任务，2026-10-20 用户消息。原话：“可以，报告按这版来，把待审草稿状态更新后，单独提交并 push 到当前开发分支。”类型为 `REPORT_REVIEW_AND_GIT_ACTION`，仅覆盖本报告的状态更新及单独提交／推送；不扩大 C004 的授权，也不包含其他未提交改动或分支合并。

<a id="validation"></a>
## 验证与证据索引

| 编号 | 类型与可访问范围 | 本次实际核对内容 | 结论边界 |
| --- | --- | --- | --- |
| `V001` | `CURRENT_STATIC`；仓库内可复核 | 固定提交差异、4 次提交、20 个文件及 856/76 行统计；逐项阅读云、共用文件、资源及配置。 | 确认变更范围与接线，不等于运行通过。 |
| `V002` | `CURRENT_STATIC` + `LOCAL_ARTIFACT` | 当前提交中的 6 份核心源码，与历史 `preview-project/shaders/` 中对应文件逐字节一致，详见下表；历史预览项目配置匹配原边界记录，与当前配置仅键顺序不同，按节／键比较的值一致。 | 确认列出的源码及项目配置值一致；不证明整份项目、工具链或设备环境一致。 |
| `V003` | `CURRENT_GIT_CHECK`；工作区可复核 | `git check-ignore -v -- AGENTS.md` 命中根目录规则；Git 跟踪列表不含该规则文件；`ab989dd` 仅改 `.gitignore`。 | 确认忽略和提交范围；没有测试游戏行为。 |
| `H001` | `HISTORICAL_RUNTIME_RECORD`；`LOCAL_ARTIFACT` | 历史截断修复记录：7 个平移位置、近／远景、4:3 与超宽比例；记录中云影像素一致。 | 原始材料未随本报告共享；不是本轮重新运行。 |
| `H002` | `HISTORICAL_RUNTIME_RECORD`；`LOCAL_ARTIFACT` | 历史缓存性能对比：1080p、RTX 3060 Laptop、GL Compatibility；记录包括动态演化和缓存边界检查。 | 属于高度调整前的性能阶段，不把其数值作为当前最终版本耗时。 |
| `H003` | `HISTORICAL_RUNTIME_RECORD`；`LOCAL_ARTIFACT` | 最后高度版本记录：最近视角与隐藏云图像一致；缩放不改变物理云影；内部演化与层间遮光；4 次运行退出码为 0。 | 本轮未重新执行，也未验证合作者设备或其他分支组合。 |

V002 源码 SHA-256：

| 仓库路径 | `reviewed_commit` 内容 SHA-256 |
| --- | --- |
| `shaders/cloud_field.gdshaderinc` | `ed05bdb6de4aff3b1f9536bf08d3991807edf4da9ce459f74ad24f11720aa9fc` |
| `shaders/cloud_layer.gdshader` | `6fd5f9f907151e32e2e197d7b5a605687995b3ed11af31a33abde54352012488` |
| `shaders/CloudWeather.gd` | `f94cd2c3ba72671d37b746299a3f08a90c17de2c97e02335021a7fa00b25bcda` |
| `shaders/cloud_sunlight.gdshader` | `2dc50839a36689d2edb27eafe077254d6675a6ed9a58d464bb139c4c967ae492` |
| `shaders/cloud_weather.gdshader` | `51a3ec1fb3d7ddd102961c50fc6a19d77d1567c55074af856f51a989d035681b` |
| `shaders/low_poly.gdshader` | `640a7a9338e6dd0a6a0d0f477f53c05c2deed3dfda7dc02641b81673b448e399` |

本地历史材料以云任务为归属，保存名称如下；这里只记录逻辑位置及摘要哈希，不写本机绝对路径，不提供不可移植的文件链接：

| 编号 | 材料名 | SHA-256 |
| --- | --- | --- |
| H001 | `clouds/round-5-ray-clipping/validation-summary.json` | `0a7db4d6f4c6e708dea4594d4bd76280308f1dfeedae40fe227289c74818b96f` |
| H002 | `clouds/round-6-performance/validation-summary.json` | `b69b52c7b9e0c27c6e0989bc6e5267d2a9602bb4dcdf5001bd340fb895fc1044` |
| H003 | `clouds/round-7-altitudes/validation-summary.json` | `99f927434b09f183b2494406ea18c17d6d94eaeda0d7de9ae53956aaac0dc061` |

H003 的历史记录给出近景 size 20／远景 size 40 GPU 总耗时约 `8.9 / 11.6 ms`，包含缓存；条件为 Godot 4.6.1 Mono、GL Compatibility、1920×1080、RTX 3060 Laptop、60 FPS 默认上限。此为本机历史记录，不能用于承诺其他环境的耗时。

完整项目一致性限制：历史高度阶段的 `source-boundary.json` 中，`Piece.cs` 的哈希匹配当前本地未提交修复，而不匹配已提交版本。`project.godot` 的字节差异已核对为属性序列化顺序变化，配置值一致。V002 只确认上述源码及配置范围，不能将包含本地单位修复的历史运行扩大为整份纯提交版已经运行验证。本轮没有启动 Godot、构建项目或执行新的 GPU 测试。

<a id="collaboration"></a>
## 合作接入检查与范围限制

| 检查项 | 已确认事实 | 后续接入要求 |
| --- | --- | --- |
| 玩法与数据 | 固定差异中没有 `.cs`、`.csproj`、`.sln`、`Map/` 或 `world/UI/`、`template/` 内的变更。 | 不据此推断所有玩法已回归；合并后检查选中、移动、攻击提示和相机操作。 |
| 渲染和依赖 | `project.godot` 仅新增 FPS 键；渲染模式仍为 `gl_compatibility`，SDK／依赖文件未改。 | 对方若调整相机投影、渲染模式、地形或模型高度，复核 C001 的深度和高度假设。 |
| 共用材质 | 接收云影路径发生变化，其他地表公式未重写。 | 对方修改材质或光照时，核对 C003 和共享 uniform，不能只看是否有文本冲突。 |
| 缓存范围 | 固定世界范围，范围外直接计算。 | 保持 A002 暂缓边界；需要相机跟随缓存时另提方案。 |
| 动态参数 | 模板、接收材质、两份缓存存在初始化复制关系。 | 引入天气／风向／太阳动态控制时同步所有消费者参数，见 C002。 |
| 目标分支 | 本次远端目标仍等于起点。 | 未来主分支更新后记录新目标，区分同步来源，重新核对交集与运行结果。 |

<a id="local-only"></a>
## 本地排除项与报告自身

以下不是上述 20 个已提交变更文件。既有两处改动在生成本报告前已存在；本次未将它们归入云开发结果。

| 项目 | 核对状态／归属 | 归属与处理 |
| --- | --- | --- |
| `template/Piece/Piece.cs` | `M`；未暂存 | 本地存在单位销毁时释放未挂树技能节点的改动；不在云分支已提交差异中，不随报告纳入提交。 |
| `template/tile/GrassWindNoise.tres` | `D`；未暂存 | 本地删除；已提交对象为零字节。排除在云提交和本报告的提交候选之外。 |
| `AGENTS.md` | `IGNORED`；本地维护规则 | 本地个人约定，不跟踪、不随报告共享；其正文不是已提交的仓库规范。 |
| `docs/branch-reports/codex-realistic-clouds.md` | `REPORT_ONLY`；`USER_REVIEWED` | 报告本身按 A006 单独提交，不计入 `reviewed_commit` 覆盖的 20 个变更文件。 |

既有已跟踪工作区差异的核对指纹：`git diff --binary HEAD` 的 SHA-256 为 `471cb73f0d640b4fdb98b69085453faedb385690c6d54a89eedb5ceab8965a6e`。该指纹用于本次保留现场检查，不是未来工作区必须长期保持的值。

<a id="decisions"></a>
## 待决定事项与报告更新

| 事项 | 当前状态 | 下一步 |
| --- | --- | --- |
| 第一版报告的内容和结构 | `USER_REVIEWED` | A006 已确认报告，并授权按 `REPORT_ONLY` 单独提交／推送；AGENTS.md 保持本地。 |
| C004 的全局帧率策略 | `EXTRA_APPROVAL_UNCONFIRMED` | 在准备合入共同版本时明确保留或另行调整；本轮只记录，不修改设置。 |
| 与合作者最新模块的兼容 | `NOT_ASSESSED` | 获得对应版本并准备接入时，按共用文件与参数依赖核对。 |
| 纯已提交版本的运行回归 | `NOT_RUN_THIS_REVIEW` | 需要运行验收时使用明确提交及隔离验证环境，不混入本地排除项。 |

维护时机：仅在用户明确要求 commit／push（提交或推送）时，在执行前集中更新报告；日常开发、调试、验证和任务交付不要求同步维护。用户另外明确要求查看、生成或修订报告时，按该指令处理。

更新要求：维护时保持 C001～C006 编号与报告路径稳定；新增行为建立新编号，修订现有行为更新原条目。撤回项保留编号并退出候选清单。届时集中汇总实际准备提交／推送的内容，重新核对完整文件索引和固定基准统计；同步主分支时标明来源。`reviewed_commit` 只承诺已核对的内容；后续若仅提交本报告，可保留该值，其他文件有变化则在下一次报告维护时补充核对。清理已过期的草稿、待审和工作区状态，不把旧验证或旧授权无条件沿用。
