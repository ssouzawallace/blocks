# Deployment Checklist

> **Project:** Blocks Programming (Unity 6000.2.10f1)  
> **Target Platforms:** Apple App Store · Google Play Store · Web (WebGL)

---

## 1. Apple App Store (iOS / iPadOS)

### Prerequisites
- [ ] macOS machine with Xcode 15+ installed
- [ ] Apple Developer Program membership (USD 99/yr)
- [ ] Provisioning profile & certificates configured in Xcode
- [ ] Bundle ID registered in App Store Connect

### Unity Build
- [ ] Switch platform to **iOS** (`File → Build Settings → iOS → Switch Platform`)
- [ ] Set `Bundle Identifier` (`Project Settings → Player → iOS → Other Settings`)
- [ ] Set `Version` and `Build` numbers
- [ ] Enable `Scripting Backend: IL2CPP`
- [ ] Set `Target minimum iOS Version` (recommend ≥ 14.0)
- [ ] Build to Xcode project (`Build`)

### Xcode
- [ ] Open generated `.xcodeproj` in Xcode
- [ ] Select correct Team and Signing Certificate
- [ ] Test on physical device (iPhone/iPad) — Play mode
- [ ] Run all Xcode tests / instruments (Memory, CPU)
- [ ] Archive: `Product → Archive`

### App Store Connect
- [ ] Create new App (or new version) in App Store Connect
- [ ] Upload build via Xcode Organizer or `altool`/`xcrun notarytool`
- [ ] Fill in metadata: name, description, keywords, support URL, privacy URL
- [ ] Add screenshots (6.7", 5.5", 12.9" iPad sizes at minimum)
- [ ] Set age rating
- [ ] Submit for Review
- [ ] Respond to any review feedback within 48 h

---

## 2. Google Play Store (Android)

### Prerequisites
- [ ] Google Play Developer account (USD 25 one-time)
- [ ] Android SDK / JDK installed (or Unity's embedded SDK)
- [ ] Keystore file created and stored securely (`keytool -genkey ...`)

### Unity Build
- [ ] Switch platform to **Android** (`File → Build Settings → Android → Switch Platform`)
- [ ] Set `Package Name` (`Project Settings → Player → Android → Other Settings`)
- [ ] Set `Version Name` and `Version Code`
- [ ] Enable `Scripting Backend: IL2CPP`
- [ ] Set `Target API Level` (≥ API 33 / Android 13 for new apps)
- [ ] Set `Minimum API Level` (recommend ≥ API 24 / Android 7.0)
- [ ] Enable `Split APKs by target architecture` (arm64-v8a + armeabi-v7a)
- [ ] Configure keystore under `Project Settings → Player → Android → Publishing Settings`
- [ ] Build **App Bundle** (`.aab`) — required by Play Store

### Testing
- [ ] Run on physical Android device or emulator (API 33)
- [ ] Test on at least one low-end device (≤ 2 GB RAM)
- [ ] Run Android Profiler (CPU, Memory, GPU)
- [ ] Run Firebase Test Lab or similar cloud device farm

### Play Console
- [ ] Create app in Google Play Console
- [ ] Upload `.aab` to **Internal Testing** track
- [ ] Complete Data Safety form (sensors, IronPython network usage)
- [ ] Add store listing: title, short description, full description, screenshots
- [ ] Add feature graphic (1024×500 px)
- [ ] Set content rating via questionnaire
- [ ] Promote to **Closed Testing** → **Open Testing** → **Production**
- [ ] Respond to any review flags within 7 days

---

## 3. Web (WebGL)

### Prerequisites
- [ ] A web hosting solution (GitHub Pages, Itch.io, Netlify, custom server)
- [ ] HTTPS enabled on the target domain
- [ ] `Cross-Origin-Opener-Policy: same-origin` and `Cross-Origin-Embedder-Policy: require-corp` headers configured (required for SharedArrayBuffer / threading)

### Unity Build
- [ ] Switch platform to **WebGL** (`File → Build Settings → WebGL → Switch Platform`)
- [ ] Enable `Compression Format: Brotli` (best file size)
- [ ] Set `Memory Size` (at least 256 MB recommended for IronPython)
- [ ] Disable `Exception Support: None` in Release, `Full (with stacktrace)` in Debug
- [ ] Build (`Build`)

### Testing
- [ ] Open build locally via a local HTTP server (`python -m http.server 8080`)
- [ ] Test in Chrome, Firefox, Safari, Edge
- [ ] Verify IronPython execution in browser (check browser console for errors)
- [ ] Test on mobile browser (iOS Safari, Chrome Android)

### Deployment
- [ ] Upload `Build/` output to hosting provider
- [ ] Set correct MIME types (`.wasm → application/wasm`, `.br → content-encoding: br`)
- [ ] Verify HTTPS and required COOP/COEP headers
- [ ] Add `index.html` with embed snippet or use Unity's generated template
- [ ] Smoke-test the live URL in multiple browsers
- [ ] Configure caching headers (`.wasm` and `.data` are large; cache aggressively)

---

## Cross-Platform Pre-Deploy Checklist

- [ ] All unit/PlayMode tests passing (`Window → General → Test Runner → Run All`)
- [ ] No compiler errors or warnings in Console
- [ ] IronPython scripts validated end-to-end in Play mode
- [ ] Accessibility review (font sizes, color contrast for color-blind users)
- [ ] Privacy policy URL set for all platforms
- [ ] Version bumped (`ProjectSettings/ProjectSettings.asset` → `bundleVersion`)
- [ ] `git tag vX.Y.Z` created and pushed
- [ ] Release notes / CHANGELOG updated
- [ ] Build artifacts stored in `docs/run-evidences/`
