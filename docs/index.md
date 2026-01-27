---
_layout: landing
---

# Velvet

heavily unfinished framework for .net 10

you can make games, tools, whatever

there are better frameworks/libraries out there that can do a lot more than this library can, like [Bliss](https://github.com/MrScautHD/Bliss). this was only made for fun and to learn more about how graphics and rendering work.

## features

- basic window
- drawing rectangles, circles, any polygon you could think of, with color!
- shaders
  - uniforms supported
- render textures
  - multisampling supported (however, drawing a VelvetTexture to a multisampled VelvetRenderTexture does not do anything on the OpenGL backend. the Vulkan backend also doesn't render the multisampled VelvetRenderTexture smoothly. D3D11 however has no issues!)
  - OpenGL behaves differently from all the other GraphicsAPIs when drawing to a VelvetRenderTexture. all texture samples are flipped across the x-axis. VelvetTextures will still work fine on OpenGL and behave like all the other GraphicsAPIs, however VelvetRenderTextures don't. **in order to compensate, Velvet flips the UVs of anything drawn across the x-axis when using OpenGL to render a VelvetRenderTexture. keep this in mind when writing shader code!**
- textures
- input from keyboard and mouse
- support for all four major graphics APIs (if OpenGL counts)
  - D3D11
  - Vulkan
  - Metal
  - OpenGL

## support

| Operating System | Direct3D 11 | Vulkan | Metal | OpenGL |
| -                | -           | -      | -     | -      |
| Windows | Yes | Yes | No | Yes |
| macOS | No (Possibly if you use Wine but you would have to build for Windows) | No ( Hasn't been implemented yet) | Yes | No (OpenGL is depracated on macOS) |
| Linux | No (Possibly if you use Wine but you would have to build for Windows) | Yes | No | Yes |
| iOS (Not implemented) |
| Android (Not implemented) |

# license

this framework is available under the [MIT license.](https://github.com/taxevaiden/Velvet/blob/main/LICENSE)