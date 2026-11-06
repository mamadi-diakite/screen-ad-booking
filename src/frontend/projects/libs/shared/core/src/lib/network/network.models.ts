export interface NetworkCity {
  readonly id: string;
  readonly name: string;
  readonly latitude: number;
  readonly longitude: number;
}

export interface NetworkRegion {
  readonly id: string;
  readonly name: string;
  readonly cities: readonly NetworkCity[];
}

export interface NetworkCountry {
  /** ISO 3166-1 alpha-2, e.g. `FR`. */
  readonly code: string;
  readonly name: string;
  readonly regions: readonly NetworkRegion[];
}

export interface Venue {
  readonly id: string;
  readonly name: string;
  readonly type: string;
  readonly footfall: string;
  readonly locationType: string;
  readonly address: string;
  readonly latitude: number;
  readonly longitude: number;
  readonly screenCount: number;
  readonly averageAudience: number;
  readonly countryCode: string;
  readonly regionName: string;
  readonly cityId: string;
  readonly cityName: string;
}

/** Empty values mean "any". */
export interface VenueFilters {
  readonly country: string;
  readonly regionId: string;
  readonly cityId: string;
  readonly type: string;
  readonly footfall: string;
  readonly locationType: string;
}

export interface PagedResponse<T> {
  readonly items: readonly T[];
  /** 1-based page number. */
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
}

export function pageCount(response: PagedResponse<unknown>): number {
  return Math.max(1, Math.ceil(response.totalCount / response.pageSize));
}

export interface Option {
  readonly value: string;
  readonly label: string;
}

export const VENUE_TYPES: readonly Option[] = [
  { value: 'Supermarket', label: 'Supermarket' },
  { value: 'Pharmacy', label: 'Pharmacy' },
  { value: 'Bakery', label: 'Bakery' },
  { value: 'Gym', label: 'Gym' },
  { value: 'ShoppingMall', label: 'Shopping mall' },
  { value: 'Station', label: 'Station' },
  { value: 'Restaurant', label: 'Restaurant' },
  { value: 'Cinema', label: 'Cinema' },
];

export const FOOTFALL_LEVELS: readonly Option[] = [
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
];

export const LOCATION_TYPES: readonly Option[] = [
  { value: 'CityCentre', label: 'City centre' },
  { value: 'ShoppingCentre', label: 'Shopping centre' },
  { value: 'TransportHub', label: 'Transport hub' },
  { value: 'Suburb', label: 'Suburb' },
];

/** The label of a value, or the value itself when it is unknown. */
export function labelOf(options: readonly Option[], value: string): string {
  return options.find((option) => option.value === value)?.label ?? value;
}
