import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { APP_ENVIRONMENT } from '@screen-ad-booking/shared/environment';
import { Observable } from 'rxjs';
import { Video } from './videos.models';

/** The video library of the advertiser's company. */
@Injectable({ providedIn: 'root' })
export class VideosService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(APP_ENVIRONMENT).apiBaseUrl}/videos`;

  /** Newest first. */
  list(): Observable<Video[]> {
    return this.http.get<Video[]>(this.baseUrl);
  }

  /** The new video waits for the agency's review. */
  upload(file: File): Observable<Video> {
    const body = new FormData();
    body.append('file', file, file.name);
    return this.http.post<Video>(this.baseUrl, body);
  }

  /** The stream needs the access token, so a `<video>` element cannot load it directly. */
  content(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/content`, { responseType: 'blob' });
  }
}
