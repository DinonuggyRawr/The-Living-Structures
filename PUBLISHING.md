# How to publish The Living Structures to GitHub

## 1. Prepare your ORIGINAL Unity source

Extract this kit somewhere convenient, e.g.:

`C:\Users\tanne\Downloads\TLS_GitHub_Publication_Kit_v1.0.0`

Open PowerShell **in the extracted kit** and run the script shown in the publication kit `START_HERE.md`, updating your actual Unity project folder if it is different.

The script **copies** selected Unity project files; it never changes your Unity source or uploads anything. It preserves required `.meta` files and flags excluded binary/private files for review.

## 2. Verify staged source

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ".\scripts\Verify-TLSGitHubSource.ps1" -SourceFolder "$env:USERPROFILE\Downloads\TLS_GitHub_Source\TheLivingStructures_GitHub_Source"
```

Read `docs/SOURCE_PREP_REPORT.txt`, `THIRD_PARTY_NOTICES.md`, `SECURITY.md`, and `docs/RELEASE_CHECKLIST.md`. The source integrity verification does not establish legal/security sign-off.

## 3. Choose the original project LICENSE

- If you want other developers to use/modify/redistribute **your original code**, you can choose the included MIT license option by preparing source with `-UseMITLicense` (only after checking every asset is yours to license). If you already have a LICENSE in your actual Unity project, the script preserves it instead.
- If you prefer source visibility without granting reuse permissions, publish without a project LICENSE; default copyright protection applies. **Public is not automatically open source.**
- Do not apply your own MIT license to Unity, Zig, Wasmtime or other proprietary/third-party material. They retain their own licenses.

## 4. Create the GitHub repository

Use GitHub's website to create **an empty repository** named `TheLivingStructures` (don't auto-add README or a license, because these will come from the prepared source).

From PowerShell, with Git for Windows installed:

```powershell
cd "$env:USERPROFILE\Downloads\TLS_GitHub_Source\TheLivingStructures_GitHub_Source"
git init
git add .
git status --short
git diff --cached --stat
# REVIEW THE STAGED FILES BEFORE THE NEXT COMMAND.
git commit -m "Initial The Living Structures source release"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/TheLivingStructures.git
git push -u origin main
```

Replace `YOUR_USERNAME` with your actual GitHub username. Do not upload your ZIP *as a single repository file*; publish its individual source tree files. Consider enabling private repository visibility until the final third-party asset check is done, then change visibility when ready.

## 5. Release the Windows player only after Part B of the checklist

GitHub repository's **Releases → Draft a new release → tag `v1.0.0` (or `v1.0.0-rc1` while still testing)**. Attach the **complete approved Windows ZIP** (not only `.exe`), publish a SHA256 checksum, add `RELEASE_NOTES_v1.0.0-rc1.md` details, and label as prerelease while legal/security sign-off is incomplete.

**Avoid committing the bundled multi-gigabyte toolchain to Git.** GitHub rejects normal Git files over 100 MiB; release assets are the appropriate way to distribute large complete player ZIPs (individual release assets must be under 2 GiB).
