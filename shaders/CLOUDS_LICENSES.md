# 云渲染参考与许可证

本项目的三维云密度、视线积分和太阳透射实现参考了以下 MIT 开源项目。
针对本项目的 GL Compatibility、正交俯视镜头、场景深度和地面云影做了独立适配。
没有打包这些项目的插件、天空系统或贴图；二维和三维噪声都由 Godot 在本地生成。

## Clay John / godot-volumetric-cloud-demo

- 来源：https://github.com/clayjohn/godot-volumetric-cloud-demo
- 参考版本：`6f5ecab924fa1493d5e9eeea6dc7ae5b684eb184`
- 参考内容：高度密度分布、视线步进累积与沿太阳方向计算透射。
- 原项目是 Vulkan 天空着色器示例；本项目使用空间着色器重建正交相机视线。

原始 MIT 许可证：

Copyright (c) 2007-2021 Juan Linietsky, Ariel Manzur.
Copyright (c) 2014-2021 Godot Engine contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## MangoButtermilch / Godot-volumetric-renderer

- 来源：https://github.com/MangoButtermilch/Godot-volumetric-renderer
- 参考版本：`9783f079e4672fdfafcad8ac90dd2573f3961cf2`
- 参考内容：空间着色器中的三维密度采样及太阳方向二次采样。
- 该项目定位为示例；本项目未导入其体积贴图和控制参数体系。

原始 MIT 许可证：

MIT License

Copyright (c) 2023 MangoButtermilch

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Sebastian Lague / Clouds

- 来源：https://github.com/SebLague/Clouds
- 参考版本：`fcc997c40d36c7bedf95a294cd2136b8c5127009`
- 参考内容：三维主体和细节侵蚀分离、沿太阳方向积分密度、将太阳消光与视线消光分开。原项目采用 Unity，本项目使用 Godot 自带噪声并适配 GL Compatibility。

原始 MIT 许可证：

MIT License

Copyright (c) 2019 Sebastian Lague

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.


## Valerie Bettaque / volumetric-clouds

- 来源：https://github.com/vbettaque/volumetric-clouds
- 参考版本：`8b4b11349a1f8d0ee1a4fc357ff28a6580e27d9c`
- 参考内容：Perlin 与 cellular 组合云体、边缘侵蚀，以及向近处集中太阳方向采样以保留云内明暗。原项目采用计算着色器，本项目没有导入其天空系统。

原始 MIT 许可证：

MIT License

Copyright (c) 2023 Valerie Bettaque

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
