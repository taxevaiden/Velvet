# Boist.Graphics

A cross-platform graphics library supporting Direct3D 11, Vulkan, Metal, and OpenGL. Shader code is written in SPIR-V style GLSL.

It does not matter what windowing system you use, as long as you provide the native windowing handles appropriate for your platform along with OpenGL functions. 

You can obtain these through `Boist.Windowing.Window`, or manually through libraries like SDL3 or GLFW.