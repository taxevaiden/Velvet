using System;
using System.Drawing;
using System.Numerics;

using Velvet;
using Velvet.Graphics;
using Velvet.Input;

// Generated using Google Gemini
// Did this for fun kinda impressed at how well it did LOL

public class GeminiApp : VelvetApplication
{
    // Store player position and speed
    private Vector2 _playerPosition = new Vector2(375, 275);
    private float _moveSpeed = 300f;

    public GeminiApp(GraphicsAPI graphicsAPI) : base(800, 600, "Velvet Framework Example", graphicsAPI) { }

    protected override void OnInit()
    {
        Console.WriteLine("Game Initialized!");
    }

    protected override void Update(float deltaTime)
    {
        if (InputManager.IsKeyDown(KeyCode.W)) _playerPosition.Y += _moveSpeed * deltaTime;
        if (InputManager.IsKeyDown(KeyCode.S)) _playerPosition.Y -= _moveSpeed * deltaTime;
        if (InputManager.IsKeyDown(KeyCode.A)) _playerPosition.X -= _moveSpeed * deltaTime;
        if (InputManager.IsKeyDown(KeyCode.D)) _playerPosition.X += _moveSpeed * deltaTime;
    }

    protected override void Draw()
    {
        Renderer.Begin();

        Renderer.ClearColor(Color.Black);

        Renderer.DrawRectangle(
            _playerPosition,
            new Vector2(50, 50),
            Color.Red
        );

        Renderer.DrawCircle(
            new Vector2(400, 300),
            30f,
            Color.SkyBlue
        );

        Renderer.End();
    }

    protected override void OnShutdown()
    {
        Console.WriteLine("Shutting down...");
    }
}