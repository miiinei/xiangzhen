---
schema_version: 1
report_status: UPDATED_NOT_USER_REVIEWED
report_reviewed_on: null
report_approval_ref: null
prior_report_approval_ref: A006
branch: codex/realistic-clouds
base_ref: master
base_commit: d6b1545b527bd8165b8a6c1e8b4817ec695828bc
reviewed_commit: e9dafc8001d52878151435bb21b6a749fcdb29c8
target_ref: origin/master
target_observed_commit: d6b1545b527bd8165b8a6c1e8b4817ec695828bc
remote_branch_observed_commit: 8696b8b6e94a85e84835790db9013556436e546e
remote_observation_phase: PRE_PUSH
checked_on: 2026-10-20
timezone: Asia/Shanghai
comparison: base_commit..reviewed_commit
scope: COMMITTED_NET_DIFF_EXCLUDING_REPORT
changed_file_count: 34
insertions: 1275
deletions: 184
post_base_commit_count: 12
post_base_merge_commit_count: 0
upstream_sync_status: NO_POST_BASE_MAINLINE_UPDATE_OBSERVED
runtime_validation_this_review: NOT_RUN
merge_decision: NOT_AUTHORIZED_BY_THIS_REPORT
---

# 云与草原开发分支变更报告

本报告沿用固定起点，保留 A006 对初版报告的审阅事实。本次按 A014 保存并推送第二版草原原型，新增 C011～C014；本次扩展记录为 `UPDATED_NOT_USER_REVIEWED`，不要求重复确认已授权的提交／推送。`reviewed_commit` 已核对到本次草原代码提交 `e9dafc8`；统计覆盖固定起点到该核对点的 34 个非报告文件，完整文件索引另含报告自身，合计 35 个路径。工作区原有工具链差异见本地排除项。用户仍指出地势、植被层次不足和草浪塑料感，当前推送用于保存可运行原型，不表示美术效果验收完成。远端指针是本次推送前读取值。

