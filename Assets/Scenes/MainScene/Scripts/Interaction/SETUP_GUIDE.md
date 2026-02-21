# Desktop Interaction System — Prefab Setup Guide

## Overview

The new interaction system replaces the old per-item `PlayerKB_GrabObject` approach with a centralized `HandGrabSystem` + `GrabbableItem` architecture that works for both Desktop (KB&M) and VR.

---

## Step 1: PlayerKB Root GameObject

Add these components to the **PlayerKB** root GameObject:

| Component | Notes |
|-----------|-------|
| `PlayerKB_GrabController` | Central desktop interaction handler |
| `DesktopHandDriver` | Positions hands relative to camera |
| `DesktopGestures` | F=Wave, G=Follow, T=Point |

### PlayerKB_GrabController Inspector Setup:
- **Left Hand** → drag `hand_l` (needs HandGrabSystem — see Step 2)
- **Right Hand** → drag `hand_r` (needs HandGrabSystem — see Step 2)
- **Hand Driver** → drag the `DesktopHandDriver` component on this same object
- **Left Hand Animator** → drag HandAnimator on `wrist_l` (see Step 3)
- **Right Hand Animator** → drag HandAnimator on `wrist_r` (see Step 3)
- **Player Camera** → drag `Main Camera KB`
- **HUD** → drag the HUD GameObject's `PlayerKB_HUD` component
- **Crosshair UI** → drag the crosshair Image (see Step 5)

### DesktopHandDriver Inspector Setup:
- **Camera Transform** → drag `Main Camera KB`
- **Left Hand Grab** → drag `hand_l` HandGrabSystem
- **Right Hand Grab** → drag `hand_r` HandGrabSystem

### DesktopGestures Inspector Setup:
- **Left Hand Animator** → drag HandAnimator on `wrist_l`
- **Right Hand Animator** → drag HandAnimator on `wrist_r`
- **Left Hand Grab** → drag `hand_l` HandGrabSystem
- **Right Hand Grab** → drag `hand_r` HandGrabSystem

---

## Step 2: Hand Setup (hand_l / hand_r)

Add `HandGrabSystem` to each hand:

### hand_l:
- **Is Left Hand** → ✅ checked
- **Hand Rigidbody** → leave **empty** (null) for desktop kinematic mode
- **Grip Point** → drag `hand_l` transform itself (or create a child "GripPoint" for precise positioning)
- **Hand Collider** → assign the existing BoxCollider (trigger)

### hand_r:
- **Is Left Hand** → ❌ unchecked
- **Hand Rigidbody** → leave **empty** (null) for desktop kinematic mode
- **Grip Point** → drag `hand_r` transform itself
- **Hand Collider** → assign the existing BoxCollider (trigger)

> **Desktop mode**: When `handRigidbody` is null, HandGrabSystem uses kinematic parenting (items become kinematic children of the grip point). No physics joints needed.

---

## Step 3: Hand Animators (wrist_l / wrist_r)

Add `HandAnimator` to each wrist:

### wrist_l:
- **Hand Animator** → drag the existing Animator component on `wrist_l`
- **Is Left Hand** → ✅ checked
- Finger bones are optional (for VR finger tracking only)

### wrist_r:
- **Hand Animator** → drag the Animator component on `wrist_r`
- **Is Left Hand** → ❌ unchecked
- ⚠️ **Note**: In the current prefab, `wrist_r`'s Animator is **disabled**. You should **enable it** for the new system to work.

---

## Step 4: PlayerManager

The `PlayerManager` script (on the scene root or manager object) has been updated:

- **Left Hand Grab** → drag `hand_l` HandGrabSystem
- **Right Hand Grab** → drag `hand_r` HandGrabSystem
- Remove old hand GameObject references (`_leftHandKB`, `_rightHandKB`)

---

## Step 5: Crosshair UI

1. Create a **Canvas** (Screen Space - Overlay) if one doesn't exist
2. Add an **Image** child centered on screen (anchor: center, pivot: center)
3. Set size to 4x4 pixels, white sprite or circle
4. Add `DesktopCrosshair` component
5. Set **Grab Controller** → drag `PlayerKB_GrabController` from PlayerKB

---

## Step 6: Items (GrabbableItem)

Replace `PlayerKB_GrabObject` on each grabbable item with `GrabbableItem`:

1. Remove old `PlayerKB_GrabObject` component
2. Add `GrabbableItem` component
3. Set the **Hold Pose** enum to match the item type (Bag, Flashlight, Peach, etc.)
4. Adjust hold position/rotation offsets for each hand
5. Ensure the item has a **Rigidbody** (auto-created if missing) and a **Collider**
6. Optionally add an **Outline** component for highlight effect

### Special Items:
- **Flashlight**: Add `Flashlight` component (now implements `IInteractable` — no more `PlayerKB_GrabObject` dependency)
- **Bag**: Add `Inventory` component. Set HandGrabSystem references for both hands and HUD.
- **Doors**: `InteractDoor` already implements `IInteractable`

---

## Step 7: Remove Old Scripts

These scripts are superseded and can be removed from GameObjects (keep files for reference):

| Old Script | Replaced By |
|------------|-------------|
| `PlayerKB_GrabObject` (on items) | `GrabbableItem` |
| `ObjectHoldPositions` | `GrabbableItem` hold offsets |
| `AnimationIndexes` | `HandAnimator` + `GrabbableItem.HoldPose` |
| `PlayerKB_Camera` | `PlayerKB_Movement` (already handles look) |
| `AnimationHandler` | `HandAnimator` |

---

## Desktop Controls Summary

| Key | Action |
|-----|--------|
| WASD | Move |
| Mouse | Look |
| Shift | Run |
| Space | Jump |
| C | Crouch |
| LMB | Grab/Drop (left hand) |
| RMB | Grab/Drop (right hand) |
| E | Interact (doors, held items, NPCs) |
| Q | Toggle Bag (when held) |
| Scroll | Extend/Retract hands |
| F | Wave gesture |
| G | Follow gesture |
| T | Point (hold) |
| M | Toggle map |
| 1 | Switch KBM ↔ VR |

---

## Architecture Diagram

```
PlayerKB (root)
├── PlayerKB_BasicActions      — Movement, crouch, jump, map
├── PlayerKB_GrabController    — Grab, interact, crosshair, HUD hints
├── DesktopHandDriver          — Hand positioning, reach, head-lock
├── DesktopGestures            — Wave/Follow/Point
│
├── Main Camera KB
│   └── Player_Prefab
│       └── PlayerArmature
│           ├── wrist_l [HandAnimator]
│           │   └── hand_l [HandGrabSystem, BoxCollider(trigger)]
│           └── wrist_r [HandAnimator]
│               └── hand_r [HandGrabSystem, BoxCollider(trigger)]
│
└── HUD Canvas
    ├── Crosshair [DesktopCrosshair, Image]
    └── HUD [PlayerKB_HUD]

Items in Scene:
├── RubberDucky [GrabbableItem, Rigidbody, Collider, Outline]
├── Flashlight  [GrabbableItem, Flashlight, Rigidbody, Collider]
├── Bag         [GrabbableItem, Inventory, Rigidbody, Collider, Animator]
├── Peach       [GrabbableItem, Rigidbody, Collider]
└── Door        [InteractDoor (implements IInteractable)]
```
