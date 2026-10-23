# The Living Structures v1.0 — GitHub publication checklist

## A. Public source repository (can proceed after source/rights review)

- [ ] Stage from the **actual latest working Unity project** with `Prepare-TLSGitHubSource.ps1`. Keep `.meta`, `Packages`, `ProjectSettings`, `Assets`.
- [ ] Verify `SOURCE_SHA256SUMS.txt` using the included source verifier.
- [ ] Check `docs/SOURCE_PREP_REPORT.txt` for excluded plugins/assets, secrets, and >50 MiB files. Carefully review `Packages/manifest.json`, `ProjectSettings`, Unity services, and all bundled/imported assets for rights.
- [ ] Select a license for your **original project-owned code**. The kit includes an MIT option but does not apply it without `-UseMITLicense`. If no LICENSE exists, public source defaults to copyright-protected (not open source).
- [ ] Confirm creator credits, screenshots and descriptions are accurate and approved for publication.
- [ ] Make a new empty GitHub repository; push the SOURCE FOLDER CONTENTS. No `.exe`, `.dll`, `*_Data`, Zig libs, `Library`, OneDrive personal paths, PlayerPrefs files, or standalone ZIPs in Git history.
- [ ] Add issues/privacy/security contacts and repository topics.

## B. Windows downloadable release (additional requirements)

- [ ] Build from accepted hotfixed project and run full Unity EditMode suite (user reported passing; test the exact final build).
- [ ] Run relevant PlayMode/smoke tests, responsive buttons, navigation and Exit App.
- [ ] Verify clean-machine offline full archive (previous developer-reported pass; **repeat against exact distributable ZIP**).
- [ ] Run the release binary audit helper and the earlier RC builder/verifier: verify file hashes, DLLs, licensed components and complete archive.
- [ ] Provide full license/notice texts for exact Zig, Wasmtime, LLVM/Clang and other transitive native binaries. Confirm Unity Engine/Asset Store/Package Manager distribution terms and all imported assets/fonts.
- [ ] Check version-specific Zig/Wasmtime security advisories and binary provenance. Log any compiler timeout with reproduction information.
- [ ] Review the native compiler risk: student code is passed to Zig **with normal Windows user permissions**, outside the Wasmtime sandbox. This is a **local educational tool**, not an adversarial input environment. Confirm protections and user-facing warning are appropriate for planned audience. Consider native process isolation for stronger claims.
- [ ] Complete responsible maintainer sign-off; upload only reviewed/approved whole application ZIP to GitHub Releases (not regular source tree), supply SHA256 checksum, startup instructions and known limitations.

## C. Public announcement

- [ ] Make clear the app is a Windows x64 Unity application, not a standalone single EXE, and source-only cloning doesn't include the compiler bundle.
- [ ] Make no formal security certification or guaranteed pedagogy claim.
- [ ] Invite tester feedback via issues and private vulnerability reporting.

**Conclusion:** This checklist deliberately separates a publishable source repository from an approved Windows binary release. Do not mark Part B complete simply because prior local tests succeeded.
