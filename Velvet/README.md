# Velvet

A collection of cross-platform libraries for building modern applications, all handled by `VelvetApplication`.

`VelvetApplication` is an abstract class that you can derive from to create applications easily. It automatically manages a `VelvetWindow`, a `VelvetRenderer`, and an `InputManager` for you.

Currently built on SDL3, will work on abstracting everything so you will be able to use any windowing/input system.