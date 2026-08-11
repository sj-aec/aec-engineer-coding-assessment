# AEC Coding Assessment — Agent Instructions

This repository is an interview assessment. AI-Agent use is expected, while the candidate remains responsible for scope, design decisions, code, and verification.

## Start each task

1. Read the root `README.md` and the selected exercise README.
2. Inspect the relevant source and tests before proposing changes.
3. Work only on the exercise selected by the candidate. Do not attempt every exercise unless explicitly requested.
4. State assumptions when BIM host behavior, document ownership, transaction semantics, metadata, or performance expectations are ambiguous.

Completion means the selected scope is implemented or clearly marked incomplete, focused tests have been run, and remaining risks are recorded.

## AI collaboration log

After each meaningful analysis, implementation, debugging, or verification step, append an entry to:

`solutions/ai-collaboration-log.md`

Each entry must include:

- timestamp and step title;
- candidate prompt or intent;
- AI action and recommendation;
- files inspected or changed;
- commands and observed result;
- assumptions;
- candidate decision: accepted, rejected, modified, or pending;
- remaining risks or incomplete work.

Record only observed facts. When the candidate has not responded to a recommendation, write `Pending`; never infer acceptance. Keep a chronological decision trail rather than copying the full conversation.

## Engineering boundaries

- Preserve public starter interfaces unless the candidate explicitly chooses a change and records the reason.
- Keep domain logic independent from commercial host APIs such as Revit or Rhino.
- Prefer focused tests for the selected exercise before running the whole solution.
- Treat generated code as untrusted until it compiles and its behavior is tested.
- Surface failed commands and partial implementations directly.
- Preserve TODOs outside the candidate's chosen scope.

## Before finishing

Report:

1. selected and completed scope;
2. files changed;
3. tests run and exact outcome;
4. AI recommendations the candidate changed or rejected;
5. remaining TODOs and risks.

Confirm that `solutions/ai-collaboration-log.md` contains the meaningful steps from the session.
