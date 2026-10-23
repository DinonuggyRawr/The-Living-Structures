# Unity Phase 3.1 Acceptance Checklist

1. Back up and import patch; check Unity Console and run full EditMode suites, including TLSPointerMemoryModelTests, TLSPointerMemorySessionTests, TLSPointerMemoryLayoutTests.
2. Academy now has `ENTER POINTER & MEMORY LAB` below the Array/Queue row. Ensure it does not overlap and BACK TO ACADEMY works.
3. Select MALLOC + FIELDS. Before allocation no heap block; after malloc, symbolic 0x1000 exists but data/next say UNINIT. Step to data=42; next must still be UNINIT. Step to next=NULL; fields now independently initialized.
4. Select ALIASING. Choose a prediction at step 0; at last step head and alias both point to 0x1000, data=99; no second allocation. Answer is reviewed only after finishing.
5. Select LINK NODES. After finishing, head -> 0x1020, 0x1020.next -> 0x1000; values 20 and 10, with no false leak label.
6. Select DANGLING. At `free(head)` memory shows FREED/NO ACCESS and alias is marked DANGLING conceptually; clearing head alone leaves alias dangling. In SAFE CLEANUP both local references finish NULL.
7. Select LOST POINTER. Finishing without free marks live 0x1000 UNREACHABLE (LEAK?).
8. Select MALLOC FAIL. No heap block is created; NULL guard prevents access.
9. Select INVALID ACCESS. Final step prominently says BLOCKED; never runs unsafe C or writes a block.
10. Exercise NEXT/PREVIOUS/FINISH/RESET, changing scenarios and predicting a correct and incorrect answer. Prediction review should never reveal an answer until the last trace step.
11. Regression: Stack PUSH/POP, Array insert, Queue wraparound; Stack WASM PUSH 8/8, Array Append 10/10 and Queue Enqueue 10/10. Confirm no new saves and all existing learning views still open.
12. Visual QA at 1600×900 and a smaller Game View resolution; inspect code truncation, button spacing, header visibility, pointer arrow labels, non-overlapping controls, and minimum readable font sizes.

The authored NUnit suites and static checks in the source ZIP are **not evidence** that Unity has compiled or run the patch. Record actual Unity results separately.
