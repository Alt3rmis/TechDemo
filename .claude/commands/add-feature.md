# /add-feature

Orchestrate a full MVVM feature lifecycle: plan, implement, test, review, and fix — using specialized iOS agents in a strict pipeline.

---

## Inputs

- `$ARGUMENTS`: The feature description provided by the user (e.g., "MultiUserSupport", "Add a user profile screen with avatar upload and email validation")

---

## Step 1: Plan

**Agent:** `ios-planner`

**Prompt:**

```
The user wants to add the following feature to this UIKit MVVM iOS project:

$ARGUMENTS

Analyze the current codebase and produce a structured execution plan. The plan must:
- Break the feature into atomic, agent-assignable tasks
- Identify which files to create and which to modify
- Specify the task order and dependencies
- Assign each task to one of: dev, test, or review
- Output the plan to a file called plan.md in the project root
```

**Context given:** Feature description only. No plan.md contents, no implementation details.

**Gate:** Wait for plan.md to exist before proceeding.

---

## Step 2: Implement

**Agent:** `ios-mvvm-dev`

**Prompt:**

```
Implement the following feature according to the plan in plan.md:

$ARGUMENTS

Read plan.md and execute all tasks assigned to the "dev" agent in order.
Follow MVVM conventions: Models in Models/, ViewModels alongside ViewControllers in feature folders, Services in Services/.
All UI must be programmatic UIKit with Auto Layout (no storyboards or XIBs).
Do NOT write any test files.
```

**Context given:** plan.md contents. No test files. No review output.

**Output:** Modified or new Swift source files under `TechDemo/`.

**Gate:** Proceed after all dev-assigned tasks in plan.md are marked complete.

---

## Step 3: Test

**Agent:** `ios-xctest-generator`

**Prompt:**

```
Read plan.md and generate XCTest unit tests for all ViewModel and Service code that was created or modified during the feature implementation.

Focus on:
- Business logic paths in ViewModels
- Service layer behavior
- Edge cases and error states
- State transitions

Place tests in TechDemoTests/ following existing naming conventions (e.g., HomeViewModelTests).
Do NOT modify any files under TechDemo/ (production code).
```

**Context given:** plan.md contents. Access to modified ViewModel and Service source files for reading only.

**Output:** New or updated test files under `TechDemoTests/`.

**Gate:** Proceed after test files are written.

---

## Step 4: Review

**Agent:** `ios-diff-reviewer`

**Prompt:**

```
Review the git diff for the changes made during this feature implementation.

Run `git diff` to see all unstaged changes, then review for:
- MVVM architectural violations (e.g., business logic in ViewControllers, direct service calls from views)
- Bugs or logic errors
- Missing test coverage for critical paths
- Code quality issues

Report each finding with:
- Severity: HIGH / MEDIUM / LOW
- File and line reference
- Description
- Suggested fix
```

**Context given:** `git diff` output only. No plan.md. No source files directly.

**Output:** Review report with severity-ranked findings.

---

## Step 5: Fix Loop (conditional)

If the review report from Step 4 contains any **HIGH** severity findings:

**Agent:** `ios-mvvm-dev`

**Prompt:**

```
The following HIGH severity issues were found during code review. Fix each one:

<insert HIGH severity findings from review report>

Only modify the specific files referenced in the findings. Do not alter test files or unrelated code.
```

**Context given:** HIGH severity findings from review report. Affected file paths.

After fixes are applied, re-run Step 4 (Review) on the updated diff.

**Loop termination:** The fix loop runs at most 2 additional iterations. If HIGH issues persist after the final iteration, stop and report the remaining issues to the user.

---

## Execution Rules

| Rule | Description |
|---|---|
| Context isolation | Each agent receives only the context described in its step. No agent sees the full project unless its step explicitly requires reading source files. |
| Plan as single source of truth | plan.md is the only artifact shared across agents. It is created in Step 1 and consumed read-only in Steps 2, 3, and 4. |
| Review is diff-only | The reviewer operates exclusively on `git diff` output. It never receives plan.md, source files, or test files directly. |
| Test-production boundary | The test agent may only read files under `TechDemo/` and write files under `TechDemoTests/`. It must never modify production code. |
| Dev-test boundary | The dev agent must never write files under `TechDemoTests/`. It must never modify existing test files. |
| Deterministic ordering | Steps execute sequentially: 1 → 2 → 3 → 4 → 5(conditional). No step runs in parallel with another. |

---

## Example

**Input:**
```
/add-feature MultiUserSupport
```

**Execution trace:**
1. `ios-planner` reads the codebase, creates `plan.md` with tasks for adding a user list, multi-selection, and shared state management
2. `ios-mvvm-dev` reads `plan.md`, creates `UserListViewController`, `UserListViewModel`, updates `UserService`, adds `UserSession` model
3. `ios-xctest-generator` reads the new ViewModel and Service code, writes `UserListViewModelTests` and `UserSessionTests`
4. `ios-diff-reviewer` runs `git diff`, reviews all changes, reports one HIGH issue (missing nil check in `UserListViewModel`)
5. `ios-mvvm-dev` fixes the nil check in `UserListViewModel`; reviewer re-runs diff and reports no HIGH issues
6. Feature complete — report summary to user
