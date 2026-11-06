import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { ApiHealthService } from '@screen-ad-booking/shared/core';
import { Shell } from '@screen-ad-booking/shared/ui';

@Component({
  selector: 'sab-root',
  imports: [RouterOutlet, Shell],
  template: `
    <sab-shell appName="Screen Ad Booking" [apiStatus]="apiStatus()">
      <router-outlet />
    </sab-shell>
  `,
})
export class App {
  protected readonly apiStatus = toSignal(inject(ApiHealthService).check(), {
    initialValue: 'checking',
  });
}
