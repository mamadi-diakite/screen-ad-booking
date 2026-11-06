import { DecimalPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import {
  FOOTFALL_LEVELS,
  labelOf,
  LoadState,
  LOCATION_TYPES,
  NetworkService,
  pageCount,
  PagedResponse,
  Venue,
  VENUE_TYPES,
  VenueFilters,
  withLoadState,
} from '@screen-ad-booking/shared/core';
import { switchMap } from 'rxjs';

/** The venue network, filtered by place and by kind of venue. */
@Component({
  selector: 'sab-venues-page',
  imports: [DecimalPipe, ReactiveFormsModule],
  template: `
    <section class="space-y-8">
      <div class="space-y-2">
        <h1 class="text-2xl font-bold tracking-tight">Venues</h1>
        <p class="text-slate-600">Find the screens where your customers shop, eat and travel.</p>
      </div>

      <form class="panel" [formGroup]="form" (ngSubmit)="search()" novalidate>
        @if (locations().status === 'error') {
          <p class="alert-error" role="alert">
            The countries, regions and cities could not be loaded.
          </p>
        }
        <div class="grid gap-4 sm:grid-cols-3">
          <div>
            <label class="form-label" for="country">Country</label>
            <select id="country" class="form-input" formControlName="country">
              <option value="">All countries</option>
              @for (country of countries(); track country.code) {
                <option [value]="country.code">{{ country.name }}</option>
              }
            </select>
          </div>
          <div>
            <label class="form-label" for="regionId">Region</label>
            <select id="regionId" class="form-input" formControlName="regionId">
              <option value="">All regions</option>
              @for (region of regions(); track region.id) {
                <option [value]="region.id">{{ region.name }}</option>
              }
            </select>
          </div>
          <div>
            <label class="form-label" for="cityId">City</label>
            <select id="cityId" class="form-input" formControlName="cityId">
              <option value="">All cities</option>
              @for (city of cities(); track city.id) {
                <option [value]="city.id">{{ city.name }}</option>
              }
            </select>
          </div>
          <div>
            <label class="form-label" for="type">Venue type</label>
            <select id="type" class="form-input" formControlName="type">
              <option value="">All types</option>
              @for (option of venueTypes; track option.value) {
                <option [value]="option.value">{{ option.label }}</option>
              }
            </select>
          </div>
          <div>
            <label class="form-label" for="footfall">Footfall</label>
            <select id="footfall" class="form-input" formControlName="footfall">
              <option value="">Any footfall</option>
              @for (option of footfallLevels; track option.value) {
                <option [value]="option.value">{{ option.label }}</option>
              }
            </select>
          </div>
          <div>
            <label class="form-label" for="locationType">Location</label>
            <select id="locationType" class="form-input" formControlName="locationType">
              <option value="">Any location</option>
              @for (option of locationTypes; track option.value) {
                <option [value]="option.value">{{ option.label }}</option>
              }
            </select>
          </div>
        </div>
        <div class="flex flex-wrap gap-3">
          <button class="button-primary sm:w-auto" type="submit">Search</button>
          <button class="button-secondary" type="button" (click)="clear()">Clear filters</button>
        </div>
      </form>

      @let current = state();
      @if (current.status === 'loading') {
        <p class="text-slate-600" role="status">Loading the venues…</p>
      } @else if (current.status === 'ready') {
        <section class="space-y-4" aria-labelledby="results-title">
          <h2 id="results-title" class="text-lg font-semibold">
            {{ current.value.totalCount }} {{ current.value.totalCount === 1 ? 'venue' : 'venues' }}
          </h2>
          <ul class="grid gap-3 md:grid-cols-2" data-testid="venues">
            @for (venue of current.value.items; track venue.id) {
              <li class="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
                <p class="font-medium" data-testid="name">{{ venue.name }}</p>
                <p class="text-sm text-slate-500" data-testid="kind">
                  {{ typeLabel(venue.type) }} · {{ locationLabel(venue.locationType) }} ·
                  {{ footfallLabel(venue.footfall) }} footfall
                </p>
                <p class="text-sm text-slate-600" data-testid="place">
                  {{ venue.address }}, {{ venue.cityName }} · {{ venue.regionName }},
                  {{ venue.countryCode }}
                </p>
                <p class="mt-2 text-sm" data-testid="audience">
                  {{ venue.screenCount }} {{ venue.screenCount === 1 ? 'screen' : 'screens' }} ·
                  average audience {{ venue.averageAudience | number }}
                </p>
              </li>
            } @empty {
              <li class="text-slate-600">No venues match these filters.</li>
            }
          </ul>
          @if (pages(current.value) > 1) {
            <nav class="flex items-center gap-4 text-sm" aria-label="Pages">
              <button
                type="button"
                class="button-secondary"
                [disabled]="current.value.page <= 1"
                (click)="goTo(current.value.page - 1)"
              >
                Previous
              </button>
              <span>Page {{ current.value.page }} of {{ pages(current.value) }}</span>
              <button
                type="button"
                class="button-secondary"
                [disabled]="current.value.page >= pages(current.value)"
                (click)="goTo(current.value.page + 1)"
              >
                Next
              </button>
            </nav>
          }
        </section>
      } @else {
        <p class="alert-error" role="alert">
          The venues could not be loaded. Check the filters or try again later.
        </p>
      }
    </section>
  `,
})
export class VenuesPage {
  private readonly network = inject(NetworkService);
  private readonly formBuilder = inject(NonNullableFormBuilder);

  protected readonly venueTypes = VENUE_TYPES;
  protected readonly footfallLevels = FOOTFALL_LEVELS;
  protected readonly locationTypes = LOCATION_TYPES;
  protected readonly pages = pageCount;
  protected readonly form = this.formBuilder.group({
    country: [''],
    regionId: [''],
    cityId: [''],
    type: [''],
    footfall: [''],
    locationType: [''],
  });

  private readonly query = signal<{ filters: VenueFilters; page: number }>({
    filters: this.form.getRawValue(),
    page: 1,
  });
  private readonly country = toSignal(this.form.controls.country.valueChanges, {
    initialValue: '',
  });
  private readonly regionId = toSignal(this.form.controls.regionId.valueChanges, {
    initialValue: '',
  });

  protected readonly locations = toSignal(withLoadState(this.network.locations()), {
    requireSync: true,
  });
  protected readonly countries = computed(() => {
    const locations = this.locations();
    return locations.status === 'ready' ? locations.value : [];
  });
  protected readonly regions = computed(
    () => this.countries().find((country) => country.code === this.country())?.regions ?? [],
  );
  protected readonly cities = computed(
    () => this.regions().find((region) => region.id === this.regionId())?.cities ?? [],
  );
  protected readonly state = toSignal(
    toObservable(this.query).pipe(
      switchMap(({ filters, page }) => withLoadState(this.network.searchVenues(filters, page))),
    ),
    { initialValue: { status: 'loading' } as LoadState<PagedResponse<Venue>> },
  );

  constructor() {
    this.form.controls.country.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.regionId.setValue(''));
    this.form.controls.regionId.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.cityId.setValue(''));
  }

  protected typeLabel(value: string): string {
    return labelOf(VENUE_TYPES, value);
  }

  protected footfallLabel(value: string): string {
    return labelOf(FOOTFALL_LEVELS, value);
  }

  protected locationLabel(value: string): string {
    return labelOf(LOCATION_TYPES, value);
  }

  protected search(): void {
    this.query.set({ filters: this.form.getRawValue(), page: 1 });
  }

  protected clear(): void {
    this.form.reset();
    this.search();
  }

  protected goTo(page: number): void {
    this.query.update((query) => ({ ...query, page }));
  }
}
