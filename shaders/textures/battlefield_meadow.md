# 战场草原地表

2026-10-20 用户认可当前实机的美术风格：“美术风格很对味”。本文件保存可继续使用的参考资产、制作方法和完整提示词；按关卡设计约束地形位置的生成流程属于后续设计方向，尚未实现。

随后用户要求按当前俯瞰视角重新生图并替换战场底片试效果。当前材质使用 [battlefield_meadow_view_v2.png](battlefield_meadow_view_v2.png)，上一版 [battlefield_meadow.png](battlefield_meadow.png) 原样保留为风格基准。用户后续确认图片本身没有问题，指出实机铺图比例过大；当前已缩小景物尺度，详见下文空间映射说明，尚待用户审阅调整后的实机比例。

## 美术目标与来源

用户认可的平缓草原概念图保存为 [battlefield_reference.png](battlefield_reference.png)。目标是自然景观中的细节概括：整体青翠，有轻微地势感、植被疏密和少量水洼、石质斑块，局部细节克制；画面层次由草色与云影共同形成。参考图中的云与大块投影不是地表固有色。

[battlefield_meadow.png](battlefield_meadow.png) 于 2026-10-20 使用内置 imagegen 制作：先对该参考移除边缘云朵、提亮大块云影，再以去云结果为输入扩展更广域的草原，收细草冠与零星景物的尺度。保留俯瞰绘制、色彩关系及局部明暗；生成过程重新组织了部分景物，最终图不与参考逐像素对应。生成的 PNG 原样复制进项目，没有用程序重绘或重新着色。该图保留了绘画明暗，不是物理扫描得到的纯反照率，也不包含准确的高度、草高或水文数据。完整提示词在文末。

| 文件 | 分辨率 | SHA-256 |
| --- | --- | --- |
| `battlefield_reference.png` | 1536 × 1024 | `4a4316bd1cbd7c11c9c8d0f41831d8192a73177515625d226a6f451046983c18` |
| `battlefield_meadow_clear.png` | 1536 × 1024 | `f6d0f0518931e1d5b688a32ac4ddb490c58c84a55836a5d5cc2d9ac3747264be` |
| `battlefield_meadow.png` | 1536 × 1024 | `1fcc08eb85280c5a0a485ac9ed7a1848ba2479532aae823045acfdbb735cf80d` |
| `battlefield_meadow_view_v2.png` | 1536 × 1024 | `333372431a390fc4224184d7f91ede9c9cb243fd85c84d0390b20214b078067c` |

地表图保留完整分辨率和自动 mipmap，不再限制到 1024，也不强制读取粗级 mip。参考图与去云中间图仅用于美术制作和审阅，不由战场材质加载。

初次隔离运行验证采用 `compress/mode=0` 的无损导入；本次保存方法时，工作区导入记录已为 `compress/mode=2` 的 VRAM 压缩，产物为 S3TC，`detect_3d/compress_to=0`。`mipmaps/generate=true` 与 `process/size_limit=0` 保持一致，PNG 源图及上表哈希未变。本次仅记录现值，没有回写导入配置；后续复现画面或对比性能时应同时核对压缩设置。

## 保留的美术制作方法

