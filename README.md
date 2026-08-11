# AEC Engineer Coding Assessment

This repository contains independent C# exercises for AEC engineers working in design technology, BIM automation, and engineering software. The exercises cover domain modeling, transaction reliability, large-model processing, and Rhino geometry Bake/update workflows. They focus on production engineering judgment rather than complex algorithms.

| Exercise | Directory | Suggested time | Focus |
|---|---|---:|---|
| 01 Safe BIM Auto-fix Planning | [`01-auto-fix-plan`](01-auto-fix-plan/README.md) | 45–60 minutes | Diagnose safety policy, writeability, conflicts, and audit behavior |
| 02 Transaction Execution | [`02-transaction-execution`](02-transaction-execution/README.md) | 45–60 minutes | Diagnose transaction, cancellation, exception, and rollback defects |
| 03 Large-model Validation | [`03-large-model-performance`](03-large-model-performance/README.md) | 60–90 minutes | Optimize rule lookup while preserving streaming and progress behavior |
| 04 Rhino Geometry Bake and Update | [`04-rhino-geometry-bake-update`](04-rhino-geometry-bake-update/README.md) | 60–90 minutes | Diagnose idempotent Bake, object updates, Undo, and user-object protection |

## Environment

- .NET 8 SDK or later
- No Revit, Navisworks, Rhino, or Grasshopper installation is required
- Test framework: xUnit

## How to approach the assessment

You are **not expected to complete every exercise** in one interview or take-home session. Work within the agreed time:

1. Choose the exercise that best demonstrates your skills.
2. You may leave incomplete work as TODOs, but explain your intended approach, risks, and next steps.
3. Design decisions, code quality, and validation matter more than the number of completed exercises.
4. If specific exercises were agreed in advance, complete those exercises.

All four exercises contain baselines that require review. The initial repository intentionally contains both passing and failing tests; existing code is not a reference solution. Run the tests and record your observations before changing code.

The exercises do not require complex algorithms. Prioritize production boundaries, failure handling, maintainability, and evidence.

## AI-Agent collaboration

AI-Agent use is allowed and expected. The assessment evaluates whether you can provide useful context, control scope, review recommendations, correct mistakes, and validate generated changes. It does not reward the amount of AI-generated code.

Record each meaningful AI interaction as it happens in:

```text
solutions/ai-collaboration-log.md
```

The log should include:

- your goal or a summary of the prompt;
- files inspected and commands run by the Agent;
- important recommendations and assumptions;
- what you accepted, rejected, or modified, and why;
- tests or other validation evidence;
- unresolved risks.

Do not reconstruct a summary at the end or paste the complete conversation. Keep a chronological decision trail. You do not need to disagree with the Agent artificially, but you remain responsible for every accepted result.

[`AGENTS.md`](AGENTS.md) contains the shared Agent instructions. Before submission, run:

```bash
bash scripts/verify-ai-collaboration.sh
```

The script checks only that the log contains chronological entries and an explicit candidate decision. It does not assess collaboration quality.

## Submission by Pull Request

Submit through **Fork + Pull Request**. Evaluation is based on the diff, commits, description, and evidence visible in the PR.

1. Fork this repository on GitHub.
2. Clone your Fork and ensure it is synchronized with the latest version of this repository.
3. Complete the work within the agreed time, commit it, and push it to your Fork. Incomplete TODOs are allowed when documented in the PR.
4. Open a PR against this repository.
5. The PR description must include:
   - selected exercises and actual completed scope;
   - important design decisions and trade-offs;
   - incomplete work, known risks, and next steps;
   - exact test commands and observed results;
   - performance evidence when Exercise 03 is selected;
   - confirmation that `solutions/ai-collaboration-log.md` is updated.
6. Keep the PR open for interviewer review. A Draft PR may be used during the exercise; mark it Ready for review when finished.
7. Do not commit `bin/`, `obj/`, IDE settings, test artifacts, or other generated files.

Unselected exercises may intentionally contain failing tests, so the entire solution is **not required to have all tests passing**. However, this command must succeed:

```bash
dotnet build CodingAssessment.sln
```

List focused test results for selected exercises and any expected failures accurately in the PR.

Run one exercise independently with:

```bash
cd 01-auto-fix-plan
dotnet test
```

You may also inspect the whole baseline with:

```bash
dotnet test CodingAssessment.sln
```

## General rules

1. Preserve public interfaces unless you explain why a change is necessary.
2. Do not add commercial host dependencies such as Revit or Rhino; starter interfaces and data types represent those boundaries.
3. Prefer correctness, testability, and explicit failure semantics.
4. Do not swallow exceptions or depend on accidental collection iteration order.
5. You may add internal types, helper methods, and tests.
6. Record important decisions and trade-offs in the selected exercise README.

## Evaluation

All exercises use these shared dimensions:

- Correctness and edge cases: 25%
- Code design and readability: 20%
- Tests and validation evidence: 20%
- AI-Agent guidance, review, and correction: 20%
- Explanation, trade-offs, and incomplete-work risks: 15%

Each exercise README also defines exercise-specific priorities, such as transaction semantics, large-model performance, or Rhino Bake integration. Evaluation combines the shared dimensions with the priorities relevant to the selected exercise.

Exercises are independent and may be submitted separately. You are not penalized simply for leaving unselected exercises incomplete. A 90–120 minute interview will normally focus on one primary exercise, with a second exercise attempted only if time permits.
