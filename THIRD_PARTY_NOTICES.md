# Third-party software and attribution

This document identifies components observed in the project scripts. **It is an inventory and review aid, not a completed binary-level license audit.** The exact binaries and Unity Package Manager manifest used for a released Windows player determine the final obligations.

| Component | Version observed in setup scripts | Upstream license/sources |
|---|---|---|
| Zig compiler | 0.17.0 | MIT/Expat — https://codeberg.org/ziglang/zig/src/tag/0.17.0/LICENSE |
| Wasmtime CLI | 50.0.0 | Apache-2.0 WITH LLVM-exception — https://github.com/bytecodealliance/wasmtime/blob/v50.0.0/LICENSE |
| LLVM / Clang and Zig toolchain dependencies | Versions embedded in pinned Zig distribution | Generally Apache-2.0 WITH LLVM-exception for LLVM; additional component notices may apply — https://llvm.org/docs/DeveloperPolicy.html |
| Unity Engine and Unity packages | Inspect local `ProjectSettings/ProjectVersion.txt` and `Packages/manifest.json` | Unity software/asset terms and package-specific notices — https://unity.com/legal |

## Requirements for a Windows binary release

1. Check the exact built `TLS_Toolchain_Windows` contents and the provenance of every native executable and dependent binary.
2. Include **full, version-matched upstream license text** (and applicable NOTICE or third-party attributions) in the distributable ZIP. This inventory is **not a substitute for those texts**.
3. Review Unity's active plan/tier, distribution terms, and every imported package/asset/font's redistribution grant. Verify source ownership; some imported content may not be sublicensable.
4. Retain the software's independent creator copyright notice and avoid implying endorsement by Zig, Wasmtime, LLVM, Unity, or a university.
5. Maintain a versioned list of exact versions/hashes, binary sources, relevant known vulnerabilities and reviewer sign-off.

**As prepared, this file does not establish legal clearance to distribute bundled third-party binaries.** See `RELEASE_CHECKLIST.md` and the release-kit review notes.
