# The Living Structures

**Learn data structures by seeing them, tracing them, and writing the C code yourself.**

The Living Structures is an interactive **Computing II and data structures learning academy** built with **Unity**. It combines a navigable 3D learning environment with focused 2D laboratories, step-by-step algorithm visualizations, pointer and memory diagrams, and hands-on **C11 programming challenges**.

The goal is to make abstract computer science concepts understandable through experimentation: see what changes in memory, predict what happens next, debug a mistake, and then implement the underlying logic in C.

> **Project status:** Developed through its initial learning curriculum and undergoing private Windows standalone and release-quality assurance. Public distribution, licensing, security review, and performance validation are not yet finalized.

## What You Can Learn

| Topic | Interactive learning focus |
| --- | --- |
| **Arrays** | Fixed-capacity storage, indexing, insertion, removal, and element shifting |
| **Stacks** | LIFO operations, push/pop/peek, bounds, and state tracing |
| **Circular queues** | FIFO operations, head/tail movement, wraparound, and capacity |
| **Pointers and memory** | `NULL`, `struct`, aliases, `malloc`, `free`, dangling pointers, and leaks |
| **Linked lists** | Singly, doubly, and circular links, traversal, insertion, deletion, and ownership |
| **Recursion** | Base cases, call stacks, recursive calls, and return flow |
| **Trees and BSTs** | Tree structure, traversal, insertion, search, and deletion cases |
| **Heaps** | Min/max heaps, heapify, extraction, and priority ordering |
| **Graphs** | Directed/undirected graphs, breadth-first search, and depth-first search |
| **Hash tables** | Chaining, linear probing, collisions, and tombstones |
| **Algorithms and complexity** | Search/sort exercises, operation counts, and Big-O reasoning |
| **Debugging and testing** | Diagnose errors, reason about edge cases, and improve C code |

## How Learning Works

The experience is designed to progress from **understanding** to **independent implementation**:

1. **Explore:** Enter the academy and choose a data structure or topic.
2. **Watch:** Step through a visual model of what the algorithm or operation does.
3. **Trace:** Follow highlighted C statements alongside changing data and memory state.
4. **Predict:** Answer what should happen before advancing the simulation.
5. **Practice:** Complete C statements, fix bugs, and reason about edge cases.
6. **Implement:** Write whole C functions and inspect feedback from supported tests.
7. **Assess:** Work through independent problems and topic checks to verify understanding.

Guided activities and independent assessments are different: watching a demonstration alone is not evidence that you can implement the concept unaided.

## C11 First, Not C++

The instructional language is **C11** throughout. Exercises emphasize the mechanics that matter in Computing II: pointers, `struct`, arrays, dynamic allocation, explicit memory management, recursion, and carefully defined function behavior. The project does not substitute C++ containers or `new`/`delete` for these fundamentals.

## Real C Code and Visual Simulations

The Living Structures uses **two complementary execution approaches**:

- **Educational visual models** provide deterministic, rewindable representations of structures, pointers, memory, and algorithm steps. These help explain behavior, but are not themselves a complete C runtime.
- **Compiler-backed C practice** uses a local Zig C toolchain to compile supported C11 exercises to WebAssembly and Wasmtime to run bounded test cases. Supported exercises compare behavior and state against expected outcomes, including important boundary cases.

A passing exercise is useful test evidence, not proof that arbitrary C programs are correct. The compiler integration is intended for local learning; it is **not a security-certified service for running untrusted submissions**.

## Technology

- **Unity / C#** for the academy, interface, visualizations, and learning state
- **C11** for programming examples and coding assessments
- **Zig** for the supported C-to-WebAssembly compilation workflow
- **Wasmtime** for bounded WebAssembly execution
- **NUnit / Unity Test Framework** for automated project tests

No Docker setup is required for the existing local compiler workflow.

## Getting Started

### Open the Unity project

1. Clone or download the repository when source access is available.
2. Use the Unity Editor version specified in `ProjectSettings/ProjectVersion.txt`.
3. Open the project folder through Unity Hub.
4. Open the project's academy scene and enter Play Mode.
5. Choose a topic to explore its visual lab and practice activities.

Visual demonstrations can be explored separately from compiler-backed coding. To run the supported real-C exercises in development, configure the project-local **Zig** and **Wasmtime** toolchain according to the included project setup and toolchain checks. These tools are not automatically downloaded at runtime.

### Windows standalone builds

The project also has a Windows offline toolchain-bundling workflow for private builds. A packaged player must be built and validated with the intended compiler/runtime files; simply cloning the source does not provide a ready-to-run standalone executable. Check release notes and the toolchain integrity checks before distributing a build.

## Current Development Status

The core curriculum and interactive labs have been developed and tested during project development. Current release work focuses on:

- Windows standalone responsiveness and UI performance
- Regression testing and compiler-toolchain integrity
- Clean-machine and offline build verification
- Security review and third-party redistribution/license requirements
- Final accessibility, packaging, and public-release readiness

Some functionality may change while this work is underway. **Do not treat this repository as a finished or security-hardened public release.**

## Educational Philosophy

**The Living Structures is built around a simple idea: students should be able to understand *why* code behaves the way it does, not just memorize an implementation.**

Visual explanations make new concepts approachable. Optional deeper inspection, C programming, debugging, and independent challenges help students move beyond recognition toward real understanding.

## License

A public source-code license has **not yet been selected**. Unless and until a license is added, no permission to copy, modify, or redistribute the source code is granted by default. Third-party toolchain components remain subject to their own licenses.

## Creator

Created by **Tanner Yasi** as an educational project focused on making data structures, algorithms, and C programming more accessible through interactive learning.
