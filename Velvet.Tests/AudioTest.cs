using System.Numerics;

using Velvet.Audio;
using Velvet.Graphics;
using Velvet.Input;
using Velvet.Windowing;

namespace Velvet.Tests
{
    /// <summary>
    /// Controls:
    ///   Space                 play a one-shot sound at the origin
    ///   Left / Right arrow    move the spatial sound source left / right
    ///   S                     stop the spatial sound
    ///   P                     play / resume the spatial sound
    ///   L                     toggle looping on the spatial sound
    ///   Up / Down             raise / lower the spatial sound volume
    /// </summary>
    class AudioTest : VelvetApplication
    {
        private const string OneShotPath = "assets/audio/boom.mp3";
        private const string SpatialPath = "assets/audio/funny.mp3";

        private VelvetAudioEngine _audioEngine = null!;

        private readonly List<VelvetAudio> _oneShotPool = new();
        private const int PoolSize = 8;

        private VelvetAudio _spatialSound = null!;
        private float _spatialX = 0f;

        private const float MoveSpeed = 200f;
        private const float MaxDistance = 500f;
        private VelvetFont font;

        public AudioTest(GraphicsAPI graphicsAPI = GraphicsAPI.Default)
            : base(1600, 900, "Audio Test", graphicsAPI) { }

        protected override void OnInit()
        {
            base.OnInit();

            font = new VelvetFont(Renderer, "assets/sans.ttf", 16);

            _audioEngine = new VelvetAudioEngine();

            for (int i = 0; i < PoolSize; i++)
                _oneShotPool.Add(new VelvetAudio(_audioEngine, OneShotPath));

            _spatialSound = new VelvetAudio(_audioEngine, SpatialPath)
            {
                Loop = true,
                Spatialization = true,
                Volume = 1f,
            };
            _spatialSound.SetAttenuationRange(50f, MaxDistance);
            _spatialSound.SetPosition(new Vector3(_spatialX, 0f, -10f));
            _spatialSound.Play();
        }

        protected override void Update(float deltaTime)
        {
            if (InputManager.IsKeyPressed(KeyCode.Space))
                PlayOneShot();

            if (InputManager.IsKeyDown(KeyCode.Left))
                _spatialX -= MoveSpeed * deltaTime;

            if (InputManager.IsKeyDown(KeyCode.Right))
                _spatialX += MoveSpeed * deltaTime;

            _spatialX = Math.Clamp(_spatialX, -MaxDistance, MaxDistance);
            _spatialSound.SetPosition(new Vector3(_spatialX, 0f, -10f));

            if (InputManager.IsKeyDown(KeyCode.Up))
                _spatialSound.Volume = Math.Min(2f, _spatialSound.Volume + deltaTime);

            if (InputManager.IsKeyDown(KeyCode.Down))
                _spatialSound.Volume = Math.Max(0f, _spatialSound.Volume - deltaTime);

            if (InputManager.IsKeyPressed(KeyCode.P)) _spatialSound.Play();
            if (InputManager.IsKeyPressed(KeyCode.S)) _spatialSound.Stop();
            if (InputManager.IsKeyPressed(KeyCode.L)) _spatialSound.Loop = !_spatialSound.Loop;
        }

        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.ClearColor(System.Drawing.Color.FromArgb(30, 30, 30));

            DrawSpatialVisualiser();

            Renderer.End();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            _spatialSound.Dispose();

            foreach (var s in _oneShotPool)
                s.Dispose();

            _audioEngine.Dispose();
        }

        private int _poolIndex = 0;

        private void PlayOneShot()
        {
            var sound = _oneShotPool[_poolIndex];
            _poolIndex = (_poolIndex + 1) % PoolSize;

            sound.Stop();
            sound.Play();
        }

        private void DrawSpatialVisualiser()
        {
            float cx = Window.Width * 0.5f;
            float cy = Window.Height * 0.5f;

            Renderer.DrawCircle(new Vector2(cx, cy), MaxDistance * (cx / MaxDistance) * 0.4f,
                System.Drawing.Color.FromArgb(60, 60, 60));

            Renderer.DrawCircle(new Vector2(cx, cy), 10f, System.Drawing.Color.LightGray);

            float screenX = cx + (_spatialX / MaxDistance) * cx * 0.8f;
            Renderer.DrawCircle(
                new Vector2(screenX, cy), 14f,
                _spatialSound.IsPlaying
                    ? System.Drawing.Color.DodgerBlue
                    : System.Drawing.Color.DimGray);

            Renderer.DrawLine(new Vector2(cx, cy), new Vector2(screenX, cy), 2f,
                System.Drawing.Color.FromArgb(100, 100, 180));

            // controls text (each control separated by a newline. do a drawtext for each line)

            float ty = Window.Height - 400f;
            Renderer.DrawText(font, "controls:", 16, new Vector2(10, ty), System.Drawing.Color.LightGray); ty += 60;
            Renderer.DrawText(font, "space: play one-shot sound", 16, new Vector2(10, ty), System.Drawing.Color.LightGray); ty += 20;
            Renderer.DrawText(font, "left/right: move spatial sound", 16, new Vector2(10, ty), System.Drawing.Color.LightGray); ty += 20;
            Renderer.DrawText(font, "p: play/resume spatial sound", 16, new Vector2(10, ty), System.Drawing.Color.LightGray); ty += 20;
            Renderer.DrawText(font, "s: stop spatial sound", 16, new Vector2(10, ty), System.Drawing.Color.LightGray); ty += 20;
            Renderer.DrawText(font, "l: toggle spatial sound looping", 16, new Vector2(10, ty), System.Drawing.Color.LightGray); ty += 20;
            Renderer.DrawText(font, "up/down: raise/lower spatial sound volume", 16, new Vector2(10, ty), System.Drawing.Color.LightGray);
        }
    }
}