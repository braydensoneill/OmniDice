# 🎯 How to Create Products in Google Play Console

## The Exact Steps You Need to Take

### 1. Open Google Play Console
1. Go to **https://play.google.com/console** in your web browser
2. Sign in with your Google account
3. Find your **OmniDice** app and click on it

### 2. Navigate to In-App Products
1. Look at the **left sidebar menu**
2. Click **"Monetization"** (💰 icon)
3. Click **"Products"**
4. Click **"In-app products"**

### 3. Create Your First Product
You'll see a page that might say "No in-app products" - that's normal!

1. Click the big blue **"Create product"** button
2. A form will appear with these fields:

### 4. Fill Out the Form (Example with first skin)
```
Product ID: skin_black_gloss_red
⚠️ CRITICAL: Copy this exactly - no spaces, no capitals!

Name: Black Gloss Red Dice Skin
(This is what users see when they buy it)

Description: Premium Black Gloss Red dice skin for OmniDice
(Short description of what they're buying)

Status: Active ✅
(MUST be Active or purchases won't work!)

Price: $0.99 (or whatever you want to charge)
```

3. Click **"Save"** to create the product

### 5. Repeat 7 More Times
Create a product for each of these **exact** Product IDs:

1. ✅ `skin_black_gloss_red` (just did this one)
2. `skin_chalk_stone_green`
3. `skin_chrome_damaged_white` 
4. `skin_chrome_matte_white`
5. `skin_eroded_marble_blue`
6. `skin_eroded_marble_white`
7. `skin_gold_fringed_marble_black`
8. `skin_white_on_black_matte`

### 6. Copy/Paste Template
For each product, use this template and just change the Product ID and Name:

```
Product ID: [paste exact ID from list above]
Name: [Skin Name] Dice Skin
Description: Premium [skin name] dice skin for OmniDice  
Status: Active
Price: $0.99
```

### 7. What You Should See When Done
After creating all 8 products, you should see:
- A list showing **8 in-app products**
- All showing **"Active"** status
- All showing your prices

### 8. Common Problems & Solutions

**Problem**: "Product not available" error in your game
**Solution**: Make sure Product ID is **exactly** right (no typos, no capitals)

**Problem**: Purchase screen doesn't appear  
**Solution**: Make sure product Status is **"Active"**, not "Inactive"

**Problem**: Can't find Monetization menu
**Solution**: Make sure you selected your app first from the main console page

### 9. Test Your Products
1. Go to **"Release" → "Testing" → "Internal testing"**
2. Upload your signed APK 
3. Install from Play Store (not directly from Unity)
4. Test purchases - they'll be **free** during internal testing

## 🎉 That's It!
Once you create these 8 products in Google Play Console, your Unity game will automatically connect to them and start processing real Google Pay purchases!

The Product IDs in your code match exactly with what you create in Google Play Console, so everything will work seamlessly.

## Need the Product IDs Again?
They're all in `PRODUCT_IDS.md` - just copy/paste from there to avoid typos!
