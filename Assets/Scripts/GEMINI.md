**Project Brief for AI (Start-From-Zero Master Description)**

**Game Title:** **Cluckstorm**

**Project Goal:**
Create a multiplayer-first, physics-driven FPS with simple systems, high replayability, and a strong mechanical identity. The game must be easy to prototype, easy to expand, and suitable for a small indie team. All previous work is lost; this document is the single source of truth.

---

## CORE GAME CONCEPT

Cluckstorm is a **multiplayer-focused first-person shooter** where players control armed chickens. The defining mechanic is **recoil-based movement**: firing a weapon applies physical force to the player, allowing them to move, dodge, jump, and fly using gun recoil.

Traditional movement (WASD) exists but is **intentionally slow and weak**. Players must rely on recoil movement to survive and play effectively. If a player refuses to use recoil, they will lose.

The tone is **funny, absurd, and meme-worthy**, but the gameplay is **tight, skill-based, and competitive**.

No battle royale focus. No complex realism. No futuristic sci-fi.
Short matches. Clear objectives. High mechanical skill ceiling.

---

## ENGINE & NETWORKING (VERY IMPORTANT)

**Engine:** Unity
**Networking:** **FishNet**

Networking model:

* **Host–Client architecture**
* One player hosts, others connect as clients
* Server-authoritative logic:

  * Player movement
  * Shooting
  * Damage
  * Objectives
* FishNet is chosen because:

  * Fully free
  * No CCU limits
  * Better control than NGO
  * Suitable for indie multiplayer FPS

All gameplay systems must be written with **networking in mind from day one**. No singleplayer-only assumptions.

---

## CORE MECHANICS

### Movement

* States:

  * **Idle**: standing still, high friction
  * **Walking**: slow WASD movement, grounded
  * **Airborne**: main gameplay state, controlled by recoil
* Shooting applies force opposite to aim direction
* Recoil strength depends on weapon
* Air control exists but is physics-based, not arcade teleporting

### Shooting

* Hitscan for Glock
* Projectile for Egg Launcher
* Shooting:

  * Deals damage
  * Applies recoil force to player
* Reloading locks shooting briefly (important for balance)

---

## WEAPONS (VERY SMALL POOL)

Initial weapons only:

1. **Glock 18**

   * Infinite ammo
   * Medium recoil
   * Fast fire rate
   * Core movement tool
2. **Egg Launcher**

   * Limited ammo
   * High recoil
   * Explosive damage
   * Strong knockback for movement plays

Weapon system must be built using a **BaseWeapon.cs** style architecture so more weapons can be added later.

---

## CHARACTERS (PLAYABLE)

Only these 4 chickens exist:

1. **Default Chicken**

   * Balanced
   * Starter character
2. **Fat Chicken**

   * Heavier
   * Less recoil movement
   * More momentum once airborne
3. **Fried Chicken**

   * Slightly faster
   * Lower health
   * Crisp animations
4. **Rubber Chicken**

   * Bouncy physics
   * Less fall damage
   * Squeaky reactions

Characters differ mainly through **physics modifiers**, not abilities.

---

## MULTIPLAYER FOCUS

The game is **entirely multiplayer-focused** for now. Campaign is removed and will be a separate future project.

Matches are:

* Short (5–10 minutes)
* Objective-driven
* Small maps
* Designed for replayability

Primary design goal:

> “Simple like CS 1.6, chaotic like physics games, unique because of recoil movement.”

---

## ART STYLE

* Stylized low-poly
* Cartoon proportions
* Exaggerated animations
* Clean, readable silhouettes
* No realism
* Bright environments
* Minimal textures, strong colors

Think:

* CS 1.6 simplicity
* Modern indie low-poly
* Meme-friendly visuals

---

## AUDIO & TONE

* Comedic
* Over-the-top chicken sounds
* Loud recoil effects
* Funny but not distracting
* Streamer-friendly (no copyrighted music)

---

## DESIGN PHILOSOPHY (IMPORTANT)

* Small scope
* Few systems, polished deeply
* No unnecessary modes
* No feature creep
* Multiplayer first
* Physics-driven gameplay
* Easy to prototype, hard to master

---

## SUMMARY FOR AI

You are building **Cluckstorm**, a Unity multiplayer FPS using **FishNet host-client networking**, focused on **recoil-based movement**, **small weapon pool**, **four physics-different chicken characters**, **short competitive matches**, and a **funny but skill-based tone**.

Everything must be written with **network authority**, **physics correctness**, and **simplicity** in mind.

This description replaces all previous lost work and should be treated as the complete foundation of the project.
