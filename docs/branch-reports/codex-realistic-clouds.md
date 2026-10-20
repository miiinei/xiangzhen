---
schema_version: 1
report_status: UPDATED_NOT_USER_REVIEWED
report_reviewed_on: null
report_approval_ref: null
prior_report_approval_ref: A006
branch: codex/realistic-clouds
base_ref: master
base_commit: d6b1545b527bd8165b8a6c1e8b4817ec695828bc
reviewed_commit: 08515f05f49fa0ea771b39f145d97514132cd6a2
target_ref: origin/master
target_observed_commit: d6b1545b527bd8165b8a6c1e8b4817ec695828bc
remote_branch_observed_commit: b601af16cbcfb6fbafc203f69fdb732e9dff8b81
remote_observation_phase: PRE_PUSH
checked_on: 2026-10-20
timezone: Asia/Shanghai
comparison: base_commit..reviewed_commit
scope: COMMITTED_NET_DIFF_EXCLUDING_REPORT
changed_file_count: 23
insertions: 1063
deletions: 167
post_base_commit_count: 8
post_base_merge_commit_count: 0
upstream_sync_status: NO_POST_BASE_MAINLINE_UPDATE_OBSERVED
runtime_validation_this_review: NOT_RUN
merge_decision: NOT_AUTHORIZED_BY_THIS_REPORT
---

# 云开发分支变更报告

