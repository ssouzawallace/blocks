# Blocks Repository Compliance and License Review

Date reviewed: 2026-05-15

This is an engineering review, not legal advice. Any release or commercial distribution should be checked by the project owner or counsel.

## Scope

Reviewed repository layout, file types, dependency manifests, CI configuration, license files, bundled third-party assets, and publish/private license posture.

Primary sources:

- `LICENSE`
- `README.md`
- `Packages/manifest.json`
- `.github/workflows/ci.yml`
- `.github/dependabot.yml`
- `.gitignore`
- `Logs/Packages-Update.log`
- `Assets/Plugins/IronPython/Lib/LICENSE.txt`
- `Assets/Resources/IronPython/Lib/LICENSE.txt`
- `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`
- `Assets/TextMesh Pro/Sprites/EmojiOne Attribution.txt`

## Step 1: Languages and notable file types

| Language / format | Evidence | Notes |
| --- | --- | --- |
| C# | `Assets/**/*.cs` | Unity gameplay, editor tooling, block programming, robot simulation, and edit mode tests. |
| Python | `Assets/Models/**/*.py`, `Assets/Tests/Python/**/*.py`, `Assets/**/IronPython/**/*.py`, `parsetab.py` | Blender generation scripts, Python tests, and bundled IronPython/CPython library content. |
| ShaderLab / HLSL / Cg | `Assets/TextMesh Pro/Shaders/**/*.shader`, `*.cginc` | TextMesh Pro shader assets. |
| YAML / Unity serialized assets | `.github/**/*.yml`, `Assets/**/*.prefab`, `Assets/**/*.unity`, `ProjectSettings/**/*.asset` | GitHub Actions, Dependabot, Unity scenes, prefabs, settings, materials, and metadata. |
| JSON | `Packages/manifest.json`, `ProjectSettings/SceneTemplateSettings.json`, `Assets/Resources/BillingMode.json` | Unity package manifest and configuration. |
| Markdown | `README.md`, `docs/*.md` | Project documentation. |
| XML | IDE/project cache files under `.vs/` and `.idea/` | Local IDE metadata is present in the repo. |
| HTML, VBScript, Batch, data fixtures | `Assets/**/IronPython/Lib/**` | Vendored standard-library/test data from bundled IronPython/Python libraries. |
| Binary assets | `*.dll`, `*.exe`, `*.png`, `*.gif`, `*.ttf`, `*.dfont`, `*.pdf` | Bundled libraries, Unity assets, images, fonts, and documentation. |

High-volume findings:

- The repository contains many vendored IronPython/Python library files under both `Assets/Plugins/IronPython/` and `Assets/Resources/IronPython/`.
- Unity `.meta` files dominate file count, which is normal for Unity projects.
- Tracked ignored/generated files are present: `.vs/...`, `Logs/Packages-Update.log`, and `obj/Debug/...`.

## Step 2: Dependency and project compliance review

### Unity packages

Current packages from `Packages/manifest.json`:

| Package | Version | Review status |
| --- | ---: | --- |
| `com.unity.ai.navigation` | `2.0.9` | Verify Unity 6.2 compatibility and license terms before release. |
| `com.unity.collab-proxy` | `2.10.0` | Editor integration; likely unnecessary in player builds. Confirm it is needed. |
| `com.unity.ide.rider` | `3.0.38` | Editor-only IDE integration. |
| `com.unity.ide.visualstudio` | `2.0.25` | Editor-only IDE integration. |
| `com.unity.test-framework` | `1.6.0` | Test dependency. |
| `com.unity.timeline` | `1.8.10` | Runtime/editor package; confirm actual usage. |
| `com.unity.ugui` | `2.0.0` | Runtime UI dependency; used by block/editor UI code. |
| `com.unity.modules.*` | `1.0.0` | Built-in Unity modules. Confirm only required modules remain enabled. |

Findings:

- `Packages/packages-lock.json` is not present. Unity projects commonly use a lock file to make package resolution reproducible.
- `.github/dependabot.yml` has an empty `package-ecosystem`, so Dependabot version updates are not correctly configured.
- `Logs/Packages-Update.log` shows old packages such as Ads, Analytics, Purchasing, and VSCode support were historically present. They are not in the current manifest, but this should be verified in Unity Package Manager before release.

### GitHub Actions and CI dependencies

Current workflow in `.github/workflows/ci.yml`:

- `actions/checkout@v4`
- `actions/cache@v4`
- `actions/upload-artifact@v4`
- `game-ci/unity-test-runner@v4`
- `pip install pytest --quiet`

Findings:

- GitHub Actions use major-version pins only. For stricter supply-chain compliance, pin actions to full commit SHAs.
- `pytest` is installed unpinned in CI. Add a small Python requirements file or pin the version in the workflow for reproducibility.
- Unity tests depend on Unity credentials/secrets (`UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`). Ensure these are repository or organization secrets, never committed.

### Python and Blender scripts

Relevant areas:

- `Assets/Models/Board/`
- `Assets/Models/Robots/`
- `Assets/Models/Scenarios/`
- `Assets/Tests/Python/`

Findings:

- Blender scripts import `bpy`, `math`, and `os`; `bpy` is provided by Blender and is not declared in a dependency file.
- Python tests import `pytest`, but no pinned Python dependency file is present.
- CI syntax-checks Blender model scripts and runs `Assets/Tests/Python/`.

### Bundled IronPython

Relevant areas:

- `Assets/Plugins/IronPython/`
- `Assets/Resources/IronPython/`

Findings:

- IronPython DLLs and Python library content appear duplicated in both `Assets/Plugins` and `Assets/Resources`.
- Bundled DLLs include `IronPython.dll`, `Microsoft.Dynamic.dll`, and `Microsoft.Scripting*.dll`.
- Bundled EXEs exist under the Python `distutils` library directories.
- Python license files are present in both bundled library trees.

Risk:

- Duplicated runtime libraries can increase build size, complicate license attribution, and create ambiguity about which copy is loaded.
- Bundled EXEs may be unnecessary for a Unity player build and should be reviewed before distribution.

### TextMesh Pro and font / emoji assets

Relevant files:

- `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`
- `Assets/TextMesh Pro/Fonts/LiberationSans.ttf`
- `Assets/TextMesh Pro/Sprites/EmojiOne Attribution.txt`
- `Assets/TextMesh Pro/Sprites/EmojiOne.png`

Findings:

- Liberation Sans includes the SIL Open Font License text.
- EmojiOne attribution says to review EmojiOne licensing terms externally. That should be resolved before publishing builds containing the sprite asset.

### Local/generated files committed to the repo

Tracked ignored/generated files found:

- `.vs/Blocks Programming/xs/UserPrefs.xml`
- `.vs/Blocks Programming/xs/project-cache/*.json`
- `Logs/Packages-Update.log`
- `obj/Debug/*`

Risk:

- IDE and build cache files can contain environment-specific paths or stale metadata.
- The `.gitignore` already excludes `.vs/`, `Logs/`, and `obj/`; these files should likely be removed from tracking in a cleanup PR.

## Step 3: Blocks publish and private license terms

### Repository license

The root `LICENSE` is MIT:

- Allows use, copy, modification, merge, publishing, distribution, sublicense, and sale.
- Requires preserving copyright and permission notices.
- Disclaims warranty.

### Publish posture

The project can be published under MIT from the repository license perspective, provided third-party notices and package terms are respected.

Before publishing a release:

1. Include the root MIT license in source and binary distributions.
2. Include third-party notices for bundled IronPython/Python libraries, TextMesh Pro assets, Liberation Sans, EmojiOne, Unity packages, and any other assets included in builds.
3. Verify whether bundled Unity assets and packages permit the intended distribution model.
4. Confirm whether IronPython DLLs and standard library files are required in both `Plugins` and `Resources`.
5. Confirm whether EmojiOne sprite licensing permits the intended commercial/non-commercial use.

