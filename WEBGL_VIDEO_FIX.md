# WebGL Video Fix Instructions

## Problem
WebGL builds don't support embedded VideoClip assets. Videos must use URL mode instead.

## Solution Steps

### 1. Move Video Files to StreamingAssets

**IMPORTANT:** Move these video files from their current location to the StreamingAssets folder:

- **Source:** `Assets/Sprites/UI/closing-scene.mp4`
- **Source:** `Assets/Sprites/UI/opening-scene.mp4`
- **Destination:** `Assets/StreamingAssets/`

**Steps:**
1. In Unity, navigate to `Assets/Sprites/UI/`
2. Select `closing-scene.mp4` and `opening-scene.mp4`
3. Cut (Ctrl+X) or drag these files
4. Navigate to `Assets/StreamingAssets/` (folder should now exist)
5. Paste (Ctrl+V) or drop the files here

### 2. Update VideoPlayer Components in Unity Editor

#### For Closing Scene:
1. Open the **ClosingScene** in Unity
2. Find the GameObject with the **VideoPlayer** component
3. In the Inspector:
   - Change **Source** from "Video Clip" to **"URL"**
   - Remove any assigned VideoClip asset
   - The script will now handle the URL path automatically
4. Make sure the **ClosingSceneController** script is attached and has the VideoPlayer reference

#### For Opening Scene:
**Option A:** If opening scene currently uses VideoPlayer:
1. Open the **OpeningScene** in Unity
2. Find the GameObject with the **VideoPlayer** component
3. In the Inspector:
   - Change **Source** from "Video Clip" to **"URL"**
   - Remove any assigned VideoClip asset
4. Replace **OpeningSceneController** with **OpeningVideoController** script
5. Assign the VideoPlayer reference

**Option B:** If opening scene uses Animator (train animation):
- If you prefer to keep the existing Animator-based approach, you can skip the opening scene video entirely
- The current `OpeningSceneController.cs` doesn't use videos, so no changes needed

### 3. Test the Build

1. **Test in Editor first:**
   - Play each scene to verify videos load and play correctly
   - Check console for the log messages showing video paths

2. **Build for WebGL:**
   - File → Build Settings
   - Select WebGL platform
   - Click "Build and Run" or "Build"
   - Videos should now work in the WebGL build

### 4. Deploy to itch.io

1. Zip your entire WebGL build folder
2. Upload to itch.io
3. Select "This file will be played in the browser"
4. Set as the "Main HTML file"

## What Changed in Code

### ClosingSceneController.cs
- Added URL-based video loading
- Video path: `Application.streamingAssetsPath + "closing-scene.mp4"`
- Automatically works for both Editor and WebGL builds

### OpeningVideoController.cs (NEW)
- New optional script for opening scene if using video
- Same URL-based approach as closing scene
- Can replace the existing Animator-based opening scene if desired

## Why This Works

- **StreamingAssets** folder is special - Unity copies files verbatim to builds
- `Application.streamingAssetsPath` automatically resolves to the correct location:
  - **Editor:** `Assets/StreamingAssets/`
  - **WebGL:** `StreamingAssets/` (served via HTTP)
- **URL mode** allows VideoPlayer to stream files instead of embedding them
- WebGL can stream video files via URLs, solving the embedded clip limitation

## Troubleshooting

**Videos don't play in WebGL:**
- Check browser console for errors
- Ensure videos are in MP4 format with H.264 codec (WebGL compatible)
- Try a different browser (Chrome/Firefox recommended)

**"File not found" errors:**
- Verify videos are in `Assets/StreamingAssets/`
- Check that VideoPlayer Source is set to "URL" not "Video Clip"
- Rebuild the project

**Videos lag or stutter:**
- Compress videos to reduce file size
- Use lower resolution if needed (720p or lower for WebGL)
