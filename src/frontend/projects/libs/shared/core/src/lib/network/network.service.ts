import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { APP_ENVIRONMENT } from '@screen-ad-booking/shared/environment';
import { Observable } from 'rxjs';
import { NetworkCountry, PagedResponse, Venue, VenueFilters } from './network.models';

export const VENUES_PAGE_SIZE = 20;

@Injectable({ providedIn: 'root' })
export class NetworkService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(APP_ENVIRONMENT).apiBaseUrl;

  /** Countries, regions and cities, to fill the location filters. */
  locations(): Observable<NetworkCountry[]> {
    return this.http.get<NetworkCountry[]>(`${this.apiBaseUrl}/network/locations`);
  }

  searchVenues(filters: VenueFilters, page: number): Observable<PagedResponse<Venue>> {
    let params = new HttpParams().set('page', page).set('pageSize', VENUES_PAGE_SIZE);
    for (const [name, value] of Object.entries(filters)) {
      if (value) {
        params = params.set(name, value);
      }
    }
    return this.http.get<PagedResponse<Venue>>(`${this.apiBaseUrl}/venues`, { params });
  }
}