- 继续使用已认可的 [battlefield_meadow.png](battlefield_meadow.png) 作为草冠尺度、配色与细节概括的直接参考；[battlefield_reference.png](battlefield_reference.png) 保存完整景观和光影关系。新关卡不能仅凭“油画风”几个字重新定义风格。
- 画面保留具体的植被、水、石质地表等景物。绘画感来自自然的冷暖、明暗、边缘软硬和细节取舍，避免用粗大笔触、整片涂色或全图模糊替代景观内容。河流、坡谷等关卡主体应清楚可读，装饰水洼与石砾保持克制。
- 固定正交镜头的 45° 朝向、35.264° 俯角和 1 km 六边格尺度。用游戏实际世界坐标确定图像映射范围；本关卡的中心和跨度是当前实例参数，不能原样当成所有关卡的通用尺寸。
- 生成可观赏的完整景观绘制，保留局部材质明暗和轻微起伏感。运行资产移除云朵及大块云影，由实时天气补充；不把所有局部明暗清空后只留下均匀底色。
- 当前资产链为认可概念图、[去云中间图](battlefield_meadow_clear.png)、最终广域草原，文末保留两步真实提示词。继续制作时可直接使用现有认可资产，不必每次从头重生成整条链。
- 进入引擎后保留原生分辨率与自动 mipmap，用实际战术视距和最大缩放检查细节、格子尺度与外围衔接。当前风格试作使用单张绘制的局部取样延展，后文说明它在精确关卡地图中的适用边界。

## 按实机视角重生底片的试作

2026-10-20 使用内置 imagegen，先以基准地表图、已认可概念图、实机截图为输入生成一版，再针对草丛团块感做一次编辑。实机截图只提供镜头与尺度参考，不把六边格、蓝褐地形格、棋子或 UI 绘入地表。最终 PNG 未经程序重绘或重新着色，直接复制为 `battlefield_meadow_view_v2.png`。

约束采用当前 45° 朝向、35.264° 向下俯角的正交镜头，全画面保持相同景物尺度；要求降低植被团块的独立凸起感，保留青翠配色、概括的自然细节、极缓起伏与少量小水洼。太阳来自画面左上方，云朵与大块云影仍交给实时系统。本轮只有底片与材质引用变化，未修改镜头、着色器算法、地面几何或交互地形。

这次底片生成属于固定视角的视觉试验。生成提示词没有提供真实三维高度和法线，也不能单凭角度数字证明图内每处景物投影准确；图片保留局部绘画明暗，不能据此声称空间体积和自由重打光问题已经解决。当时使用两档采样，随后因比例过大改为下文的统一尺度取样；玩法对齐限制继续适用。

本轮在当前工作区的隔离副本中，以 Godot 4.6.3 Mono、.NET SDK 8.0.426 构建与导入，C# 构建为 0 警告、0 错误。1920×1080、GL Compatibility、RTX 3060 Laptop GPU、VSync 开启、项目 FPS 上限为 0；未更改项目设置。新图与基准均采用 S3TC / VRAM 压缩、完整 1536×1024 分辨率及自动 mipmap。共享纹理、云影缓存接线、400 个格子的状态和几何在替换前后保持一致，9 项运行检查通过；这不代表新增了玩法验证或完成美术验收。

同一进程中，在固定镜头与冻结的云缓存下关闭两版草风，以尺寸 8、20、40 截取前后对照；尺寸 20 下各采样 90 帧，主视口 GPU 中位数为基准 1.103 ms、新图 1.105 ms，draw calls 均为 3103。数值只覆盖主视口，不含云缓存子视口，可说明本次替图没有可辨认的成本变化，不作为完整性能基准。截图与运行检查产物保留在本地，未随仓库共享；本节保留验证条件与主要事实。检查结束时恢复原有天气和草风后另存实机截图。

实机审阅观察：新图的明暗与草色层次更鲜明，近距离仍可见偏大的草团和单张地表图放大的模糊感。本次生成对景物视角、体量感的修正有限；保留试用版本供用户比较，不把提示词要求写成已经实现的效果。

两次提示词如下。第一步输出为制作草稿，没有被运行材质加载；第二步输出为当前试用资产。原始生成文件保留在本地生图目录，本节保存完整提示词，便于合作者复现制作思路。

第一步：

