
# 极简多边形材质

## 缩放与晴天阴影修正

### 模型底边毛刺参数

最新设置优先于下文历史说明：Sun 的 `shadow_bias=0.2`、`shadow_normal_bias=2.0`，Piece/BodyMesh 的 `cast_shadow=2`（双面投射）。已在真实 Tile 地面上检查近距离截图，避免仅凭阴影像素数判断伪影是否消失。云影未改动。

| 文件/节点 | 参数 | 当前值 | 调整建议 |
| --- | --- | --- | --- |
| world/world.tscn / Sun | shadow_normal_bias | 2.0 | 优先控制表面毛刺，建议 1.5–2.5；过小可能出现条纹，过大可能缩短或偏移影子 |
| world/world.tscn / Sun | shadow_bias | 0.2 | 深度偏移，建议 0.1–0.3；过大可能让投影脱离脚底 |
| world/world.tscn / Sun | shadow_blur | 0.2 | 边缘过滤，建议 0.1–0.35；增大会软化，不解决自阴影根因 |
| shaders/low_poly.gdshader | object_shadow_edge | 0.32 | 阴影采样过渡，建议 0.25–0.45；过小会重新显出块状采样 |
| template/Piece/Piece.tscn / BodyMesh | cast_shadow | 2 | 双面投影，补齐底边遮挡；继承 Piece 的 Enemy 同样生效 |
| project.godot | lights_and_shadows/directional_shadow/size | 8192 | 降到 4096 可省资源，但轮廓精度下降 |

编辑主场景 Sun 可直接在 Inspector 的 Shadow 中调节。独立预览使用 shaders/StylePreview.tscn 的 Sun，需要同步相同参数。不要把法线偏移直接归零：当前网格会产生大片自阴影条纹。建议每次只改一个参数，并同时检查底边、地面和模型侧面。

正交相机不再随着放大降低物理高度，缩放仅改变 Size，保留鼠标缩放锚点。物理距离按 MaxCameraDistance + BaseCameraSize 设置，当前为 60；默认 Size 8–40 时整幅视野的射线起点均高于 Y=7 的云层。若将云层大幅抬高或扩大缩放范围，需重新检查相机与云层的间距。

云影已恢复原状：继续与白云本体共用 `cloud_softness`，`cloud_shadow_strength=0.52`，不再有独立的 cloud_shadow_softness 参数。保留你修改的云形、速度、覆盖率和透明度。环境补光恢复 `ambient_strength=0.32`、直射光恢复 `direct_strength=0.65`。

晴天硬阴影针对 Piece 等不透明模型的真实落地投影：`low_poly.gdshader` 的 `object_shadow_edge` 默认 0.32，保留窄边缘的过滤覆盖率，避免低阈值把阴影贴图台阶重新放大。它是采样值域的过渡宽度，不是世界空间模糊半径；与云影计算无关。使用该 shader 的地块和模型均接收此效果，其他材质仍使用其原始光照逻辑。

共同光源为 world 场景中的 Sun，旋转为 (-40, -30, 0) 度，从天空照向地面。已修正旧 Transform3D 中太阳方向朝下、实际从地下打光的问题。云影直接在 light() 中读取真实方向光的 LIGHT 并转换到世界空间，不再使用独立的 sun_direction uniform。旋转 Sun 即可同时改变棋子照明、实体落地投影和云影方向，云形、云影柔度与深浅保持原值。

Sun 使用小范围阴影过滤 shadow_blur=0.2、四级联模式、85 米阴影范围，适配当前远置正交相机；方向光阴影贴图提高为 8192，配合窄边缘过滤，保持暗部色块和清晰轮廓。相较 4096，阴影贴图像素数为四倍，会增加显存和阴影渲染开销，尚未做 GPU 性能基准；低配可在 project.godot 将 lights_and_shadows/directional_shadow/size 调回 4096，但近看可能更容易看到台阶。偏移为 shadow_bias=0.1、shadow_normal_bias=1。太阳仰角为 40 度，使矮棋子的影子延伸到轮廓外。上层半透明白云会降低下方投影的可见对比。这是极简色块方向的近似实现，不是复刻其他游戏的专有 shader。

## 草原版本更新

当前改为分散的多瓣白云，不再使用连续噪声云幕；以下新参数覆盖下文旧版表格中的默认值。预览场景已启用草风，主世界只有 Grass 地块启用，棋子、水和山地不受影响。

