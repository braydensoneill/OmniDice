# 🌟 Button Glow Effect Setup Guide

## What You Get
- **Dice Selection**: Blue glowing outline around selected dice
- **Skin Selection**: Orange glowing outline around selected skin
- **Smooth Animations**: Fade in/out with pulsing effects
- **Automatic Management**: Only one item glows at a time per category

## Easy Setup Steps

### Step 1: Add the SelectionGlowManager
1. Create an empty GameObject in your scene
2. Name it **"SelectionGlowManager"**
3. Add the `SelectionGlowManager.cs` component to it

### Step 2: Configure the Manager
In the SelectionGlowManager inspector:

**Dice Selection Container**: 
- Drag the parent object that contains your dice selection buttons
- Usually something like "DiceSelectionButtons" or "DiceContainer"

**Skin Selection Container**:
- Drag the parent object that contains your skin selection buttons  
- Usually something like "SkinSelectionButtons" or "SkinContainer"

### Step 3: Customize Glow Colors (Optional)
The manager comes with good defaults:
- **Dice Glow**: Blue (`#00CCFF`)
- **Skin Glow**: Orange (`#FF9900`)

You can change these in the inspector if you want different colors.

### Step 4: Test It!
- Run your game
- Select different dice → Should see blue glow
- Select different skins → Should see orange glow
- Only one item per category should glow at a time

## Advanced Customization

### Individual Button Glow Settings
Each button can have custom glow settings. Add the `UIGlowEffect` component to any button and configure:

- **Glow Color**: Color of the glow effect
- **Glow Intensity**: How bright the glow is
- **Glow Size**: How far the glow extends (in pixels)
- **Pulse Speed**: Speed of the pulsing animation
- **Enable Pulse**: Whether the glow should pulse or stay solid

### Custom Glow Colors for Different Items
```csharp
// In your custom script:
SelectionGlowManager glowManager = FindObjectOfType<SelectionGlowManager>();

// Set custom color for a specific button
Button myButton = // your button reference
UIGlowEffect glowEffect = myButton.GetComponent<UIGlowEffect>();
glowEffect.SetGlowColor(Color.red); // Red glow for this button
```

### Manual Glow Control
```csharp
// Show glow on a button
UIGlowEffect glowEffect = myButton.GetComponent<UIGlowEffect>();
glowEffect.ShowGlow();

// Hide glow
glowEffect.HideGlow();

// Toggle glow
glowEffect.ToggleGlow();
```

## How It Works

### Automatic Detection
The system automatically:
1. Finds all buttons in your dice/skin containers
2. Adds glow effects to each button
3. Listens for selection changes
4. Shows glow on selected items
5. Hides glow on deselected items

### Performance Optimized
- Glow objects are only active when glowing
- Smooth coroutine-based animations
- Cached calculations for better performance

## Troubleshooting

### "No glow effects found"
**Problem**: Glow doesn't appear
**Solution**: 
- Make sure you assigned the container references
- Check that your buttons are children of those containers
- Verify the containers have Button components

### "Glow appears behind other UI"
**Problem**: Glow is not visible
**Solution**: 
- The glow automatically positions behind the button
- Make sure your button's Canvas is set to "Screen Space - Overlay"
- Check that other UI elements aren't covering the glow

### "Multiple items glowing"
**Problem**: More than one button glows at once
**Solution**:
- This shouldn't happen with the SelectionGlowManager
- Check that you only have one SelectionGlowManager in the scene
- Verify the containers don't overlap (same button in multiple containers)

### "Glow doesn't follow selection"
**Problem**: Glow doesn't update when selecting items
**Solution**:
- Make sure your dice/skin selection scripts fire the proper events
- Check that SkinManager.Instance.OnSkinChanged is working
- Verify InfiniteScrollDiceSelection.OnSelectedDiceChanged is firing

## Easy Testing
Use these context menu options in the SelectionGlowManager:
- **Test Dice Selection 0**: Glow first dice
- **Test Dice Selection 1**: Glow second dice  
- **Test Skin Selection 0**: Glow first skin
- **Clear All**: Remove all glows

## Visual Examples

### Before (No Glow)
```
[Button] [Button] [Button]
   ^         ^         ^
Selected   Normal    Normal
```

### After (With Glow)
```
[Button] [Button] [Button]
   ✨^         ^         ^
Glowing   Normal    Normal
   Blue
```

## Integration with Existing Code

The glow system works with your current button setup without requiring changes to your existing scripts. It automatically detects selection changes and applies glows accordingly.

**Your dice selection still works exactly the same** - the glow is just a visual enhancement on top! 🎉