```text
Use case: stylized-concept.
Asset type: opaque landscape ground painting for the existing fixed orthographic camera of an aerial strategy game. Create one replacement terrain backdrop, landscape 3:2 aspect ratio.

Input images and roles:
Image 1 is the currently used meadow painting: retain its delicately simplified natural landscape style and fine surface detail.
Image 2 is the approved concept painting: retain its beautiful fresh yellow-green, olive and cooler green color relationships, natural vegetation variation and very gentle landforms. Its clouds and broad cloud shadows are added by the game and must be absent in the new ground image.
Image 3 is an engine screenshot, CAMERA AND SCALE GUIDE ONLY. Match the orthographic viewpoint and the sense of landscape scale beneath the hexagons, but omit every grid line, colored hexagon, game piece and UI element.

Primary request: repaint the meadow as the actual landscape seen by a PARALLEL ORTHOGRAPHIC ISOMETRIC CAMERA looking down at 35.264 degrees below horizontal, with a 45-degree plan heading. The camera is NOT vertical top-down. There is no horizon or vanishing point. Objects and textures retain the SAME size throughout the image, with no near/far perspective scaling and no enlarged foreground clumps. The painted surface will be mapped to the ground with camera compensation; compose this image directly as the desired on-screen aerial view.

The image represents a wide rainy-season grassland extending tens of kilometers. Create fine and coherent vegetation-cover structure, alternating broad restful swathes of lush grass and more densely grown patches, with subtle broad undulations of extremely shallow slope. The grass should lie close to the landscape, not stand up as rows of rounded cushions. At this aerial scale, the visible forms are patches of vegetation and moist ground rather than individually readable grass blades, haystacks or boulders. Very occasional tiny shallow blue-grey puddles and quiet mineral accents may remain, subordinate to the grass. Small surface features must have the same isometric projection and subdued shallow volume, with little exposed vertical side. Avoid deep grooves and pronounced slopes.

Style: the approved reference's restrained painterly naturalism. Preserve convincing fine landscape structure and softly selective detail, with crisp-enough shapes in a few places and quieter connected grass areas elsewhere. Keep the sunny light greens, rich green middle values and cooler local darks. It should feel like a lush living landscape, not an abstract painting or uniform procedural texture.
Lighting: clear daylight with high sun coming from the upper-left of the image, only gentle small local occlusion and landform shading, short restrained shadows. No clouds, broad cloud shadows, atmospheric veils or edge haze; the engine supplies these dynamically.

Full-bleed continuous ground to all four edges, consistent scale and illumination across the whole image. No trees, buildings, roads, rivers, major rocks, ridges, cliffs, mountains, visible grass blades, close-up grass tufts, thick paint daubs, curly brush marks, sponge or broccoli clumps, tiled repetitions, mirrored patterns, hexagons, units, symbols, text, border or UI. Do not reproduce the blue/brown gameplay blocks from image 3.
```

第二步：

```text
Use case: stylized-concept.
Asset type: revised orthographic aerial ground painting, opaque landscape 3:2.

Image 1 is the first draft to refine. Image 2 is the approved art reference, for its fine naturalistic painterly treatment and nuanced fresh green colors.

Make one targeted improvement to image 1: correct the projection and apparent volume of the vegetation. Many of the dark marks in image 1 read as rounded bushes, little pom-pom crowns or stones scattered on a tabletop. Repaint these into CONTINUOUS LOW GRASSLAND COVER viewed in true parallel isometric projection, camera looking down 35.264 degrees below horizontal. Keep the same scale from the top to the bottom of the frame. Across the entire picture, shallow ground patches and puddles have the consistent foreshortening of an isometric ground plane; there is no enlargement of foreground details.

Keep the existing meadow's overall patch arrangement, fresh yellow-green, olive and cooler green palette, gently simplified fine landscape detail and almost flat landforms. The grass has subtly flowing, low, soft irregular cover, with quiet open swathes merging into denser cover. The shadowed parts belong to that continuous meadow rather than standing out as separate ball-shaped shrubs. Small local texture has soft selectively resolved edges as in image 2. Maintain natural detail without uniformly peppering every part with dark dots. No raised hay mounds or obvious individual grass blades. Preserve a few subdued small blue-grey puddles and faint mineral accents, but they must remain small and infrequent. Do not emphasize stones, exposed soil or drainage grooves.

Clear daylight, high sun from image upper-left, very restrained local shading; no clouds or broad cloud shadows. The game adds moving cloud shadows. No horizon, perspective convergence, fog, sky, new hills, trees, buildings, roads, river, hexagons, text, borders or UI. Avoid thick paint strokes, abstract color swirls, a featureless color wash or photorealistic sharpening. The finished image must remain a convincing delicate landscape painting in the approved style, now with low grass cover and uniform orthographic scale.
```

