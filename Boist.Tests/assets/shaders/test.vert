#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 UV;
layout(location = 2) in vec4 Color;

layout(location = 0) out vec2 fsin_UV;
layout(location = 1) out vec4 fsin_Color;
layout(set = 0, binding = 2) uniform Globals
{
    mat4 Projection;
};

layout(set = 0, binding = 3) uniform Uniforms
{
    float time;
};

void main()
{   
    vec2 new = vec2(Position.x, Position.y + sin(Position.x / 50.0f + time * 10.0f) * 10.0f);
    gl_Position = Projection * vec4(new, 0.0, 1.0);
    fsin_UV = UV;
    fsin_Color = Color;
}