import { expect, test } from '@playwright/test';

test('High-value invoice dual approval is completed by two distinct Directors', async ({ page }) => {
  const customerName = `Atlas High Value ${Date.now()}`;
  const amount = '500001';
  const secondDirectorUsername = 'demo.director2';

  await page.goto('/');

  await page.getByLabel('Username').fill('demo.director');
  await page.getByLabel('Password').fill('ChangeMe123!');
  await page.getByRole('button', { name: 'Sign in' }).click();

  await expect(page.getByText('demo.director (Director)')).toBeVisible();

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
  expect(invoice.firstApprover).toBe('demo.director');
  expect(invoice.secondApprover).toBeNull();
  expect(invoice.decidedBy).toBe('demo.director');
  expect(invoice.decisionNote).toBe('Awaiting second Director approval.');

  await page.getByRole('button', { name: 'Sign out' }).click();

  await page.getByLabel('Username').fill(secondDirectorUsername);
  await page.getByLabel('Password').fill('ChangeMe123!');
  await page.getByRole('button', { name: 'Sign in' }).click();

  await expect(page.getByText(`${secondDirectorUsername} (Director)`)).toBeVisible();

  await page.getByRole('tab', { name: 'Approvals' }).click();

  const secondPendingItem = page.locator('#pending-list li').filter({ hasText: customerName });
  await expect(secondPendingItem).toContainText(customerName);

  await secondPendingItem.getByRole('button', { name: /Approve invoice/ }).click();

  const approvedInvoice = await page.evaluate(async ({ customer }) => {
    const session = JSON.parse(sessionStorage.getItem('session') ?? 'null');
    const response = await fetch('/api/invoices', {
      headers: {
        Authorization: `Bearer ${session.token}`,
      },
    });

    const invoices = await response.json();
    return invoices.find((item: { customerName: string }) => item.customerName === customer) ?? null;
  }, { customer: customerName });

  expect(approvedInvoice).not.toBeNull();
  expect(approvedInvoice.status).toBe('Approved');
  expect(approvedInvoice.firstApprover).toBe('demo.director');
  expect(approvedInvoice.secondApprover).toBe(secondDirectorUsername);
  expect(approvedInvoice.decidedBy).toBe(secondDirectorUsername);
  expect(approvedInvoice.decisionNote).toBe('Approved by second Director.');
});