| 参数 | 当前默认值 | 调整方式 |
| --- | --- | --- |
| `base_color` / Tile.GrassColor | (0.48, 0.68, 0.28) | 明亮草绿，地块运行时以 C# 颜色为准 |
| `ambient_strength` | 0.32 | 提高阴面亮度也会削弱阴影对比 |
| `direct_strength` | 0.65 | 阳面亮度，建议 0.45–0.8 |
| `cloud_scale` | 0.065 | 越小云团和间距越大 |
| `cloud_coverage` | 0.65 | 现在是云团出现概率，降低会留下更多空隙 |
| `cloud_warp` | 0.12 | 轻微扰动多瓣轮廓 |
| `cloud_softness` | 0.035 | 云缘柔度，建议 0.02–0.06 |
| `cloud_shadow_strength` | 0.52 | 云影对比，建议 0.35–0.6 |
| `cloud_opacity` | 0.6 | 白云本体透明度 |
| `fog_strength` | 0.07 | 极轻大气褪色 |
| `grass_wind_strength` | 普通材质 0，草地 0.45 | 草地值在 StylizedMaterials.GrassWindStrength 调整 |
| `grass_wind_direction` | (0.92, 0.38) | 风带移动方向，自动归一化 |
| `grass_wind_speed` | 3 | 世界单位/秒 |
| `grass_wind_spacing` | 32 | 相邻阵风距离，越大越稀疏 |
| `grass_wind_width` | 3.5 | 风带半宽 |

默认风带约每 10.7 秒经过一次，带缓慢弯曲扰动，跨格子连续，仅改变向上草面颜色，并非草叶几何摆动。云仍是透明平面上的风格化云团，不是体积积云。太阳保留真实投影，天空背景改为浅蓝。

性能更新：云场每个方向光的表面光照调用为 1 次噪声采样加固定 3×3 邻域解析轮廓计算，不再是旧版的 2 次采样；增加了固定 ALU 开销，尚未进行 GPU 性能基准。两份材质的云形参数仍需同步，太阳方向自动从实际灯光读取。场景应只保留一个太阳方向光。

以下为初版设计说明，涉及旧默认值及旧采样次数时以上表为准。

面向 Godot 4.6 / GL Compatibility 的低采样方案，参考干净色块的美术方向，不复制游戏原始 shader。

## 文件与接入

- `low_poly.gdshader`：不透明地块、棋子、建筑共用的面光照。
- `cloud_layer.gdshader`：半透明高空云层。
- `cloud_field.gdshaderinc`：两者共用的世界坐标云场。
- `low_poly.tres` / `cloud_layer.tres`：可在 Inspector 中调参的模板。
- `cloud_noise.tres`：共享的 512×512 无缝双倍频 Perlin 噪声，仅资源生成时计算。
- `StylePreview.tscn`：独立预览，编辑器中打开后按 F6。

当前地块和棋子的运行时材质已通过 `world/StylizedMaterials.cs` 接入；保留原来的地形颜色、敌我颜色与选中高亮。攻击、路径和格子描边维持原有信息材质。其他不透明模型可直接挂 `low_poly.tres`，修改颜色时先复制材质，不必复制 shader 或噪声。

所有风格参数应在 `low_poly.tres` 中统一设置；材质工厂复制模板，只覆盖 `base_color` 和 `selection_glow`。修改模板后重新运行场景，已创建的副本不会自动追踪模板的新参数。

## 面光照与配色

| 参数 | 默认值 | 调整方式 |
| --- | --- | --- |
| `base_color` | 草绿 | 物体固有色；棋子/地块由原 C# 颜色控制 |
| `ambient_color` | 冷灰蓝 | 阴面补色，避免纯黑；与暖色棋子形成冷暖关系 |
| `ambient_strength` | 0.48 | 阴面亮度，推荐 0.3–0.6 |
| `direct_strength` | 0.85 | 方向光贡献，推荐 0.6–1.2 |
| `light_steps` | 3 | 光照档位，2 更硬朗，4–5 更温和 |
| `selection_glow` | 黑 | 选中补光，由 C# 控制 |

片元导数重建每个三角面的几何法线，不依赖模型平滑法线；在单个方向光下，同方向的平面具有一致的基础光照。刻意保留云影、真实投影和雾的连续变化，因此开启这些效果后，不承诺整面每个像素完全同色。严格纯色面可将 `cloud_shadow_strength`、`fog_strength` 归零并关闭灯光阴影。