<a id="level-layout"></a>
## 关卡地形的后续制作约定

目标是按照关卡设计，同时得到同一片战场的美术表现和交互地形。当前蓝色、褐色地形格是河流、坡谷等障碍与交互要素的设计表达；最终画面应把这些位置表现成自然景物，其占地、连接关系与游戏实际判定对应。坡谷具体对应何种地形数据和行为，应以用户后续关卡设计为准，不直接等同于现有 Mountain 状态。

1. **关卡设计决定布局与规则。** 记录河道范围、坡谷位置、指定通路和其他交互要求，并统一六边格、世界坐标和绘制坐标。可通行性、阻挡和高度等规则以设计数据为准。
2. **把同一布局交给美术与运行时。** 从设计生成带有精确位置和区域标识的生图控制图，同时向游戏提供对应地形数据。控制图的纯色用于标明种类和边界；美术参考负责景物质感、色彩与光影。最终成图将色块表现为河道、河岸、草坡和谷地等景物。
3. **生图保持地形位置与连通关系。** 允许概括区域内部纹理和自然化边缘，但不能挪动主河道、堵塞指定通路或添加影响玩法的地形。单次生图不能保证准确遵守每个边界，需要叠加布局检查，偏离处使用受区域约束的编辑、合成或确定性材质绘制修正。玩法判定不从生成图片的颜色反推。
4. **可操作区域采用确定的空间对应。** 承载河流和坡谷的绘制必须固定在关卡坐标上。当前装饰草地使用固定世界坐标下的局部取样延展，源图中的小水洼等景物不保留唯一落点；此前的核心／外围两档缩放同样不具备这种对应。两种方法都不能直接用于必须与玩法对齐的地形。未来外围扩展应放在可操作区域之外，或保证扩展前后保留同一份内部布局。
5. **用同一坐标检查画面和交互。** 在实际镜头下叠加六边格、地形区域及通行信息，核对视觉河流是否落在设计水域、通路是否存在、坡谷边界是否对应，并检查平移和缩放后的对齐。装饰性水洼与具备规则的河流应明确区分。

例如，关卡指定一条河及一个可穿越位置，美术图应在指定范围画出连贯河道，并在指定位置表现可辨认的通路；相应交互由同一份设计数据生成。河流是否允许涉渡、坡谷是否影响移动或视线等具体规则，等待关卡要求明确后设计。本节记录目标与对应关系，不表示当前已经实现地形生成、碰撞、寻路或新的地图格式。

## 空间映射与范围

主战场仍使用边长 1 km 的平面六边格，网格、描边、单位位置和点击计算沿用现有几何。起伏感属于地表绘制，不是新增的真实山丘。

资产保留固定镜头的俯瞰绘制，因此按现有 45° 朝向、35.264° 俯角投影到世界 xz 坐标，而不再次将它作为普通正俯视贴图旋转压缩。以 `grass_texture_origin = (16.75, 14.25)` 为中心：

```text
offset = world_xz - grass_texture_origin
projected_x = (offset.x - offset.y) / sqrt(2)
projected_y = (offset.x + offset.y) / sqrt(6)
painting_uv = projected / grass_texture_span
grass_texture_span = (12, 8) km
patch_position = painting_uv * 4
cell = floor(patch_position)
local = fract(patch_position)
uv_for_corner = landscape_patch_center(cell + corner) + (local - corner) / 4
```

