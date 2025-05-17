# MoCore

A common plugin used across my other BepInEx plugins.

## Setup

### Add the dll

Download the latest version from the [release page](https://github.com/mosadie/mocore/releases/latest) and place it into a lib folder with your code. Add the following to your .csProj file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    ...

    <ItemGroup>
        <Reference Include="MoCore">
            <HintPath>lib/com.mosadie.mocore.dll</HintPath>
        </Reference>
    </ItemGroup>

</Project>
```

### In your main Plugin class

Have the plugin class implement IMoPlugin and call MoCore.RegisterPlugin and pass itself. The plugin should also make use of the [BepInDependency](https://docs.bepinex.dev/api/BepInEx.BepInDependency.html) attribute to make sure it loads after MoCore.

Example of the attribute:
```C#
[BepInDependency("com.mosadie.mocore", BepInDependency.DependencyFlags.HardDependency)]
```

Example of version checking in `Awake`:
```C#
if (!MoCore.MoCore.RegisterPlugin(this))
{
    Log.LogError("Failed to register plugin with MoCore. Please check the logs for more information.");
    return;
}
```

### Make sure to mark as dependencies on Thunderstore as well

If using the tcli here is how it looks in thunderstore.yml

```yml
[package.dependencies]
BepInEx-BepInExPack= "5.4.2100"
MoSadie-MoCore= "2.0.0"
```

## Features

### Game Version Checking

Return a url to a json file from `GetVersionCheckUrl` and you can use the return value of `RegisterPlugin` to determine if the game version supports your plugin.

Pass null for the `GetVersionCheckUrl` to disable online version checking. Pass null for the `GetCompatibleGameVersion` to disable local version checking. In addition there is a configuration option in MoCore to skip version checking entirely.

Here is a example of what the version json file looks like:
```json
{
    "1.2.3": [ // Plugin version
        "4.1579" // Application.version value (printed in console by MoCore on launch for easy access)
    ]
}
```

### Centralized HTTP server

This allows plugins to listen for HTTP requests without having to worry about port conflicts. To use this, return an object that implements `IMoHttpHandler` from the new `GetHttpHandler` method in your plugin. The server will call this object when a request comes in, and your code can modify the response from there.

If you don't want to use this system, just have `GetHttpHandler` return null.

To prevent path conflicts, paths are prefixed with an identifier of your choice, for example: With the prefix `mocore` a valid url would be `http://localhost:8001/mocore/whatever`

### Variable System

This allows strings with variables in them to be parsed and replaced with their values. This is also available via the HTTP server, for those feeling fancy. (Path is `/mocore/variable/parse` with the query parameter `string` set to the string you want to parse)

To use this, call `VariableHandler.ParseVariables` with the string you want to parse. The variables are in the format `$variable` and can be replaced with their values. For example, if you have a variable called `name`, you can use `$name` in your string and it will be replaced with the value of the variable.

#### Crew Variables:
- $captain: The display name of the Captain
- $randomCrew[id]: A random crew member's name, replace [id] to keep it consistent in the message (ex $randomCrew1)
- $crew[id]: The crew member with that numeric id, replace [id] with a number (ex $crew0)

#### Fight Variables:
(These will be blank if no fight is occurring)
- $enemyName: The name of the enemy ship
- $enemyIntel: The intel of the enemy ship
- $enemyInvaders: The invaders from the enemy ship
- $enemyThreat: The threat level of the enemy ship
- $enemySpeed: The speed of the enemy ship
- $enemyCargo: The cargo of the enemy ship

#### Run Variables:
- $campaignName: The name of the campaign (ex Pluto)
- $sectorName: The name of the sector (ex Pluto Outskirts)

#### Misc Variables:
- $version: The version of MoCore (mainly for debugging purposes, no real purpose)