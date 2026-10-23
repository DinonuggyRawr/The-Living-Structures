# Security policy

## Supported scope

The Living Structures is a **locally installed, single-user educational application**. It is not intended as a secure service for untrusted code, shared machines, remote users, or production workloads.

## Native C11 compilation warning

The educational C11 runner invokes **native Zig** under the logged-in user's permissions and then runs compiled WebAssembly via **Wasmtime** with bounded execution settings. Wasmtime resource restrictions do **not** isolate the native compiler phase. Compiler timeouts are not a Windows OS sandbox: they do not provide full filesystem, process-tree, or memory containment.

**Do not paste private secrets or execute adversarial or untrusted source code.** There is no claim of resistance to hostile local input, malicious native dependencies or compiler exploitation. Build and execute only in trusted environments, and prefer disposable test accounts/VMs for unusual inputs.

## Reporting issues

Do not publish exploitable security details in a public issue. Use GitHub's **Report a vulnerability** / private vulnerability reporting option if enabled, or contact the repository maintainer through a privately designated channel. (Repository maintainers should configure a real private contact method in their repository settings.)

## Review status

- User reports offline clean-machine operation and green Unity EditMode test suites for the accepted project.
- The source has bounded C inputs, compilation/runtime timeouts, manifest checks on bundled executables, and Wasmtime memory/fuel restrictions.
- Native compilation lacks dedicated OS-level isolation; this should be improved and independently threat-tested before enabling community/remote arbitrary submissions.
- Full transitive dependency/security review and the final packaged Windows binary's provenance require evidence from the **actual release build**.

Never treat this document as a security certification.
