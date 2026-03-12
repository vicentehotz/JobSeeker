---

description: "Task list template for feature implementation"
---

# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Automated tests are OPTIONAL unless explicitly requested in the feature
specification or warranted by risk. Verification tasks are REQUIRED for every user
story.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **API**: `JobSeeker.Api/`
- **Application/Domain**: `JobSeeker.Application/`, `JobSeeker.Domain/`
- **Infrastructure**: `JobSeeker.Infrastructure/`
- **Frontend**: `job-seeker-web/src/app/`
- **Tests**: `job-seeker-web/src/app/**/*.spec.ts` and future `JobSeeker.*.Tests/`

<!-- 
  ============================================================================
  IMPORTANT: The tasks below are SAMPLE TASKS for illustration purposes only.
  
  The /speckit.tasks command MUST replace these with actual tasks based on:
  - User stories from spec.md (with their priorities P1, P2, P3...)
  - Feature requirements from plan.md
  - Entities from data-model.md
  - Endpoints from contracts/
  
  Tasks MUST be organized by user story so each story can be:
  - Implemented independently
  - Tested independently
  - Delivered as an MVP increment
  
  DO NOT keep these sample tasks in the generated tasks.md file.
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create or confirm project structure per implementation plan
- [ ] T002 Confirm required backend and frontend dependencies for the feature
- [ ] T003 [P] Confirm linting, formatting, and local run commands still work

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

Examples of foundational tasks (adjust based on your project):

- [ ] T004 Define shared models or DTOs needed by all stories
- [ ] T005 [P] Implement or update feed/integration parsing in `JobSeeker.Infrastructure/`
- [ ] T006 [P] Setup or extend API routing and dependency injection in `JobSeeker.Api/`
- [ ] T007 Create application services or interfaces used across stories
- [ ] T008 Configure error handling and logging for upstream failures
- [ ] T009 Document any required configuration or app settings

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - [Title] (Priority: P1) 🎯 MVP

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 1 (OPTIONAL - only if tests requested) ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T010 [P] [US1] API contract test for [endpoint] in `JobSeeker.Api.Tests/`
- [ ] T011 [P] [US1] Frontend or integration test for [user journey] in `job-seeker-web/src/app/`

### Verification for User Story 1 (REQUIRED)

- [ ] T012 [US1] Document success, empty, and failure verification steps for the story

### Implementation for User Story 1

- [ ] T013 [P] [US1] Create or update shared model types in `JobSeeker.Domain/`
- [ ] T014 [P] [US1] Implement feature service or parser logic in `JobSeeker.Application/` or `JobSeeker.Infrastructure/`
- [ ] T015 [US1] Implement API endpoint or contract changes in `JobSeeker.Api/`
- [ ] T016 [US1] Consume the API in an Angular service or component in `job-seeker-web/src/app/`
- [ ] T017 [US1] Add loading, empty, and error states to the user-facing UI
- [ ] T018 [US1] Add validation, source attribution, and link-out behavior

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - [Title] (Priority: P2)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 2 (OPTIONAL - only if tests requested) ⚠️

- [ ] T019 [P] [US2] API contract test for [endpoint] in `JobSeeker.Api.Tests/`
- [ ] T020 [P] [US2] Frontend or integration test for [user journey] in `job-seeker-web/src/app/`

### Verification for User Story 2 (REQUIRED)

- [ ] T021 [US2] Document success, empty, and failure verification steps for the story

### Implementation for User Story 2

- [ ] T022 [P] [US2] Create or update shared model types in `JobSeeker.Domain/`
- [ ] T023 [US2] Implement feature service or parser logic in `JobSeeker.Application/` or `JobSeeker.Infrastructure/`
- [ ] T024 [US2] Implement API endpoint or contract changes in `JobSeeker.Api/`
- [ ] T025 [US2] Implement the frontend behavior in `job-seeker-web/src/app/`

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - [Title] (Priority: P3)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 3 (OPTIONAL - only if tests requested) ⚠️

- [ ] T026 [P] [US3] API contract test for [endpoint] in `JobSeeker.Api.Tests/`
- [ ] T027 [P] [US3] Frontend or integration test for [user journey] in `job-seeker-web/src/app/`

### Verification for User Story 3 (REQUIRED)

- [ ] T028 [US3] Document success, empty, and failure verification steps for the story

### Implementation for User Story 3

- [ ] T029 [P] [US3] Create or update shared model types in `JobSeeker.Domain/`
- [ ] T030 [US3] Implement feature service or parser logic in `JobSeeker.Application/` or `JobSeeker.Infrastructure/`
- [ ] T031 [US3] Implement API endpoint or frontend behavior in the appropriate project

**Checkpoint**: All user stories should now be independently functional

---

[Add more user story phases as needed, following the same pattern]

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] TXXX [P] Documentation updates in `README.md` or feature docs
- [ ] TXXX Code cleanup and refactoring
- [ ] TXXX Verify accessibility labels and keyboard reachability for new UI
- [ ] TXXX [P] Additional unit tests (if requested) in project-appropriate test locations
- [ ] TXXX Review configuration and secret handling
- [ ] TXXX Run documented verification steps end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story

- Tests (if included) MUST be written and FAIL before implementation
- Verification steps for success, empty, and failure states MUST be documented
- Models before services
- Services before endpoints and UI wiring
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together (if tests requested):
Task: "Contract test for [endpoint] in tests/contract/test_[name].py"
Task: "Integration test for [user journey] in tests/integration/test_[name].py"

# Launch all models for User Story 1 together:
Task: "Create [Entity1] model in src/models/[entity1].py"
Task: "Create [Entity2] model in src/models/[entity2].py"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
