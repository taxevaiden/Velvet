#version 450

// Ported from cmdFilter

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
    const float pixel_size = 4.0;
    vec2 block_size = pixel_size / Resolution.xy;

    const int N = 4;

    const float bayerMatrix[16] = float[16](
        0.0,  8.0,  2.0, 10.0,
        12.0, 4.0, 14.0, 6.0,
        3.0, 11.0, 1.0, 9.0,
        15.0, 7.0, 13.0, 5.0
    );

    vec2 pixelated_coords = floor(fsin_UV / block_size) * block_size;

    vec4 pixel = texture(sampler2D(Texture2D, Sampler), pixelated_coords);
    float largest = max(pixel.r, max(pixel.g, pixel.b));

    // ensure largest isn't zero, so we don't divide by zero (that would be bad!!)
    if (largest == 0.0) {
        fsout_Color =  vec4(0.0);
    }

    int x = int(mod(Resolution.x * fsin_UV.x, float(N)));
    int y = int(mod(Resolution.y * fsin_UV.y, float(N)));
    int index = y * N + x;

    float threshold = (bayerMatrix[index] + 0.5) / 16.0;

    float ditheredValue = largest < threshold ? 0.0 : 1.0;

    fsout_Color = vec4((pixel.rgb / largest) * ditheredValue, pixel.a);
}