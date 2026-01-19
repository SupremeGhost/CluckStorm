# Chicken Controller Development Progress

This document tracks the progress of creating the `ChickenController.cs` script.

## Completed Steps

### Step 1: Basic Script Setup
- Created the `ChickenController.cs` file.
- Set up the class to inherit from `MonoBehaviour` for Unity component functionality.

### Step 2: Input System Integration
- Added private `Vector2` variables (`_moveInput`, `_lookInput`) to store player input.
- Created public methods (`OnMove`, `OnLook`) to receive data from the `PlayerInput` component.

### Analysis: Input Action Asset
- Read the `InputSystem_Actions.inputactions` file.
- Renamed the `OnFire` method to `OnAttack` to match the action name defined in the file. This is a critical step for the controls to work.

### Step 3: Camera Look Controls
- Implemented camera rotation in the `LateUpdate()` method.
- The player body rotates horizontally (left/right).
- A separate camera object rotates vertically (up/down), and its pitch is clamped between -90 and +90 degrees to prevent flipping.
- Added a `_lookSensitivity` variable to control rotation speed.

---

## Next Steps (Brief Overview)

### Step 4: Physics & Component Setup
First, we need to get a reference to the player's `Rigidbody` component in the `Awake()` method and add variables for movement speeds.

### Step 5: Player Movement (Walk)
Next, we will implement the `FixedUpdate()` method, which is used for physics calculations. We will use our stored input to apply a **weak** walking force to the `Rigidbody`.

### Step 6: Recoil Mechanic (Core Gameplay)
We will then implement the logic inside the `OnAttack()` method. When the player attacks, we will apply a **strong** recoil force to the `Rigidbody` in the opposite direction the camera is facing. This is the core movement mechanic of the game.

### Step 7: Jump Mechanic
We will add an `OnJump()` method and use it to apply an upward force to the `Rigidbody`, allowing the chicken to jump.

### Step 8: Final Assembly & Editor Setup
Finally, we will do a full review of the script and provide a detailed checklist on how to configure everything in the Unity Editor to make it all work together.
