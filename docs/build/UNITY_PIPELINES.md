# Reproducible Unity pipelines (#97, #98, #107–109)

Status: authored, not executed in Unity. No APK, AAB, Windows player, Xcode project or IPA has been produced by this work. A licensed Unity **6000.6.4f1** installation with the target module is required. Cinemachine 6.6.0 is intentionally retained: Unity's official 6000.6.0a6 release notes include that package version.

Use a trusted runner with a preconfigured licensed Editor; this repository does not activate licenses or accept installation terms. Set `UNITY_EDITOR` to its executable. Never commit licenses or signing credentials.

```sh
scripts/run-unity-tests.sh
export HUNTRX_BUILD_NUMBER=1 # Increase for subsequent Android/iOS artifacts.
scripts/build-unity.sh android-dev /absolute/fresh/player.apk 'Assets/Scenes/VerticalSlice.unity'
scripts/build-unity.sh windows-dev /absolute/fresh/windows/player.exe 'Assets/Scenes/VerticalSlice.unity'
scripts/build-unity.sh ios /absolute/fresh/ios-project 'Assets/Scenes/VerticalSlice.unity'
```

These scene arguments select the authored seven-beat opening prototype, not a completed campaign. The default Editor build scene list also starts with VerticalSlice; SampleScene and CombatLab remain listed but disabled. Explicit script arguments can select a different reviewed lab for diagnostics. Runtime validation remains pending. Each platform requires its module and supported host: iOS project export requires macOS; signed IPA/archive creation additionally requires Xcode and an authorized Apple signing configuration, not provided here. Windows distribution includes the EXE, matching Data directory and accompanying player files, not the EXE alone.

`HUNTRX_BUILD_NUMBER` must be an explicit positive integer for Android/iOS (CI may supply `GITHUB_RUN_NUMBER`). VERSION.txt determines bundle version; platform numeric versions use its major.minor.patch. Android release produces AAB and requires environment variables `HUNTRX_ANDROID_KEYSTORE_PATH`, `HUNTRX_ANDROID_KEYSTORE_PASSWORD`, `HUNTRX_ANDROID_KEY_ALIAS`, `HUNTRX_ANDROID_KEY_ALIAS_PASSWORD`. Keystore creation, account setup and publishing remain external authorized operations. Android development uses development/debug signing; it is not a production distribution.

Every build uses a fresh output path and writes a real BuildReport summary plus SHA-256 manifest. The verifier rejects missing output, wrong target layout, unrelated hash roots, empty scene lists, failed builds and changed artifacts. Upload the complete output root and `.evidence.json`; keep `.unity.log` for diagnostics. A valid digest demonstrates consistency, not independent provenance or store acceptance. Source commit is recorded from GITHUB_SHA; build from a clean checked-out commit for release evidence.

Workflow `.github/workflows/unity-validation.yml` runs version/verifier/shell tooling checks on pull requests and pushes to develop (no Unity execution). Licensed Unity jobs are manually dispatched and uses protected `unity-validation` environment and trusted self-hosted runners labelled `unity-6000.6.4f1`. Configure repository variable `UNITY_EDITOR_PATH` and corresponding OS labels. It first executes EditMode and PlayMode, then selected development build/export. No external publishing or license activation occurs. Test results use timestamp/PID folders, preserving previous evidence; skipped, empty or failed NUnit cases reject the run.

Sources: https://unity.com/vn/releases/editor/alpha/6000.6.0a6 ; https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/buildpipeline/buildplayer
