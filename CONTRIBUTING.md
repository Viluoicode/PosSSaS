# Contributing to PosSSaS

Thanks for taking the time to contribute! This document describes the branching
model, commit conventions, and local workflow used in this repository.

## 🌿 Branching model (Git Flow)

| Branch | Purpose |
| --- | --- |
| `main` | Production-ready. Every commit is a tagged, releasable state. |
| `develop` | Integration branch. Features merge here first. |
| `feature/*` | A single feature or vertical slice. Branched from `develop`. |
| `release/*` | Release stabilisation. Branched from `develop`, merged to `main` + `develop`. |
| `hotfix/*` | Urgent production fix. Branched from `main`. |

**Flow:**

```
feature/xyz ──▶ develop ──▶ release/vX.Y.Z ──▶ main (tagged vX.Y.Z)
                                   └────────────▶ develop (back-merge)
```

- Feature branches are merged into `develop` with `--no-ff` so the history shows
  a clear merge bubble per feature.
- `main` is only updated through a `release/*` or `hotfix/*` branch, and every
  merge to `main` is tagged.

## ✍️ Commit messages — Conventional Commits

```
<type>(<scope>): <short summary>
```

**Types:** `feat`, `fix`, `refactor`, `docs`, `test`, `chore`, `build`, `ci`, `perf`.

**Examples:**

```
feat(orders): create order with automatic stock deduction by recipe
fix(auth): reject login when password hash mismatch
refactor(infrastructure): extract query-filter application into helper
docs: add API endpoint table to README
```

## 🧪 Local workflow

```bash
# Start a feature
git switch develop
git switch -c feature/my-feature

# ... make changes, commit using Conventional Commits ...

# Build & test before merging
dotnet build
dotnet test

# Merge back into develop
git switch develop
git merge --no-ff feature/my-feature
git branch -d feature/my-feature
```

## ✅ Definition of done

- Code builds (`dotnet build`) with no warnings introduced.
- Tests pass (`dotnet test`).
- No secrets committed (`appsettings.*.local.json`, real `Jwt:Key`, connection strings).
- New endpoints documented in the README endpoint table.