保留一个 `DirectionalLight3D`，可直接调灯光颜色、能量和阴影。环境光在 shader 内统一计算，不叠加 WorldEnvironment 的环境光。默认无高光，无纹理细节。不要再叠加多个方向光，否则会累加亮度。

## 巨大尺度的云与云影

世界场景已预放置 `CloudLayer` 平面，高度 7，尺寸 100×100，覆盖当前棋盘；它不投射原生阴影。地面和棋子沿太阳方向投影采样同一云场，得到跨物体连续的柔和云影，不需要第二个阴影贴图。

以下参数来自共享 include，但仍是各材质独立的 uniform。请在 `low_poly.tres` 和 `cloud_layer.tres` 中设置相同值：

| 参数 | 默认值 | 调整方式 |
| --- | --- | --- |
| `cloud_scale` | 0.035 | 越小云块越巨大；推荐 0.015–0.05 |
| `cloud_velocity` | (0.22, 0.09) | 世界单位/秒；减半会更沉稳，不是 UV 速度 |
| `cloud_warp` | 0.22 | 第二次采样扰动轮廓，推荐 0.1–0.35 |
| `cloud_coverage` | 0.46 | 越大云覆盖越多 |
| `cloud_softness` | 0.18 | 密度阈值过渡，越大云缘/云影越柔 |
| `cloud_height` | 7 | 接收面到云层的投影距离；同步 CloudLayer 的全局 Y |

太阳方向自动读取实际方向光，不需要在材质中手动同步。仅需调整场景 Sun 的旋转；太阳必须从地面上方照向地面。

| 独立参数 | 所属材质 | 调整方式 |
| --- | --- | --- |
| `cloud_shadow_strength` | low_poly | 默认 0.32；0 关闭，推荐不超过 0.45，保留阴面环境补光 |
| `cloud_opacity` | cloud_layer | 默认 0.13；推荐 0.06–0.18，避免遮住战术信息 |
| `cloud_color` | cloud_layer | 云本体颜色，默认偏冷白 |
| `edge_fade` | cloud_layer | 平面边缘淡出范围，默认 UV 0.15 |

噪声是无缝贴图，最终仍会周期重复，但大尺度平移加低频扰动可避免短时间机械循环；不是非周期体积模拟。`TIME` 使用 Godot 默认回绕时间，长时间运行可能在回绕时跳变。云平面有限大：扩大棋盘时扩大平面并重新居中；接收面云影不受平面边缘淡出影响，应保证可玩区域位于其内部。

## 可选增强

| 参数 | 默认值 | 调整方式 |
| --- | --- | --- |
| `fog_color` | 冷灰青 | 远处逐渐褪向该颜色，建议接近背景色 |
| `fog_start` / `fog_end` | 35 / 95 | 相机空间距离起止；正交相机也生效，end 必须大于 start |
| `fog_strength` | 0.16 | 轻微大气透视；0 关闭，推荐 0.05–0.2 |
| `vertex_ao_strength` | 0 | 读取模型顶点色 R 作为烘焙 AO，0 黑、1 白；无顶点 AO 时保持 0 |

雾是材质内的距离褪色，不采样深度、不影响 UI/战术预览、不是体积雾。顶点 AO 只能表达烘焙遮蔽，不会随棋子移动生成接触阴影；当前接触处依靠 Sun 的真实阴影。GL Compatibility 不支持原生 SSAO：如需动态 SSAO，需切换 Forward+ 并在 WorldEnvironment 开启 SSAO，该增强不属于此低成本方案。

## 性能与建议顺序

每个不透明片元两次共享纹理采样；云平面也两次，无屏幕颜色/深度读取、无 raymarch、无逐帧 C# 更新。噪声资源共享，但棋子和地块仍各有材质副本，不承诺合批。云透明平面会产生一次较大面积的透明过绘；可缩小平面或隐藏节点。仅将云影强度设为 0 并不保证驱动消除采样。

先关闭雾和云，确定固有色、环境强度和三档方向光；再开低强度云影，最后开极轻的云本体与雾。云块跨度大于多个格子、移动远慢于棋子，尺度感最稳定。不要靠过强 AO、浓雾或高不透明云遮挡棋盘。
