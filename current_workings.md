# Project Scripts Documentation

This document provides an overview of the main scripts in the CluckStorm project, explaining their purpose and how they interact with each other.

## Player Controller

### `Assets/Scripts/ChickenController.cs`

This is the primary script for controlling the player character, the chicken. It's a `Rigidbody`-based controller, which means it uses Unity's physics engine for movement.

**Key Features:**

*   **Rigidbody Movement:** Unlike the original `CharacterController`, this script uses a `Rigidbody` to move the player. This allows for more realistic physics interactions, including recoil-based movement.
*   **Recoil Movement:** The script exposes a public method `AddRecoilForce(Vector3 force)` which is called by the `UniversalWeaponSystem` to apply a force to the player when a weapon is fired. This is the core movement mechanic of the game.
*   **Standard Movement:** It still allows for standard WASD movement, but the speed is intentionally kept low to encourage the use of recoil movement.
*   **State Machine:** It includes a state machine to manage player states like walking, sprinting, crouching, and sliding.
*   **Camera Control:** It handles camera rotation based on mouse input, including clamping the vertical rotation.
*   **Ground Check:** It uses a spherecast to check if the player is on the ground.
*   **Jumping and Crouching:** It handles jumping and crouching, adjusting the `CapsuleCollider` height for crouching.

**Interactions:**

*   **`UniversalWeaponSystem.cs`:** The weapon system calls `AddRecoilForce` on this script to make the player move. It also gets player state information (e.g., `IsGrounded`, `MovementState`).
*   **`FPSInputManager.cs`:** It gets all its input from the `FPSInputManager`.
*   **`WeaponPickup.cs`:** The pickup script gets the player's `Rigidbody` to apply forces to the weapon when it's dropped.

## Weapon System

### `Assets/Spoiled Unknown/XtremeFPS/Scripts/UniversalWeaponSystem.cs`

This script manages all aspects of a weapon, from firing and reloading to aiming and recoil.

**Key Features:**

*   **Firing:** It handles both automatic and semi-automatic firing modes.
*   **Recoil:** It applies two types of recoil:
    1.  **Camera Recoil:** This is a visual-only recoil that affects the camera.
    2.  **Physical Recoil:** It calls the `AddRecoilForce` method on the `ChickenController` to apply a physical force to the player, which is the main movement mechanic.
*   **Aiming:** It allows the player to aim down sights (ADS), which can change the weapon's properties (e.g., reduce recoil).
*   **Reloading:** It manages the reloading process, including the reload time and animation.
*   **Bullet Management:** It keeps track of the number of bullets in the magazine and the total number of bullets. It uses the `PoolManager` to spawn bullets.
*   **Weapon Effects:** It includes various effects like weapon sway, bobbing, and muzzle flash.

**Interactions:**

*   **`ChickenController.cs`:** It has a reference to the `ChickenController` to apply recoil and get player state information.
*   **`FPSInputManager.cs`:** It gets firing, reloading, and aiming inputs from the `FPSInputManager`.
*   **`PoolManager.cs`:** It uses the `PoolManager` to spawn bullet prefabs and shell ejections.
*   **`ParabolicBullet.cs`:** It initializes the bullet with its properties (speed, damage, etc.).

### `Assets/Spoiled Unknown/XtremeFPS/Scripts/WeaponPickup.cs`

This script allows the player to pick up and drop weapons.

**Key Features:**

*   **Pickup/Drop:** It handles the logic for picking up a weapon and parenting it to the weapon holder, and for dropping a weapon and adding a `Rigidbody` to it.
*   **Rigidbody Interaction:** When a weapon is dropped, it gets the player's velocity from the `ChickenController`'s `Rigidbody` and applies it to the dropped weapon's `Rigidbody`.

**Interactions:**

*   **`ChickenController.cs`:** It has a reference to the player's `Rigidbody` to get its velocity when dropping a weapon.
*   **`UniversalWeaponSystem.cs`:** It enables/disables the `UniversalWeaponSystem` script when the weapon is picked up or dropped.

## Input and Utility Scripts

### `Assets/Spoiled Unknown/XtremeFPS/Scripts/Input Manager.cs`

This is a singleton script that manages all player input using Unity's new Input System. It reads the input actions defined in the `PlayerInputAction` asset and exposes them as public boolean and vector variables.

### `Assets/Spoiled Unknown/XtremeFPS/Scripts/Pool Manager.cs`

A generic object pooling system that can be used to pool any type of GameObject. It's used by the `UniversalWeaponSystem` to pool bullets and other effects.

### `Assets/Spoiled Unknown/XtremeFPS/Scripts/Parabolic Bullet.cs`

This script calculates the trajectory of a projectile, taking gravity into account. It's used for weapons that fire projectiles instead of using hitscan.

### `Assets/Spoiled Unknown/XtremeFPS/Scripts/Interfaces/IPickup.cs` & `IShootableObject.cs`

These are interfaces that define the behavior of pickupable and shootable objects.

## Editor Scripts

### `Assets/Spoiled Unknown/XtremeFPS/Scripts/Editor/XtremeFPSEditor.cs`

This script creates a custom editor window to make it easier to set up the player and weapons. It was updated to use the new `ChickenController`.
