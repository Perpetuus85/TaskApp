import { expect, test } from '@playwright/test';

const authStorageKey = 'taskapp_auth';

const mockBoardApi = async (page: Parameters<typeof test>[0]['page']) => {
  await page.route('https://localhost:7051/**', async (route) => {
    const request = route.request();
    const method = request.method();
    const url = new URL(request.url());
    const path = url.pathname.toLowerCase();

    if (method === 'GET' && path === '/board/getall') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([{ id: 1, name: 'Engineering Board' }]),
      });
      return;
    }

    if (method === 'GET' && path === '/board/getboardwithtasksbyid/1') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 1,
          name: 'Engineering Board',
          boardTasks: [
            { id: 'task-1', summary: 'Ship route tests', dueAt: '2026-05-20T00:00:00Z', status: 'ToDo' },
            { id: 'task-2', summary: 'Wire CI pipeline', dueAt: '2026-05-22T00:00:00Z', status: 'InProgress' },
            { id: 'task-3', summary: 'Release v1', dueAt: '2026-05-25T00:00:00Z', status: 'Done' },
          ],
        }),
      });
      return;
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({}),
    });
  });
};

test.describe('App route wiring', () => {
  test('redirects anonymous users from root to sign-in', async ({ page }) => {
    await page.goto('/');

    await expect(page).toHaveURL(/\/sign-in$/);
    await expect(page.getByRole('heading', { name: 'Sign in' })).toBeVisible();
  });

  test('supports sign-in and sign-up route navigation links', async ({ page }) => {
    await page.goto('/sign-in');
    await page.getByRole('link', { name: 'Sign up' }).click();

    await expect(page).toHaveURL(/\/sign-up$/);
    await expect(page.getByRole('heading', { name: 'Sign Up' })).toBeVisible();

    await page.getByRole('link', { name: 'Sign in' }).first().click();
    await expect(page).toHaveURL(/\/sign-in$/);
    await expect(page.getByRole('heading', { name: 'Sign in' })).toBeVisible();
  });

  test('redirects unknown route through wildcard guard', async ({ page }) => {
    await page.goto('/does-not-exist');
    await expect(page).toHaveURL(/\/sign-in$/);
  });

  test('renders nested board list route for authenticated users', async ({ page }) => {
    await page.addInitScript((key) => {
      window.localStorage.setItem(
        key,
        JSON.stringify({
          accessToken: 'fake-token',
          refreshToken: null,
          user: null,
          email: 'e2e@example.com',
        }),
      );
    }, authStorageKey);
    await mockBoardApi(page);

    await page.goto('/');

    await expect(page).toHaveURL(/\/$/);
    await expect(page.getByRole('heading', { name: 'Your Boards' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Boards' })).toBeVisible();
    await expect(page.getByText('Engineering Board')).toBeVisible();
  });

  test('renders board details route with grouped task columns', async ({ page }) => {
    await page.addInitScript((key) => {
      window.localStorage.setItem(
        key,
        JSON.stringify({
          accessToken: 'fake-token',
          refreshToken: null,
          user: null,
          email: 'e2e@example.com',
        }),
      );
    }, authStorageKey);
    await mockBoardApi(page);

    await page.goto('/boards/1');

    await expect(page).toHaveURL(/\/boards\/1$/);
    await expect(page.getByRole('heading', { name: 'Engineering Board' })).toBeVisible();
    await expect(page.getByText('To-Do')).toBeVisible();
    await expect(page.getByText('In Progress')).toBeVisible();
    await expect(page.getByText('Done')).toBeVisible();
    await expect(page.getByText('Ship route tests')).toBeVisible();
    await expect(page.getByText('Wire CI pipeline')).toBeVisible();
    await expect(page.getByText('Release v1')).toBeVisible();
  });
});
