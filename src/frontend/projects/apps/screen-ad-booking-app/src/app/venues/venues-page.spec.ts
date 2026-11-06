import { HttpRequest } from '@angular/common/http';
import { NetworkCountry, PagedResponse, Venue } from '@screen-ad-booking/shared/core';
import { AppHarness } from '../../testing/app-harness';

const locations: NetworkCountry[] = [
  {
    code: 'BE',
    name: 'Belgium',
    regions: [
      {
        id: 'r-fl',
        name: 'Flanders',
        cities: [{ id: 'c-ant', name: 'Antwerp', latitude: 51.2194, longitude: 4.4025 }],
      },
    ],
  },
  {
    code: 'FR',
    name: 'France',
    regions: [
      {
        id: 'r-idf',
        name: 'Ile-de-France',
        cities: [
          { id: 'c-par', name: 'Paris', latitude: 48.8566, longitude: 2.3522 },
          { id: 'c-ver', name: 'Versailles', latitude: 48.8049, longitude: 2.1204 },
        ],
      },
      {
        id: 'r-ara',
        name: 'Auvergne-Rhone-Alpes',
        cities: [{ id: 'c-lyo', name: 'Lyon', latitude: 45.764, longitude: 4.8357 }],
      },
    ],
  },
];

const mall: Venue = {
  id: 'v-1',
  name: 'Harbour Lights Shopping Centre',
  type: 'ShoppingMall',
  footfall: 'High',
  locationType: 'ShoppingCentre',
  address: '25 Kaailaan',
  latitude: 51.2294,
  longitude: 4.3925,
  screenCount: 7,
  averageAudience: 1680,
  countryCode: 'BE',
  regionName: 'Flanders',
  cityId: 'c-ant',
  cityName: 'Antwerp',
};

const kiosk: Venue = {
  ...mall,
  id: 'v-2',
  name: 'Station Kiosk',
  type: 'Kiosk',
  footfall: 'Low',
  locationType: 'TransportHub',
  screenCount: 1,
  averageAudience: 75,
};

function page(items: Venue[], current = 1, totalCount = items.length): PagedResponse<Venue> {
  return { items, page: current, pageSize: 20, totalCount };
}

