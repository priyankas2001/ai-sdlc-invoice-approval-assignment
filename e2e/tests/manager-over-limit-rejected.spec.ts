import { expect, test } from '@playwright/test';

test('Manager cannot approve a 50,001 invoice and the invoice remains pending', async ({ page }) => {
  const customerName = `Atlas North ${Date.now()}`;
  const amount = '50001';

  await page.goto('/');

  await page.getByLabel('Username').fill('demo.manager');
  await page.getByLabel('Password').fill('ChangeMe123!');
  await page.getByRole('button', { name: 'Sign in' }).click();

  await expect(page.getByText('demo.manager (Manager)')).toBeVisible();

  await page.getByLabel('Customer name').fill(customerName);
  await page.getByLabel('Amount').fill(amount);
  await page.getByRole('button', { name: 'Create invoice' }).click();

  const createdInvoiceRow = page.locator('tbody tr').filter({ hasText: customerName });
  await expect(createdInvoiceRow).toContainText(customerName);

  await createdInvoiceRow.getByRole('button', { name: /Submit invoice .* for approval/ }).click();

  await page.getByRole('tab', { name: 'Approvals' }).click();

  const pendingItem = page.locator('#pending-list li').filter({ hasText: customerName });
  await expect(pendingItem).toContainText(customerName);

  await pendingItem.getByRole('button', { name: /Approve invoice/ }).click();

  const decisionError = page.locator('#decision-error');
  await expect(decisionError).toBeVisible();
  await expect(decisionError).toContainText('50,000');

  const invoice = await page.evaluate(async ({ customer }) => {
    const session = JSON.parse(sessionStorage.getItem('session') ?? 'null');
    const response = await fetch('/api/invoices', {
      headers: {
        Authorization: `Bearer ${session.token}`,
      },
    });

    const invoices = await response.json();
    return invoices.find((item: { customerName: string }) => item.customerName === customer) ?? null;
  }, { customer: customerName });

  expect(invoice).not.toBeNull();
  expect(invoice.status).toBe('PendingApproval');
  expect(invoice.decidedBy).toBeNull();
  expect(invoice.decidedAt).toBeNull();
  expect(invoice.firstApprover).toBeNull();
  expect(invoice.secondApprover).toBeNull();
  expect(invoice.decisionNote).toBeNull();
});
