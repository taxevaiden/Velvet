# Boist.Windowing

A library providing cross-platform window management, built with SDL3. It provides all of the native windowing handles and OpenGL functions necessary to initialize a `Boist.Graphics.Renderer` (or to use any graphics library you may use!).

It also fires events that can be obtained with `Window.PollEvent(out WindowEvent)`.

> [!IMPORTANT]
> This project depends on `SDL3-CS`, which requires native runtime libraries to function.  
> To install these libraries, simply install the package for your platform:  
>
> | Platform | Package         |
> | -        | -               |
> | Windows  | SDL3-CS.Windows |
> | macOS    | SDL3-CS.MacOS   |
> | Linux    | SDL3-CS.Linux   |
>
> You can also insert this into your .csproj if you're building an application for multiple platforms:
> ```xml
> <ItemGroup Condition="$([System.OperatingSystem]::IsWindows())">
>   <PackageReference Include="SDL3-CS.Windows" Version="3.4.10.5" />
> </ItemGroup>
>
> <ItemGroup Condition="$([System.OperatingSystem]::IsLinux())">
>   <PackageReference Include="SDL3-CS.Linux" Version="3.4.10.5" />
> </ItemGroup>
>
> <ItemGroup Condition="$([System.OperatingSystem]::IsMacOS())">
>   <PackageReference Include="SDL3-CS.MacOS" Version="3.4.10.5" />
> </ItemGroup>
> ```