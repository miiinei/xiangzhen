---
schema_version: 1
report_status: UPDATED_NOT_USER_REVIEWED
report_reviewed_on: null
report_approval_ref: null
prior_report_approval_ref: A006
git_action_approval_ref: A017
branch: codex/realistic-clouds
base_ref: master
base_commit: d6b1545b527bd8165b8a6c1e8b4817ec695828bc
reviewed_commit: 85e1cb0db944c2f25c7cdf6e71b283b5c22eedf4
target_ref: origin/master
target_observed_commit: d6b1545b527bd8165b8a6c1e8b4817ec695828bc
remote_branch_observed_commit: 7f9b7eee71f235af7cc4be02853b2dccb24fbcd0
remote_observation_phase: PRE_PUSH
checked_on: 2026-10-20
timezone: Asia/Shanghai
comparison: base_commit..reviewed_commit
scope: COMMITTED_NET_DIFF_EXCLUDING_REPORT
changed_file_count: 74
insertions: 2414
deletions: 200
binary_file_count: 10
post_base_commit_count: 17
post_base_merge_commit_count: 0
upstream_sync_status: NO_POST_BASE_MAINLINE_UPDATE_OBSERVED
runtime_validation_this_review: NOT_RUN
merge_decision: NOT_AUTHORIZED_BY_THIS_REPORT
---

# 云、草原与机甲开发分支变更报告

本报告沿用固定起点，保留 A006 对初版报告的审阅事实。本次依据 A017 保存当前全部未提交的 30 个文件，分为战场／机甲实现提交和工具链提交，再单独提交报告。`reviewed_commit` 核对到 `85e1cb0`；起点累计净差异为 74 个非报告文件，文本新增 2414 行、删除 200 行，含 10 个二进制资产；完整索引连同报告自身共 75 个路径。本次修订 C011／C012／C014，并新增 C018～C022。报告状态仍为 `UPDATED_NOT_USER_REVIEWED`，本次 Git 授权不改写为对新报告全文的审阅。

主棋盘现已接入新的绘制地表和铁岳机甲；`Piece` 的共用移动从逐格 Tween 改为同步完成，朝向接入玩家技能及敌方准备动作；Godot.NET.Sdk 声明由 4.6.1 改为 4.6.3。C019～C022 单列共用行为与工具链影响及原方案授权核对状态。`project.godot` 和地图数据仍与起点一致，原全局 60 FPS 上限保持撤回。AGENTS.md 和已被忽略的本地文件继续排除；原先排除的 `.gitignore`、`.csproj` 及未跟踪备份 `.old.1` 本次按 A017 纳入。远端观察值记录推送前状态，报告自身的后续提交不要求预填未知哈希。