2026-10-20 用户指出图片本身没有问题，而是贴在战场上放得过大。本次保留 PNG、原生分辨率和导入设置，把源图全宽高对应的投影尺度由 `(48,32)` 改为 `(12,8)`，景物线性大小为原核心区域的四分之一。同一个源像素此前对应约 31.25 m，现在约 7.8125 m，均指俯瞰投影中的距离，并非精确的世界 xz 距离或生图景物的测量尺寸。六边格仍为边长 1 km，未通过改变格子或镜头制造缩小效果。

缩小后不再用整张图覆盖整个棋盘。源图不是无缝素材，因此 `sample_landscape()` 在投影坐标中每 `(3,2)` km 设置取样节点，混合四个相邻节点的局部绘制。节点在源图内部选择固定取样中心，仅平移，不旋转、镜像或改色；所有取样均保持同一比例。局部坐标始终落在源图范围内，过渡使用 `smoothstep(0.35,0.65,local)`，`textureGrad` 使用连续世界坐标的梯度，避免节点索引变化导致 mip 接缝。它延续同一绘制的草色与细节，不重新生成美术资产。

映射固定在世界坐标，不随相机平移、缩放滑动。原先核心和外围的两档缩放已删除，草地格和外围装饰平面都使用同一景物尺度，不再向外围放大三倍，也不靠钳制图像边缘填满地面。取样覆盖实际草地几何，源图中的装饰景物会在不同位置重新出现，不构成关卡布局。独立 `StylePreview` 的根节点仍在同一中心。主场景 `BattlefieldGround` 仍为 144 × 144 km、仅两个三角面的装饰平面，位于六边形顶面下方 1 m，没有碰撞，不参与格子类型、点击或地图保存。

本次比例调整在包含工作区当前代码的隔离副本中验证：Godot 4.6.3 Mono、.NET SDK 8.0.426 构建与导入通过，C# 为 0 警告、0 错误。使用同一张 PNG、相同镜头和冻结云缓存，暂时关闭两版草风，检查尺寸 8、20、40 及最大缩放下移到棋盘角落的外围；截图中未见硬接缝或外围景物再次放大。13 项资源、尺度、云缓存和棋盘状态检查通过，400 个格子的状态和几何保持原样。对照副本显式固定旧材质的 `grass_texture_span=(48,32)`，避免 Godot 材质复制时保留新参数、使比较基准失真；初次对照发现这个验证设置问题，修正后重跑通过。

性能条件为 1920×1080、GL Compatibility、RTX 3060 Laptop GPU、VSync 开启、项目 FPS 上限为 0，未更改项目设置。同一进程中尺寸 20 下各采样 90 帧，主视口 GPU 中位数由 1.009 ms 增至 1.189 ms，增加约 0.18 ms；draw calls 均为 3103。测量不含云缓存子视口。本次多两次源图读取，不增加地表纹理文件、分辨率或图像副本。截图、日志和检查脚本仅保留在本地，本段保存验证条件和主要事实；调整后的比例仍待用户审阅，不表示完成真实地形或关卡布局验收。

## 动态部分与成本

太阳、物体投影和云影仍实时计算，云影读取原有天气缓存。地表不绘制静止的云朵。草冠受光在原生 `TIME` 下连续变化，使用沿共享风向传播的非周期阵风与局部植被响应，混合直立、弯折草冠的受光；不移动整张地表图，也不改变地面几何法线。灰蓝水洼与缺少绿色的矿物色区域减弱草风响应；颜色判断仅用于美术响应，不改变格子的 Water 状态或移动规则。

