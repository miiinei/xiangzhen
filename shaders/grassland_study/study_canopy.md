# 草原样区美术资产

`study_canopy.png` 于 2026-10-20 使用 Codex 内置 imagegen 工具生成，输出直接复制进项目，未通过代码重新绘制。原图为 1254×1254、不透明 RGB；运行时导入到 1024×1024，启用 mipmap。该图只供独立样区使用，完整覆盖 40×40 km 的展示地表，不作为无缝循环贴图。游戏主棋盘仍使用原有素材。

第一张输入是根据 `StudyTerrain.vegetation_at()` 生成的正俯视配色布局：深色浅洼从左上经中心延伸到右下。第二张输入是用户认可的草原概念图，仅提取油画笔触、草色关系和植被组织。生成图保留了主要色带，但并非逐像素遵循控制图。生图中的笔触与色差属于美术处理；高度、法线、植被响应仍由工程数据决定，不能从这张图推断真实草高。

没有将概念图中的云、水面、天空或整体日照烘焙进资产。实际太阳和云影由样区着色器及现有 CloudWeather 计算；资产自身仍有局部笔触明暗，不应当作扫描得到的物理反照率。

## 生成提示词

Use case: stylized-concept. Create a production square top-down GRASS CANOPY ALBEDO texture, preferably 2048 x 2048, for an aerial strategy game. Image 1 is the spatial composition guide: preserve its broad gently winding darker-green swale running from upper left through the centre to lower right, surrounded by connected lighter meadow masses. Retain the guide's overall layout and relative placement, while simplifying the tiny scattered blobs into calmer connected shapes. Image 2 is the approved visual STYLE reference only: extract its delicate painterly oil-painting character, layered vegetation marks, and natural restrained green colour harmony. Do NOT copy the oblique camera, hills, water, clouds or directional lighting from image 2.

Render as a perfectly flat orthographic straight-down vegetation colour map under neutral uniform overcast illumination. All ground is fresh living rainy-season grass, no dead grass or bare soil. The majority of the light green meadow areas must be calm and open with fine subtle directional brushed grass texture. In and along the dark green swale, add richer, denser, longer-grass vegetation with small overlapping elongated brush marks and irregular interlocking green masses. Favour brush-like flowing grass structure, not round sponge-like broccoli bushes, forest tree crowns or noisy pebbles. Detail is concentrated in the dense dark grass areas and their broken margins; leave quiet areas between them. Clear scale hierarchy: broad quiet meadow masses first, medium connected vegetation groups second, fine restrained brushwork last.

Colours: fresh natural medium meadow green, muted yellow-green and cool deep emerald green, moderately saturated, enough warm/cool richness to feel beautifully painted. No fluorescent lime, no cyan, no brown. This is an evenly lit albedo asset for dynamic in-engine lighting: no cast shadows, no cloud shadows, no baked hill-side shading, no highlights or glare, no rendered hills or mountains, no apparent ground elevation or ambient occlusion. Do not add a strong global light gradient. Full bleed opaque square. No horizon, sky, clouds, water, paths, rocks, trees, flowers, buildings, grids, labels, frame, text or logo. Do not mirror or repeat the composition. Keep detailed but readable organic painting, avoid uniform all-over texture noise.
