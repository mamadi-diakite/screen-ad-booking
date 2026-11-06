import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { APP_ENVIRONMENT } from '@screen-ad-booking/shared/environment';
import { Observable } from 'rxjs';
import { CreditPack, Wallet } from './credits.models';

/** The fictional credits of the advertiser's company. No payment is involved. */
@Injectable({ providedIn: 'root' })
export class CreditsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(APP_ENVIRONMENT).apiBaseUrl}/credits`;

  packs(): Observable<CreditPack[]> {
    return this.http.get<CreditPack[]>(`${this.baseUrl}/packs`);
  }

  wallet(): Observable<Wallet> {
    return this.http.get<Wallet>(this.baseUrl);
  }

  topUp(packCode: string): Observable<Wallet> {
    return this.http.post<Wallet>(`${this.baseUrl}/top-up`, { packCode });
  }
}