每片元六次草地纹理读取：同一张地表图的四个局部取样、同一张现有噪声图的移动阵风及局部分布；此前两档缩放为四次。本次未增加纹理文件或显存中的地表图副本，云影查询沿用已有路径，外围平面绘制次数保持原状。不新增草叶实例、逐草物理、运行时数据烘焙或渲染视口。局部高光区分粗糙草冠与绘制水洼，地表的局部绘画明暗仍保留；它不是自由重打光的完整物理材质。

旧 `grassland_canopy.png` 仍由独立 `grassland_study` 样区的细节层引用，因而保留；样区是之前的独立微地形试验，不是当前主战场入口。

## 生成提示词

第一步：以保存的认可图为输入去云，结果保存为 [battlefield_meadow_clear.png](battlefield_meadow_clear.png)，只用于美术制作，不由运行材质加载。

```text
Use case: stylized-concept. PRECISE EDIT of the approved image, not a new painting. Preserve the image's exact aerial oblique camera, landscape aspect ratio, terrain/vegetation layout, scale of all surface details, delicate fine texture, restrained painterly-naturalistic handling, shapes and positions of puddles and mineral accents, and its warm yellow-green / olive / deeper green palette. The grassland should remain extremely gently rolling and mostly level, exactly as in the input. DO NOT change to a top-down camera, do not invent new grass marks, do not enlarge any clumps, and do not add more rocks or drainage.

Make only these changes: (1) remove the white clouds around the image edges and fill those regions with seamless continuation of the same ground, (2) lift the broad cast cloud-shadow patches into the landscape's normal sunlit illumination, so the whole field is seen between passing clouds. The game will supply moving clouds and their broad cast shadows. Preserve all the local subtle darker grass-cover patches, small grass-clump occlusion, gentle shallow-landform shading and fine material variations. Keep the original reference's quiet delicate surface detail; do not remove every local shadow or flatten the image into uniform color. Keep most of the ground fresh natural green, with subdued sparse water/stone/earth details.

The result is the same approved landscape with a clear view of its surface, rather than a reinterpreted texture. No thick paint daubs, curly looping grass marks, enlarged sponge-like clumps, uniform fine noise, photographic sharpening, global blur, text, border, hexagons, grid, roads or buildings. Full bleed opaque landscape image. Maintain the input's framing and fine detail scale.
```

第二步：以前一步去云结果为输入生成最终广域草原，输出即保留的基准 `battlefield_meadow.png`。

```text
Use case: stylized-concept. Extend the supplied clear-ground grassland image into a MUCH WIDER continuous surrounding landscape, seen from exactly the same fixed high aerial oblique camera. Imagine zooming out to show THREE TIMES its ground width and height: the supplied landscape occupies approximately the CENTRAL THIRD in width and height of the new image, with its puddles, terrain and vegetation reduced to the corresponding scale; fill all the surrounding area with newly painted continuous grassland matching it. Preserve a landscape 3:2 aspect ratio. This is the surrounding landscape image for an aerial strategy-game ground, not a separate framed picture.

The environment remains a mostly level lush rainy-season plain, with extremely gentle shallow landform variation. Match the reference's sunny yellow-green, olive and cool deeper-green vegetation, small-scale grass-cover structure, gently simplified naturalistic details and subdued painterly handling. Broad calm areas alternate with subtly denser/thinner grass. Keep puddles and mineral details very sparse, small and low contrast. Because this is a much wider view, individual marks are finer and more distant; do not enlarge grass clumps, stones or hills. Keep coherent natural-looking vegetation detail and local shading throughout, without conspicuous brush daubs, curls, sponge clumps or a uniformly noisy camouflage texture.

Clear uninterrupted view of the ground with no clouds and no broad cloud cast shadows; these are added dynamically in the game. Keep subtle local material and shallow-ground shading. No distinct hills, carved drainage networks, trees, buildings, roads, sky, horizon, grids, text, borders or vignette. No tiling, mirroring, bilateral symmetry or repeated copies of the reference. Do not paste the input as a visible rectangle: its central landscape should join organically into newly generated surroundings.
```
