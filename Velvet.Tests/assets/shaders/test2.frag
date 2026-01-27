#version 450

layout(location = 0) in vec2 fsin_UV;
layout(location = 1) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

layout(set = 0, binding = 0) uniform texture2D Texture2D;
layout(set = 0, binding = 1) uniform sampler Sampler;

void main()
{
    vec2 transformed = (fsin_UV * 2.0) - vec2(1, 1);
    float mask = (pow(abs(transformed.x), 5.2) + pow(abs(transformed.y), 5.2) <= 1.0) ? 1.0 : 0.0;
    vec4 color = texture(sampler2D(Texture2D, Sampler), fsin_UV);
    vec4 final = color * fsin_Color;
    fsout_Color = vec4(final.rgb, final.a * mask);
}