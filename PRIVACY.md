# Privacy (source-reviewed scope)

The Living Structures is designed for local/offline educational use. The reviewed core application maintains local learning progress through Unity's PlayerPrefs and other local state; it does not require a cloud account to complete the laboratories.

This description is **not a completed data-flow audit** of all Unity packages, engine diagnostics or a final binary. The publisher must inspect `Packages/manifest.json`, Unity services/settings, plugin network behavior, error reporting, and any imported SDKs before making categorical privacy claims to end users.

Student-written C is processed by the **local** Zig executable and Wasmtime process. Users should avoid entering personal data or secrets in lab code. Security limitations are documented in `SECURITY.md`.