describe('Venues page', () => {
  let app: AppHarness;

  beforeEach(() => (app = new AppHarness()));

  afterEach(() => app.http.verify());

  const isSearch = (request: HttpRequest<unknown>) => request.url === '/api/venues';

  function options(id: string): string[] {
    return [...app.element.querySelectorAll(`#${id} option`)].map((o) => o.textContent!.trim());
  }

  function value(id: string): string {
    return app.element.querySelector<HTMLSelectElement>(`#${id}`)!.value;
  }

  async function openPage(): Promise<void> {
    app.signIn();
    await app.open('/venues');
    expect(app.text('[role="status"]')).toBe('Loading the venues…');
    app.http.expectOne('/api/network/locations').flush(locations);
    const search = app.http.expectOne(isSearch);
    expect(search.request.params.toString()).toBe('page=1&pageSize=20');
    search.flush(page([mall, kiosk]));
    await app.settle();
  }

  it('is only for signed-in users', async () => {
    await app.open('/venues');

    expect(app.router.url).toBe('/login?returnUrl=%2Fvenues');
  });

  it('lists the venues with their kind, place and audience', async () => {
    await openPage();

    expect(app.text('#results-title')).toBe('2 venues');
    expect(app.texts('[data-testid="name"]')).toEqual([
      'Harbour Lights Shopping Centre',
      'Station Kiosk',
    ]);
    expect(app.texts('[data-testid="kind"]').map(collapse)).toEqual([
      'Shopping mall · Shopping centre · High footfall',
      'Kiosk · Transport hub · Low footfall',
    ]);
    expect(collapse(app.text('[data-testid="place"]')!)).toBe(
      '25 Kaailaan, Antwerp · Flanders, BE',
    );
    expect(app.texts('[data-testid="audience"]').map(collapse)).toEqual([
      '7 screens · average audience 1,680',
      '1 screen · average audience 75',
    ]);
    expect(app.element.querySelector('nav[aria-label="Pages"]')).toBeNull();
  });

  it('narrows the regions and cities to the chosen place, then searches with every filter', async () => {
    await openPage();
    expect(options('country')).toEqual(['All countries', 'Belgium', 'France']);
    expect(options('regionId')).toEqual(['All regions']);

    await app.select('country', 'FR');
    expect(options('regionId')).toEqual(['All regions', 'Ile-de-France', 'Auvergne-Rhone-Alpes']);
    await app.select('regionId', 'r-idf');
    expect(options('cityId')).toEqual(['All cities', 'Paris', 'Versailles']);
    await app.select('cityId', 'c-par');
    await app.select('type', 'Bakery');
    await app.select('footfall', 'High');
    await app.select('locationType', 'CityCentre');
    await app.submit();

    const search = app.http.expectOne(isSearch);
    expect(search.request.params.toString()).toBe(
      'page=1&pageSize=20&country=FR&regionId=r-idf&cityId=c-par&type=Bakery&footfall=High&locationType=CityCentre',
    );
    search.flush(page([mall], 1, 1));
    await app.settle();
    expect(app.text('#results-title')).toBe('1 venue');

    await app.select('country', 'BE');
    expect(value('regionId')).toBe('');
    expect(value('cityId')).toBe('');
    expect(options('cityId')).toEqual(['All cities']);
  });

  it('clears the filters', async () => {
    await openPage();

    await app.select('type', 'Gym');
    await app.click('form button[type="button"]');
    const search = app.http.expectOne(isSearch);
    expect(search.request.params.toString()).toBe('page=1&pageSize=20');
    search.flush(page([]));
    await app.settle();

    expect(value('type')).toBe('');
    expect(app.text('[data-testid="venues"] li')).toBe('No venues match these filters.');
  });

  it('moves between pages', async () => {
    app.signIn();
    await app.open('/venues');
    app.http.expectOne('/api/network/locations').flush(locations);
    app.http.expectOne(isSearch).flush(page([mall], 1, 45));
    await app.settle();

    const [previous, next] = app.element.querySelectorAll<HTMLButtonElement>(
      'nav[aria-label="Pages"] button',
    );
    expect(app.text('nav[aria-label="Pages"] span')).toBe('Page 1 of 3');
    expect(previous.disabled).toBe(true);

    next.click();
    await app.settle();
    const second = app.http.expectOne(isSearch);
    expect(second.request.params.get('page')).toBe('2');
    second.flush(page([mall], 3, 45));
    await app.settle();

    const buttons = app.element.querySelectorAll<HTMLButtonElement>(
      'nav[aria-label="Pages"] button',
    );
    expect(buttons[1].disabled).toBe(true);
    buttons[0].click();
    await app.settle();
    const back = app.http.expectOne(isSearch);
    expect(back.request.params.get('page')).toBe('2');
    back.flush(page([mall], 2, 45));
    await app.settle();
  });

  it('explains when the locations or the venues cannot be loaded', async () => {
    app.signIn();
    await app.open('/venues');
    app.http.expectOne('/api/network/locations').flush(null, { status: 500, statusText: 'Error' });
    app.http
      .expectOne(isSearch)
      .flush({ errors: { type: ['Choose a venue type.'] } }, { status: 400, statusText: 'Bad' });
    await app.settle();

    expect(app.texts('[role="alert"]').map(collapse)).toEqual([
      'The countries, regions and cities could not be loaded.',
      'The venues could not be loaded. Check the filters or try again later.',
    ]);
    expect(options('country')).toEqual(['All countries']);
  });
});

function collapse(text: string): string {
  return text.replace(/\s+/g, ' ');
}
