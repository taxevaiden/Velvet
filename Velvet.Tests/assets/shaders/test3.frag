#version 450

layout(location = 0) in vec2 fsin_UV;
layout(location = 1) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

layout(set = 0, binding = 0) uniform texture2D Texture2D;
layout(set = 0, binding = 1) uniform sampler Sampler;
layout(set = 0, binding = 2) uniform Globals
{
    float hehe;
};

void main()
{
    vec4 color = texture(sampler2D(Texture2D, Sampler), fsin_UV + vec2(sin(fsin_UV.y * 100.0f + hehe) * 0.0025, sin(fsin_UV.x * 100.0f + hehe) * 0.0025));
    vec4 final = color * fsin_Color;
    fsout_Color = final;
}