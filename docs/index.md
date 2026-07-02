---
_layout: landing
---

# Boist

Heavily unfinished suite of libraries for .NET 10 inspired by [raylib](https://github.com/raysan5/raylib) and [LOVE2D](https://github.com/love2d/love)

## What's done?

- Basic windowing
- Drawing rectangles, circles, arbitrary polygons, and text with color
- Textures
- Shaders with uniform support
- Render textures
  - Multisampling supported
- Keyboard and mouse input
- All four major graphics APIs
  - D3D11
  - Vulkan
  - Metal
  - OpenGL
- Audio (that can be 3D)

## Known issues

- (OpenGL) Drawing a rectangle (that has a `BoistTexture` applied to it) on a multisampled `BoistRenderTexture` results in the rectangle being invisible
- (Kinda fixed I haven't found a better solution for this) OpenGL has its texture coordinate origin in the bottom-left, whereas D3D11, Vulkan, and Metal have it in the top-left. This results in `BoistRenderTexture`s being flipped across the X-axis, so to compensate, **Boist flips the UVs of any rectangle rendered with a `BoistRenderTexture` applied.** Keep this in mind when writing custom shader code!

## Support

| Operating System | Direct3D 11 | Vulkan | Metal | OpenGL |
| -                | -           | -      | -     | -      |
| Windows | Yes | Yes | No | Yes |
| macOS | No (Possibly if you use Wine but you would have to build for Windows) | Yes[^1] | Yes | No[^2] |
| Linux | No (Possibly if you use Wine but you would have to build for Windows) | Yes | No | Yes |

[^1]: Requires MoltenVK

[^2]: OpenGL was deprecated in macOS 10.14 in favor of Metal.


# License

This library is available under the [MIT license.](https://github.com/taxevaiden/Boist/blob/main/LICENSE)