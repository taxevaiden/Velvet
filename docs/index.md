---
_layout: landing
---

# Velvet

heavily unfinished framework for .net 10

you can make games, tools, whatever

there are better frameworks/libraries out there that can do a lot more than this library can, like [Bliss](https://github.com/MrScautHD/Bliss). this was only made for fun and to learn more about how graphics and rendering work.

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

- (OpenGL) Drawing a rectangle (that has a `VelvetTexture` applied to it) on a multisampled `VelvetRenderTexture` results in the rectangle being invisible
- (Kinda fixed I haven't found a better solution for this) OpenGL has its texture coordinate origin in the bottom-left, whereas D3D11, Vulkan, and Metal have it in the top-left. This results in `VelvetRenderTexture`s being flipped across the X-axis, so to compensate, **Velvet flips the UVs of any rectangle rendered with a `VelvetRenderTexture` applied.** Keep this in mind when writing custom shader code!

## Support

| Operating System | Direct3D 11 | Vulkan | Metal | OpenGL |
| -                | -           | -      | -     | -      |
| Windows | Yes | Yes | No | Yes |
| macOS | No (Possibly if you use Wine but you would have to build for Windows) | Yes[^1] | Yes | No[^2] |
| Linux | No (Possibly if you use Wine but you would have to build for Windows) | Yes | No | Yes |

[^1]: Requires MoltenVK

[^2]: OpenGL was deprecated in macOS 10.14 in favor of Metal.


# License

This library is available under the [MIT license.](https://github.com/taxevaiden/Velvet/blob/main/LICENSE)