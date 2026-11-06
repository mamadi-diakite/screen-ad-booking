import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  CreditsService,
  FormSubmission,
  LoadState,
  movementTypeLabel,
  signedCredits,
  Wallet,
  withLoadState,
} from '@screen-ad-booking/shared/core';
import { FieldErrors } from '@screen-ad-booking/shared/ui';

/** The company's fictional credits: balance, top-up by pack, and every movement. */
@Component({
  selector: 'sab-credits-page',
  imports: [DatePipe, DecimalPipe, ReactiveFormsModule, FieldErrors],
  template: `
    <section class="space-y-8">
      <div class="space-y-2">
        <h1 class="text-2xl font-bold tracking-tight">Credits</h1>
        <p class="text-slate-600">
          Credits pay for your broadcast slots. They are fictional: no payment is involved.
        </p>
      </div>

      @let current = wallet();
      @if (current.status === 'loading') {
        <p class="text-slate-600" role="status">Loading your credits…</p>
      } @else if (current.status === 'ready') {
        <dl class="grid gap-4 sm:grid-cols-2">
          <div class="panel">
            <dt class="text-sm text-slate-500">Available</dt>
            <dd class="text-3xl font-bold" data-testid="available">
              {{ current.value.available | number }}
            </dd>
          </div>
          <div class="panel">
            <dt class="text-sm text-slate-500">Held for bookings</dt>
            <dd class="text-3xl font-bold" data-testid="held">{{ current.value.held | number }}</dd>
          </div>
        </dl>

        <form class="panel" [formGroup]="form" (ngSubmit)="topUp()" novalidate>
          <h2 class="text-lg font-semibold">Add credits</h2>
          @if (added(); as credits) {
            <p class="alert-success" role="status">{{ credits | number }} credits were added.</p>
          }
          @if (submission.errors().form; as message) {
            <p class="alert-error" role="alert">{{ message }}</p>
          }
          @let packList = packs();
          @if (packList.status === 'ready') {
            <fieldset class="grid gap-3 sm:grid-cols-3">
              <legend class="sr-only">Credit pack</legend>
              @for (pack of packList.value; track pack.code) {
                <label
                  class="flex cursor-pointer items-center gap-3 rounded-lg border border-slate-200 p-4 has-checked:border-violet-500 has-checked:bg-violet-50"
                >
                  <input
                    type="radio"
                    formControlName="packCode"
                    [id]="'pack-' + pack.code"
                    [value]="pack.code"
                  />
                  <span>
                    <span class="block font-medium">{{ pack.name }}</span>
                    <span class="text-sm text-slate-500">{{ pack.credits | number }} credits</span>
                  </span>
                </label>
              }
            </fieldset>
            <sab-field-errors [messages]="submission.messagesFor('packCode')" />
            <button
              class="button-primary sm:w-auto"
              type="submit"
              [disabled]="submission.pending()"
            >
              Add credits
            </button>
          } @else if (packList.status === 'loading') {
            <p class="text-slate-600" role="status">Loading the packs…</p>
          } @else {
            <p class="alert-error" role="alert">The credit packs could not be loaded.</p>
          }
        </form>

        <section class="space-y-4" aria-labelledby="movements-title">
          <h2 id="movements-title" class="text-lg font-semibold">Movements</h2>
          @if (current.value.movements.length > 0) {
            <div class="overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
              <table class="w-full text-left text-sm">
                <thead class="border-b border-slate-200 text-slate-500">
                  <tr>
                    <th class="px-4 py-2 font-medium">Date</th>
                    <th class="px-4 py-2 font-medium">Type</th>
                    <th class="px-4 py-2 font-medium">Details</th>
                    <th class="px-4 py-2 text-right font-medium">Credits</th>
                  </tr>
                </thead>
                <tbody>
                  @for (movement of current.value.movements; track movement.id) {
                    <tr class="border-b border-slate-100 last:border-0">
                      <td class="px-4 py-2">{{ movement.createdAtUtc | date: 'mediumDate' }}</td>
                      <td class="px-4 py-2">{{ typeLabel(movement.type) }}</td>
                      <td class="px-4 py-2">{{ movement.label }}</td>
                      <td class="px-4 py-2 text-right font-medium tabular-nums">
                        {{ signed(movement) }}
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          } @else {
            <p class="text-slate-600">No movements yet.</p>
          }
        </section>
      } @else {
        <p class="alert-error" role="alert">
          Your credits could not be loaded. Please try again later.
        </p>
      }
    </section>
  `,
})
export class CreditsPage {
  private readonly credits = inject(CreditsService);
  private readonly formBuilder = inject(NonNullableFormBuilder);

  protected readonly typeLabel = movementTypeLabel;
  protected readonly signed = signedCredits;
  protected readonly wallet = signal<LoadState<Wallet>>({ status: 'loading' });
  protected readonly packs = toSignal(withLoadState(this.credits.packs()), { requireSync: true });
  protected readonly added = signal<number | null>(null);
  protected readonly form = this.formBuilder.group({ packCode: ['', Validators.required] });
  protected readonly submission = new FormSubmission(this.form, {
    packCode: 'Choose a credit pack.',
  });

  constructor() {
    withLoadState(this.credits.wallet()).subscribe((state) => this.wallet.set(state));
  }

  protected topUp(): void {
    const { packCode } = this.form.getRawValue();
    this.added.set(null);
    this.submission.submit(
      () => this.credits.topUp(packCode),
      (wallet) => {
        this.wallet.set({ status: 'ready', value: wallet });
        this.added.set(wallet.movements[0].credits);
        this.form.reset();
      },
    );
  }
}
