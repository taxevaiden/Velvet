#version 450

layout(location = 0) in vec2 fsin_UV;
layout(location = 1) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

layout(set = 0, binding = 0) uniform texture2D Texture2D;
layout(set = 0, binding = 1) uniform sampler Sampler;

void main()
{
    vec4 color = texture(sampler2D(Texture2D, Sampler), fsin_UV);
    vec4 final = color * fsin_Color;
    fsout_Color = vec4(1.0 - vec3(final.r, final.g, final.b), final.a);
}