本报告在 A006 已审阅的初版基础上，按 A009 的本次推送要求更新；本次新增内容尚未经用户单独审阅，记录为 `UPDATED_NOT_USER_REVIEWED`，不据此要求重复确认已授权的提交／推送。统计覆盖固定起点到 `reviewed_commit` 的 23 个非报告变更文件；完整文件索引另列报告自身，合计 24 个路径。本地未提交改动见 [本地排除项](#local-only)。远端观察值在本次推送前读取，不代表推送后的分支指针。

导航：[字段约定](#schema) · [模块索引](#modules) · [完整文件索引](#files) · [额外授权索引](#permissions) · [引入依赖](#integration) · [变更条目](#changes) · [提交索引](#commits) · [授权证据](#approvals) · [验证证据](#validation) · [协作检查](#collaboration) · [本地排除项](#local-only) · [待决定事项](#decisions)

读取顺序：先核对头部提交，再按模块或文件定位 `Cxxx`；准备引入时同时读取依赖组、授权和验证记录。报告的候选引入分类不替代用户批准或合并后的验证。

<a id="schema"></a>
## 字段约定

| 字段 | 允许值及含义 |
| --- | --- |
| `report_status` / `report_approval_ref` | `USER_REVIEWED` 表示对应版本已获用户审阅；`UPDATED_NOT_USER_REVIEWED` 表示本次更新尚未单独审阅。A006 保留为初版审阅记录，A009 授权本轮提交／推送；两者均不代表合并或完整运行验收。 |
| `status` | `ACTIVE` 为当前有效变化，省略时按此值处理；`WITHDRAWN` 为已撤回、退出当前引入候选的历史条目，编号不复用。 |
| `impact_type` | `MODULE_LOCAL` 模块内部；`SHARED_INTEGRATION` 共用接线；`PROJECT_GLOBAL` 项目级运行行为；`DOCUMENTATION` 文档与来源记录；`DEVELOPMENT_WORKFLOW` 开发工具的项目约定。 |
| `requires_extra_approval` | `true` 表示按当前本地约定需要额外明确批准；`false` 表示本条位于已授权任务范围内。不能由此判断验证是否通过。 |
| `approval_status` | `IN_SCOPE` 已有任务范围覆盖；`EXPLICITLY_APPROVED` 找到对应明确要求；`UNCONFIRMED` 额外授权证据尚未确认，不等同于断言从未批准。 |
| `integration_mode` | `INDEPENDENT` 可独立审阅／选择引入的候选；`WITH_DEPENDENCIES` 应按指定依赖组引入；`REVIEW_REQUIRED` 需要单独决定。均不表示已批准合并。 |
| `validation_status` | `NOT_RUN` 本次未验证；`STATIC_CHECKED` 本次核对源码、差异或 Git 行为；`RUNTIME_CHECKED` 对明确版本实际运行验证。本次提交准备以 `STATIC_CHECKED` 核对实际源码和 Git 范围，实施阶段的运行结果按 H001～H005 单独记录。 |
| `origin` | `THIS_BRANCH` 本开发分支引入；`UPSTREAM_SYNC` 主分支同步；`LOCAL_ONLY` 工作区独有。当前已提交条目均为 `THIS_BRANCH`。 |
| `approval_refs` / `validation_refs` | 指向本报告的 `Axxx` / `Vxxx` / `Hxxx` 证据编号；缺失的原始证据不得用助手结论或提交说明替代。 |
| `depends_on` / `integration_group` | 条目依赖与配套引入组；组内文件不按单文件复制作为默认接入方式。 |

补充规则：路径相对仓库根目录；文内源码链接使用 `../../` 回到根目录。稳定定位采用函数名、配置键、资源属性或节点路径。完整文件索引中的 `A/M` 分别表示新增／修改，增删行数来自固定比较范围。

<a id="modules"></a>
## 模块索引

| 模块 | 条目 | 入口 |
| --- | --- | --- |
| 云场、体积渲染与噪声 | [C001](#c001) | `cloud_band_density()`、`march_cloud_band()`、云材质与三份噪声资源 |
| 布局初始化与世界场缓存 | [C002](#c002) | `_enter_tree()`、云量与太阳透射缓存；更新节奏见 C007 |
| 地表／单位云影与场景接线 | [C003](#c003) | `low_poly.fragment()`、`Camera3D/CloudLayer`、`CloudWeather` 节点 |
| 已撤回的项目帧率策略 | [C004](#c004) | `project.godot` 已逐字节恢复起点；原 FPS 键已移除 |
| 参数说明与来源 | [C005](#c005) | README 当前实现、参数与接入；四个开源参考的固定版本记录 |
| 本地规则文件的 Git 忽略 | [C006](#c006) | `.gitignore` 的 `/AGENTS.md` |
| 云层独立更新与视线缓存 | [C007](#c007) | `_process()`、`_update_view()`、`cloud_view.gdshader`、`cloud_time()` |
| 缓存初始化与参数组织整理 | [C008](#c008) | `_create_cache_material()`、`_create_cache_viewport()`、最终显示参数归属 |

<a id="files"></a>
## 完整文件索引

| 状态 | 文件 | 新增／删除 | 条目 | 稳定定位点 |
| --- | --- | --- | --- | --- |
| M | [.gitignore](../../.gitignore) | 3 / 0 | [C006](#c006) | `/AGENTS.md` |
| A | [shaders/CLOUDS_LICENSES.md](../../shaders/CLOUDS_LICENSES.md) | 115 / 0 | [C005](#c005) | 四个来源、固定版本和 MIT 原文 |
| A | [shaders/CloudWeather.gd](../../shaders/CloudWeather.gd) | 216 / 0 | [C002](#c002)、[C007](#c007)、[C008](#c008) | 模板绑定、三份缓存、独立时钟与共用初始化 |
| A | [shaders/CloudWeather.gd.uid](../../shaders/CloudWeather.gd.uid) | 1 / 0 | [C002](#c002) | Godot 脚本 UID |
| M | [shaders/README.md](../../shaders/README.md) | 105 / 94 | [C005](#c005)、[C007](#c007)、[C008](#c008) | 当前参数、更新节奏、接入与成本 |
| M | [shaders/StylePreview.tscn](../../shaders/StylePreview.tscn) | 21 / 10 | [C003](#c003) | 相机子节点 CloudLayer、CloudWeather 和内联材质 |
| A | [shaders/cloud_base_noise.tres](../../shaders/cloud_base_noise.tres) | 14 / 0 | [C001](#c001) | 96³ Perlin 主体噪声 |
| M | [shaders/cloud_field.gdshaderinc](../../shaders/cloud_field.gdshaderinc) | 266 / 33 | [C001](#c001)、[C007](#c007) | 密度、完整云影与 `cloud_time()` |
| M | [shaders/cloud_layer.gdshader](../../shaders/cloud_layer.gdshader) | 130 / 12 | [C001](#c001)、[C007](#c007)、[C008](#c008) | 实时深度、缓存重新投放和最终显示参数 |
| M | [shaders/cloud_layer.tres](../../shaders/cloud_layer.tres) | 8 / 2 | [C001](#c001) | 三维噪声绑定、透明绘制优先级 |
| M | [shaders/cloud_noise.tres](../../shaders/cloud_noise.tres) | 2 / 2 | [C001](#c001) | 水平天气噪声的频率与倍频数 |
| A | [shaders/cloud_optics.gdshaderinc](../../shaders/cloud_optics.gdshaderinc) | 81 / 0 | [C001](#c001)、[C007](#c007)、[C008](#c008) | 共用的视线积分与采样参数 |
| A | [shaders/cloud_optics.gdshaderinc.uid](../../shaders/cloud_optics.gdshaderinc.uid) | 1 / 0 | [C007](#c007) | Godot include UID |
| A | [shaders/cloud_sunlight.gdshader](../../shaders/cloud_sunlight.gdshader) | 16 / 0 | [C002](#c002) | 完整云场的太阳透射 RGB 通道 |
| A | [shaders/cloud_sunlight.gdshader.uid](../../shaders/cloud_sunlight.gdshader.uid) | 1 / 0 | [C002](#c002) | Godot 着色器 UID |
| A | [shaders/cloud_view.gdshader](../../shaders/cloud_view.gdshader) | 29 / 0 | [C007](#c007) | 两层预乘颜色／透明度图集 |
| A | [shaders/cloud_view.gdshader.uid](../../shaders/cloud_view.gdshader.uid) | 1 / 0 | [C007](#c007) | Godot 着色器 UID |
| A | [shaders/cloud_volume_noise.tres](../../shaders/cloud_volume_noise.tres) | 17 / 0 | [C001](#c001) | 96³ cellular 云泡与侵蚀噪声 |
| A | [shaders/cloud_weather.gdshader](../../shaders/cloud_weather.gdshader) | 15 / 0 | [C002](#c002) | 上下层覆盖与云丝 RG/BA 通道 |
| A | [shaders/cloud_weather.gdshader.uid](../../shaders/cloud_weather.gdshader.uid) | 1 / 0 | [C002](#c002) | Godot 着色器 UID |
| M | [shaders/low_poly.gdshader](../../shaders/low_poly.gdshader) | 3 / 7 | [C003](#c003) | `fragment()` 的云影接收 |
| M | [shaders/low_poly.tres](../../shaders/low_poly.tres) | 7 / 2 | [C003](#c003) | 共用云场的三维噪声绑定 |
| M | [world/world.tscn](../../world/world.tscn) | 10 / 5 | [C003](#c003) | `Camera3D/CloudLayer` 与 CloudWeather 节点 |
| A | [docs/branch-reports/codex-realistic-clouds.md](codex-realistic-clouds.md) | 不计入代码统计 | `REPORT_ONLY` | 报告自身，随独立文档提交维护 |

代码统计：23 个文件，1063 行新增、167 行删除。另有报告自身 1 个路径，不参与代码增删统计。`project.godot` 已与固定起点逐字节一致，退出净差异文件表；其撤回历史仍保留在 C004。

<a id="permissions"></a>
## 额外授权索引

| 分类 | 条目 | 状态依据 |
| --- | --- | --- |
| 已明确要求撤回的全局设置 | [C004](#c004) | `PROJECT_GLOBAL`；A007 明确要求恢复开发分支起点的主支设置，当前已撤回原 60 FPS 键。 |
| 任务范围内的云实现、独立更新与整理 | [C001](#c001)、[C002](#c002)、[C007](#c007)、[C008](#c008) | A001、A002 确定云效果和缓存范围；A007 允许云单独降频，A008 要求在不影响性能的条件下清理。 |
| 任务范围内的必要共用接线 | [C003](#c003) | 只涉及云影与云时钟接收，草风仍使用原生 TIME，未扩大为地表风格或玩法重做。 |
| 配套文档 | [C005](#c005) | 参数、来源和当前实现说明，随云功能配套更新。 |
| 本地约定的对应明确要求 | [C006](#c006) | A004 授权忽略规则单独提交；AGENTS.md 正文继续只保留在本地。 |
| 本轮提交／推送 | 当前两笔代码提交及报告更新 | A009 授权推送当前版本；不包含本地排除项和主分支合并。 |

原 C004 添加时的事前独立批准仍记为未确认；本次明确撤回不追溯改写该历史。

<a id="integration"></a>
## 引入依赖索引

| 组／候选 | 条目 | 引入边界 |
| --- | --- | --- |
| `G001` 云功能配套组 | C001、C002、C003、C005、C007、C008 | 云场、噪声、控制器、三份缓存、共用积分、接收材质、场景和说明配套引入；新的云层 shader 会读取视线缓存，不能只复制单个文件。 |
| 独立候选 | C006 | `ab989dd` 仅修改 `.gitignore`，按目标项目的本地规则约定审阅。 |
| 已撤回，退出候选 | C004 | `7d3d951` 添加全局上限；`d9557f2` 已单独移除。当前分支末端的 `project.godot` 与固定起点完全一致。 |

选择历史 `7d3d951` 时仍会带入旧 FPS 键；选择当前云版本时应同时包含其后续恢复与独立更新提交，或审阅相应净差异。本次没有重写历史，也没有合并主分支。

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
approval_refs: [A001, A002, A007, A008]
source_commits: [5731b0a, 799da11, 7d3d951, 08515f0]
integration_mode: WITH_DEPENDENCIES
integration_group: G001
depends_on: [C005]
validation_status: STATIC_CHECKED
validation_refs: [V001, V002, H001, H002, H003, V004, H004, H005]
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

当前可见积分由 `cloud_optics.gdshaderinc` 共用；常规像素读取 C007 的视线缓存，缓存余量之外或物体表面进入该高度带时保留直接积分。最终透明度、近景单位保护和背景云丝参数仍位于 `cloud_layer.gdshader`。

<a id="c002"></a>
### C002 — 布局初始化与世界场缓存

```yaml
module: cloud_runtime
origin: THIS_BRANCH
impact_type: MODULE_LOCAL
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A001, A002, A007, A008]
source_commits: [5731b0a, 799da11, 7d3d951, 08515f0]
integration_mode: WITH_DEPENDENCIES
integration_group: G001
depends_on: [C001]
validation_status: STATIC_CHECKED
validation_refs: [V001, V002, V004, H002, H003, H004, H005]
```

起点没有 CloudWeather 和两份世界场缓存。当前连接如下，第三份视线缓存见 C007：

| 生产端 | 输出 | 消费端 |
| --- | --- | --- |
| `CloudWeather._enter_tree()` | 本局 weather_origin；weather_seed=0 随机，非零可复现，使用独立 RNG。 | 云和 low_poly 模板，以及工厂后续创建的材质副本。 |
| `cloud_weather.gdshader` / CloudCoverage | 512×512；RG 为下层覆盖和云丝，BA 为上层；按 C007 的 20 Hz 节奏更新。 | 可见密度、自遮光及阴影的水平天气查询。 |
| `cloud_sunlight.gdshader` / CloudSunlight | 256×256；R/G 为下／上层地面透射，B 为上层向下层透射；同节奏更新。 | 地表、单位云影及下层云受光。 |
| `_ready()` | 对预览内联材质绑定同一原点、时钟和缓存。 | 独立美术预览。 |

两份世界场缓存仍固定覆盖 (-48,-48) 至 (80,80)，不随相机移动。范围外继续直接计算，不清空云；保持 A002 的暂缓边界。C007 随相机更新的是屏幕视线缓存，未扩大上述世界缓存。

初始化从云模板复制覆盖值。以后动态调云形、风或太阳时，需同步云材质、接收材质、_weather_material、_sunlight_material 和 _view_material；当前没有自动传播全部后续参数的公共接口。复现需同时控制种子、时间和相机。

<a id="c003"></a>
### C003 — 共用材质及场景接线

```yaml
module: cloud_receivers_and_scenes
origin: THIS_BRANCH
impact_type: SHARED_INTEGRATION
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A001, A002, A007, A008]
source_commits: [5731b0a, 08515f0]
integration_mode: WITH_DEPENDENCIES
integration_group: G001
depends_on: [C001, C002]
validation_status: STATIC_CHECKED
validation_refs: [V001, V002, H003, V004, H004, H005]
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

C007 的共享时钟纹理在 C# 工厂复制模板前绑定，副本因此使用同一云时刻；草风原生 `TIME` 和其他地表公式不随云降频。

<a id="c004"></a>
### C004 — 已撤回的全局 60 FPS 上限

```yaml
module: project_runtime_settings
status: WITHDRAWN
origin: THIS_BRANCH
impact_type: PROJECT_GLOBAL
requires_extra_approval: true
approval_status: EXPLICITLY_APPROVED
approval_refs: [A003, A007]
source_commits: [7d3d951, d9557f2]
integration_mode: INDEPENDENT
integration_group: null
depends_on: []
validation_status: STATIC_CHECKED
validation_refs: [V001, V004, H004, H005]
```

历史 `7d3d951` 在 [application] 添加 run/max_fps=60，影响整个项目。原添加方案的事前独立批准未确认，A003 的后续 Git 授权不追溯替代它。

用户现已在 A007 明确要求恢复开发分支起点的主支设置；`d9557f2524c5e72b71c5e0e493ff06a3e9eb9a4b` 单独删除该键。当前 project.godot 与 `d6b1545b527bd8165b8a6c1e8b4817ec695828bc` 中的文件逐字节相同，VSync、分辨率、渲染模式和物理设置沿用起点。此条退出当前引入候选，编号保留；实际帧率仍受原有 VSync、显示器与硬件条件影响。

<a id="c005"></a>
### C005 — 参数说明与开源来源记录

```yaml
module: cloud_documentation
origin: THIS_BRANCH
impact_type: DOCUMENTATION
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A001, A002, A007, A008]
source_commits: [5731b0a, 799da11, 7d3d951, 08515f0]
integration_mode: WITH_DEPENDENCIES
integration_group: G001
depends_on: []
validation_status: STATIC_CHECKED
validation_refs: [V001, V004, H005]
```

新增 `CLOUDS_LICENSES.md`，记录 Clay John、MangoButtermilch、Sebastian Lague、Valerie Bettaque 四个参考项目的固定版本、参考范围和 MIT 原文。现有实现没有把它们的插件、天空系统或素材包作为新增项目依赖导入。

README 统一说明当前两层云、三份缓存、独立更新、默认值及接入条件，移除多代方案互相覆盖的旧云参数。地表现有材质接入和参数说明仍保留；代码清理阶段文档从 185 行整理为 114 行。来源许可文件未改动，文档与来源说明随 G001 配套保留。

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

<a id="c007"></a>
### C007 — 云层独立更新与视线缓存

```yaml
module: cloud_update_cadence
origin: THIS_BRANCH
impact_type: MODULE_LOCAL
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A007]
source_commits: [08515f0]
integration_mode: WITH_DEPENDENCIES
integration_group: G001
depends_on: [C001, C002, C003, C005]
validation_status: STATIC_CHECKED
validation_refs: [V001, V004, H004, H005]
```

此前主视口每帧进行完整云体积分，两份世界场缓存也每帧重算。当前 CloudWeather.cloud_update_fps 默认 20：通过一张共享 1×1 RF 浮点纹理给材质副本提供同一 cloud_time，云形、光照与云影按实际累计秒数更新，漂移与演化速度不变。草风仍使用原生 TIME。

cloud_view.gdshader 分别积分上下云带，在图集两半保存预乘颜色及透明度。cloud_resolution_scale 默认 0.75，按主视口宽高各 75% 的密度计算，四周额外 10% 平移余量；噪声资源、物理云厚、可见 24/16 步及自遮光预算不变。

主视口每帧按当前正交视线重新投放缓存，读取实时场景深度并执行近景单位保护。常规平移不降低相机、棋子或 UI 的显示频率；缩放、旋转、窗口改变或大幅平移会立即刷新云体，因此这类操作可使云本体更新超过 20 Hz。缓存余量外或不透明表面进入某层高度时，保留按真实深度停止的直接积分；最近视角跳过云本体缓存，完整云影继续更新。背景云丝在实时合成中读取同一云时钟。

本条只使用当前 GL Compatibility 的着色器和 SubViewport，不新增插件、全局 FPS 设置、Autoload 或项目级 shader globals。当前缓存投影适配既有正交相机，其他投影不使用视线缓存。

<a id="c008"></a>
### C008 — 初始化与参数组织清理

```yaml
module: cloud_code_organization
origin: THIS_BRANCH
impact_type: MODULE_LOCAL
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A008]
source_commits: [08515f0]
integration_mode: WITH_DEPENDENCIES
integration_group: G001
depends_on: [C002, C007]
validation_status: STATIC_CHECKED
validation_refs: [V001, V004, H005]
```

三份缓存共用 _create_cache_material() 和 _create_cache_viewport()，保留各自排除自身反馈纹理的规则、尺寸和更新模式。ColorRect 通过全尺寸锚点跟随视口，删除单独保存的 _view_surface 及每次手动同步尺寸的代码。

将 9 个仅负责透明度、单位保护和背景云丝的参数放回 cloud_layer；cloud_optics 共用积分函数与必要调度参数。参数名、类型、默认值与计算函数内容保持一致，没有新增每帧积分或纹理采样。缓存之外和高物体遮挡的直接积分仍被使用，未当作旧代码删除。H005 四档视距的清理前后截图逐像素一致。文档整理归入 C005。

<a id="commits"></a>
## 提交索引

| 完整提交 | 主题 | 关联条目 |
| --- | --- | --- |
| `5731b0ae9ee11ac44ef8dffe51d6e2033b782579` | feat: add evolving volumetric clouds and shadows | C001、C002、C003、C005 |
| `799da1183c1ef6b261e801d4d0a81a06ed58cbcd` | feat: add layered clouds and fix orthographic clipping | C001、C002、C005 |
| `7d3d95165284adc4f9d584b13eb461ef0528825a` | perf: cache cloud coverage and tune cloud layers | C001～C005；包含后来撤回的 FPS 键 |
| `ab989dd380733573dafc7f6a993e80c36f26b735` | chore: ignore local AGENTS.md instructions | C006 |
| `fb7e07815fa65a8892c15e6651b208fbf2b318c0` | docs: add reviewed cloud branch change report | REPORT_ONLY；初版报告 |
| `b601af16cbcfb6fbafc203f69fdb732e9dff8b81` | docs: update branch report maintenance timing | REPORT_ONLY；报告维护时机 |
| `d9557f2524c5e72b71c5e0e493ff06a3e9eb9a4b` | fix: restore mainline frame rate settings | C004；单独恢复项目配置 |
| `08515f05f49fa0ea771b39f145d97514132cd6a2` | perf: update clouds independently and simplify cache setup | C001、C002、C003、C005、C007、C008 |

固定起点为 `d6b1545b527bd8165b8a6c1e8b4817ec695828bc`。代码核对点范围内共 8 次提交，0 次合并提交；本次推送前观察的远端云分支为 `b601af16cbcfb6fbafc203f69fdb732e9dff8b81`，目标主支仍为固定起点。更新本报告的后续独立文档提交属于 REPORT_ONLY，保留上述 reviewed_commit，不要求报告记录包含自身内容的未知哈希。

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

<a id="a007"></a>
**A007 — 恢复主支项目设置，允许云单独降频。** 来源：同一云任务，2026-10-20 可见用户消息及随后批注。原话：“现在我需要你回滚这项设置，然后考虑可以将云朵帧率单独调慢。因为云朵速度太慢，所以帧率低一点也看不出来，这个你自己取舍”。随后明确：“不要画蛇添足，回滚到开发分支起始点的主支设置就行”。覆盖 C004 的恢复和 C007 的云模块内取舍，不授权其他全局设置变化。

<a id="a008"></a>
**A008 — 清理多轮修改的冗余。** 来源：同一云任务，2026-10-20 用户消息。要求检查多轮修改造成的冗余，并提出“如果有那种不影响性能的屎山，那就修改”。覆盖云初始化、参数组织和过时说明的清理，不扩展为玩法或基础框架重做。

<a id="a009"></a>
**A009 — 当前版本提交／推送。** 来源：同一云任务，2026-10-20 最新用户消息，原话：“将这一版推送吧”。按上一轮交付的范围，将项目设置恢复、云层独立更新及代码清理提交到现有开发分支，并按本地约定集中更新本报告；不包含 Piece.cs、空资源删除、AGENTS.md 正文或主分支合并。

<a id="validation"></a>
## 验证与证据索引

| 编号 | 类型与可访问范围 | 实际核对内容 | 结论边界 |
| --- | --- | --- | --- |
| V001 | CURRENT_STATIC；仓库可复核 | 固定基准至代码核对点的 23 个非报告文件、1063/167 行、8 次提交；完整路径另包含报告自身。 | 确认范围、接线和统计，不等于完整运行验收。 |
| V002 | HISTORICAL_STATIC + LOCAL_ARTIFACT | 初版报告核对过的六份旧核心源码及当时项目配置，旧哈希保留如下。 | 属于 7d3d951 版本，不再声称与当前已改源码逐字节相同。 |
| V003 | CURRENT_GIT_CHECK | AGENTS.md 命中根忽略规则，未被跟踪；ab989dd 仅改 .gitignore。 | 忽略和提交范围，不包含游戏行为验证。 |
| V004 | CURRENT_STATIC + LOCAL_ARTIFACT | 待提交源码与 H004/H005 的对应记录核对；project.godot 与起点逐字节相同。最终 cloud_optics 仅移除记录版本末尾一个多余空行，去除末尾空白后文本相同。H005 所查积分和合成函数内容、参数名／类型／默认值保持一致。 | 复用实施阶段证据；本次 Git 请求没有重新启动 Godot、构建或跑 GPU 测试。 |
| H001 | HISTORICAL_RUNTIME_RECORD；LOCAL_ARTIFACT | 截断修复：7 个平移位置、近远景、4:3 与超宽比例。 | 原始材料仅本机可访问。 |
| H002 | HISTORICAL_RUNTIME_RECORD；LOCAL_ARTIFACT | 高度调整前的云量缓存性能、动态演化及缓存边界检查。 | 不作为当前最终版本耗时。 |
| H003 | HISTORICAL_RUNTIME_RECORD；LOCAL_ARTIFACT | 高度版本：最近景淡出、缩放不改物理云影、内部演化和层间遮光，4 次运行退出码 0。 | 当时项目默认 60 FPS；不是本次恢复设置后的结果。 |
| H004 | IMPLEMENTATION_RUNTIME_RECORD；LOCAL_ARTIFACT | 独立更新阶段 12 项检查通过，静止／平移云时钟约 20.3 Hz、场景约 165.4／164.5 FPS；缓存外、云内高物体和 resize 检查通过。 | 清理前版本，后续差异由 H005 复核；原始材料未随仓库共享。 |
| H005 | IMPLEMENTATION_RUNTIME_RECORD；LOCAL_ARTIFACT | 清理后 12 项检查通过；size 8/20/30/40 清理前后逐像素一致；静止／平移场景约 165.5／164.4 FPS，云约 20.3 Hz，云体纹理继续变化。1280×720 缓存与直接积分全图平均 RGB 差约 0.034/255。 | Godot 4.6.1 Mono、GL Compatibility、1920×1080、RTX 3060 Laptop；Engine.max_fps=0、原有 VSync 开启。隔离副本，复用已有 C# 程序集，含本地 Piece 修复，未证明纯提交版完整运行。 |

V002 历史源码 SHA-256（旧核对点 7d3d951；并非当前 reviewed_commit）：

| 仓库路径 | `7d3d951` 内容 SHA-256 |
| --- | --- |
| `shaders/cloud_field.gdshaderinc` | `ed05bdb6de4aff3b1f9536bf08d3991807edf4da9ce459f74ad24f11720aa9fc` |
| `shaders/cloud_layer.gdshader` | `6fd5f9f907151e32e2e197d7b5a605687995b3ed11af31a33abde54352012488` |
| `shaders/CloudWeather.gd` | `f94cd2c3ba72671d37b746299a3f08a90c17de2c97e02335021a7fa00b25bcda` |
| `shaders/cloud_sunlight.gdshader` | `2dc50839a36689d2edb27eafe077254d6675a6ed9a58d464bb139c4c967ae492` |
| `shaders/cloud_weather.gdshader` | `51a3ec1fb3d7ddd102961c50fc6a19d77d1567c55074af856f51a989d035681b` |
| `shaders/low_poly.gdshader` | `640a7a9338e6dd0a6a0d0f477f53c05c2deed3dfda7dc02641b81673b448e399` |


当前代码核对点源码 SHA-256：

| 路径 | `08515f0` 内容 SHA-256 |
| --- | --- |
| `project.godot` | `c28ee3ef9eaff9a96ff42d8943f5e705c00f99b25d2cfd05f71077bb5b9bed55` |
| `shaders/CloudWeather.gd` | `432510b8bbb4cf934f692f521c7a74a874d79ab7c724329a77927f909ac2b468` |
| `shaders/cloud_field.gdshaderinc` | `72788ed25caafa41819a9a52031fdef91a653982c5c1c20fad39d673fadd35e7` |
| `shaders/cloud_layer.gdshader` | `0b0812a548354f3c13442877ef4b12782ef416287046e7be80a802ce9dbe0ebd` |
| `shaders/cloud_optics.gdshaderinc` | `f6f495664eb946808801ea10e8caa50486a7ad85fae9a5b69788d318253f488e` |
| `shaders/cloud_view.gdshader` | `aabc90e12375cdd0c73487b043deb1daf9a17ed94515988066d608fe6858d266` |
| `shaders/cloud_weather.gdshader` | `51a3ec1fb3d7ddd102961c50fc6a19d77d1567c55074af856f51a989d035681b` |
| `shaders/cloud_sunlight.gdshader` | `2dc50839a36689d2edb27eafe077254d6675a6ed9a58d464bb139c4c967ae492` |
| `shaders/low_poly.gdshader` | `640a7a9338e6dd0a6a0d0f477f53c05c2deed3dfda7dc02641b81673b448e399` |

本机证据逻辑位置及摘要哈希如下。原始日志、截图和测试脚本仅属于 LOCAL_ARTIFACT，没有写入仓库，也没有依赖合作者无法访问的绝对路径：

| 编号 | 材料名 | SHA-256 |
| --- | --- | --- |
| H001 | `clouds/round-5-ray-clipping/validation-summary.json` | `0a7db4d6f4c6e708dea4594d4bd76280308f1dfeedae40fe227289c74818b96f` |
| H002 | `clouds/round-6-performance/validation-summary.json` | `b69b52c7b9e0c27c6e0989bc6e5267d2a9602bb4dcdf5001bd340fb895fc1044` |
| H003 | `clouds/round-7-altitudes/validation-summary.json` | `99f927434b09f183b2494406ea18c17d6d94eaeda0d7de9ae53956aaac0dc061` |
| H004 | `clouds/round-8-cloud-cadence/validation-summary.json` | `8dd677aba904479ae42a655613e6cd9b6ceae57ef70c1f97474d69a2f86adaf6` |
| H005 | `clouds/round-9-cleanup/validation-summary.json` | `0103fb0e9558891801338f77833e3650a6491b6ee0784874e996e6c9f8f707ed` |

H003 的近／远景 GPU 总耗时约 8.9/11.6 ms，属于旧 60 FPS 上限条件。H004/H005 是恢复主支设置后的运行记录，不能把两类条件下的数字直接等同。间歇视口的 GPU 查询可能保留旧值，早期 benchmark.json 的更新计数与派生总耗时不采信；更新频率另由云时钟变化、实际绘制次数和图像变化确认。

完整项目一致性限制：历史预览使用的 Piece.cs 清理仍是本地排除项，程序集包含它。当前只将与实际源码和条件一致的云模块检查列为实施阶段结果，不将其扩写成纯提交版、所有玩法、其他设备或合作者分支已回归通过。

<a id="collaboration"></a>
## 合作接入检查与范围限制

| 检查项 | 已确认事实 | 后续接入要求 |
| --- | --- | --- |
| 玩法与数据 | 已提交净差异没有 .cs、.csproj、.sln、Map/、world/UI/ 或 template/ 内的变化。 | 合并后仍按目标版本检查选中、移动、攻击提示和相机操作。 |
| 项目与依赖 | project.godot 与固定起点逐字节一致；渲染模式、SDK 与依赖未改。 | 若对方更换投影、渲染方式或模型高度，重新核对云显示假设。 |
| 共用材质 | 云影路径和云时钟绑定变化，其他地表公式保持原样。 | 核对共享 uniform、噪声和初始化时序，不能仅按文本无冲突判断。 |
| 世界缓存范围 | 仍固定，范围外直接计算；没有改成相机跟随。 | 保持 A002 的暂缓边界；C007 的屏幕视线缓存另行随相机更新。 |
| 动态参数 | 模板、接收材质和三份缓存存在初始化复制关系。 | 增加动态天气／太阳控制时同步全部消费者。 |
| 目标主支 | 推送前 fetch 后仍等于固定起点。 | 未来主支更新时保留固定起点，记录同步来源并重核兼容。 |
| 帧率条件 | 云主要计算约 20 Hz；缩放／旋转等会强制刷新云体。 | 实际显示帧率受原有 VSync、硬件及场景影响，不视作全局帧率保证。 |

<a id="local-only"></a>
## 本地排除项与报告自身

| 项目 | 核对状态／归属 | 处理 |
| --- | --- | --- |
| template/Piece/Piece.cs | 本地既有修改，未暂存 | 单位销毁时释放未挂树技能节点，不归入云提交。 |
| template/tile/GrassWindNoise.tres | 本地既有删除，未暂存 | 已提交对象为零字节；本轮保留删除现场，不混入云提交。 |
| AGENTS.md | IGNORED | 正文未跟踪，继续只保留本地。 |
| docs/branch-reports/codex-realistic-clouds.md | REPORT_ONLY；UPDATED_NOT_USER_REVIEWED | 本次按 A009 与既有维护约定更新后单独提交；保留代码核对点 08515f0。 |

两项既有已跟踪工作区差异的 `git diff --binary` SHA-256 仍为 `471cb73f0d640b4fdb98b69085453faedb385690c6d54a89eedb5ceab8965a6e`，与初版报告时一致。本次提交／推送不包含这些差异。

<a id="decisions"></a>
## 待决定事项与报告更新

| 事项 | 当前状态 | 下一步 |
| --- | --- | --- |
| 初版报告审阅 | USER_REVIEWED，A006 | 保留历史审阅事实；本次扩展未被追认成已单独审阅。 |
| 当前版本提交／推送 | EXPLICITLY_APPROVED，A009 | 提交现有云版本并更新报告，不包含排除项或主分支合并。 |
| C004 全局帧率策略 | WITHDRAWN，A007 | 当前文件恢复起点，已退出候选，无需再决定保留旧 60 FPS 键。 |
| 合作者最新模块的兼容 | NOT_ASSESSED | 准备接入对应版本时核对共用文件及参数依赖。 |
| 纯提交版完整运行回归 | NOT_RUN_THIS_REVIEW | 云实施阶段已有本机隔离证据，完整纯提交版验收仍需另行明确版本与环境。 |

维护时机：仅在用户明确要求 commit／push 时，在执行前集中汇总实际内容并更新本报告；日常开发、调试、验证和交付不要求同步维护。用户另行要求查看、生成或修订时按其指令处理。

维护时保持 C001～C008 编号及报告路径稳定，新增行为使用新编号，撤回项保留历史并退出候选。reviewed_commit 只承诺已核对代码；后续仅改本报告的 REPORT_ONLY 提交可保留此值，出现其他路径变化时在下一次维护中补充核对。不得把旧运行、旧授权或旧主分支观察值无条件沿用。
