# DiscussionBoard test infrastructure

## Design rules

- Prefer behavior over implementation.
- Inspect repository/source structure only when repository/source structure is itself a requirement.
- Put reusable discovery, setup, and assertion logic in `Infrastructure`; do not duplicate it in lesson tests.
- Lesson tests may depend on `Infrastructure`; lesson tests must not depend on other lesson tests.
- Failure messages should state the student-facing requirement that failed.
- Do not test irrelevant implementation choices.
- CourseUpdater installs tests before students implement the lesson, so lesson tests must continue to compile when lesson types/pages/services do not yet exist.

## Lesson 1 infrastructure

- `ProjectFixture`: repository-root discovery and file inspection.
- `GitRepositoryInspector`: asks Git whether representative files are ignored/tracked.
- `ApplicationTypeLocator`: discovers student-created types without compile-time references.
- `ReflectionAssert`: reusable structural assertions with student-readable messages.
- `PageModelLocator`: semantic Razor PageModel discovery without generated class-name assumptions.
- `DatabaseFixture`: isolated in-memory SQLite database.
- `ApplicationFixture`: shared ASP.NET Core integration host with isolated SQLite and minimal test authentication.
- `TestAuthenticationHandler`: one fixed authenticated identity for Lesson 1; Lesson 2 will generalize authentication testing.
- `TestDataFactory`: deterministic test-data constants; strongly typed builders are deferred until the common model exists.