导航：[字段约定](#schema) · [模块索引](#modules) · [完整文件索引](#files) · [额外授权索引](#permissions) · [引入依赖](#integration) · [变更条目](#changes) · [提交索引](#commits) · [授权证据](#approvals) · [验证证据](#validation) · [协作检查](#collaboration) · [本地排除项](#local-only) · [待决定事项](#decisions)

读取顺序：先核对头部提交，再按模块或文件定位 `Cxxx`；准备引入时同时读取依赖组、授权和验证记录。报告的候选引入分类不替代用户批准或合并后的验证。

<a id="schema"></a>
## 字段约定

| 字段 | 允许值及含义 |
| --- | --- |
| `report_status` / `report_approval_ref` | `USER_REVIEWED` 表示对应版本已获用户审阅；`UPDATED_NOT_USER_REVIEWED` 表示新增报告内容尚未单独审阅。A006 保留为初版审阅记录，A007～A016 保留各阶段原授权；A017 为当前全部变更的保存、报告维护、提交及推送授权。各项不代表主分支合并或完整运行／美术验收。 |
| `git_action_approval_ref` / `git_action_approval_refs` | 对应用户对保存现有内容及 Git 操作的明确授权；与 `approval_status` 所记录的实施方案／额外权限证据分开。C019～C022 的 A017 只确认本次保存，未在本轮核对到其原始方案的独立批准。 |
| `status` | `ACTIVE` 为当前有效变化，省略时按此值处理；`WITHDRAWN` 为已撤回、退出当前引入候选的历史条目，编号不复用。 |
| `impact_type` | `MODULE_LOCAL` 模块内部；`SHARED_INTEGRATION` 共用接线；`PROJECT_GLOBAL` 项目级运行行为；`DOCUMENTATION` 文档与来源记录；`DEVELOPMENT_WORKFLOW` 开发工具的项目约定。 |
| `requires_extra_approval` | `true` 表示按当前本地约定需要额外明确批准；`false` 表示本条位于已授权任务范围内。不能由此判断验证是否通过。 |
| `approval_status` | `IN_SCOPE` 已有任务范围覆盖；`EXPLICITLY_APPROVED` 找到对应明确要求；`UNCONFIRMED` 额外授权证据尚未确认，不等同于断言从未批准。 |
| `integration_mode` | `INDEPENDENT` 可独立审阅／选择引入的候选；`WITH_DEPENDENCIES` 应按指定依赖组引入；`REVIEW_REQUIRED` 需要单独决定。均不表示已批准合并。 |
| `validation_status` | `NOT_RUN` 未验证；`STATIC_CHECKED` 核对源码、资源或 Git 行为；`RUNTIME_CHECKED` 对明确版本实际运行验证。旧记录 H001～H008 保留原版本；H009 仅为随仓库素材文档中的运行摘要，本轮未独立复跑。本次对新增／修订内容做 V008／V009 静态核对，没有重新构建或运行游戏。 |
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
| 主战场草原材质与风动 | [C011](#c011) | sample_landscape()、fragment()/light()、Tile.GrassColor、StylizedMaterials.Create() |
| 草原云影、外围地面与预览 | [C012](#c012) | CloudWeather 接收绑定、BattlefieldGround、StylePreview 根节点 |
| 停用草风清理 | [C013](#c013) | low_poly 旧草风及工厂旧参数赋值删除 |
| 生图草冠素材 | [C014](#c014) | grassland_canopy.png、导入参数和生成提示词 |
| 独立地形样区与烘焙 | [C015](#c015) | GrasslandStudy.tscn、StudyTerrain.height_at()/make_mesh()/bake_fields() |
| 样区草冠风动与受光 | [C016](#c016) | grassland_study.fragment()/light()、wind_time |
| 样区生图资产与配色布局 | [C017](#c017) | study_canopy.png、colour_layout.png、完整提示词与导入设置 |
| 战场绘制素材与制作约定 | [C018](#c018) | battlefield_meadow_view_v2.png、参考／中间图、battlefield_meadow.md |
| 铁岳机甲与默认单位替换 | [C019](#c019) | 铁岳.glb、MechPiece.tscn、铁岳.tscn、Enemy.tscn、World.PieceScene |
| 共用移动完成时机及朝向 | [C020](#c020) | Piece.MoveAlongPathAsync()/FaceTowards()、Skill.Execute()、Enemy 准备动作 |
| Godot.NET SDK 与原工程备份 | [C021](#c021) | 陷阵.csproj 的 Project.Sdk、陷阵.csproj.old.1 |
| 工程文件忽略规则 | [C022](#c022) | .gitignore 中的 陷阵.csproj／陷阵.csproj.old |

<a id="files"></a>
## 完整文件索引

| 状态 | 文件 | 新增／删除 | 条目 | 稳定定位点 |
| --- | --- | --- | --- | --- |
| M | [.gitignore](../../.gitignore) | 5 / 0 | [C006](#c006)、[C022](#c022) | /AGENTS.md；工程文件及精确旧备份名忽略规则 |
| A | [shaders/CLOUDS_LICENSES.md](../../shaders/CLOUDS_LICENSES.md) | 115 / 0 | [C005](#c005) | 云参考来源、固定版本与许可 |
| A | [shaders/CloudWeather.gd](../../shaders/CloudWeather.gd) | 218 / 0 | [C002](#c002)、[C007](#c007)、[C008](#c008)、[C012](#c012) | _enter_tree()、缓存创建、_ready() 草地接收绑定 |
| A | [shaders/CloudWeather.gd.uid](../../shaders/CloudWeather.gd.uid) | 1 / 0 | [C002](#c002) | Godot 脚本 UID |
| M | [shaders/README.md](../../shaders/README.md) | 119 / 86 | [C005](#c005)、[C011](#c011)、[C018](#c018) | 云参数；当前主战场地表与六次草地采样说明 |
| M | [shaders/StylePreview.tscn](../../shaders/StylePreview.tscn) | 22 / 16 | [C003](#c003)、[C012](#c012) | Ground.material_override、根节点 position |
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
| A | [shaders/grassland.gdshader](../../shaders/grassland.gdshader) | 136 / 0 | [C011](#c011) | sample_landscape()、fragment()/light()、grass_texture_origin/span |
| A | [shaders/grassland.gdshader.uid](../../shaders/grassland.gdshader.uid) | 1 / 0 | [C011](#c011) | Godot shader UID |
| A | [shaders/grassland.tres](../../shaders/grassland.tres) | 16 / 0 | [C011](#c011)、[C018](#c018) | grass_canopy_color 绑定 battlefield_meadow_view_v2.png |
| A | [shaders/grassland_study/BakeStudy.gd](../../shaders/grassland_study/BakeStudy.gd) | 21 / 0 | [C015](#c015) | _initialize() 离线烘焙数据图和配色布局 |
| A | [shaders/grassland_study/BakeStudy.gd.uid](../../shaders/grassland_study/BakeStudy.gd.uid) | 1 / 0 | [C015](#c015) | Godot 资源 UID；BakeStudy.gd |
| A | [shaders/grassland_study/GrasslandStudy.gd](../../shaders/grassland_study/GrasslandStudy.gd) | 94 / 0 | [C015](#c015) | _ready() 本地云绑定、_make_grid()、_place_camera()、输入与风时钟 |
| A | [shaders/grassland_study/GrasslandStudy.gd.uid](../../shaders/grassland_study/GrasslandStudy.gd.uid) | 1 / 0 | [C015](#c015) | Godot 资源 UID；GrasslandStudy.gd |
| A | [shaders/grassland_study/GrasslandStudy.tscn](../../shaders/grassland_study/GrasslandStudy.tscn) | 64 / 0 | [C015](#c015) | Ground、ScaleGrid、Camera3D、CloudWeather 与本地说明标签 |
| A | [shaders/grassland_study/README.md](../../shaders/grassland_study/README.md) | 61 / 0 | [C015](#c015)、[C016](#c016)、[C017](#c017)、[C018](#c018) | 独立样区边界；当前主战场入口说明 |
| A | [shaders/grassland_study/StudyTerrain.gd](../../shaders/grassland_study/StudyTerrain.gd) | 97 / 0 | [C015](#c015) | height_at()、vegetation_at()、normal_at()、make_mesh()、bake_fields() |
| A | [shaders/grassland_study/StudyTerrain.gd.uid](../../shaders/grassland_study/StudyTerrain.gd.uid) | 1 / 0 | [C015](#c015) | Godot 资源 UID；StudyTerrain.gd |
| A | [shaders/grassland_study/colour_layout.png](../../shaders/grassland_study/colour_layout.png) | 二进制，331,456 字节 | [C015](#c015)、[C017](#c017) | 768² 生图输入配色布局，不由运行材质加载 |
| A | [shaders/grassland_study/colour_layout.png.import](../../shaders/grassland_study/colour_layout.png.import) | 40 / 0 | [C015](#c015)、[C017](#c017) | 资源 UID 与无损导入；配色输入图导入 |
| A | [shaders/grassland_study/grassland_study.gdshader](../../shaders/grassland_study/grassland_study.gdshader) | 97 / 0 | [C016](#c016) | fragment() 固定地形法线／草冠分布、light() 两类草冠受光 |
| A | [shaders/grassland_study/grassland_study.gdshader.uid](../../shaders/grassland_study/grassland_study.gdshader.uid) | 1 / 0 | [C016](#c016) | Godot 资源 UID；grassland_study.gdshader |
| A | [shaders/grassland_study/grassland_study.tres](../../shaders/grassland_study/grassland_study.tres) | 25 / 0 | [C016](#c016) | 样区 shader、草冠、控制／法线图与原噪声绑定 |
| A | [shaders/grassland_study/study_canopy.md](../../shaders/grassland_study/study_canopy.md) | 15 / 0 | [C017](#c017) | 内置 imagegen 来源、两张输入角色、完整提示词和限制 |
| A | [shaders/grassland_study/study_canopy.png](../../shaders/grassland_study/study_canopy.png) | 二进制，3,325,236 字节 | [C017](#c017) | 1254² RGB 新草冠主色素材，保留生成输出 |
| A | [shaders/grassland_study/study_canopy.png.import](../../shaders/grassland_study/study_canopy.png.import) | 40 / 0 | [C017](#c017) | 资源 UID 与无损导入；1024 尺寸上限与 mipmap |
| A | [shaders/grassland_study/surface_normals.png](../../shaders/grassland_study/surface_normals.png) | 二进制，882,935 字节 | [C015](#c015) | 1024² RGB 世界空间 XYZ 地表／弱草冠法线 |
| A | [shaders/grassland_study/surface_normals.png.import](../../shaders/grassland_study/surface_normals.png.import) | 40 / 0 | [C015](#c015) | 资源 UID 与无损导入；线性数据、mipmap、关闭 alpha 边缘修正 |
| A | [shaders/grassland_study/vegetation.png](../../shaders/grassland_study/vegetation.png) | 二进制，3,237,665 字节 | [C015](#c015)、[C016](#c016) | 1024² RGBA 植被覆盖、草高、方向／响应差异与草丛覆盖 |
| A | [shaders/grassland_study/vegetation.png.import](../../shaders/grassland_study/vegetation.png.import) | 40 / 0 | [C015](#c015)、[C016](#c016) | 资源 UID 与无损导入；线性数据、mipmap、关闭 alpha 边缘修正 |
| M | [shaders/low_poly.gdshader](../../shaders/low_poly.gdshader) | 3 / 21 | [C003](#c003)、[C013](#c013) | 云影接收；删除停用的旧草风参数与计算 |
| M | [shaders/low_poly.tres](../../shaders/low_poly.tres) | 7 / 2 | [C003](#c003) | 云场三维噪声绑定 |
| A | [shaders/textures/battlefield_meadow.md](../../shaders/textures/battlefield_meadow.md) | 154 / 0 | [C018](#c018) | 素材来源、完整提示词、空间映射、运行摘要与后续关卡约定 |
| A | [shaders/textures/battlefield_meadow.png](../../shaders/textures/battlefield_meadow.png) | 二进制，3,510,181 字节 | [C018](#c018) | 上一版认可风格地表，保留为制作依据 |
| A | [shaders/textures/battlefield_meadow.png.import](../../shaders/textures/battlefield_meadow.png.import) | 41 / 0 | [C018](#c018) | 基准素材 VRAM 压缩、mipmap 与资源 UID |
| A | [shaders/textures/battlefield_meadow_clear.png](../../shaders/textures/battlefield_meadow_clear.png) | 二进制，3,495,769 字节 | [C018](#c018) | 去云制作中间图，不由当前材质加载 |
| A | [shaders/textures/battlefield_meadow_clear.png.import](../../shaders/textures/battlefield_meadow_clear.png.import) | 40 / 0 | [C018](#c018) | 去云中间图导入与资源 UID |
| A | [shaders/textures/battlefield_meadow_view_v2.png](../../shaders/textures/battlefield_meadow_view_v2.png) | 二进制，3,206,044 字节 | [C018](#c018) | 当前运行材质的 1536×1024 主地表绘制 |
| A | [shaders/textures/battlefield_meadow_view_v2.png.import](../../shaders/textures/battlefield_meadow_view_v2.png.import) | 41 / 0 | [C018](#c018) | 当前地表原生尺寸、mipmap、VRAM 导入与 UID |
| A | [shaders/textures/battlefield_reference.png](../../shaders/textures/battlefield_reference.png) | 二进制，3,152,133 字节 | [C018](#c018) | 用户认可概念参考，含原光影，仅供制作 |
| A | [shaders/textures/battlefield_reference.png.import](../../shaders/textures/battlefield_reference.png.import) | 40 / 0 | [C018](#c018) | 概念参考导入与资源 UID |
| A | [shaders/textures/grassland_canopy.md](../../shaders/textures/grassland_canopy.md) | 13 / 0 | [C014](#c014)、[C018](#c018) | 旧草冠提示词；当前只供样区细节层引用 |
| A | [shaders/textures/grassland_canopy.png](../../shaders/textures/grassland_canopy.png) | 二进制，3,015,254 字节 | [C014](#c014) | 1254² RGB 生图草冠源图 |
| A | [shaders/textures/grassland_canopy.png.import](../../shaders/textures/grassland_canopy.png.import) | 40 / 0 | [C014](#c014) | 1024 尺寸上限、mipmap 与资产 UID |
| M | [template/Enemy/Enemy.cs](../../template/Enemy/Enemy.cs) | 2 / 1 | [C020](#c020) | _preparedTarget 确认后的 FaceTowards() |
| M | [template/Enemy/Enemy.tscn](../../template/Enemy/Enemy.tscn) | 1 / 1 | [C019](#c019) | 基础 PackedScene 改为 MechPiece.tscn |
| M | [template/Piece/Piece.cs](../../template/Piece/Piece.cs) | 35 / 12 | [C009](#c009)、[C020](#c020) | _Notification()、VisualRoot、FaceTowards()、MoveAlongPathCore() |
| A | [template/Piece/铁岳/MechPiece.tscn](../../template/Piece/铁岳/MechPiece.tscn) | 22 / 0 | [C019](#c019) | Piece 基场景、BodyMesh 标记、VisualRoot 与 0.002 模型比例 |
| A | [template/Piece/铁岳/铁岳.cs](../../template/Piece/铁岳/铁岳.cs) | 11 / 0 | [C019](#c019) | 铁岳._Ready() 设置 FlyingUnit 并调用基类 |
| A | [template/Piece/铁岳/铁岳.cs.uid](../../template/Piece/铁岳/铁岳.cs.uid) | 1 / 0 | [C019](#c019) | Godot C# 资源 UID |
| A | [template/Piece/铁岳/铁岳.glb](../../template/Piece/铁岳/铁岳.glb) | 二进制，1,488,976 字节 | [C019](#c019) | 自包含 glTF 2 模型，173 mesh／9 material，来源未独立核对 |
| A | [template/Piece/铁岳/铁岳.glb.import](../../template/Piece/铁岳/铁岳.glb.import) | 42 / 0 | [C019](#c019) | 模型导入选项、UID 与生成资源路径 |
| A | [template/Piece/铁岳/铁岳.tscn](../../template/Piece/铁岳/铁岳.tscn) | 18 / 0 | [C019](#c019) | 四类既有炮技能、MoveRange=3、HP=5 的玩家模板 |
| M | [template/Skill/Skill.cs](../../template/Skill/Skill.cs) | 2 / 0 | [C020](#c020) | Execute() 中 _originTile == null 的 Caster 朝向 |
| D | `template/tile/GrassWindNoise.tres` | 0 / 0 | [C010](#c010) | 已删除的无引用零字节资源 |
| M | [template/tile/Tile.cs](../../template/tile/Tile.cs) | 2 / 1 | [C011](#c011) | GrassColor = Colors.White |
| M | [world/StylizedMaterials.cs](../../world/StylizedMaterials.cs) | 3 / 4 | [C011](#c011)、[C013](#c013) | Create() 选择草地模板，移除旧草风赋值 |
| M | [world/world.tscn](../../world/world.tscn) | 21 / 6 | [C003](#c003)、[C012](#c012)、[C019](#c019) | CloudLayer／CloudWeather；BattlefieldGround；PieceScene |
| M | [陷阵.csproj](../../陷阵.csproj) | 1 / 1 | [C021](#c021) | Project.Sdk = Godot.NET.Sdk/4.6.3 |
| A | [陷阵.csproj.old.1](../../陷阵.csproj.old.1) | 7 / 0 | [C021](#c021) | 原 4.6.1 工程声明备份，逐字节对应固定起点 |
| A | [docs/branch-reports/codex-realistic-clouds.md](codex-realistic-clouds.md) | 不计入代码统计 | `REPORT_ONLY` | 固定分支报告，单独文档提交 |

代码统计：74 个非报告文件，文本 2414 行新增、200 行删除，另有 10 个二进制资产；报告自身作为第 75 个路径单列。`project.godot` 与固定起点逐字节一致，SDK 声明已由 C021 改为 4.6.3；原先排除的工程与忽略规则本次已经提交，不再属于本地排除项。

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
| 上一版草原提交／推送 | C011～C014 | A014 为当时的明确 Git 授权，不表示新增地形功能、主分支合并或最终美术验收。 |
| 独立草原样区 | C015～C017 | A015 在独立样区方案之后明确同意试作；A013 覆盖生图素材。限独立场景、材质、资源与本地接线，不授权改写主棋盘、单位贴地、碰撞或项目设置。 |
| 上一轮样区提交／推送 | C015～C017 与报告 | A016 为独立样区阶段的 Git 授权，不构成主分支合并或美术验收。 |
| 历史质量修复提交／推送 | 质量修复及报告更新 | A010；不包含 AGENTS.md 正文、其他新模块或主分支合并。 |
| 当前主战场草原修订与绘制素材 | C011、C012、C014、C018 | 在既有草原范围内保存当前版本；A017 明确允许本次提交／推送，素材文档中的风格认可不扩展为全玩法或调整后比例验收。 |
| 机甲接入与默认单位替换 | [C019](#c019) | `SHARED_INTEGRATION`，`requires_extra_approval=true`，原始方案批准 `UNCONFIRMED`；A017 确认保存现有内容，不追认其他聊天中的实施方案。 |
| 共用移动／技能朝向 | [C020](#c020) | `SHARED_INTEGRATION`，`requires_extra_approval=true`；所有 Piece 派生类的完成时机变化，原方案批准 `UNCONFIRMED`。本次 Git 操作由 A017 明确授权。 |
| Godot.NET SDK 升级 | [C021](#c021) | `PROJECT_GLOBAL`，`requires_extra_approval=true`；4.6.1 → 4.6.3 及原工程备份单独在 85e1cb0 保存。原升级方案批准 `UNCONFIRMED`，本次 Git 操作由 A017 授权。 |
| 共享忽略策略 | [C022](#c022) | `DEVELOPMENT_WORKFLOW`，`requires_extra_approval=true`；忽略规则不停止跟踪已跟踪的 .csproj。原方案批准 `UNCONFIRMED`，本次 Git 操作由 A017 授权。 |
| 当前全部变更、报告与工作区整理 | A017 | 30 个工作区文件全部纳入两个实现／工具链提交；报告另交。仅通过提交消除未提交状态，不删除 AGENTS.md 或已忽略的本地文件，不合并主线、不强推。 |

原 C004 添加时的事前独立批准仍记为未确认；本次明确撤回不追溯改写该历史。

<a id="integration"></a>
## 引入依赖索引

| 组／候选 | 条目 | 引入边界 |
| --- | --- | --- |
| `G001` 云功能配套组 | C001、C002、C003、C005、C007、C008 | 云场、噪声、控制器、三份缓存、共用积分、接收材质、场景和说明配套引入；新的云层 shader 会读取视线缓存，不能只复制单个文件。 |
| 独立候选 | C006 | `ab989dd` 仅修改 `.gitignore`，按目标项目的本地规则约定审阅。 |
| 独立质量修复 | C009、C010 | 技能释放和空资源删除不依赖云功能组；本轮在同一独立修复提交中提供，可按对应差异审阅。 |
| `G002` 主战场草原配套组 | C011、C012、C013、C018、C005 | 草原 shader／材质、当前战场绘制、材质工厂、Grass 调色、天气接收、外围地面与预览配套；依赖 G001。主战场已不加载 C014 的旧草冠图，C014 继续供 G003 使用。 |
| `G003` 独立草原样区 | C015、C016、C017 | 样区场景、生成脚本、shader／材质、两张线性数据图、生图资产及导入记录配套引入；依赖 G001 的云资源与控制器、C011 的 grass_land_noise.tres、C014 的原草冠细节图。独立运行，不替换 G002 主棋盘模板，也不改地图格式或公共接口。 |
| `G004` 机甲及公共单位行为 | C019、C020 | 机甲 GLB／导入、基场景、玩家场景、Enemy 基场景替换与公共朝向调用配套。C020 的即时移动影响全部 Piece 派生类，必须审阅依赖动画、忙碌状态与 await 的调用方。 |
| `G005` 工具链与工程策略 | C021、C022 | 85e1cb0 为单独审阅候选，包含 SDK 声明、原工程备份和忽略规则。引入前统一协作者的 Godot.NET SDK；C022 不能实现已跟踪工程文件的本地隔离。 |
| 已撤回，退出候选 | C004 | `7d3d951` 添加全局上限；`d9557f2` 已单独移除。当前分支末端的 `project.godot` 与固定起点完全一致。 |

选择历史 `7d3d951` 时仍会带入旧 FPS 键；选择当前云版本时应同时包含其后续恢复与独立更新提交，或审阅相应净差异。本次没有重写历史，也没有合并主分支。

当前 `91ceb1d` 同时包含 G002 草原修订和 G004 机甲／移动行为；整次 cherry-pick 会一起带入。`world/world.tscn` 同时包含 G001 云接线、G002 外围地面和 G004 玩家模板替换，按单文件覆盖也会一起带入；需要只引入其中一组时，按条目对应的节点和差异拆分审阅，不能把该提交称为可独立引入的草原优化。

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

云影接线本身没有改写消费端 C# 业务流程；此前独立的技能释放修复见 C009，当前新增的公共移动／朝向见 C020。已有消费路径：`template/tile/Tile.cs` 与 `template/Piece/Piece.cs` 使用 `world/StylizedMaterials.cs::Create()`。云接线阶段统一复制 `low_poly.tres`，未改写颜色、草风、雾或分档光照；Grass 的专用模板选择和调色另见 C011～C014／C018，不追溯扩大原云任务范围。

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
source_commits: [5731b0a, 799da11, 7d3d951, 08515f0, e9dafc8001d52878151435bb21b6a749fcdb29c8, 91ceb1d8b4213cf06c0329eb8716bf29e9bfe71e]
integration_mode: WITH_DEPENDENCIES
integration_group: G001
depends_on: []
validation_status: STATIC_CHECKED
validation_refs: [V001, V004, H005, V008]
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
source_commits: [e9dafc8001d52878151435bb21b6a749fcdb29c8, 91ceb1d8b4213cf06c0329eb8716bf29e9bfe71e]
integration_mode: WITH_DEPENDENCIES
integration_group: G002
depends_on: [C001, C002, C012, C013, C018]
validation_status: STATIC_CHECKED
validation_refs: [V008, H009]
git_action_approval_refs: [A017]
```

原 Grass 与山地、水面、单位共用 low_poly 分档光照和颜色风带。当前 StylizedMaterials.Create() 在 grass=true 时复制 grassland.tres，Tile.GrassColor 已改为白色。主战场使用 C018 的 battlefield_meadow_view_v2.png，原草冠图保留供 G003 样区使用；山地、水面和原材质选择逻辑保持独立。机甲主体来自 C019 的导入模型，不能把旧单位 low_poly 接收方式直接推定为新模型主体的材质行为。

grassland.fragment() 将世界 xz 相对 (16.75,14.25) 的坐标按固定相机朝向／俯角投影，grass_texture_span=(12,8) km。sample_landscape() 混合四个同向局部取样，使用 textureGrad 的连续导数处理 mip 衔接；不旋转、镜像或随相机移动素材。相对素材文档记录的 (48,32) 版本，景物线性尺度缩为四分之一；这不表示具有精确关卡地形布局。

草风使用原生 TIME 和既有共享风向：两次 grass_land_noise 读取提供移动阵风与局部响应，light() 混合直立／弯折叶片法线的受光，不整体倾斜地面法线。图片颜色只用于草、水洼与石质区域的美术响应，不决定 Water 状态或玩法。每片元六次草地纹理读取，沿用云影查询；强度 0.75、视觉传播速度 0.085 km/s、宽高光 0.025、环境补光 0.22、云影强度 0.44。旧 spacing／width 参数已移除。草原修订未新增地形高度、草叶实例、碰撞或地图格式；本次单位共用行为变化单独见 C020。

旧 H007 的三次采样和性能数字只适用于 e9dafc8 阶段。本版以 V008 做静态核对；素材文档中的运行摘要单列为 H009，本轮没有复跑，也没有把已认可风格等同于调整后比例验收。

<a id="c012"></a>
### C012 — 草原天气接收与美术预览

```yaml
module: grassland_weather_and_preview
origin: THIS_BRANCH
impact_type: SHARED_INTEGRATION
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A011, A012, A013]
source_commits: [e9dafc8001d52878151435bb21b6a749fcdb29c8, 91ceb1d8b4213cf06c0329eb8716bf29e9bfe71e]
integration_mode: WITH_DEPENDENCIES
integration_group: G002
depends_on: [C002, C007, C011]
validation_status: STATIC_CHECKED
validation_refs: [V008, H009]
git_action_approval_refs: [A017]
```

CloudWeather._enter_tree() 的模板列表包含 grassland.tres，在 C# 创建地块材质副本前绑定天气原点、云时钟和两份世界缓存；_ready() 识别预览中的草地 shader，绑定相同天气资源。原云算法、缓存范围和 20 Hz 调度未在本次修改。

world/world.tscn 新增 BattlefieldGround：144×144 km 的 PlaneMesh，中心 (16.75,0.079,14.25)，cast_shadow=0，共用 grassland.tres。它只延展装饰地面，没有碰撞节点，不参与地块或地图保存。StylePreview 根节点平移到 (16.75,0,14.25)，Ground 使用相同草原模板。该场景同时还替换了玩家模板，后者属于 C019，不能把整个 world.tscn 视为仅草地接线。

H007 的接收绑定检查保留为历史，当前场景与资源引用由 V008 核对。World.cs、启动场景设置、Autoload、全局 FPS 没有在本次修改。

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
integration_group: G003
depends_on: []
validation_status: RUNTIME_CHECKED
validation_refs: [V006, H007]
```

e9dafc8 新增 grassland_canopy.png、.png.import 和 .md 素材说明。内置 imagegen 根据用户认可的草原概念图制作 1254² RGB 源图；PNG 保留生成结果，提示词与限制随仓库共享。导入时限制到 1024 并生成 mipmap；本次仅补充说明，没有替换该旧 PNG。

旧主战场曾以 64 km 跨度和粗级 mip 使用它；当前 G002 已改为 C018 的战场绘制。G003 独立样区仍引用该图作为细节层，因此继续保留，不能按停用主材质误删。素材不含可交互地形／水文数据，历史 H007 也不证明当前主战场材质运行通过。

<a id="c015"></a>
### C015 — 独立缓坡样区、植被数据与贴地格线

```yaml
module: grassland_study_terrain
origin: THIS_BRANCH
impact_type: MODULE_LOCAL
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A015]
source_commits: [46510692ed9306268f6300a08ddf3c4a1cf6b8bc]
integration_mode: WITH_DEPENDENCIES
integration_group: G003
depends_on: [C001, C002, C007, C016, C017]
validation_status: RUNTIME_CHECKED
validation_refs: [V007, H008]
```

此前美术预览为平面，主棋盘仍为平顶六边形。新增独立的 GrasslandStudy.tscn，通过 F6 运行；F5 启动场景不变。场景展示 40×40 km 的缓坡与弯曲浅洼，StudyTerrain.height_at() 给出同一套高度数据，vegetation_at() 按浅洼及弱连贯变化组织草高／疏密，稀疏处仍为绿色植被。

make_mesh() 只在进入场景时创建一次 192×192 网格，37,249 顶点、73,728 三角形、一个地面绘制面；风不移动顶点。normal_at() 与离线法线图来自同一高度数据，另加弱草冠起伏。BakeStudy._initialize() 显式生成 vegetation.png、surface_normals.png 和 colour_layout.png，游戏运行不重建这些图，不自动调用生图服务。两张运行数据图为 1024²、线性采样、mipmap；RGBA 植被通道记录覆盖、相对草高、方向／响应差异、草丛变化。

GrasslandStudy._make_grid() 将边长 1 km 的六边格线沿高度函数生成；镜头保留正交投影、35.264° 俯角、45° 朝向，在独立节点处理滚轮、拖动、格线和暂停风动，不修改全局 InputMap。_ready() 只把自己的材质绑定到既有 CloudWeather 的时钟／天气／太阳缓存，未修改 CloudWeather.gd 或公共模板。主棋盘地形、单位贴地、碰撞、地图保存与寻路均未接入。最大缩放可见有限样区边界；原有主棋盘代码仍在使用，没有删除。

<a id="c016"></a>
### C016 — 固定地形上的两类草冠受光与阵风

```yaml
module: grassland_study_canopy_wind
origin: THIS_BRANCH
impact_type: MODULE_LOCAL
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A015]
source_commits: [46510692ed9306268f6300a08ddf3c4a1cf6b8bc]
integration_mode: WITH_DEPENDENCIES
integration_group: G003
depends_on: [C001, C002, C011, C014, C015, C017]
validation_status: RUNTIME_CHECKED
validation_refs: [V007, H008]
```

此前主棋盘材质以连续地表法线倾斜和高光增强表现风，用户指出塑料感。样区新增 grassland_study.gdshader／.tres，fragment() 固定地形法线，按阵风、草高、草丛分布混合直立与伏倒两类草冠的受光。长草的伏倒比例较高，短草较弱；局部方向和响应差异打断整齐风带。light() 采用轻微受光对比曲线和弱宽高光，风不额外提升一条亮带，也不滚动固有色贴图。

wind_time 由样区本地 _process(delta) 更新，空格只暂停这个草冠时钟；现有云时钟仍独立以约 20 Hz 更新。默认强度 0.85、视觉传播速度 0.085 km/s、sheen_strength=0.035；伏倒方向按草高在 1.15～1.75 的法线倾斜量间插值。参数为高空画面的近似表达，不是当地气象风速或完整草叶力学模拟。

草冠主色图与低级 mip 组织大色块，原草冠图的镜像采样只提供受覆盖度约束、最多 ±6% 的细纹调制；共有 6 次草原专用纹理采样，另复用既有云影查询。未增加单根草几何、逐帧地形噪声计算或新的运行时视口。该 shader 仅由 G003 样区材质引用，C011 主棋盘材质保持原样。

<a id="c017"></a>
### C017 — 按样区配色布局生成美术资产与制作记录

```yaml
module: grassland_study_painted_assets
origin: THIS_BRANCH
impact_type: MODULE_LOCAL
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A013, A015]
source_commits: [46510692ed9306268f6300a08ddf3c4a1cf6b8bc]
integration_mode: WITH_DEPENDENCIES
integration_group: G003
depends_on: [C014, C015, C016]
validation_status: RUNTIME_CHECKED
validation_refs: [V007, H008]
```

新增 study_canopy.png、导入设置和 study_canopy.md。内置 imagegen 的第一张输入为 StudyTerrain.vegetation_at() 生成的 768² 配色布局 colour_layout.png，第二张为用户认可的概念图，仅提取油画笔触、草色和植被组织。生成的 1254² RGB 草冠源 PNG 直接复制进项目，运行时导入到 1024、启用 mipmap；原输出未被代码重新绘制。完整提示词、输入角色和局限随仓库共享。

主色图覆盖整块 40×40 km 样区，不作为无缝循环图；深草带和安静坡面提供层次，细节集中在密草区域。美术图只近似遵循控制布局，不是逐像素草高或物理反照率。资产自身有局部笔触明暗；整体太阳与云影仍实时计算，没有把概念图的水、云或地形数据作为可交互内容引入。colour_layout.png 为制作输入，不由运行材质加载。README.md 保存 F6 入口、操作、烘焙方式、数据通道和实际验证条件。

<a id="c018"></a>
### C018 — 主战场绘制素材链与后续关卡约定

```yaml
module: battlefield_painted_assets
origin: THIS_BRANCH
impact_type: MODULE_LOCAL
requires_extra_approval: false
approval_status: IN_SCOPE
approval_refs: [A013]
git_action_approval_refs: [A017]
source_commits: [91ceb1d8b4213cf06c0329eb8716bf29e9bfe71e]
integration_mode: WITH_DEPENDENCIES
integration_group: G002
depends_on: [C011, C012]
validation_status: STATIC_CHECKED
validation_refs: [V008, H009]
```

新增 battlefield_reference.png、battlefield_meadow_clear.png、battlefield_meadow.png、battlefield_meadow_view_v2.png、各自导入记录及 battlefield_meadow.md，共 9 个文件。四张图均为 1536×1024，当前运行材质仅绑定 view_v2；参考／去云中间图及上一版地表保留制作与风格依据。PNG 哈希与随仓库素材文档逐项相符，实际导入使用 VRAM 压缩、mipmap、size_limit=0；源图保留绘画明暗，不是纯反照率或高度／水文数据。

素材说明保存真实提示词、投影公式、统一尺度的四局部取样、源图／实机认可边界及性能记录。其“按关卡布局生成河流／坡谷”的流程仅为后续制作约定，没有接入关卡控制图、通行、碰撞、寻路或新地图格式。素材文档中的认可与历史运行属于该文件记录，本轮没有读取新的原始聊天或复跑这些记录。

<a id="c019"></a>
### C019 — 铁岳模型、机甲基场景与默认单位替换

```yaml
module: mech_unit_scene_integration
origin: THIS_BRANCH
impact_type: SHARED_INTEGRATION
requires_extra_approval: true
approval_status: UNCONFIRMED
approval_refs: []
git_action_approval_refs: [A017]
source_commits: [91ceb1d8b4213cf06c0329eb8716bf29e9bfe71e]
integration_mode: REVIEW_REQUIRED
integration_group: G004
depends_on: [C020]
validation_status: STATIC_CHECKED
validation_refs: [V008]
```

新增 template/Piece/铁岳/ 下的 6 个文件。MechPiece.tscn 继承 Piece.tscn，以六边形 CylinderMesh 作为原 BodyMesh 标记，另建 VisualRoot，并以 0.002 比例加载铁岳.glb。GLB 为 glTF 2，1,488,976 字节，静态记录含 173 个 mesh、9 个 material，没有外部 URI 依赖；这不是实际 draw calls 或性能结果。随附文件没有单独模型来源／许可说明，本轮未核对来源。

铁岳.tscn 指定四个既有炮类技能、MoveRange=3、HP／MaxHP=5；铁岳._Ready() 设置 ObstacleGroups.FlyingUnit。world/world.tscn 的 PieceScene 从龙骑士切换为铁岳，Enemy.tscn 的基础场景从 Piece.tscn 切换为 MechPiece.tscn；因此玩家默认模板和敌方外观都发生变化。没有删除旧龙骑士资产。

本次授权是保存这些已存在的修改，原始模块方案批准未在当前会话核对。接入目标分支时需审阅默认单位、导入材质、模型性能及 C020 的共同影响；不能把它归为仅云或仅草原修改。

<a id="c020"></a>
### C020 — 共用移动同步完成与可视模型朝向

```yaml
module: shared_piece_movement_and_facing
origin: THIS_BRANCH
impact_type: SHARED_INTEGRATION
requires_extra_approval: true
approval_status: UNCONFIRMED
approval_refs: []
git_action_approval_refs: [A017]
source_commits: [91ceb1d8b4213cf06c0329eb8716bf29e9bfe71e]
integration_mode: REVIEW_REQUIRED
integration_group: G004
depends_on: []
validation_status: STATIC_CHECKED
validation_refs: [V008]
```

Piece 删除 MovementStepDuration=0.15 和逐格 Tween／ToSignal 等待。MoveAlongPathAsync() 的 Task 签名保留，StartMoveAlongPath() 现在同步执行 MoveAlongPathCore() 并返回 Task.CompletedTask；每个路径点立即设置位置并更新朝向，_isMoving 在返回前复位。该完成时机影响所有 Piece 派生类，不局限于铁岳；需要时序、动画或忙碌状态的调用方不能继续假设存在逐格 0.15 秒等待。路径选择算法不在这次差异中改写。

新增公共 FaceTowards(Vector3)：只旋转 VisualRoot，忽略垂直差值，使用 +Z 为模型前向；无该节点或方向为零时直接返回。移动过程中调用，Skill.Execute() 在 _originTile 为 null 时令 Caster 朝向目标，Enemy 在准备技能／目标后调用。C009 的未挂树技能释放逻辑继续保留。

A017 覆盖本次保存与推送，未核对到这次公共行为变更的原方案批准。V008 仅核对源码关系；当前环境没有可用 SDK，没有编译、移动／技能时序或战斗运行回归结果。

<a id="c021"></a>
### C021 — Godot.NET SDK 4.6.3 与原工程备份

```yaml
module: shared_godot_dotnet_toolchain
origin: THIS_BRANCH
impact_type: PROJECT_GLOBAL
requires_extra_approval: true
approval_status: UNCONFIRMED
approval_refs: []
git_action_approval_refs: [A017]
source_commits: [85e1cb0db944c2f25c7cdf6e71b283b5c22eedf4]
integration_mode: REVIEW_REQUIRED
integration_group: G005
depends_on: []
validation_status: STATIC_CHECKED
validation_refs: [V008]
```

陷阵.csproj 的 Project.Sdk 从 Godot.NET.Sdk/4.6.1 改为 4.6.3。桌面 TargetFramework=net8.0、Android 条件 net9.0、EnableDynamicLoading 没有变化；这仍是全项目 C# 构建依赖变化，需要协作者按 4.6.3 声明还原／构建，不能归为本地编辑器偏好。

原先未跟踪的陷阵.csproj.old.1 也按“全部保存”纳入；其内容与固定起点的 .csproj 逐字节相同，只保留旧声明，不作为当前构建入口。原先已忽略的 .old 文件继续本地保留。工具链与 C022 在独立提交 85e1cb0 中，便于单独审阅／选择引入；不与当前美术和移动实现捆成同一个提交。

当前可发现的 dotnet.exe --info 返回“No SDKs were found”；本轮未安装或升级任何软件，没有验证新 SDK 在此执行环境或合作者设备上的构建。历史 H007／H008 使用 4.6.3 是其原运行条件，不能当作本轮整体版本验证。原升级方案的独立批准仍为 UNCONFIRMED，A017 只记录当前版本保存授权。

<a id="c022"></a>
### C022 — 工程文件与旧备份的共享忽略规则

```yaml
module: shared_project_ignore_policy
origin: THIS_BRANCH
impact_type: DEVELOPMENT_WORKFLOW
requires_extra_approval: true
approval_status: UNCONFIRMED
approval_refs: []
git_action_approval_refs: [A017]
source_commits: [85e1cb0db944c2f25c7cdf6e71b283b5c22eedf4]
integration_mode: REVIEW_REQUIRED
integration_group: G005
depends_on: []
validation_status: STATIC_CHECKED
validation_refs: [V008, V009]
```

.gitignore 新增“陷阵.csproj.old”和“陷阵.csproj”两行。前者忽略该精确备份名，不匹配 .old.1；后者不会使已经被 Git 跟踪的陷阵.csproj 自动退出版本控制。本次 SDK 差异仍在 C021 提交中，后续对已跟踪 .csproj 的改动也仍会出现在 Git 状态中。本轮没有执行 git rm --cached 或改变工程文件的跟踪方式。

C006 的 /AGENTS.md 规则保留，正文继续只在本地。A017 授权保存当前 .gitignore；原先为何以忽略规则处理工程文件的方案未在本轮核对，合作接入前需要明确该策略的预期用途。

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
| `c6ed710dc4fb188b75e8a22dc8a543f260270b8d` | docs: record grassland prototype scope and validation | REPORT_ONLY；上一版草原报告 |
| `46510692ed9306268f6300a08ddf3c4a1cf6b8bc` | feat: add rolling grassland study and canopy wind | C015～C017；20 个独立样区代码／资产／说明文件 |
| `7f9b7eee71f235af7cc4be02853b2dccb24fbcd0` | docs: record grassland study scope and validation | REPORT_ONLY；上一轮样区报告 |
| `91ceb1d8b4213cf06c0329eb8716bf29e9bfe71e` | feat: update battlefield art and mech unit behavior | C011、C012、C014、C018～C020；27 个实现／素材／说明文件 |
| `85e1cb0db944c2f25c7cdf6e71b283b5c22eedf4` | build: align Godot SDK to 4.6.3 and preserve project backup | C021、C022；3 个工具链／忽略策略／备份文件 |

固定起点仍为 `d6b1545b527bd8165b8a6c1e8b4817ec695828bc`。代码核对点范围内共 17 次提交、0 次合并；本次推送前远端开发分支为 `7f9b7eee71f235af7cc4be02853b2dccb24fbcd0`，目标 master 经 ls-remote 核对仍等于固定起点。本次 30 个原工作区文件分别纳入 27 文件实现提交和 3 文件工具链提交，内容保持原样；报告另作 REPORT_ONLY 提交。没有合并或重写历史，不记录未知的报告自身提交哈希。

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
**A014 — 保存并推送上一版草原。** 来源：同一任务，2026-10-20 当时的用户消息。原话：“先把这一版push了”。用户同时指出细节、地势起伏、植被疏密仍不足，草浪太塑料，并要求讨论改进看法。授权当前版本的必要提交、报告更新与推送，不把推送理解为美术验收或继续实现新地形的授权。

<a id="a015"></a>
**A015 — 按独立样区方案试作。** 来源：同一草原任务，2026-10-20 用户消息“你可以试试”。此前方案明确先做独立样区，验证实际缓坡／浅洼、植被疏密／草高与新的草冠风动，再用现有镜头观察；生图美术资产延续 A013。该授权覆盖 C015～C017 及其本地云接收，不扩展为主棋盘、单位贴地、碰撞、地图格式或公共设置重做。

<a id="a016"></a>
**A016 — 上一轮样区提交／推送。** 来源：独立样区任务，2026-10-20 当时的用户消息，原话：“先把这一版给push了吧”。授权将当时已交付的独立样区及必要说明保存到开发分支，集中更新本报告并推送；不表示已接受最终美术效果，也不授权主分支合并。

<a id="a017"></a>
**A017 — 保存当前全部变更、维护报告并推送。** 来源：当前协作审查任务 `01a156ff-c59b-7621-a636-898a0b07d45c`，2026-10-20 本轮用户直接消息。原话：“我又进行了一些工作，现在将当前的变更全部push，清空工作区，记得维护开发报告”。类型为 `GIT_ACTION_AND_REPORT_MAINTENANCE`，明确覆盖当前所有已修改及未跟踪项目文件，包括此前排除的 .gitignore、.csproj 与 .old.1；沿用 AGENTS.md 仅本地安排。工作区通过提交消除未提交状态，已忽略的个人规则／备份／缓存保留。本轮不新增实现、不反向修改用户已有行为，不把 Git 授权写成原始方案批准、报告全文审阅、运行验收或主分支合并授权。

<a id="validation"></a>
## 验证与证据索引

| 编号 | 类型与可访问范围 | 实际核对内容 | 结论边界 |
| --- | --- | --- | --- |
| V001 | CURRENT_STATIC；仓库可复核 | 固定基准至 85e1cb0 的 74 个非报告文件、文本 2414/200 行、17 次提交、0 次合并和 10 个二进制资产；完整索引含报告共 75 路径。 | 确认范围、映射和统计，不等于构建或运行验收。 |
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
| V006 | HISTORICAL_STATIC；仓库可复核 | 上一版代码提交的 13 个草原文件与 H007 测试副本逐字节相同；原 PNG 与生图输出一致。核对固定起点、远端指针、净差异索引、依赖与本地排除项；git diff --check 通过。 | 本次 push 准备没有重跑游戏，不将上轮运行改写为本轮运行；未提交的 SDK 差异单列。 |
| H007 | IMPLEMENTATION_RUNTIME_RECORD；LOCAL_ARTIFACT | 草原第二版实施阶段构建 0 警告／0 错误，资源导入成功，17 项检查通过，运行日志无错误；三档视距 V1/V2 对照和草浪实录已生成。 | Godot 4.6.3 Mono、GL Compatibility、1920×1080、RTX 3060 Laptop，VSync=1、max_fps=0；测试副本使用本地 SDK 4.6.3，当时仓库声明为 4.6.1，未证明该旧版在 4.6.1 工具链上运行通过。当前声明见 C021。 |
| V007 | HISTORICAL_STATIC；仓库可复核 + LOCAL_ARTIFACT | 本次 20 个样区文件在暂存前及提交后逐字节对应 H008 的最终测试副本／源码清单；PNG 与内置生图原输出一致。核对分支／远端、固定起点、完整索引、依赖和原有差异排除，git diff --check 通过。 | 本次 push 准备没有重跑游戏或烘焙；保留实施阶段实际运行条件，不等同于 Godot 4.6.1 或完整游戏验收。 |
| H008 | IMPLEMENTATION_RUNTIME_RECORD；LOCAL_ARTIFACT | 独立样区阶段编译 0 警告／0 错误、资源导入及运行成功，8 项检查全部通过；数据／输入布局重烘焙逐字节相同。正交 8／12／20／40 观察、主视口 GPU 对照及按材质时钟采样的草浪动画已保存。 | Godot 4.6.3 Mono、.NET SDK 8.0.426、GL Compatibility、1920×1080、RTX 3060 Laptop、VSync=1、max_fps=0；该历史阶段 SDK 差异未提交，当前共享声明见 C021。不是主棋盘接入、其他设备或 4.6.1 运行结果。 |
| V008 | CURRENT_STATIC；仓库可复核 | 本轮 30 个路径差异和新增文本核对；新增／修订资源引用无缺失，5 个导入记录的源文件存在；4 张新增 PNG 均为 1536×1024，SHA-256 与素材文档一致；机甲 GLB 2 结构／长度正确、无外部 URI；两个工程 XML 可解析，.old.1 与起点工程逐字节相同，project.godot 与起点相同。git diff --check 通过。 | 未重新构建或运行；当前可发现的 dotnet.exe --info 明确返回 No SDKs were found。没有安装软件或改写已有实现。 |
| V009 | CURRENT_GIT_CHECK；仓库可复核 | 初始 30 文件清单与提交内容逐字节核对，功能提交 27 文件、工具链提交 3 文件；报告完整索引／内部引用／来源提交与固定差异核对；AGENTS.md 保持被忽略、未跟踪且内容原样。 | 本报告记录推送前版本；报告末尾提交后再核对本地与远端一致、无未提交／未跟踪项。已忽略文件保留，不使用丢弃式清理。 |
| H009 | DOCUMENTED_RUNTIME_NOT_REVERIFIED；仓库摘要＋LOCAL_ARTIFACT | battlefield_meadow.md 保存替图阶段 9 项、比例调整阶段 13 项检查及 GPU 摘要；所述环境为 Godot 4.6.3 Mono、SDK 8.0.426、GL Compatibility、1920×1080、RTX 3060 Laptop、VSync 开启、max_fps=0。比例调整的主视口 GPU 中位数 1.009→1.189 ms，draw calls=3103。 | 本轮仅读取并保存原文摘要，未读取原始运行产物或独立复跑；测量冻结云缓存、不包含其子视口成本，不证明 C020 公共移动或当前全版本验收。 |


V002 历史源码 SHA-256（旧核对点 7d3d951；并非当前 reviewed_commit）：

| 仓库路径 | `7d3d951` 内容 SHA-256 |
| --- | --- |
| `shaders/cloud_field.gdshaderinc` | `ed05bdb6de4aff3b1f9536bf08d3991807edf4da9ce459f74ad24f11720aa9fc` |
| `shaders/cloud_layer.gdshader` | `6fd5f9f907151e32e2e197d7b5a605687995b3ed11af31a33abde54352012488` |
| `shaders/CloudWeather.gd` | `f94cd2c3ba72671d37b746299a3f08a90c17de2c97e02335021a7fa00b25bcda` |
| `shaders/cloud_sunlight.gdshader` | `2dc50839a36689d2edb27eafe077254d6675a6ed9a58d464bb139c4c967ae492` |
| `shaders/cloud_weather.gdshader` | `51a3ec1fb3d7ddd102961c50fc6a19d77d1567c55074af856f51a989d035681b` |
| `shaders/low_poly.gdshader` | `640a7a9338e6dd0a6a0d0f477f53c05c2deed3dfda7dc02641b81673b448e399` |


H006 历史源码 SHA-256（`2cbfa39`；并非当前 reviewed_commit）：

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

版本一致性：H004/H005 当时使用的 Piece.cs 修复在那次云推送中仍被排除，后来由 A010 授权作为 C009 纳入分支。V005 的生产源码对应 2cbfa39，仅隔离测试脚本额外加入；当前 Piece 又有 C020 的新变化，不能复用旧结果声称当前全部玩法、其他设备或合作者分支已回归通过。

H007 工程检查包含：草／山材质选择、编辑重建后的天气绑定、342 个 Grass 材质数量、非草对象保持原 shader、两张草原纹理共享、草冠图确为项目资产且运行时 1024²／带 mipmap、旧草风及旧法线资源删除、固定云时钟时录制草浪、云保持约 20 Hz、独立预览材质和云缓存绑定。视觉实录只证明当前表现可供查看；用户已指出其塑料感和缺少地势层次，不作美术验收结论。

H007 同进程主视口 GPU 数据（缓存冻结，180 帧；不含云缓存耗时）：

| 正交 size | V1 中位数 ms | V2 中位数 ms | V2 P95 ms | V1/V2 绘制次数 |
| --- | --- | --- | --- | --- |
| 8 | 0.690 | 0.542 | 1.526 | 719 / 719 |
| 20 | 1.207 | 0.689 | 2.060 | 1539 / 1539 |
| 40 | 0.569 | 0.553 | 1.653 | 1617 / 1617 |

开启正常云更新后的场景约 165.5 FPS、云时钟 20.0 Hz。受当前 VSync 和 GPU 动态频率影响，不以中位数宣称稳定加速；V2 P95 高于 V1，保留实际记录。没有为了数据改变项目设置。H007 对应的主棋盘草原每片元 3 次专用纹理采样，无新增草叶几何、视口或绘制次数。

H007 证据仅本机可访问，未将完整日志、隔离副本和动画放入仓库。材料名与 SHA-256：

| 材料 | SHA-256 |
| --- | --- |
| `grassland-v2/runtime-validation.json` | `448a332160d76673fe87dd5facc5651c7130f5eaa947da2385e0e80400f264a2` |
| `grassland-v2/final-source-check.json` | `81a8a9b0eb7efc3066d6ea76083d3bf67ad8c64ff270336fc91d6701637e50df` |
| `grassland-v2/painted-final-stdout.log` | `33f0e708c72505cd956ad3a58186e560e9f88d73b755fa4fb0bf049e690ea9ec` |
| `grassland-v2/painted-final-stderr.log` | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |

草原 e9dafc8 阶段的代码与素材曾通过 V006 对照 H007；该阶段提交的 .csproj 仍声明 Godot.NET.Sdk/4.6.1、net8.0，本机 4.6.3 差异当时排除。当前 A017 已将该声明和忽略规则纳入 C021／C022；这是后来发生的共享工具链变化，不追改 H007 的证据边界。

H008 的 8 项检查为：实际网格起伏、离线法线与高度对应、正交俯角／朝向、既有太阳透射缓存绑定、既有云时钟绑定、格线切换、风动运行和暂停。法线抽样最大夹角误差约 1.55°；数据图和配色输入重烘焙逐字节一致。源码最终清单覆盖 20 个样区文件，本次 V007 再核对到代码提交。

H008 同进程主视口 GPU 记录（每组 180 帧，冻结云缓存；不包含缓存生成）：

| 展示地面／正交 size | 中位数 ms | P95 ms | 绘制次数中位数 |
| --- | --- | --- | --- |
| 旧版平地／12 | 0.603 | 0.648 | 36 |
| 新样区／12 | 1.193 | 1.201 | 36 |
| 新样区／8 | 0.795 | 0.801 | 36 |
| 新样区／20 | 1.197 | 1.352 | 36 |
| 新样区／40 | 0.972 | 0.984 | 36 |

旧版参考为两三角形平面和原材质，新样区包含真实网格与新材质，不能把这里的数据等同于整个棋盘性能。恢复正常云更新后的独立样区约 165.33 FPS、云约 19.96 Hz，受本机 VSync 及动态频率约束；一次场景准备约 373 ms，包含资源读取、地形和格线生成。没有修改测试条件来提高数字。两份 64 帧草浪动画按材质时间每 0.16 s 采样、播放约 10.24 s，录制时冻结云影以分辨草浪；同步截图读取的墙钟耗时不作为游戏帧率。

H008 的原始脚本、帧序列和日志仅本机可访问；工程摘要在样区 README 中共享，以下哈希定位当时的本机证据，不依赖绝对路径：

| 材料 | SHA-256 |
| --- | --- |
| `grassland-v3/runtime-validation.json` | `04c9e84e99c343b4fc19df723c14a3d3e4bb282e1549f8fa73ab12c13b99937b` |
| `grassland-v3/final-source-check.json` | `5663fe7468ce64b2017f2545b6207bf66e51f892e430596e408ba89c514c91a7` |
| `grassland-v3/bake-reproduction.json` | `52381f4949e94d1baee533ca77fe45471ba33594322a3a1ce75f99e901dff049` |
| `grassland-v3/motion-summary.json` | `5a976de6eedb6eb32857a543143e4b4d0a8ac5c5bc9d3986aa6677ea527333e8` |
| `grassland-v3/build-final.log` | `761726630fc4faf4f25f08ce3d7c8ba5dc7d081e02c176632b4234a6e43f5a3e` |
| `grassland-v3/final-stdout.log` | `ad087a0b49141d223a5fc94fee8c00dc06aba5bd3aec08ffec373afb842be1f6` |
| `grassland-v3/final-stderr.log` | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |
| `grassland-v3/motion-stdout.log` | `803f8cd7c9863c1a85335402abf2cef791533905683bbfec6d4cbad55479658c` |
| `grassland-v3/motion-stderr.log` | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |

H008 阶段没有切换主棋盘及其当时的 C011～C014 材质。8 项工程检查、截图和动画仅证明该版本可运行／可观察；近景笔触仍偏软，美术效果未记录为用户验收。当时仓库 SDK 为 4.6.1，样区验证用 4.6.3；当前主战场材质和共享 SDK 已另由 C011／C018／C021 更新，H008 不覆盖这些后续变化或其他设备。

<a id="collaboration"></a>
## 合作接入检查与范围限制

| 检查项 | 已确认事实 | 后续接入要求 |
| --- | --- | --- |
| 当前改动范围 | 分支包含 G001 云、G002 主战场草原、G003 独立样区、G004 机甲／公共单位行为和 G005 工具链。 | 不能再将整个分支称为仅云或仅草地优化；依模块与条目选择引入。 |
| 玩法与公共时序 | C020 删除共用移动的逐格 Tween／等待，Task 同步完成；玩家技能与敌方准备新增模型朝向。 | 合作者如依赖 MoveAlongPathAsync 的动画、busy 状态或 await 时机，需单独回归；本轮没有运行结果。 |
| 默认单位与材质 | C019 替换玩家默认模板和 Enemy 基场景，模型有独立导入材质；BodyMesh 作为选择标记保留。 | 核对合作者的单位模板、模型材质／云影接收和性能；未提供模型来源的独立核对。 |
| 地图与地势 | Map/、World.cs、project.godot 与起点一致。G002 是平面上的绘制，G003 的缓坡只在独立样区，新增 BattlefieldGround 无碰撞。 | 绘制水洼不构成 Water 数据，图像不能替代关卡规则；真实棋盘地势、单位贴地、碰撞与地图格式仍未接入。 |
| 共享工具链 | C021 将 Godot.NET.Sdk 改为 4.6.3，桌面仍 net8.0；工具链在单独提交中。 | 与协作者明确是否引入及其安装／还原条件；本轮没有 SDK 构建结果，不能由历史运行推定当前版本通过。 |
| 忽略规则 | C022 忽略精确工程名和 .old，但 .csproj 已跟踪，SDK 修改仍已提交。AGENTS.md 不跟踪。 | 不把 ignore 规则当作本地工程隔离；改变跟踪方式需要另行决定。 |
| 云与运行参数 | 云实现未在本轮改动，世界缓存范围固定，云约 20 Hz 的历史设计保留；project.godot 与起点一致，C004 撤回状态保留。 | 当前图像与导入设置下的整体性能需对应验证；H009 只记录过去的主视口测量。 |
| 场景与提交拆分 | world/world.tscn 含云、草原和玩家模板三类接线；91ceb1d 同时含 G002 和 G004。 | 只引入草原时按节点／差异拆分，避免整文件覆盖或整次 cherry-pick 带入公共移动。 |
| 目标主支与兼容 | 本次 ls-remote 核对 origin/master 仍为固定起点，没有合并主支或重写历史。 | 合作者分支内容未取得，兼容结论为 UNCONFIRMED，准备合并时按明确版本核对。 |
| 美术与历史验证 | H007/H008 为旧实现记录；当前主战场说明记载风格认可与比例调整，后者仍待用户实机审阅。H009 未独立复跑。 | 图片、提示词或工程检查不替代最终美术、玩法与多设备验收。 |

<a id="local-only"></a>
## 本地排除项与报告自身

| 项目 | 核对状态／归属 | 处理 |
| --- | --- | --- |
| AGENTS.md | IGNORED；LOCAL_ONLY | 继续未跟踪，只保留本地；本轮内容保持原样。 |
| 陷阵.csproj.old | IGNORED；LOCAL_ONLY | 已有精确名备份继续本地保留，不删除或强制添加。 |
| .godot/、构建缓存及忽略的本机产物 | IGNORED；LOCAL_ONLY | 不作为项目修改提交，也不因清空工作区而删除。 |
| 原 .gitignore／.csproj／.old.1 排除项 | THIS_BRANCH；C021／C022 | A017 明确要求全部保存，本次已经纳入工具链提交；不再列为未提交差异。 |
| docs/branch-reports/codex-realistic-clouds.md | REPORT_ONLY；UPDATED_NOT_USER_REVIEWED | 按 A017 单独更新及提交，reviewed_commit 为真实代码核对点 85e1cb0。 |

本轮初始状态共 30 个已修改／未跟踪文件，其内容原样分入两个提交；报告是本轮唯一额外编辑的项目文件。通过提交和推送消除未提交状态，保留所有已忽略的本地规则、备份和缓存，不执行丢弃式 reset／clean。第一版从未提交的旧法线资源删除仍不伪装成历史删除项；旧测试脚本、日志与隔离副本不因本次保存自动进入 Git。

<a id="decisions"></a>
## 待决定事项与报告更新

| 事项 | 当前状态 | 下一步 |
| --- | --- | --- |
| 初版报告审阅 | USER_REVIEWED，A006 | 保留历史事实；当前扩展报告仍为 UPDATED_NOT_USER_REVIEWED。 |
| 当前全部内容保存／推送 | EXPLICITLY_APPROVED，A017 | 两个实现／工具链提交加单独报告提交后推送当前开发分支，核对远端与工作区。 |
| C004 全局帧率策略 | WITHDRAWN，A007 | project.godot 保持起点设置。 |
| C019／C020 公共单位变化 | EXTRA_APPROVAL_UNCONFIRMED；GIT_ACTION_APPROVED | 保存原有修改；在共享版本接入时决定默认机甲和共用即时移动，核对调用时序。 |
| C021／C022 工具链与忽略策略 | EXTRA_APPROVAL_UNCONFIRMED；GIT_ACTION_APPROVED | 可单独审阅 85e1cb0；明确团队 SDK 声明及工程文件策略。 |
| 调整后的主战场景物比例 | NOT_USER_REVIEWED_AFTER_SCALE_CHANGE | 保留当前代码及图片；按素材文档条件查看实机，不把保存操作当作美术验收。 |
| 关卡设计驱动的绘制／交互地形 | DESIGN_ONLY；NOT_IMPLEMENTED | 素材文档仅保留后续制作约定；当前没有关卡控制图、碰撞或新地图格式。 |
| 合作者模块兼容及当前版本运行 | UNCONFIRMED；CURRENT_STATIC_ONLY | 当前 SDK 不可用，未重新构建／运行；取得目标版本与对应环境后进行回归。 |

维护时机：仅在用户明确要求 commit／push 时，在执行前集中汇总实际内容并更新本报告；日常开发、调试、验证和交付不要求同步维护。用户另行要求查看、生成或修订时按其指令处理。

维护时保持 C001～C022 编号及报告路径稳定，新增行为使用新编号，修订原行为更新原条目；撤回项保留历史并退出候选。reviewed_commit 只承诺已核对代码；后续仅改本报告的 REPORT_ONLY 提交可保留此值，出现其他路径变化时在下一次维护中补充核对。不得把旧运行、旧授权或旧主分支观察值无条件沿用。
