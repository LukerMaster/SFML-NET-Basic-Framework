# SFML-Basic-Framework
In its core is a simple library that creates a system of **Levels** that each of them contains its own set of **Actors** (any kind of object).

### Problem
Lets say you want to create SFML.NET game or simulation that requires game-loop and that contains a set of objects that need to know about each other (for example enemies that need to detect the player.)
This library lets you create one object that handles all the timings, window, viewports, views and everything with simple adjustable settings.
### How to
***(Of course first you need to link SFML to your project.)***

To start working with **SFBF** (Name of the namespace, short for **SF**ML-**B**asic-**F**ramework), you just need to create an object of `SFBF.Engine` and call `Run()` method on its object. That will create the window, program loop, and run everything. This function is **blocking** since it runs the loop of the program.
```csharp
SFBF.Engine engine = new SFBF.Engine();
engine.Run();
```
After we run this code you can see your window blinks for a moment and `Run()` returns. Great! Or not quite. To make the window stay for a little longer we need to give it a **Level**.

When you create `SFBF.Engine` you create just the game loop, but this loop needs to loop over *something*. If you ever did stuff in Unity or Unreal Engine you may remember that they have concept of Scenes/Levels. Each level contains a set of Actors (or GameObjects.).

SFBF provides ~~*disgustingly*~~ easy way to create a level. All you have to do is create a new class that derives from `SFBF.Level`. This is an abstract class that need to have 2 methods implemented to function.
```csharp
class SomeMyLevel : SFBF.Level
{
	protected override void FixedUpdateScript(float dt, Instance inst)
	{
		//This function is called every somewhat fixed amount of time.
		//Can be used for physics for example.
	}

	protected override void UpdateScript(float dt, Instance inst)
	{
		//This function is called every frame. Can be used for scripting.
	}
}
```
*(Instance parameter explained later. Do not worry.)*

Now you have access to two methods in which you can script whatever you want Think of `Update()` and `FixedUpdate()` as your own level scripts/blueprints. Okay, but now how to add our new level into the engine? Again, simple:
```csharp
engine.Data.InstantiateLevel(new SomeMyLevel());
```
*(`Data` property explained later.)*

`InstantiateLevel()` is a method that adds a level to the current level list. You can add multiple levels and each of them is going to be updated but only **one** drawn at a time. (Explained later.)



Okay, so now lets run the application with some Level added. It should open the window now and run until we close it explicitly (Through the console, the "X" is not handled by default).

But now the window is just black, nothing is going on. There is where **Actors** come in. Remember, actors are inside of a level and they actually do stuff. To create an Actor you do the same you did when creating Level. Create a class that derives from `SFBF.Actor` and implement its abstract methods.
```csharp
class MyActor : Actor
    {
        protected override void Draw(RenderWindow w, Level level, AssetManager assets)
        {
		// Called every frame. Used to draw stuff.
		// Look, it gets SFML.RenderWindow as argument so you can actually draw stuff.

		// Test code to see if it draws stuff correctly
		RectangleShape s = new RectangleShape();
            	s.Size = new Vector2f(100, 200);
           	s.FillColor = new Color(255, 255, 0, 255);
            	s.Position = new Vector2f(100, 100);
		w.Draw(s);
        }

        protected override void FixedUpdate(float dt, Level level, AssetManager assets)
        {
		// Called every fixed amount of time.
        }

        protected override void Update(float dt, Level level, AssetManager assets)
        {
		// Called every frame.
        }
    }
```
Make sure to add `using` statements for `RenderWindow` and other SFML things to work.
```csharp
using SFML.Graphics;
using SFML.System;
```

And the same way as you added your level into the instance of the Framework you can add your actor into the scene using `InstantiateActor()` method inside your level.
```csharp
class SomeMyLevel : SFBF.Level
{
	public SomeMyLevel() // Constructor
	{
		InstantiateActor(new MyActor());
		
		// --- OR if you want to keep reference to it (if its an important actor like a player)
		
		MyActor m = InstantiateActor(new MyActor());
		// InstantiateActor() returns a referenced to added actor for easier assingment.
	}
	// Other functions below
}
```
Now that we instantiated a new actor we can see it being drawn onto the screen every frame.

