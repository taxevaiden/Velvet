using FreeTypeSharp;

using Velvet.Graphics.Textures;

using static FreeTypeSharp.FT;

namespace Velvet.Graphics
{
    struct glyph_info
    {
        public int x0, y0, x1, y1; // coords of glyph in the texture atlas
        public int x_off, y_off;   // left & top bearing when rendering
        public int advance;        // x advance when rendering
    };

    /// <summary>
    /// Represents a font that can be used for text rendering.
    /// </summary>
    public unsafe class VelvetFont : IDisposable
    {
        private FT_FaceRec_* _face;
        private FT_LibraryRec_* _library;
        /// <summary>
        /// The texture atlas containing the glyphs for this font. Each glyph is rendered into the texture atlas at initialization.
        /// </summary>
        public VelvetTexture TextureAtlas { get; private set; } = null!;
        /// <summary>
        /// The font size in pixels. This is determined at initialization and cannot be changed. To use a different font size, create a new instance of VelvetFont with the desired size.
        /// </summary>
        public int FontSize { get; private set; }
        internal glyph_info[] glyphs = new glyph_info[128];

        // Constructors

        /// <summary>
        /// Initializes a new instance of the VelvetFont class from a font file.
        /// </summary>
        /// <param name="renderer">The VelvetRenderer to use for creating the texture atlas.</param>
        /// <param name="fontPath">The path to the font file.</param>
        public VelvetFont(VelvetRenderer renderer, string fontPath)
        {
            FT_Error err;

            fixed (FT_LibraryRec_** lib = &_library)
            {
                err = FT_Init_FreeType(lib);
                if (err != FT_Error.FT_Err_Ok)
                    throw new Exception($"FT_Init_FreeType failed: {err}");
            }

            byte[] pathBytes = System.Text.Encoding.UTF8.GetBytes(fontPath + '\0');

            fixed (FT_FaceRec_** face = &_face)
            {
                fixed (byte* pathPtr = pathBytes)
                {
                    err = FT_New_Face(_library, pathPtr, 0, face);
                    if (err != FT_Error.FT_Err_Ok)
                        throw new Exception($"FT_New_Face failed: {err}");
                }
            }

            CreateAtlas(renderer);
        }

        /// <summary>
        /// Initializes a new instance of the VelvetFont class from a font file.
        /// </summary>
        /// <param name="renderer">The VelvetRenderer to use for creating the texture atlas.</param>
        /// <param name="fontPath">The path to the font file.</param>
        /// <param name="size">The font size.</param>
        public VelvetFont(VelvetRenderer renderer, string fontPath, int size)
        {
            FT_Error err;

            fixed (FT_LibraryRec_** lib = &_library)
            {
                err = FT_Init_FreeType(lib);
                if (err != FT_Error.FT_Err_Ok)
                    throw new Exception($"FT_Init_FreeType failed: {err}");
            }

            byte[] pathBytes = System.Text.Encoding.UTF8.GetBytes(fontPath + '\0');

            fixed (FT_FaceRec_** face = &_face)
            {
                fixed (byte* pathPtr = pathBytes)
                {
                    err = FT_New_Face(_library, pathPtr, 0, face);
                    if (err != FT_Error.FT_Err_Ok)
                        throw new Exception($"FT_New_Face failed: {err}");
                }
            }

            CreateAtlas(renderer, size);
        }

        // Helpers

        private void CreateAtlas(VelvetRenderer renderer, int size = 48)
        {
            FontSize = size;

            FT_Error err;
            err = FT_Set_Pixel_Sizes(_face, 0, (uint)size);
            if (err != FT_Error.FT_Err_Ok)
                throw new Exception($"FT_Set_Pixel_Sizes failed: {err}");

            int max_dim = (int)((1 + (_face->size->metrics.height >> 6)) * MathF.Ceiling(MathF.Sqrt(128)));
            int tex_width = 1;
            while (tex_width < max_dim) tex_width <<= 1;
            int tex_height = tex_width;

            byte[] rgba = new byte[tex_width * tex_height * 4];
            int pen_x = 0, pen_y = 0;

            for (char c = (char)0; c < 128; c++)
            {
                err = FT_Load_Char(_face, c, FT_LOAD.FT_LOAD_RENDER);
                if (err != FT_Error.FT_Err_Ok)
                    throw new Exception($"FT_Load_Char failed: {err}");

                uint width = _face->glyph->bitmap.width;
                uint height = _face->glyph->bitmap.rows;
                int pitch = _face->glyph->bitmap.pitch;

                byte* src = _face->glyph->bitmap.buffer;

                if (pen_x + width >= tex_width)
                {
                    pen_x = 0;
                    pen_y += (int)(_face->size->metrics.height >> 6) + 1;
                }

                // Copy final into rgba at pen_x, pen_y
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcO = (y * (int)width + x) * 4;
                        int dstO = ((pen_y + y) * tex_width + pen_x + x) * 4;

                        byte v = src[y * pitch + x];
                        rgba[dstO + 0] = 255;
                        rgba[dstO + 1] = 255;
                        rgba[dstO + 2] = 255;
                        rgba[dstO + 3] = v;
                    }
                }


                glyphs[c].x0 = pen_x;
                glyphs[c].y0 = pen_y;
                glyphs[c].x1 = pen_x + (int)width;
                glyphs[c].y1 = pen_y + (int)height;

                glyphs[c].x_off = _face->glyph->bitmap_left;
                glyphs[c].y_off = _face->glyph->bitmap_top;
                glyphs[c].advance = (int)(_face->glyph->advance.x >> 6);

                pen_x += (int)width + 1;
            }


            TextureAtlas = new VelvetTexture(renderer, rgba, (uint)tex_width, (uint)tex_height);
        }

        // IDisposable

        /// <summary>
        /// Disposes of the resources used by the VelvetFont.
        /// </summary>
        public void Dispose()
        {
            TextureAtlas.Dispose();
            if (_face != null)
            {
                FT_Done_Face(_face);
                _face = null;
            }

            if (_library != null)
            {
                FT_Done_FreeType(_library);
                _library = null;
            }
        }
    }
}