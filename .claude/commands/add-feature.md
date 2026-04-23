Orchestrate a full MVVM feature lifecycle for: $ARGUMENTS

Execute the following pipeline sequentially. Each step must complete before the next begins.

---

## Step 1 — Plan

Use the `ios-planner` agent to analyze the codebase and produce a structured execution plan.

Prompt the agent with:
- The feature description: $ARGUMENTS
- Requirements: break into atomic tasks, identify files to create/modify, specify task order and dependencies, assign each task to dev/test/review
- Output the plan to `plan.md` in the project root

Wait for `plan.md` to be created before proceeding.

---

## Step 2 — Implement

Use the `ios-mvvm-dev` agent to implement the feature according to `plan.md`.

Instructions:
- Read `plan.md` and execute all tasks assigned to "dev" in order
- Follow MVVM conventions: Models in `Models/`, ViewModels alongside ViewControllers in feature folders, Services in `Services/`
- All UI must be programmatic UIKit with Auto Layout — no storyboards or XIBs
- Do NOT write any test files

---

## Step 3 — Test

Use the `ios-xctest-generator` agent to generate XCTest unit tests.

Instructions:
- Read `plan.md` and generate tests for all ViewModel and Service code created or modified
- Focus on business logic paths, service layer behavior, edge cases, error states, and state transitions
- Place tests in `TechDemoTests/` following existing naming conventions
- Do NOT modify any files under `TechDemo/` (production code)

---

## Step 4 — Review

Use the `ios-diff-reviewer` agent to review the git diff.

Instructions:
- Review all changes for MVVM violations, bugs, missing test coverage, and code quality issues
- Report findings with severity (HIGH/MEDIUM/LOW), file and line reference, description, and suggested fix

---

## Step 5 — Fix Loop (conditional)

If Step 4 reports any HIGH severity findings:
- Use the `ios-mvvm-dev` agent to fix each HIGH issue (only modify files referenced in findings)
- Re-run Step 4 (Review) on the updated diff
- Repeat at most 2 additional iterations; if HIGH issues persist, stop and report remaining issues to the user

---

## Execution Rules

- **Context isolation**: Each agent receives only the context described in its step
- **plan.md as source of truth**: Created in Step 1, consumed read-only in Steps 2–4
- **Boundary enforcement**: The dev agent never writes to `TechDemoTests/`; the test agent never modifies `TechDemo/`
- **Sequential only**: Steps execute 1 → 2 → 3 → 4 → 5(conditional), never in parallel

After all steps complete, report a summary of what was implemented, tested, and any remaining issues to the user.
