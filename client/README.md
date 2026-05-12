# TaskApp Client

Frontend client for a task/board management application built with React, TypeScript, Vite, Material UI, and Playwright.

## Overview

This project provides:

- Authentication flows (`sign-in`, `sign-up`) with persisted auth state
- Protected application routes for boards and board tasks
- Board management (list, create, update)
- Task management inside a board (create, update, delete)
- Token refresh handling for authenticated API calls

The app is structured around React context providers for auth and board data, and uses React Router for nested route composition.

## Tech Stack

- React 19
- TypeScript
- Vite
- Material UI (`@mui/material`, `@mui/icons-material`)
- React Router 7
- Axios
- Day.js
- Playwright (E2E)

## Project Structure

```text
client/
  src/
    api/
      axios.tsx
    components/
      boards/
      home/
      sign-in/
      sign-up/
      tasks/
    context/
      AuthProvider.tsx
      BoardProvider.tsx
    routes/
      AppRoutes.tsx
    main.tsx
    App.tsx
  tests/
    example.spec.ts
  playwright.config.ts
```

## Routing

Defined in `src/routes/AppRoutes.tsx`.

- `/`
  - Protected route
  - Renders `Home` when authenticated
  - Redirects to `/sign-in` when not authenticated
- Nested under `/`:
  - Index route: `BoardList`
  - `/boards`: `BoardList`
  - `/boards/:boardId`: `Board`
- Public routes:
  - `/sign-in`
  - `/sign-up`
- Wildcard route:
  - `*` redirects to `/`

## State Management

### `AuthProvider`

`src/context/AuthProvider.tsx` manages:

- `auth` state (`accessToken`, `refreshToken`, `user`, `email`)
- `isAuthenticated` and `isLoading`
- `login`, `register`, `logout`, `refreshAccessToken`

Auth persistence:

- Stored in `localStorage` under key `taskapp_auth`
- Restored at startup

Auth refresh/interceptors:

- Adds `Authorization: Bearer <token>` to private requests
- On `401`, attempts one refresh/retry cycle via `/auth/refresh`

### `BoardProvider`

`src/context/BoardProvider.tsx` manages:

- In-memory `boards` list
- Board APIs:
  - `GetAllBoards`
  - `CreateBoard`
  - `UpdateBoard`
  - `GetBoardWithTasks`
- Task APIs:
  - `CreateBoardTask`
  - `UpdateBoardTask`
  - `DeleteBoardTask`

## API Configuration

Configured in `src/api/axios.tsx`:

- Base URL: `https://localhost:7051`
- `api`: general auth/client calls
- `axiosPrivate`: JSON headers + credentials for authenticated calls

## UI Behavior Summary

- `SignIn`:
  - Validates email/password client-side
  - Calls `login`
  - Redirects authenticated users to `/`
- `SignUp`:
  - Validates first/last name, email, password
  - Calls `register`
  - Shows post-registration check-email state
- `Home`:
  - Loads boards on first mount
  - Provides sidebar navigation
  - Exposes account popover with logout
- `BoardList`:
  - Shows boards from outlet context
  - Supports create-board dialog
- `Board`:
  - Loads board + tasks by URL param
  - Organizes tasks by status (`ToDo`, `InProgress`, `Done`)
  - Supports task create/update/delete dialogs
  - Supports board rename dialog

## Getting Started

### Prerequisites

- Node.js 20+ recommended
- npm
- Backend API available at `https://localhost:7051`

### Install

```bash
npm install
```

### Run in Development

```bash
npm run dev
```

Default Vite URL:

- `http://localhost:5173`

### Build

```bash
npm run build
```

### Preview Production Build

```bash
npm run preview
```

### Lint

```bash
npm run lint
```

## Testing (Playwright)

E2E tests live in `tests/`.

- Current route/component wiring tests are in `tests/example.spec.ts`
- Playwright config is in `playwright.config.ts`
- `baseURL` is configured as `http://localhost:5173`

Run tests:

```bash
npx playwright test
```

If PowerShell execution policy blocks `npm`/`npx` scripts, run through `cmd`:

```bash
cmd /c npx playwright test
```

## Notes and Assumptions

- This client assumes API contract compatibility with the endpoints used by `AuthProvider` and `BoardProvider`.
- Route protection is access-token based (`Boolean(auth.accessToken)`).
- Refresh behavior depends on a valid stored `refreshToken` and successful `/auth/refresh` response.

## Suggested Next Improvements

1. Add environment-based API configuration
   - Move `BASE_URL` in `src/api/axios.tsx` to Vite environment variables (`.env`, `.env.production`) so local/dev/staging/prod can be switched without code changes.
2. Add runtime nginx config for SPA routing
   - Include a custom nginx config in production image to route unknown paths to `index.html` for client-side routing reliability.
3. Add stricter error and loading states
   - Standardize API error boundaries/messages and loading skeletons for board/task screens to improve UX under slow or failing network conditions.
4. Improve auth/session security posture
   - Review localStorage token persistence strategy, consider shorter token lifetimes and stricter refresh handling, and document security assumptions.
5. Expand test coverage
   - Add component/unit tests for provider logic and form validation, plus additional Playwright flows (login/register success/failure, board/task CRUD, logout).
6. Add CI pipeline
   - Run `lint`, `build`, and Playwright smoke tests in CI on every PR, and publish Playwright HTML artifacts on failure.
7. Add Docker Compose for local full-stack development
   - Provide a `docker-compose.yml` that brings up client + API + dependencies with shared network and environment defaults.
8. Add observability basics
   - Add structured client logging, error reporting integration, and request tracing IDs to simplify diagnosing production issues.

## My Thought Process
- I wanted to create a simple frontend that had auth forms and a simple layout that was easy to understand.
- The refresh token callback in the AuthProvider file seems to be buggy but after trying many fixes, I was unable to come up with a solution and wanted to focus on the app with my time.
- The isLoading from AuthProvider can cause page flashes, but I couldn't fix that after spending too much time on that as well.
- I wanted to handle board crud in modals and make sure pages update when saving.
- Same idea with board task crud.
- I used MUI templates for sign in and sign up and then wired them to auth provider.
- Test coverage is rather limited, Codex had trouble running its tests, despite myself running them easily.
- I'm not a flashy designer, so the UI is rather bland.  It would have been cool to add drag and drop, filtering, a search bar, notifications for past due tasks, but all that is too much scope for a simple MVP.