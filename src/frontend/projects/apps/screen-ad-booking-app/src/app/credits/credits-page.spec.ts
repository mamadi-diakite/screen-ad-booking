import { CreditPack, Wallet } from '@screen-ad-booking/shared/core';
import { AppHarness } from '../../testing/app-harness';

const packs: CreditPack[] = [
  { code: 'starter', name: 'Starter', credits: 500 },
  { code: 'standard', name: 'Standard', credits: 2000 },
  { code: 'large', name: 'Large', credits: 5000 },
];

const wallet: Wallet = {
  available: 1700,
  held: 300,
  movements: [
    {
      id: 'm-4',
      type: 'Release',
      credits: 100,
      label: 'Cancelled booking',
      createdAtUtc: '2026-11-05T10:00:00Z',
    },
    {
      id: 'm-3',
      type: 'Charge',
      credits: 100,
      label: 'Played slot',
      createdAtUtc: '2026-11-04T10:00:00Z',
    },
    {
      id: 'm-2',
      type: 'Hold',
      credits: 500,
      label: 'Booking in Antwerp',
      createdAtUtc: '2026-11-03T10:00:00Z',
    },
    {
      id: 'm-1',
      type: 'TopUp',
      credits: 2000,
      label: 'Standard pack top-up',
      createdAtUtc: '2026-11-01T10:00:00Z',
    },
  ],
};

describe('Credits page', () => {
  let app: AppHarness;

  beforeEach(() => (app = new AppHarness()));

  afterEach(() => app.http.verify());

  async function openPage(current: Wallet = wallet): Promise<void> {
    app.signIn();
    await app.open('/credits');
    expect(app.text('[role="status"]')).toBe('Loading your credits…');
    app.http.expectOne('/api/credits/packs').flush(packs);
    app.http.expectOne('/api/credits').flush(current);
    await app.settle();
  }

  it('is only for advertisers', async () => {
    await app.open('/credits');
    expect(app.router.url).toBe('/login?returnUrl=%2Fcredits');
  });

  it('shows the balance and every movement', async () => {
    await openPage();

    expect(app.text('[data-testid="available"]')).toBe('1,700');
    expect(app.text('[data-testid="held"]')).toBe('300');
    const rows = [...app.element.querySelectorAll('tbody tr')].map((row) =>
      [...row.querySelectorAll('td')].map((cell) => cell.textContent!.trim()),
    );
    expect(rows).toEqual([
      ['Nov 5, 2026', 'Release', 'Cancelled booking', '+100'],
      ['Nov 4, 2026', 'Charge', 'Played slot', '−100'],
      ['Nov 3, 2026', 'Hold', 'Booking in Antwerp', '−500'],
      ['Nov 1, 2026', 'Top-up', 'Standard pack top-up', '+2,000'],
    ]);
  });

  it('asks for a pack, then adds its credits', async () => {
    await openPage({ available: 0, held: 0, movements: [] });
    expect(app.text('[aria-labelledby="movements-title"] p')).toBe('No movements yet.');
    expect(app.texts('fieldset label span.block')).toEqual(['Starter', 'Standard', 'Large']);
    expect(app.texts('fieldset label span.text-sm')).toEqual([
      '500 credits',
      '2,000 credits',
      '5,000 credits',
    ]);

    await app.submit();
    expect(app.text('sab-field-errors')).toBe('Choose a credit pack.');

    await app.click('#pack-large');
    await app.submit();
    const request = app.http.expectOne({ method: 'POST', url: '/api/credits/top-up' });
    expect(request.request.body).toEqual({ packCode: 'large' });
    request.flush({
      available: 5000,
      held: 0,
      movements: [
        {
          id: 'm-9',
          type: 'TopUp',
          credits: 5000,
          label: 'Large pack top-up',
          createdAtUtc: '2026-11-06T10:00:00Z',
        },
      ],
    } satisfies Wallet);
    await app.settle();

    expect(app.text('form [role="status"]')).toBe('5,000 credits were added.');
    expect(app.text('[data-testid="available"]')).toBe('5,000');
    expect(app.element.querySelector<HTMLInputElement>('#pack-large')!.checked).toBe(false);
  });

  it('shows the server message when the top-up fails', async () => {
    await openPage();

    await app.click('#pack-starter');
    await app.submit();
    app.http
      .expectOne('/api/credits/top-up')
      .flush(
        { title: 'Only advertisers have a credits wallet.' },
        { status: 403, statusText: 'Forbidden' },
      );
    await app.settle();

    expect(app.text('form [role="alert"]')).toBe('Only advertisers have a credits wallet.');
  });

  it('waits for the packs, and explains when they cannot be loaded', async () => {
    app.signIn();
    await app.open('/credits');
    app.http.expectOne('/api/credits').flush(wallet);
    await app.settle();
    expect(app.text('form [role="status"]')).toBe('Loading the packs…');

    app.http.expectOne('/api/credits/packs').flush(null, { status: 500, statusText: 'Error' });
    await app.settle();
    expect(app.text('form [role="alert"]')).toBe('The credit packs could not be loaded.');
  });

  it('explains when the credits cannot be loaded', async () => {
    app.signIn();
    await app.open('/credits');
    app.http.expectOne('/api/credits/packs').flush(packs);
    app.http.expectOne('/api/credits').flush(null, { status: 500, statusText: 'Error' });
    await app.settle();

    expect(app.text('[role="alert"]')).toBe(
      'Your credits could not be loaded. Please try again later.',
    );
  });
});

function collapse(text: string): string {
  return text.replace(/\s+/g, ' ');
}
