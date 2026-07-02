using System.Drawing;
using System.Numerics;

using Boist;
using Boist.Graphics;
using Boist.Input;

// Generated using Google Gemini
// Don't use this to learn, use the other tests

// Did this for fun kinda impressed at how well it did LOL

public class GeminiApp : Application
{
    // Store player position and speed
    private Vector2 _playerPosition = new Vector2(375, 275);
    private float _moveSpeed = 300f;

    public GeminiApp(GraphicsAPI graphicsAPI) : base(800, 600, "Boist Framework Example", graphicsAPI) { }

    protected override void OnInit(int argc, string[]? argv)
    {
        Console.WriteLine("Game Initialized!");
    }

    protected override void Update()
    {
        if (Input.IsKeyDown(KeyCode.W)) _playerPosition.Y -= _moveSpeed * (float)DeltaTime;
        if (Input.IsKeyDown(KeyCode.S)) _playerPosition.Y += _moveSpeed * (float)DeltaTime;
        if (Input.IsKeyDown(KeyCode.A)) _playerPosition.X -= _moveSpeed * (float)DeltaTime;
        if (Input.IsKeyDown(KeyCode.D)) _playerPosition.X += _moveSpeed * (float)DeltaTime;
    }

    protected override void Draw()
    {
        Renderer.Begin();

        Renderer.ClearColor(Color.Black);


        Renderer.DrawCircle(
            new Vector2(400, 300),
            30f,
            Color.SkyBlue
        );

        Renderer.DrawRectangle(
            _playerPosition,
            new Vector2(50, 50),
            Color.Red
        );
        Renderer.End();
    }

    protected override void OnShutdown()
    {
        Console.WriteLine("Shutting down...");
    }
}