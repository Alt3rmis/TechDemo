Orchestrate a full MVVM feature lifecycle for: $ARGUMENTS

Execute the following pipeline sequentially. Each step must complete before the next begins.

---

## Step 0 — Setup Workspace

Create a dedicated working directory for this feature requirement.

Instructions:
- Derive a short slug from the feature description (e.g., "city-switching" from "Add city switching feature")
- Create directory `.specs/<slug>/` in the project root
- This folder holds all artifacts for this requirement: PRD, implementation plan, and review reports
- If `.specs/` does not exist, create it
- Add `.specs/` to `.gitignore` if it is not already listed

Output:
- Workspace ready at `.specs/<slug>/`

---

## Step 1 — PRD (Product Requirements Document)

Use the `ios-planner` agent to analyze the codebase and produce a PRD.

Prompt the agent with:
- The feature description: $ARGUMENTS
- Analyze the current codebase architecture, existing models, services, ViewModels, and ViewControllers
- Produce a PRD at `.specs/<slug>/prd.md` containing:
  - **Overview**: What the feature does and why
  - **User Stories**: Concrete user-facing behaviors (As a user, I want...)
  - **Acceptance Criteria**: Verifiable conditions for "done"
  - **Architecture Impact**: Which layers change and how (Model/Service/ViewModel/View)
  - **Files to Create/Modify/Delete**: Complete list with paths
  - **Out of Scope**: Explicitly list what this feature does NOT cover
  - **Constraints**: Technical constraints (MVVM, UIKit only, no third-party deps, etc.)

Wait for `prd.md` to be created before proceeding.

---

## Step 2 — Implementation Plan

Use the `ios-planner` agent to derive a structured implementation plan from the PRD.

Prompt the agent with:
- Read `.specs/<slug>/prd.md` as the source of truth
- Produce an implementation plan at `.specs/<slug>/plan.md` containing:
  - **Task Breakdown**: Atomic tasks with description, affected files, and agent assignment (dev/test/review)
  - **Execution Order**: Task sequence with explicit dependencies
  - **Risk Notes**: Edge cases, threading concerns, testability risks
  - **Interface Contracts**: Exact method signatures, property types, and protocol definitions for each new/modified type

Wait for `plan.md` to be created before proceeding.

---

## Step 3 — Implement

Use the `ios-mvvm-dev` agent to implement the feature according to `plan.md`.

Instructions:
- Read `.specs/<slug>/plan.md` and execute all tasks assigned to "dev" in order
- Follow MVVM conventions: Models in `Models/`, ViewModels alongside ViewControllers in feature folders, Services in `Services/`
- All UI must be programmatic UIKit with Auto Layout — no storyboards or XIBs
- Do NOT write any test files

---

## Step 4 — Build & Verify

Use the `ios-build-agent` to build the project and verify the implementation compiles correctly.

Instructions:
- Build the project to confirm all changes from Step 3 compile without errors
- If the build fails, report the errors clearly so they can be fixed before proceeding
- Do NOT run tests at this stage (tests are handled in Step 5)

Wait for a successful build before proceeding to Step 5.

---

## Step 5 — Test

Use the `ios-xctest-generator` agent to generate XCTest unit tests.

Instructions:
- Read `.specs/<slug>/plan.md` and generate tests for all ViewModel and Service code created or modified
- Focus on business logic paths, service layer behavior, edge cases, error states, and state transitions
- Place tests in `TechDemoTests/` following existing naming conventions
- Do NOT modify any files under `TechDemo/` (production code)

---

## Step 6 — Review

Use the `ios-diff-reviewer` agent to review the git diff.

Instructions:
- Review all changes for MVVM violations, bugs, missing test coverage, and code quality issues
- Report findings with severity (HIGH/MEDIUM/LOW), file and line reference, description, and suggested fix
- Save the review report to `.specs/<slug>/review.md`

---

## Step 7 — Fix Loop (conditional)

If Step 6 reports any HIGH severity findings:
- Use the `ios-mvvm-dev` agent to fix each HIGH issue (only modify files referenced in findings)
- Re-run Step 4 (Build & Verify) to confirm fixes compile
- Re-run Step 6 (Review) on the updated diff, append findings to `.specs/<slug>/review.md`
- Repeat at most 2 additional iterations; if HIGH issues persist, stop and report remaining issues to the user

---

## Step 8 — Launch on Simulator

Use the `ios-build-agent` to build and launch the app on an iOS Simulator for the user to inspect.

Instructions:
- Build the project and run it on an available iOS Simulator
- Wait for the app to launch successfully in the simulator
- Report the simulator device name and confirm the app is running so the user can interact with it

---

## Execution Rules

- **Spec-driven**: PRD (`prd.md`) is the source of truth for requirements; plan (`plan.md`) is derived from it
- **Workspace isolation**: Each requirement gets its own `.specs/<slug>/` folder with all artifacts
- **Context isolation**: Each agent receives only the context described in its step
- **Boundary enforcement**: The dev agent never writes to `TechDemoTests/`; the test agent never modifies `TechDemo/`
- **Sequential only**: Steps execute 0 → 1 → 2 → 3 → 4 → 5 → 6 → 7(conditional) → 8, never in parallel
- **Build gates**: Step 4 must pass before Step 5; after fix loop, Step 4 must re-pass before Step 8

After all steps complete, report a summary of what was implemented, tested, and any remaining issues to the user.