导航：[字段约定](#schema) · [模块索引](#modules) · [完整文件索引](#files) · [额外授权索引](#permissions) · [引入依赖](#integration) · [变更条目](#changes) · [提交索引](#commits) · [授权证据](#approvals) · [验证证据](#validation) · [协作检查](#collaboration) · [本地排除项](#local-only) · [待决定事项](#decisions)

读取顺序：先核对头部提交，再按模块或文件定位 `Cxxx`；准备引入时同时读取依赖组、授权和验证记录。报告的候选引入分类不替代用户批准或合并后的验证。

<a id="schema"></a>
## 字段约定

| 字段 | 允许值及含义 |
| --- | --- |
| `report_status` / `report_approval_ref` | `USER_REVIEWED` 表示对应版本已获用户审阅；`UPDATED_NOT_USER_REVIEWED` 表示本次更新尚未单独审阅。A006 保留为初版审阅记录，A009 授权上一轮云版本推送，A010 授权两项质量修复提交／推送；A011～A013 授权草原实现、修订清理与生图资产，A014 授权本次草原版本提交／推送。各项不代表合并或完整运行／美术验收。 |
| `status` | `ACTIVE` 为当前有效变化，省略时按此值处理；`WITHDRAWN` 为已撤回、退出当前引入候选的历史条目，编号不复用。 |
| `impact_type` | `MODULE_LOCAL` 模块内部；`SHARED_INTEGRATION` 共用接线；`PROJECT_GLOBAL` 项目级运行行为；`DOCUMENTATION` 文档与来源记录；`DEVELOPMENT_WORKFLOW` 开发工具的项目约定。 |
| `requires_extra_approval` | `true` 表示按当前本地约定需要额外明确批准；`false` 表示本条位于已授权任务范围内。不能由此判断验证是否通过。 |
| `approval_status` | `IN_SCOPE` 已有任务范围覆盖；`EXPLICITLY_APPROVED` 找到对应明确要求；`UNCONFIRMED` 额外授权证据尚未确认，不等同于断言从未批准。 |
| `integration_mode` | `INDEPENDENT` 可独立审阅／选择引入的候选；`WITH_DEPENDENCIES` 应按指定依赖组引入；`REVIEW_REQUIRED` 需要单独决定。均不表示已批准合并。 |
| `validation_status` | `NOT_RUN` 本次未验证；`STATIC_CHECKED` 本次核对源码、差异或 Git 行为；`RUNTIME_CHECKED` 对明确版本实际运行验证。云模块保留 H001～H005，质量修复保留 V005/H006；草原实现阶段运行记录为 H007，本次推送准备仅做 V006 静态核对，没有重跑游戏。 |
| `origin` | `THIS_BRANCH` 本开发分支引入；`UPSTREAM_SYNC` 主分支同步；`LOCAL_ONLY` 工作区独有。当前已提交条目均为 `THIS_BRANCH`。 |
| `approval_refs` / `validation_refs` | 指向本报告的 `Axxx` / `Vxxx` / `Hxxx` 证据编号；缺失的原始证据不得用助手结论或提交说明替代。 |
| `depends_on` / `integration_group` | 条目依赖与配套引入组；组内文件不按单文件复制作为默认接入方式。 |

补充规则：路径相对仓库根目录；文内源码链接使用 `../../` 回到根目录。稳定定位采用函数名、配置键、资源属性或节点路径。完整文件索引中的 `A/M/D` 分别表示新增／修改／删除，增删行数来自固定比较范围。

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
| 技能节点生命周期修复 | [C009](#c009) | `Piece._Notification(NotificationPredelete)` |
| 删除无引用空资源 | [C010](#c010) | 删除零字节 `template/tile/GrassWindNoise.tres` |
| 草原专用材质与风动 | [C011](#c011) | grassland.fragment()/light()、Tile.GrassColor、StylizedMaterials.Create() |
| 草原云影与预览接线 | [C012](#c012) | CloudWeather 模板／接收绑定、StylePreview.Ground |
| 停用草风清理 | [C013](#c013) | low_poly 旧草风及工厂旧参数赋值删除 |
| 生图草冠素材 | [C014](#c014) | grassland_canopy.png、导入参数和生成提示词 |

<a id="files"></a>
## 完整文件索引

| 状态 | 文件 | 新增／删除 | 条目 | 稳定定位点 |
| --- | --- | --- | --- | --- |
| M | [.gitignore](../../.gitignore) | 3 / 0 | [C006](#c006) | 本地规则文件的忽略条目 |
| A | [shaders/CLOUDS_LICENSES.md](../../shaders/CLOUDS_LICENSES.md) | 115 / 0 | [C005](#c005) | 云参考来源、固定版本与许可 |
| A | [shaders/CloudWeather.gd](../../shaders/CloudWeather.gd) | 218 / 0 | [C002](#c002)、[C007](#c007)、[C008](#c008)、[C012](#c012) | _enter_tree()、缓存创建、_ready() 草地接收绑定 |
| A | [shaders/CloudWeather.gd.uid](../../shaders/CloudWeather.gd.uid) | 1 / 0 | [C002](#c002) | Godot 脚本 UID |
| M | [shaders/README.md](../../shaders/README.md) | 116 / 86 | [C005](#c005)、[C011](#c011)、[C014](#c014) | 云与草原参数、接入和成本说明 |
| M | [shaders/StylePreview.tscn](../../shaders/StylePreview.tscn) | 21 / 16 | [C003](#c003)、[C012](#c012) | Ground.material_override、CloudWeather |
| A | [shaders/cloud_base_noise.tres](../../shaders/cloud_base_noise.tres) | 14 / 0 | [C001](#c001) | 96³ Perlin 主体噪声 |
| M | [shaders/cloud_field.gdshaderinc](../../shaders/cloud_field.gdshaderinc) | 266 / 33 | [C001](#c001)、[C007](#c007) | cloud_band_density()、cloud_time()、共享风向 |
| M | [shaders/cloud_layer.gdshader](../../shaders/cloud_layer.gdshader) | 130 / 12 | [C001](#c001)、[C007](#c007)、[C008](#c008) | 实时深度、视线缓存投放与显示参数 |
| M | [shaders/cloud_layer.tres](../../shaders/cloud_layer.tres) | 8 / 2 | [C001](#c001) | 云噪声绑定与透明优先级 |
| M | [shaders/cloud_noise.tres](../../shaders/cloud_noise.tres) | 2 / 2 | [C001](#c001) | 水平天气噪声 |
| A | [shaders/cloud_optics.gdshaderinc](../../shaders/cloud_optics.gdshaderinc) | 81 / 0 | [C001](#c001)、[C007](#c007)、[C008](#c008) | 共用视线积分与调度 |
| A | [shaders/cloud_optics.gdshaderinc.uid](../../shaders/cloud_optics.gdshaderinc.uid) | 1 / 0 | [C007](#c007) | Godot include UID |
| A | [shaders/cloud_sunlight.gdshader](../../shaders/cloud_sunlight.gdshader) | 16 / 0 | [C002](#c002) | 太阳透射缓存 RGB 通道 |
| A | [shaders/cloud_sunlight.gdshader.uid](../../shaders/cloud_sunlight.gdshader.uid) | 1 / 0 | [C002](#c002) | Godot shader UID |
| A | [shaders/cloud_view.gdshader](../../shaders/cloud_view.gdshader) | 29 / 0 | [C007](#c007) | 两层预乘颜色与透明度图集 |
| A | [shaders/cloud_view.gdshader.uid](../../shaders/cloud_view.gdshader.uid) | 1 / 0 | [C007](#c007) | Godot shader UID |
| A | [shaders/cloud_volume_noise.tres](../../shaders/cloud_volume_noise.tres) | 17 / 0 | [C001](#c001) | 96³ cellular 云泡与侵蚀噪声 |
| A | [shaders/cloud_weather.gdshader](../../shaders/cloud_weather.gdshader) | 15 / 0 | [C002](#c002) | 上下层覆盖与云丝 RG/BA 通道 |
| A | [shaders/cloud_weather.gdshader.uid](../../shaders/cloud_weather.gdshader.uid) | 1 / 0 | [C002](#c002) | Godot shader UID |
| A | [shaders/grass_land_noise.tres](../../shaders/grass_land_noise.tres) | 14 / 0 | [C011](#c011) | 256² 单倍频 Perlin 阵风形状 |
| A | [shaders/grassland.gdshader](../../shaders/grassland.gdshader) | 98 / 0 | [C011](#c011) | fragment() 草冠采样与阵风、light() 连续受光和宽高光 |
| A | [shaders/grassland.gdshader.uid](../../shaders/grassland.gdshader.uid) | 1 / 0 | [C011](#c011) | Godot shader UID |
| A | [shaders/grassland.tres](../../shaders/grassland.tres) | 16 / 0 | [C011](#c011)、[C014](#c014) | 草冠、阵风和云噪声资源绑定 |
| M | [shaders/low_poly.gdshader](../../shaders/low_poly.gdshader) | 3 / 21 | [C003](#c003)、[C013](#c013) | 云影接收；删除停用的旧草风参数与计算 |
| M | [shaders/low_poly.tres](../../shaders/low_poly.tres) | 7 / 2 | [C003](#c003) | 云场三维噪声绑定 |
| A | [shaders/textures/grassland_canopy.md](../../shaders/textures/grassland_canopy.md) | 11 / 0 | [C014](#c014) | 生成来源、完整提示词、采样尺度与限制 |
| A | [shaders/textures/grassland_canopy.png](../../shaders/textures/grassland_canopy.png) | 二进制，3,015,254 字节 | [C014](#c014) | 1254² RGB 生图草冠源图 |
| A | [shaders/textures/grassland_canopy.png.import](../../shaders/textures/grassland_canopy.png.import) | 40 / 0 | [C014](#c014) | 1024 尺寸上限、mipmap 与资产 UID |
| M | [template/Piece/Piece.cs](../../template/Piece/Piece.cs) | 14 / 0 | [C009](#c009) | Piece._Notification(NotificationPredelete) |
| D | `template/tile/GrassWindNoise.tres` | 0 / 0 | [C010](#c010) | 已删除的无引用零字节资源 |
| M | [template/tile/Tile.cs](../../template/tile/Tile.cs) | 2 / 1 | [C011](#c011) | GrassColor 改为草冠图的乘法调色 |
| M | [world/StylizedMaterials.cs](../../world/StylizedMaterials.cs) | 3 / 4 | [C011](#c011)、[C013](#c013) | Create() 选择草地模板，移除旧草风赋值 |
| M | [world/world.tscn](../../world/world.tscn) | 10 / 5 | [C003](#c003) | Camera3D/CloudLayer 与 CloudWeather 节点 |
| M | [docs/branch-reports/codex-realistic-clouds.md](codex-realistic-clouds.md) | 不计入代码统计 | `REPORT_ONLY` | 固定分支报告，单独文档提交 |

代码统计：34 个非报告文件，文本 1275 行新增、184 行删除，另有 1 个二进制素材；报告自身单列。`project.godot` 继续与固定起点逐字节一致；`.gitignore` 与 `.csproj` 的工作区原有差异不在上述提交统计内。

<a id="permissions"></a>
## 额外授权索引

| 分类 | 条目 | 状态依据 |
| --- | --- | --- |
| 已明确要求撤回的全局设置 | [C004](#c004) | `PROJECT_GLOBAL`；A007 明确要求恢复开发分支起点的主支设置，当前已撤回原 60 FPS 键。 |
| 任务范围内的云实现、独立更新与整理 | [C001](#c001)、[C002](#c002)、[C007](#c007)、[C008](#c008) | A001、A002 确定云效果和缓存范围；A007 允许云单独降频，A008 要求在不影响性能的条件下清理。 |
| 任务范围内的必要共用接线 | [C003](#c003) | 只涉及云影与云时钟接收，草风仍使用原生 TIME，未扩大为地表风格或玩法重做。 |
| 配套文档 | [C005](#c005) | 参数、来源和当前实现说明，随云功能配套更新。 |
| 本地约定的对应明确要求 | [C006](#c006) | A004 授权忽略规则单独提交；AGENTS.md 正文继续只保留在本地。 |
| 新增的明确改进范围 | [C009](#c009)、[C010](#c010) | A010 在两项差异已解释后，授权将有实际改进作用的内容推送；两项均保留，单独作为质量修复提交。 |
| 草原模块与必要共用接线 | [C011](#c011)、[C012](#c012) | A011 授权实施，A012 授权修订；只改变 Grass 的材质及其天气接收，不改变项目设置、战斗和地图格式。 |
| 旧草风清理 | [C013](#c013) | A012 明确要求删除无用旧版代码；保留其他对象的分档光照与共享云风方向。 |
| 生图素材 | [C014](#c014) | A013 明确提出用生图制作美术资产，纳入项目并保留完整提示词。 |
| 本次草原提交／推送 | C011～C014 | A014 为明确 Git 授权，不表示新增地形功能、主分支合并或最终美术验收。 |
| 本轮提交／推送 | 质量修复及报告更新 | A010；不包含 AGENTS.md 正文、其他新模块或主分支合并。 |

原 C004 添加时的事前独立批准仍记为未确认；本次明确撤回不追溯改写该历史。

<a id="integration"></a>
## 引入依赖索引

| 组／候选 | 条目 | 引入边界 |
| --- | --- | --- |
| `G001` 云功能配套组 | C001、C002、C003、C005、C007、C008 | 云场、噪声、控制器、三份缓存、共用积分、接收材质、场景和说明配套引入；新的云层 shader 会读取视线缓存，不能只复制单个文件。 |
| 独立候选 | C006 | `ab989dd` 仅修改 `.gitignore`，按目标项目的本地规则约定审阅。 |
| 独立质量修复 | C009、C010 | 技能释放和空资源删除不依赖云功能组；本轮在同一独立修复提交中提供，可按对应差异审阅。 |
| `G002` 草原配套组 | C011、C012、C013、C014、C005 | 草原 shader／材质／纹理、材质工厂、Grass 调色、天气接收和预览配套；依赖 G001 的云场 include 与控制器。旧草风清理与模板切换一并引入，不能只复制工厂或只删 shader 参数。 |
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
| `CloudWeather._enter_tree()` | 本局 weather_origin；weather_seed=0 随机，非零可复现，使用独立 RNG。 | 云、low_poly 与草原模板，以及工厂后续创建的材质副本；草原新增绑定见 C012。 |
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
| `grass_wind_direction` | 在 `low_poly` 内定义。 | 移到共享 include；名字与默认值 `(0.92,0.38)` 不变。云接线阶段未修改草风公式；当前 Grass 风动已由 C011/C013 替换。 |
| `world/world.tscn` | 根节点下的 `CloudLayer`；100×100 PlaneMesh，位置 `(16,7,14)`。 | 改为 `Camera3D/CloudLayer`；2×2 QuadMesh，相机局部位置 `(0,0,-1)`；新增根节点 `CloudWeather`。世界云高由 C001 的参数决定。 |
| `StylePreview.tscn` | 根节点下的平面云层，缺少新控制器与三维噪声绑定。 | 同步相机子节点云层、控制器与内联材质资源。 |

云影接线本身没有改写消费端 C# 业务流程；本轮独立的技能释放修复另见 C009。已有消费路径：`template/tile/Tile.cs` 与 `template/Piece/Piece.cs` 使用 `world/StylizedMaterials.cs::Create()`。云接线阶段统一复制 `low_poly.tres`，未改写颜色、草风、雾或分档光照；当前 Grass 的专用模板选择和调色另由 A011～A013 授权，见 C011～C014，不追溯扩大原云任务范围。

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
source_commits: [5731b0a, 799da11, 7d3d951, 08515f0, e9dafc8001d52878151435bb21b6a749fcdb29c8]
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

<a id="c009"></a>
### C009 — 补上未挂树技能节点的释放

```yaml
module: piece_skill_lifecycle
origin: THIS_BRANCH
impact_type: SHARED_INTEGRATION
requires_extra_approval: true
approval_status: EXPLICITLY_APPROVED
approval_refs: [A010]
source_commits: [2cbfa39]
integration_mode: INDEPENDENT
integration_group: null
depends_on: []
validation_status: RUNTIME_CHECKED
validation_refs: [V005, H006]
```

此前 Piece.EnsureSkills() 会实例化尚未挂树的 Skill，面板稍后才将其放入 UI；销毁棋子时，这些脱离场景树的节点没有清理。当前 Piece._Notification() 仅在 NotificationPredelete 时，释放仍有效且 GetParent()==null 的技能。已挂在 UI 等父节点下的技能不在此处主动释放。

修改共 14 行，位于共用棋子基类，作用于技能的生命周期；不改变技能效果、移动、攻击或数值规则。V005 在当时的 2cbfa39 源码副本中复核未选中退出、面板切换后退出、单位死亡，三种情况各检查 14 个技能节点，均通过，未发现清理其他存活棋子技能或退出残留节点的问题。

<a id="c010"></a>
### C010 — 删除无引用的零字节资源

```yaml
module: project_resource_cleanup
origin: THIS_BRANCH
impact_type: MODULE_LOCAL
requires_extra_approval: false
approval_status: EXPLICITLY_APPROVED
approval_refs: [A010]
source_commits: [2cbfa39]
integration_mode: INDEPENDENT
integration_group: null
depends_on: []
validation_status: RUNTIME_CHECKED
validation_refs: [V005, H006]
```

删除 template/tile/GrassWindNoise.tres：旧 Git 对象大小为 0 字节，没有有效资源内容，当前项目无场景、材质或代码引用。删除空资源时草风由 low_poly.gdshader 直接计算；之后 C011/C013 将 Grass 风动迁入专用材质。空 .tres 曾在编辑器扫描时触发解析错误；本次删除后重新导入通过。没有修改草风着色器或参数。

该文件已不存在，因此净差异索引用删除状态和仓库相对路径记录；旧内容可从父提交查询。它是独立的资源加载修复，不作为新增天气噪声或素材变更处理。

<a id="c011"></a>
### C011 — 草原专用材质、受光与阵风

```yaml
module: grassland_material_and_wind
origin: THIS_BRANCH
impact_type: SHARED_INTEGRATION
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A011, A012, A013]
source_commits: [e9dafc8001d52878151435bb21b6a749fcdb29c8]
integration_mode: WITH_DEPENDENCIES
integration_group: G002
depends_on: [C001, C002, C012, C013, C014]
validation_status: RUNTIME_CHECKED
validation_refs: [V006, H007]
```

原 Grass 与山地、水面、单位共用 low_poly 分档光照和颜色风带。当前 StylizedMaterials.Create() 仅在 grass=true 时复制 grassland.tres；其他对象继续使用原模板。Tile.GrassColor 从固有草色 (0.48,0.68,0.28) 改为贴图乘法调色 (0.86,0.96,1)，选中补光参数保留。

grassland.fragment() 使用世界 xz 坐标：草冠图两次采样分别提供大片平缓草色和弱笔触，grass_land_noise.tres 的单次采样控制阵风形状。该噪声为 256² 单倍频 Perlin，不决定地表植被分布。light() 使用连续漫反射和简化宽高光。草冠方向、等效叶片受光与高光随 TIME 更新，固有色贴图保持固定，云影继续衰减直接光。

当前阵风参数：强度 0.75，视觉传播速度 0.085 km/s，间距 4.8 km，基础恢复宽度 0.85 km；宽高光强度 0.075。速度用于战术镜头表现，不是实测风速。没有新增地形高度、草叶几何、碰撞、地图格式或玩法规则。已知不足：连续亮带仍显塑料，真实地势起伏和植被疏密尚未实现；17 项工程检查不代表美术验收。

<a id="c012"></a>
### C012 — 草原天气接收与美术预览

```yaml
module: grassland_weather_and_preview
origin: THIS_BRANCH
impact_type: SHARED_INTEGRATION
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A011, A012, A013]
source_commits: [e9dafc8001d52878151435bb21b6a749fcdb29c8]
integration_mode: WITH_DEPENDENCIES
integration_group: G002
depends_on: [C002, C007, C011]
validation_status: RUNTIME_CHECKED
validation_refs: [V006, H007]
```

CloudWeather._enter_tree() 的模板列表新增 grassland.tres，在 C# 创建地块材质副本前绑定天气原点、云时钟和两份世界缓存；_ready() 识别预览中的草地 shader，绑定相同天气资源。StylePreview.Ground 从旧内联草材质改为草原模板，单位继续使用 low_poly。云算法、缓存范围、20 Hz 调度和主场景节点均不因此改变。

H007 检查地图编辑后 Grass 材质重建及缓存／时钟绑定，独立预览绑定亦通过。没有修改 World.cs、项目启动场景、Autoload 或全局运行参数。

<a id="c013"></a>
### C013 — 删除停用的旧草风

```yaml
module: legacy_grass_wind_cleanup
origin: THIS_BRANCH
impact_type: SHARED_INTEGRATION
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A012]
source_commits: [e9dafc8001d52878151435bb21b6a749fcdb29c8]
integration_mode: WITH_DEPENDENCIES
integration_group: G002
depends_on: [C011, C012]
validation_status: RUNTIME_CHECKED
validation_refs: [V006, H007]
```

low_poly.gdshader 删除旧草风的强度、速度、间距、宽度四个 uniform 及颜色风带计算，共 14 行。工厂同步移除旧草风常量和 SetShaderParameter() 赋值。旧共享材质的山地、水面、单位风强度原为 0，相关分档光照、颜色、云影和雾保持原样；共享 include 的 grass_wind_direction 仍供云与新草原使用。

第一版草原工作区中未提交的 grass_normal_noise.tres 已删除，其专用采样、旧草和湿土混色均被替换，没有为已废弃的试验保留分支。该资源从未进入本分支 Git 历史，因此不计为起点净差异中的删除文件。

<a id="c014"></a>
### C014 — 生图草冠资产与导入设置

```yaml
module: grassland_painted_assets
origin: THIS_BRANCH
impact_type: MODULE_LOCAL
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A013]
source_commits: [e9dafc8001d52878151435bb21b6a749fcdb29c8]
integration_mode: WITH_DEPENDENCIES
integration_group: G002
depends_on: [C011]
validation_status: RUNTIME_CHECKED
validation_refs: [V006, H007]
```

新增 grassland_canopy.png、.png.import 和 .md 素材说明。内置 imagegen 根据用户认可的草原概念图制作 1254² RGB 源图；PNG 保留生成结果，提示词与限制随仓库共享。导入时限制到 1024 并生成 mipmap，不引入插件或新的工具链依赖。

64 km 跨度的第 4 级 mip 提供缓慢色块，同图的较小尺度镜像采样提供弱笔触明暗。当前棋盘与 40×40 km 预览平面的大色块坐标处于单张图范围内；素材未认证为严格无缝图。贴图不含可交互地形／水文数据，草浪不滚动该图，也不能据此推断已经实现概念图的地形体积。

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
| `6615014bc73055978033dd846223232ac9711c52` | docs: update cloud branch report for independent cadence | REPORT_ONLY；云更新和整理的报告 |
| `2cbfa3975b6de1f296b2a6ebcbca2d8bedc136f4` | fix: release detached skills and remove empty resource | C009、C010；独立质量修复 |
| `8696b8b6e94a85e84835790db9013556436e546e` | docs: record skill lifecycle and resource fixes | REPORT_ONLY；上一轮质量修复报告 |
| `e9dafc8001d52878151435bb21b6a749fcdb29c8` | feat: add painted wet-season grassland prototype | C011～C014、C005；草原代码与素材 |

固定起点仍为 `d6b1545b527bd8165b8a6c1e8b4817ec695828bc`。代码核对点范围内共 12 次提交、0 次合并；推送前远端开发分支为 `8696b8b6e94a85e84835790db9013556436e546e`，目标 master 经 ls-remote 核对仍等于固定起点。当前没有合并或重写历史；本报告自身的后续 REPORT_ONLY 提交不要求记录未知的自身哈希。

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
**A009 — 上一轮云版本提交／推送。** 来源：同一云任务，2026-10-20 当时的用户消息，原话：“将这一版推送吧”。按上一轮交付的范围，将项目设置恢复、云层独立更新及代码清理提交到现有开发分支，并按本地约定集中更新本报告；不包含 Piece.cs、空资源删除、AGENTS.md 正文或主分支合并。

<a id="a010"></a>
**A010 — 对剩余两项差异按改进价值处理并推送。** 来源：同一任务，2026-10-20 最新用户消息。用户在已经看到 Piece.cs 技能释放与空资源删除的具体说明后，要求：“如果不是业务代码那就撤销，如果是改进那就推送”。本轮核对两项均有实际修复作用，保留原差异并单独提交／推送，未增加其他实现或项目设置变化。

<a id="a011"></a>
**A011 — 草原 demo 实施。** 来源：当前草原任务，2026-10-20 本地用户消息。原话：“这个概念图效果还不错，你可以先按照这个方案先做了试试，注意性能优化，这毕竟只是一个demo”。覆盖草原材质、风动、资源和必要的共用接线，不授权项目全局设置或玩法变化。

<a id="a012"></a>
**A012 — 第二版修订与旧代码清理。** 来源：同一任务。原话：“按照你的思路再做一版，旧版的代码如果没用就删了”。同时要求雨季草原青翠，指出第一版灰暗、脏和草浪不明显；覆盖材质修订和确认停用的旧草风清理。

<a id="a013"></a>
**A013 — 以概念图画感为方向使用生图美术资产。** 来源：同一任务。用户认可概念图的油画感、清晰色彩与光影，并说：“而且不一定要随机生成啊，可以考虑通过生图功能制作美术资产啊”。覆盖本次生成并接入草冠 PNG，不等同于修改地形高度、渲染模式或新增第三方插件。

<a id="a014"></a>
**A014 — 保存并推送当前草原版本。** 来源：同一任务，2026-10-20 最新用户消息。原话：“先把这一版push了”。用户同时指出细节、地势起伏、植被疏密仍不足，草浪太塑料，并要求讨论改进看法。授权当前版本的必要提交、报告更新与推送，不把推送理解为美术验收或继续实现新地形的授权。

<a id="validation"></a>
## 验证与证据索引

| 编号 | 类型与可访问范围 | 实际核对内容 | 结论边界 |
| --- | --- | --- | --- |
| V001 | CURRENT_STATIC；仓库可复核 | 固定基准至本次代码核对点的 34 个非报告文件、文本 1275/184 行、12 次提交，另有 1 个二进制素材；完整路径另含报告自身。 | 确认范围、接线和统计，不等于完整运行验收。 |
| V002 | HISTORICAL_STATIC + LOCAL_ARTIFACT | 初版报告核对过的六份旧核心源码及当时项目配置，旧哈希保留如下。 | 属于 7d3d951 版本，不再声称与当前已改源码逐字节相同。 |
| V003 | CURRENT_GIT_CHECK | AGENTS.md 命中根忽略规则，未被跟踪；ab989dd 仅改 .gitignore。 | 忽略和提交范围，不包含游戏行为验证。 |
| V004 | HISTORICAL_STATIC + LOCAL_ARTIFACT | 待提交源码与 H004/H005 的对应记录核对；project.godot 与起点逐字节相同。最终 cloud_optics 仅移除记录版本末尾一个多余空行，去除末尾空白后文本相同。H005 所查积分和合成函数内容、参数名／类型／默认值保持一致。 | 这是上一轮云版本推送准备的源码核对；当时没有重新运行游戏。本轮 C009/C010 的新验证另见 V005。 |
| H001 | HISTORICAL_RUNTIME_RECORD；LOCAL_ARTIFACT | 截断修复：7 个平移位置、近远景、4:3 与超宽比例。 | 原始材料仅本机可访问。 |
| H002 | HISTORICAL_RUNTIME_RECORD；LOCAL_ARTIFACT | 高度调整前的云量缓存性能、动态演化及缓存边界检查。 | 不作为当前最终版本耗时。 |
| H003 | HISTORICAL_RUNTIME_RECORD；LOCAL_ARTIFACT | 高度版本：最近景淡出、缩放不改物理云影、内部演化和层间遮光，4 次运行退出码 0。 | 当时项目默认 60 FPS；不是本次恢复设置后的结果。 |
| H004 | IMPLEMENTATION_RUNTIME_RECORD；LOCAL_ARTIFACT | 独立更新阶段 12 项检查通过，静止／平移云时钟约 20.3 Hz、场景约 165.4／164.5 FPS；缓存外、云内高物体和 resize 检查通过。 | 清理前版本，后续差异由 H005 复核；原始材料未随仓库共享。 |
| H005 | IMPLEMENTATION_RUNTIME_RECORD；LOCAL_ARTIFACT | 清理后 12 项检查通过；size 8/20/30/40 清理前后逐像素一致；静止／平移场景约 165.5／164.4 FPS，云约 20.3 Hz，云体纹理继续变化。1280×720 缓存与直接积分全图平均 RGB 差约 0.034/255。 | Godot 4.6.1 Mono、GL Compatibility、1920×1080、RTX 3060 Laptop；Engine.max_fps=0、原有 VSync 开启。隔离副本，复用已有 C# 程序集，含本地 Piece 修复，未证明纯提交版完整运行。 |
| V005 | HISTORICAL_RUNTIME + LOCAL_ARTIFACT | 隔离副本中 95 个生产文件与 `2cbfa39` 逐字节对应，另加独立测试 harness；构建 0 警告、0 错误，资源导入成功，实际 GL Compatibility 演示运行 120 帧成功，三类生命周期检查全部通过，每类检查 14 个技能节点。日志无 ERROR、WARNING、泄漏或 CHECK FAIL。 | Godot 4.6.1 Mono、.NET SDK 8.0.426；运行用 1920×1080，测试副本包含暖机后的生命周期场景。测试脚本不进入产品提交；不等同于全部玩法或其他设备验收。 |
| H006 | HISTORICAL_RUNTIME_RECORD；LOCAL_ARTIFACT | V005 对应的质量修复阶段构建、导入、演示及生命周期日志和源码快照。 | 原始文件只在本机，摘要与哈希如下；报告更新本身在代码验证之后完成。 |
| V006 | CURRENT_STATIC；仓库可复核 | 本次代码提交的 13 个草原文件与 H007 测试副本逐字节相同；原 PNG 与生图输出一致。核对固定起点、远端指针、净差异索引、依赖与本地排除项；git diff --check 通过。 | 本次 push 准备没有重跑游戏，不将上轮运行改写为本轮运行；未提交的 SDK 差异单列。 |
| H007 | IMPLEMENTATION_RUNTIME_RECORD；LOCAL_ARTIFACT | 草原第二版实施阶段构建 0 警告／0 错误，资源导入成功，17 项检查通过，运行日志无错误；三档视距 V1/V2 对照和草浪实录已生成。 | Godot 4.6.3 Mono、GL Compatibility、1920×1080、RTX 3060 Laptop，VSync=1、max_fps=0；测试副本使用本地 SDK 4.6.3，仓库声明仍为 4.6.1，未证明纯提交版在 4.6.1 工具链上运行通过。 |

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

| 路径 | `2cbfa39` 内容 SHA-256 |
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
| `template/Piece/Piece.cs` | `952a8bf99fb249e66cd68f7d80fb535e9b0f583b50aa4c37ddbcf9a49fde534c` |

本机证据逻辑位置及摘要哈希如下。原始日志、截图和测试脚本仅属于 LOCAL_ARTIFACT，没有写入仓库，也没有依赖合作者无法访问的绝对路径：

| 编号 | 材料名 | SHA-256 |
| --- | --- | --- |
| H001 | `clouds/round-5-ray-clipping/validation-summary.json` | `0a7db4d6f4c6e708dea4594d4bd76280308f1dfeedae40fe227289c74818b96f` |
| H002 | `clouds/round-6-performance/validation-summary.json` | `b69b52c7b9e0c27c6e0989bc6e5267d2a9602bb4dcdf5001bd340fb895fc1044` |
| H003 | `clouds/round-7-altitudes/validation-summary.json` | `99f927434b09f183b2494406ea18c17d6d94eaeda0d7de9ae53956aaac0dc061` |
| H004 | `clouds/round-8-cloud-cadence/validation-summary.json` | `8dd677aba904479ae42a655613e6cd9b6ceae57ef70c1f97474d69a2f86adaf6` |
| H005 | `clouds/round-9-cleanup/validation-summary.json` | `0103fb0e9558891801338f77833e3650a6491b6ee0784874e996e6c9f8f707ed` |
| H006 | `clouds/round-10-lifecycle-fixes/validation-summary.json` | `398558b7ea2295b03c339f3bb97ab4662925f4e32faa266270ddb910ad02d33d` |

H003 的近／远景 GPU 总耗时约 8.9/11.6 ms，属于旧 60 FPS 上限条件。H004/H005 是恢复主支设置后的运行记录，不能把两类条件下的数字直接等同。间歇视口的 GPU 查询可能保留旧值，早期 benchmark.json 的更新计数与派生总耗时不采信；更新频率另由云时钟变化、实际绘制次数和图像变化确认。

版本一致性：H004/H005 当时使用的 Piece.cs 修复在那次云推送中仍被排除，现由 A010 授权作为 C009 纳入分支。V005 的生产源码已核对到本轮代码提交，只有隔离测试脚本额外加入；旧记录仍保留原版本与条件，不反推全部玩法、其他设备或合作者分支已回归通过。

H007 工程检查包含：草／山材质选择、编辑重建后的天气绑定、342 个 Grass 材质数量、非草对象保持原 shader、两张草原纹理共享、草冠图确为项目资产且运行时 1024²／带 mipmap、旧草风及旧法线资源删除、固定云时钟时录制草浪、云保持约 20 Hz、独立预览材质和云缓存绑定。视觉实录只证明当前表现可供查看；用户已指出其塑料感和缺少地势层次，不作美术验收结论。

H007 同进程主视口 GPU 数据（缓存冻结，180 帧；不含云缓存耗时）：

| 正交 size | V1 中位数 ms | V2 中位数 ms | V2 P95 ms | V1/V2 绘制次数 |
| --- | --- | --- | --- | --- |
| 8 | 0.690 | 0.542 | 1.526 | 719 / 719 |
| 20 | 1.207 | 0.689 | 2.060 | 1539 / 1539 |
| 40 | 0.569 | 0.553 | 1.653 | 1617 / 1617 |

开启正常云更新后的场景约 165.5 FPS、云时钟 20.0 Hz。受当前 VSync 和 GPU 动态频率影响，不以中位数宣称稳定加速；V2 P95 高于 V1，保留实际记录。没有为了数据改变项目设置。当前草原每片元 3 次专用纹理采样，无新增草叶几何、视口或绘制次数。

H007 证据仅本机可访问，未将完整日志、隔离副本和动画放入仓库。材料名与 SHA-256：

| 材料 | SHA-256 |
| --- | --- |
| `grassland-v2/runtime-validation.json` | `448a332160d76673fe87dd5facc5651c7130f5eaa947da2385e0e80400f264a2` |
| `grassland-v2/final-source-check.json` | `81a8a9b0eb7efc3066d6ea76083d3bf67ad8c64ff270336fc91d6701637e50df` |
| `grassland-v2/painted-final-stdout.log` | `33f0e708c72505cd956ad3a58186e560e9f88d73b755fa4fb0bf049e690ea9ec` |
| `grassland-v2/painted-final-stderr.log` | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |

草原代码与素材的内容已通过 V006 对照 H007；仓库 .csproj 仍声明 Godot.NET.Sdk/4.6.1、net8.0。本机 4.6.3 声明与忽略规则是原有工作区改动，本次不提交，不能将其算入代码兼容或安装验证。

<a id="collaboration"></a>
## 合作接入检查与范围限制

| 检查项 | 已确认事实 | 后续接入要求 |
| --- | --- | --- |
| 玩法与数据 | 既有 C009/C010 修复保留；新增草原只改显示材质和资源，Map/、World.cs、碰撞、战斗与地图格式不改。 | 高度与单位贴地、描边适配尚未实现，不当作已有能力。 |
| 项目与依赖 | project.godot 与固定起点逐字节一致；提交中的 SDK 仍为 4.6.1。H007 用本地 4.6.3 环境，差异明确排除。 | 原声明工具链及其他设备需要对应验证，不默认继承本机结果。 |
| 共用材质 | Grass 改用专用模板；山、水、单位仍用 low_poly；旧草风由 C013 删除。 | 接入 G002 时连同工厂、接收绑定、资源和预览引入。 |
| 世界缓存范围 | 仍固定，范围外直接计算；没有相机跟随世界缓存。 | 保持 A002 暂缓边界，屏幕视线缓存仍由 C007 随相机变化。 |
| 动态参数 | 草原模板在初始化时接收同一云资源；后续动态参数仍需同步消费者。 | 不默认已有自动传播全部天气参数的接口。 |
| 目标主支 | 本次 ls-remote 核对 origin/master 仍为固定起点；未合并主支。 | 目标主支变化时重新核对兼容性。 |
| 画面与帧率 | 原生 TIME 草浪、云约 20 Hz，工程接线已检查；当前草浪塑料感和缺少起伏是用户已指出的问题。 | 美术效果尚待继续迭代，不能用 17 项检查替代视觉验收。 |

<a id="local-only"></a>
## 本地排除项与报告自身

| 项目 | 核对状态／归属 | 处理 |
| --- | --- | --- |
| AGENTS.md | IGNORED | 未跟踪，只保留本地，不提交。 |
| .gitignore 工作区差异 | LOCAL_ONLY | 原有两行本地工具链忽略规则，本次保留且不暂存；分支原有 AGENTS.md 忽略条目继续有效。 |
| 陷阵.csproj 工作区差异 | LOCAL_ONLY | 原有 Godot.NET.Sdk/4.6.1 → 4.6.3，本次不提交，不更改仓库声明。 |
| 陷阵.csproj.old / 陷阵.csproj.old.1 | IGNORED / UNTRACKED；LOCAL_ONLY | 原有工具链备份，保留，不清理、不提交。 |
| docs/branch-reports/codex-realistic-clouds.md | REPORT_ONLY；UPDATED_NOT_USER_REVIEWED | 按 A014 更新后单独提交，reviewed_commit 记录本次实际草原代码提交。 |

草原代码与资源净变化计入 C011～C014；第一版未提交的旧法线资源删除不伪装成历史删除项。实现测试脚本、日志、动画和隔离副本均保留在本机，不进入 Git。原有 .gitignore、.csproj、未跟踪备份和地图通过哈希确认未被本次修改覆盖。

<a id="decisions"></a>
## 待决定事项与报告更新

| 事项 | 当前状态 | 下一步 |
| --- | --- | --- |
| 初版报告审阅 | USER_REVIEWED，A006 | 保留历史事实；后续扩展不追认成已单独审阅。 |
| 当前草原提交／推送 | EXPLICITLY_APPROVED，A014 | 保存现有原型，代码与报告分开提交后推送当前开发分支，不合并主支。 |
| C004 全局帧率策略 | WITHDRAWN，A007 | project.godot 继续保持起点设置。 |
| 地势、植被疏密与草浪美术迭代 | UNCONFIRMED | 当前仅讨论后续方向，没有实现新地形或单位贴地接线的授权记录。 |
| 合作者最新模块的兼容 | UNCONFIRMED | 准备接入明确版本时核对共用文件及参数依赖。 |
| 当前版本验证 | RUNTIME_CHECKED，H007；本次 V006 为 STATIC_CHECKED | 保留 4.6.3 实现阶段运行与 4.6.1 提交声明的差异，不声称本次重跑或全玩法验收。 |

维护时机：仅在用户明确要求 commit／push 时，在执行前集中汇总实际内容并更新本报告；日常开发、调试、验证和交付不要求同步维护。用户另行要求查看、生成或修订时按其指令处理。

维护时保持 C001～C014 编号及报告路径稳定，新增行为使用新编号，撤回项保留历史并退出候选。reviewed_commit 只承诺已核对代码；后续仅改本报告的 REPORT_ONLY 提交可保留此值，出现其他路径变化时在下一次维护中补充核对。不得把旧运行、旧授权或旧主分支观察值无条件沿用。
