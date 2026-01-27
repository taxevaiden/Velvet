#version 450

// Ported from cmdFilter (Never pushed to the repository)

layout(location = 0) in vec2 fsin_UV;
layout(location = 1) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

layout(set = 0, binding = 0) uniform texture2D Texture2D;
layout(set = 0, binding = 1) uniform sampler Sampler;
layout(set = 0, binding = 2) uniform Globals
{
    vec2 Resolution;
};

void main()
{
    const int range = 1;
    const float y_interval = 0.05;
    const float cbcr_interval = 0.0125;
    const float pixel_size = 8.0;
    vec2 block_size = pixel_size / Resolution.xy;

    vec2 pixelated_coords = floor(fsin_UV / block_size) * block_size;
    // vec4 sum = vec4(0.0);
    // float count = 0.0;

    // for (int y = -range; y <= range; y++)
    // {
    //     for (int x = -range; x <= range; x++)
    //     {
    //         vec2 offset = vec2(x, y) / Resolution.xy;
    //         vec2 sampleUV = fsin_UV + offset;
    // 
    //         // Clamp to this 8x8 block
    //         sampleUV = clamp(
    //             sampleUV,
    //            pixelated_coords,
    //             pixelated_coords + block_size
    //         ); 
    // 
    //         sum += texture(sampler2D(Texture2D, Sampler), sampleUV);
    //         count += 1.0;
    //     }
    // }

    // vec4 yTex = sum / count;
    vec4 yTex = texture(sampler2D(Texture2D, Sampler), fsin_UV + (pixelated_coords - fsin_UV) * 0.06125);
    vec4 cbcrTex = texture(sampler2D(Texture2D, Sampler), fsin_UV + (pixelated_coords - fsin_UV) * 0.25, 3.0);

    float y = dot(yTex.rgb, vec3(0.299, 0.587, 0.114));
    y = round(y / y_interval) * y_interval;
    float cb = (cbcrTex.b - y) * 0.564;
    cb = round(cb / cbcr_interval) * cbcr_interval;
    float cr = (cbcrTex.r - y) * 0.713;
    cr = round(cr / cbcr_interval) * cbcr_interval;

    vec3 rgb;
    rgb.r = y + 1.403 * cr;
    rgb.g = y - 0.344 * cb - 0.714 * cr;
    rgb.b = y + 1.773 * cb;

    fsout_Color = vec4(rgb, 1.0);
}