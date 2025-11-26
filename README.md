# Velvet

heavily unfinished library

you can make games, tools, whatever

tbh idk if i'll finish this i'll just put this here for progress, if anyone wants to help out please do i am not great at C# :sob:

there are better frameworks/libraries out there that can do a lot more than this library can,, like [Bliss](https://github.com/MrScautHD/Bliss). this was only made for fun and to learn more about how graphics and rendering work.

## things i'm done with

- basic window
- drawing rectangles, circles, any polygon you could think of, with color!
- textures
- input from keyboard and mouse
- support for all four major graphics APIs (if OpenGL counts)
    - D3D11
    - Vulkan
    - Metal
    - OpenGL

![Stress Test](assets/image.png)

Stress Test, ~60 FPS with ~100,000 rectangles (with a vertex buffer size of 20MB, it has changed to 8MB)

## how to test

i have tried this on linux and macOS a while a ago, and i can confirm that they do work. however, there are some issues when trying to run this on macOS (and maybe linux i don't remember). when trying to run Velvet.Tests, you'll get an exception saying that `libSDL3.dylib` does not exist. i've tried installing SDL3 with Homebrew to no avail. however there is a solution (for macOS) ahead!

### windows

first, clone the repo:

    git clone https://github.com/taxevaiden/Velvet.git

cd into the cloned repo and do

    dotnet restore

it should just work if you're on windows,,

    dotnet run

### macos

first, clone the repo:

    git clone https://github.com/taxevaiden/Velvet.git

cd into the cloned repo and do

    dotnet restore

#### homebrew

you will have to do this in the terminal instead of testing through vscode.

if you installed sdl3 through homebrew, you can do

    DYLD_LIBRARY_PATH=/opt/homebrew/lib:$DYLD_LIBRARY_PATH

so that the sdl3 library file is detected. after that you can do

    dotnet run  

and it works just fine!

#### if you have libSDL3.dylib...

just put it in `Velvet.Tests/bin/Debug/net8.0 then do

    dotnet run

it'll work