That is enough to make basic SFBF application.
### Important list of things
- The same way you add Actors/Levels using `InstantiateActor()`/`InstantiateLevel()` you can delete them from the list using `DestroyActor()`/`DestroyLevel()`. Keep in mind that **nothing** is forcefully deleted from the **memory** (Standard C# GC rules apply here) i.e. You can keep a reference to these objects keeping them alive but they wont be updated nor drawn since they are not in the list.

- You can override protected `bool ToDestroy` property of each **Actor** and **Level** to automatically delete them from the list. Once this property returns `true` Actor/Level is removed from the list.

- You can override protected `int DrawOrder` property of each **Actor** to make it be drawn on top or bottom.

- Each Level has access to `SFBE.WindowSettings` member which you can use to change window settings for each level.

- Each Level has access to `MousePos` member which you can use to check mouse position in the SFML world.

- Each Level has access to `WindowName` member which you can use to change window name for this level.

- `SFBE.Engine.Data` is a property that gives us access to settings of the `SFBF.Engine`. There are a few of them and you should try to play with them yourself

### Communication between objects
- Instantiated levels are inside a list stored inside Instance.
- Instantiated actors are inside a list stored inside a Level.

Remember this.

As you could have noticed `UpdateScript()` and `FixedUpdateScript()` have access to `SFBE.Instance inst` parameter. You can use it to call level-list modyfing methods `inst.InstantiateLevel(new OtherLevel())` or `inst.DestroyLevel(referenceToOtherLevel)` inside of a level effectively manipulating the level list from the level itself.
For example calling `inst.DestroyLevel(this)` will remove the current level from the list. So you just made the level destroy itself. *(Panic? Ragequit?)*

But how to get access to other levels that are currently in the list **from the level**?
Simple: `inst.GetLevelsOfClass<T>()` returns a `List<T>` that contains all the levels that are of class `T`. Now you can save reference to them and do anything you want with them.
```csharp
protected override void UpdateScript(float dt, Instance inst)
{
	List<OtherWeirdLevel> list = inst.GetLevelsOfClass<OtherWeirdLevel>();
	list[0].SomePublicMemberInt = 30;
// I just modified another level from current one
}
```
Okay, so now we know how to make levels communicate with each other.

How about actors? That is certainly WAAAAAAAAY more important. Same principle.
Noticed `Level` parameter inside `Update()` and `FixedUpdate()` methods?
The same way we can call `level.GetActorsOfClass<T>()` and get a full list of other actors that we can manipulate freely.

```csharp
protected override void FixedUpdate(float dt, Level level, AssetManager assets)
{
	List<SomeTestActor> list = level.GetActorsOfClass<SomeTestActor>();
	list[0].DrawOrder = -10;
}
```

Worth mentioning that GetXOfClass<>() is quite heavy and should not be called each frame. Rather conditionally.

### AssetManager...?
AssetManager is interface reference that is created inside the instance (So only one per `SFBE.Engine`). But by default, references `null`. As we all know, things like textures, save data or sound buffers should be always loaded **ONCE**. You NEVER need two same textures loaded to create sprites. So you *may* use this reference to create your own AssetManager class responsible for loading textures, sounds etc. All you have to do is create a class that derives from `SFBF.AssetManager` and add it into Engine.

It forces you to add `UnloadAllAssets()` method that should unload everything but you can... *sigh...* leave it empty. This object will now be accessible inside every actors `Draw()` method.
```csharp
class MyAssetManager : SFBF.AssetManager
    {
        public void UnloadAllAssets()
        {
            // Does not need to do anything
            // in particular. But SHOULD unload
            // all assets.
        }
	public SomeAsset MyFunctionToLoadSomeAsset(string path) // User defined function.
	{
		// Loady load.
	}
    }
```
then:
```csharp
engine.Data.assets = new MyAssetManager();
```
and then you can do this inside an actor:
```csharp
protected override void Draw(RenderWindow w, Level level, AssetManager assets)
{
	SomethingThatNeedsSomeAsset.SomeAsset = (assets as MyAssetManager).MyFunctionToLoadSomeAsset(pathToAssetString);
}
```
### Data.Settings overview
- UpdateRate:
Used to determine maximum rate at which Update() and Draw() methods are called. Can be set to 0 to remove the cap.
Setting negative value will ignore minus.

- FixedUpdateRate:
Used to determine rate at which FixedUpdate() method is called.
If computer can't keep up with FixedUpdate() rate, deltaTime given to FixedUpdate() methods of actors and levels
will be capped at FluctuationTolerancy times deltaTime.
- FluctuationTolerancy:
Used to determine maximum fluctuation in FixedUpdate() rate deltaTime.
Since time between each call to FixedUpdate() is fixedUpdateDeltaTime = 1/FixedUpdateRate
When computer cant keep up with FixedUpdate() call rate, the time that passed since last call increases which may cause
bugs in physics calculations inside actors and levels. To prevent this you can cap the maximum deltaTime GIVEN to FixedUpdate() method
to a multiplier of desired fixedUpdateDeltaTime. FluctuationTolerancy is exactly this. Setting it to 0 will make deltaTime given to method
always the same regardless of call rate which can slow down the game but prevent any physics bugs. Setting it to 0.5 will make the maximum
deltaTime given to 150% of fixedUpdateDeltaTime so in case of computer lag game still runs at constant speed but calculates with less accuracy.

- IsOn:
~~Gee... I wonder what it does...?~~ Turns off the engine if false. Effectively unlocking `Run()` method.
