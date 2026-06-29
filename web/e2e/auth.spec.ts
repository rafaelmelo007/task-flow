import { test, expect } from '@playwright/test';

const DEMO_EMAIL = 'demo@taskflow.app';
const DEMO_PASSWORD = 'Demo123!';

test.describe('Authentication', () => {
  test.beforeEach(async ({ page }) => {
    // Clear auth state before each test
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());
  });

  test('unauthenticated user is redirected to /login', async ({ page }) => {
    await page.goto('/tasks');
    await expect(page).toHaveURL(/\/login/);
  });

  test('login with demo credentials and reach task list', async ({ page }) => {
    await page.goto('/login');

    await page.getByPlaceholder('you@example.com').fill(DEMO_EMAIL);
    await page.getByPlaceholder('••••••••').fill(DEMO_PASSWORD);
    await page.getByRole('button', { name: /Sign In/i }).click();

    await expect(page).toHaveURL(/\/tasks/, { timeout: 10_000 });
    await expect(page.getByText('My Tasks')).toBeVisible();
  });

  test('login with wrong credentials shows error', async ({ page }) => {
    await page.goto('/login');
    await page.getByPlaceholder('you@example.com').fill('bad@user.com');
    await page.getByPlaceholder('••••••••').fill('wrongpassword');
    await page.getByRole('button', { name: /Sign In/i }).click();

    await expect(page.locator('.form-error')).toBeVisible({ timeout: 5_000 });
  });

  test('register with new account redirects to tasks', async ({ page }) => {
    const unique = `test-${Date.now()}@example.com`;

    await page.goto('/register');
    await page.getByPlaceholder('you@example.com').fill(unique);
    await page.getByPlaceholder(/Min 8 chars/i).fill('TestPass1!');
    await page.getByRole('button', { name: /Create Account/i }).click();

    await expect(page).toHaveURL(/\/tasks/, { timeout: 10_000 });
  });

  test('logout returns user to /login', async ({ page }) => {
    // Login first
    await page.goto('/login');
    await page.getByPlaceholder('you@example.com').fill(DEMO_EMAIL);
    await page.getByPlaceholder('••••••••').fill(DEMO_PASSWORD);
    await page.getByRole('button', { name: /Sign In/i }).click();
    await expect(page).toHaveURL(/\/tasks/, { timeout: 10_000 });

    // Logout
    await page.getByRole('button', { name: /Sign out/i }).click();
    await expect(page).toHaveURL(/\/login/);
  });

  test('no console errors on login page', async ({ page }) => {
    const errors: string[] = [];
    page.on('console', msg => {
      if (msg.type() === 'error') errors.push(msg.text());
    });
    await page.goto('/login');
    await page.waitForLoadState('networkidle');
    expect(errors.filter(e => !e.includes('favicon'))).toHaveLength(0);
  });
});
