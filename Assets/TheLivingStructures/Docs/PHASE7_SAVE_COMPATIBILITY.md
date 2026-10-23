# Phase 7 Save/Compatibility Boundary

This patch does not migrate, rewrite, or clear Phase 1–6 saved progress. New data uses only `PlayerPrefs` key `tls_phase7_learning_evidence_v1` and a `version` integer with value `1`.

Record contains:
- 13 bounded topic evidence counters: views, assisted work, independent reasoning, compiler first clean passes, mistakes and test designs
- per-topic UTC ticks for prior study and next due review
- bounded unique attempted/clean IDs for first-attempt evidence
- first completed baseline/final conceptual summary scores.

Record omits source code drafts, contact information, names, health/financial data, telemetry, real network information or OS shell configuration. It is local to Unity PlayerPrefs for that application/device and not a cross-device student profile.

On load, invalid JSON, unsupported version, missing topics, mismatched IDs, duplicate attempt keys or implausible counts are rejected. The existing key is NOT overwritten on invalid load; the dashboard explicitly warns and persists no further Phase 7 attempts until the record is repaired externally from backup. This is intentionally conservative. Manual reset/import/export UI is not included in this patch. Back up before experiments.

Tests should confirm: legacy Academy save survives; Phase 7 untracked status is not converted into mastery; first seen/assisted/clean attempts remain distinct across reload; invalid records do not silently reset.