### Private license posture

No separate private/commercial license terms were found in the repo. If Blocks needs private licensing terms, add explicit documentation that answers:

- Whether private forks or commercial builds are allowed under different terms.
- Whether contributors license contributions under MIT only or under dual terms.
- How third-party notices are handled in private builds.
- Who owns project trademarks, logos, screenshots, robot/block designs, and generated model assets.

## Compliance risks and recommended action plan

### Priority 1: Release blockers / high value cleanup

1. Create a third-party notices inventory.
   - Include IronPython/Python, Unity packages, TextMesh Pro, Liberation Sans, EmojiOne, Blender-generated assets if applicable, GitHub Actions, and CI Python dependencies.
2. Resolve EmojiOne licensing.
   - Confirm whether `EmojiOne.png` can be shipped with Blocks under the intended release model or replace it.
3. Audit bundled IronPython.
   - Determine why both `Assets/Plugins/IronPython/` and `Assets/Resources/IronPython/` exist.
   - Remove duplicate or unused runtime/library copies if safe.
   - Confirm whether bundled `distutils` EXEs are needed.
4. Remove tracked generated/local files.
   - Untrack `.vs/`, `obj/`, and `Logs/` files that are already covered by `.gitignore`.

### Priority 2: Dependency reproducibility

1. Add or regenerate Unity package lock metadata if appropriate for this Unity version.
2. Fix `.github/dependabot.yml`.
   - Configure valid ecosystems, such as `github-actions` for `/.github/workflows`.
   - Add other ecosystems only where manifests exist.
3. Pin Python test dependencies.
   - Add a small requirements file for CI or pin `pytest` in the workflow.
4. Consider pinning GitHub Actions to commit SHAs for stricter supply-chain compliance.

### Priority 3: Documentation and policy

1. Add `THIRD_PARTY_NOTICES.md`.
2. Add `SECURITY.md` with vulnerability reporting instructions.
3. Add `CONTRIBUTING.md` with contributor license expectations.
4. Add release checklist documentation covering Unity version, test commands, third-party notices, and asset license verification.
5. Decide whether a private/commercial licensing statement is needed.

## Suggested PR breakdown

### PR 1: Compliance inventory documentation

- Add this review.
- Add initial `THIRD_PARTY_NOTICES.md`.
- Document known license questions and owners.

### PR 2: Remove generated/local files

- Untrack `.vs/`, `obj/`, and `Logs/` files.
- Keep `.gitignore` coverage.
- Verify Unity still opens and tests still run.

### PR 3: Dependency automation

- Fix Dependabot configuration.
- Pin Python test dependencies.
- Consider action SHA pinning.

### PR 4: IronPython cleanup

- Identify which IronPython tree is used at runtime.
- Remove duplicate/unneeded content only after Unity/editor validation.
- Re-test Python editor and runtime features.

### PR 5: Publish/license readiness

- Resolve EmojiOne licensing.
- Add final third-party notices.
- Add release checklist and private/commercial licensing statement if needed.

## Suggested issues for backtracking

These should be created as GitHub issues if the project owner wants issue-based tracking:

1. **Create third-party notices inventory**
   - Track all bundled code, packages, fonts, images, and generated assets.
2. **Resolve EmojiOne sprite licensing**
   - Confirm permission or replace asset.
3. **Audit duplicate IronPython bundles**
   - Determine whether both `Assets/Plugins/IronPython` and `Assets/Resources/IronPython` are needed.
4. **Remove tracked generated files**
   - Untrack IDE/build/log cache files already covered by `.gitignore`.
5. **Fix Dependabot configuration**
   - Replace empty `package-ecosystem` with valid entries.
6. **Pin CI Python dependencies**
   - Make Python tests reproducible.
7. **Add publish/license release checklist**
   - Document required tests, Unity version, notices, and asset-license checks.

