# Velvet

heavily unfinished library

you can make games, tools, whatever

tbh idk if i'll finish this i'll just put this here for progress, if anyone wants to help out please do i am not great at C# :sob:

## things i'm done with

- basic window
- drawing rectangles, circles, any polygon you could think of, with color!
- input from keyboard and mouse

![Stress Test](assets/image.png)

Stress Test, ~60 FPS with ~100,000 rectangles

## how to test

first, clone the repo:

    git clone https://github.com/taxevaiden/Velvet.git

before trying to do anything, you should edit [Velvet.csproj](https://github.com/taxevaiden/Velvet/blob/main/Velvet/Velvet.csproj):

```xml
<RuntimeIdentifier>osx-arm64</RuntimeIdentifier>
<!-- Change the RuntimeIdentifier to whatever platform you're on. -->
```

here, i have it set to `osx-arm64` (since that's what i'm currently using)

you'll need to change the RuntimeIdentifier to what platform you're on.

- Windows = `win-x64`
- macOS (Intel) = `osx-x64`
- macOS (Apple Silicon) = `osx-arm64`
- Linux = `linux-x64`

this is because you'll need SDL3.dll (libSDL3.dylib on mac, libSDL3.so on linux) beside the executable file of your application. i don't think these are provided by the SDL3 bindings i'm using (except for windows)

after setting that up, cd into the cloned repo and do

    dotnet restore

it should just work.