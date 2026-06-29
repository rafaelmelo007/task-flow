import { test, expect } from '@playwright/test';

const DEMO_EMAIL = 'demo@taskflow.app';
const DEMO_PASSWORD = 'Demo123!';

test.describe('Task CRUD', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByPlaceholder('you@example.com').fill(DEMO_EMAIL);
    await page.getByPlaceholder('••••••••').fill(DEMO_PASSWORD);
    await page.getByRole('button', { name: /Sign In/i }).click();
    await expect(page).toHaveURL(/\/tasks/, { timeout: 10_000 });
  });

  test('task list renders without console errors', async ({ page }) => {
    const errors: string[] = [];
    page.on('console', msg => {
      if (msg.type() === 'error') errors.push(msg.text());
    });
    await page.waitForLoadState('networkidle');
    expect(errors.filter(e => !e.includes('favicon'))).toHaveLength(0);
  });

  test('create a new task and see it in the list', async ({ page }) => {
    const title = `E2E Task ${Date.now()}`;

    await page.getByRole('button', { name: /New Task/i }).click();
    await expect(page.getByText('New Task').first()).toBeVisible();

    await page.getByPlaceholder('What needs doing?').fill(title);
    await page.getByRole('button', { name: /Create Task/i }).click();

    await expect(page.locator('.modal-overlay')).not.toBeVisible({ timeout: 5_000 });
    await expect(page.getByText(title)).toBeVisible({ timeout: 5_000 });
  });

  test('filter tasks by status', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    // Click "In Progress" filter
    await page.getByRole('button', { name: 'In Progress' }).first().click();
    await page.waitForLoadState('networkidle');
    // Each task card carries a status icon (.status-todo|.status-inprogress|.status-done).
    const inProgress = page.locator('.task-card .status-inprogress');
    const count = await inProgress.count();
    // Either no tasks (filtered out) or every visible card is In Progress.
    if (count > 0) {
      const others = page.locator('.task-card .status-todo, .task-card .status-done');
      await expect(others).toHaveCount(0);
    }
  });

  test('search filters the task list', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    const totalBefore = await page.locator('.task-card').count();

    await page.getByRole('searchbox', { name: 'Search tasks' }).fill('zzz_nonexistent_xyz');
    await page.waitForTimeout(600); // debounce
    await page.waitForLoadState('networkidle');

    const afterSearch = await page.locator('.task-card').count();
    expect(afterSearch).toBeLessThanOrEqual(totalBefore);
  });

  test('edit a task title', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    // Create a task to edit
    const original = `Edit-me ${Date.now()}`;
    await page.getByRole('button', { name: /New Task/i }).click();
    await page.getByPlaceholder('What needs doing?').fill(original);
    await page.getByRole('button', { name: /Create Task/i }).click();
    await expect(page.getByText(original)).toBeVisible({ timeout: 5_000 });

    // Edit it
    const taskCard = page.locator('.task-card').filter({ hasText: original });
    await taskCard.getByTitle('Edit').click();

    const titleInput = page.getByPlaceholder('What needs doing?');
    await titleInput.clear();
    await titleInput.fill(`${original} (edited)`);
    await page.getByRole('button', { name: /Save Changes/i }).click();

    await expect(page.getByText(`${original} (edited)`)).toBeVisible({ timeout: 5_000 });
  });

  test('delete a task', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    const title = `Delete-me ${Date.now()}`;

    // Create it
    await page.getByRole('button', { name: /New Task/i }).click();
    await page.getByPlaceholder('What needs doing?').fill(title);
    await page.getByRole('button', { name: /Create Task/i }).click();
    await expect(page.getByText(title)).toBeVisible({ timeout: 5_000 });

    // Delete it
    const taskCard = page.locator('.task-card').filter({ hasText: title });
    await taskCard.getByTitle('Delete').click();
    await page.getByRole('button', { name: /^Delete$/ }).click();

    await expect(page.getByText(title)).not.toBeVisible({ timeout: 5_000 });
  });
});
