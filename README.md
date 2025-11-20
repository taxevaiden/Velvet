# Velvet

heavily unfinished library

you can make games, tools, whatever

tbh idk if i'll finish this i'll just put this here for progress, if anyone wants to help out please do i am not great at C# :sob:

there are better frameworks/libraries out there that can do a lot more than this library can. this was only made for fun and to learn more about how graphics and rendering work.

## things i'm done with

- basic window
- drawing rectangles, circles, any polygon you could think of, with color!
- input from keyboard and mouse
- support for all four major graphics APIs (if OpenGL counts)
    - D3D11
    - Vulkan
    - Metal
    - OpenGL

![Stress Test](assets/image.png)

Stress Test, ~60 FPS with ~100,000 rectangles (with a vertex buffer size of 20MB, it has changed to 8MB)

## how to test

first, clone the repo:

    git clone https://github.com/taxevaiden/Velvet.git

cd into the cloned repo and do

    dotnet restore

it should just work if you're on windows

i have tried this on linux and macOS a while a ago, and i can confirm that they do work. however, there are some issues when trying to run this on macOS (and maybe linux i don't remember). when trying to run Velvet.Tests, you'll get an exception saying that `libSDL3.dylib` does not exist. i've tried installing SDL3 with Homebrew to no avail.

i've solved this issue where you just have to run it in the terminal instead of vscode  but i am too lazy to put it here. for now i think you can put `libSDL3.dylib` (or whatever the library file is) into the bin directory for Velvet.Tests.