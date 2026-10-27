# 雨季草原草冠贴图

本文件记录上一版资产及其原使用方式。当前主战场已改用 [battlefield_meadow.png](battlefield_meadow.md)；本图仍作为独立 `grassland_study` 样区的细节素材保留。

本资产于 2026-10-20 使用 Codex 内置 `imagegen` 生成功能制作，用于草地材质，非照片或程序噪声。参考为同一任务中用户认可的草原概念图，提取其油画感、成片草色和冷暖关系。最终源 PNG 保持生成结果，不另行绘制云、地图或地形。

`grassland_canopy.png` 为 1254×1254 RGB 图像。项目导入设置将运行时纹理限制到 1024，生成 mipmap，使用无损压缩。材质以世界坐标采样：64 km 跨度的第 4 级 mip 提供平缓色块，较小尺度的镜像采样提供弱笔触明暗。当前棋盘和 40×40 km 的美术预览地面各自都处于大色块采样的同一张图范围内，不在单个六边形上重复贴图。细节的镜像采样保证接边处颜色连续。该资产没有被认证为严格无缝贴图，扩大地表范围前需要检查大色块的重复边缘。

贴图提供植被固有色与笔触，太阳受光、宽高光、云影和草浪由 `grassland.gdshader` 实时计算。草浪不滚动这张贴图，不包含新的地形高度或水文数据。基础色参数作为乘法调色，默认 (0.86, 0.96, 1)，稍微收住黄绿色。

## 生成提示词

Use case: stylized-concept. Asset type: square production albedo / base-color texture for the ground of an aerial Godot strategy game. Input image: visual reference for painterly vegetation colour and broad brush-like surface organization ONLY. Transform the supplied oblique landscape into a completely flat, orthographic, straight-down GRASS CANOPY COLOUR MAP. Full-bleed square, ideally 1024x1024. The entire image is lush living wet-season grassland. Preserve the reference's restrained oil-painting-like handling, organized broad green masses, warm yellow-green and cool emerald-green colour relationships, and quiet flowing brush strokes. Rich fresh green with moderate saturation, natural and luminous, no fluorescent lime. Use a hierarchy: large connected calm colour areas occupy most of the map, with subtle medium-scale directional painterly canopy strokes providing a little character. Keep readable larger shapes; avoid all-over noisy speckling and camouflage. The image should look like a beautifully painted surface colour map that can be lit dynamically in a game, NOT a finished scene or a concept-art landscape. Lighting: neutral, evenly lit albedo, no baked sunlight direction, no cast shadows, no cloud shadows, no highlights/glare, no ambient occlusion, no raised hills, no apparent mountains or three-dimensional terrain shading. No clouds, sky, horizon, fog, water, rivers, ponds, paths, trees, rocks, bare soil, brown dry patches, flowers, buildings, grids, text, frames, logos or border. No individual grass blades or photographic microdetail. Continuous vegetation reaches all four edges; make opposite edges visually compatible for repeating. Do not copy the perspective or the original scene's composition. Opaque colour texture.
