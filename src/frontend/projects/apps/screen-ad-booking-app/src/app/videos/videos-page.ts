import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  FormSubmission,
  LoadState,
  MAX_VIDEO_BYTES,
  toFormErrors,
  Video,
  videoStatusLabel,
  VideosService,
  withLoadState,
} from '@screen-ad-booking/shared/core';
import { FieldErrors } from '@screen-ad-booking/shared/ui';
import { switchMap } from 'rxjs';
import { formatFileSize } from '../shared/file-size';

const STATUS_CLASSES: Record<Video['status'], string> = {
  PendingReview: 'bg-amber-50 text-amber-800',
  Approved: 'bg-emerald-50 text-emerald-800',
  Rejected: 'bg-rose-50 text-rose-700',
};

/** The company's videos: upload, review status and preview. */
@Component({
  selector: 'sab-videos-page',
  imports: [DatePipe, ReactiveFormsModule, FieldErrors],
  template: `
    <section class="space-y-8">
      <h1 class="text-2xl font-bold tracking-tight">Videos</h1>

      <form class="panel" [formGroup]="form" (ngSubmit)="upload(fileInput)" novalidate>
        <h2 class="text-lg font-semibold">Add a video</h2>
        <p class="text-sm text-slate-600">
          MP4 or WebM, 10 MB max. The agency reviews each new video before it can be broadcast.
        </p>
        @if (uploaded(); as video) {
          <p class="alert-success" role="status">
            "{{ video.name }}" was added and is waiting for review.
          </p>
        }
        @if (submission.errors().form; as message) {
          <p class="alert-error" role="alert">{{ message }}</p>
        }
        <div>
          <label class="form-label" for="file">Video file</label>
          <input
            #fileInput
            id="file"
            class="mt-1 block text-sm"
            type="file"
            accept=".mp4,.webm,video/mp4,video/webm"
            (change)="choose(fileInput)"
          />
          <sab-field-errors [messages]="fileMessages()" />
        </div>
        <button class="button-primary sm:w-auto" type="submit" [disabled]="submission.pending()">
          Upload video
        </button>
      </form>

      @if (preview(); as current) {
        <section class="panel" aria-labelledby="preview-title" data-testid="preview">
          <div class="flex items-center justify-between gap-4">
            <h2 id="preview-title" class="text-lg font-semibold">{{ current.name }}</h2>
            <button type="button" class="link text-sm" (click)="closePreview()">
              Close preview
            </button>
          </div>
          <video class="w-full rounded-lg bg-black" controls [src]="current.url"></video>
        </section>
      }

      <section class="space-y-4" aria-labelledby="library-title">
        <h2 id="library-title" class="text-lg font-semibold">Your library</h2>
        @if (actionError(); as message) {
          <p class="alert-error" role="alert">{{ message }}</p>
        }
        @let current = state();
        @if (current.status === 'loading') {
          <p class="text-slate-600" role="status">Loading your videos…</p>
        } @else if (current.status === 'ready') {
          <ul class="space-y-3" data-testid="videos">
            @for (video of current.value; track video.id) {
              <li
                class="flex flex-wrap items-start justify-between gap-4 rounded-xl border border-slate-200 bg-white p-4 shadow-sm"
              >
                <div class="space-y-1">
                  <p class="font-medium" data-testid="name">{{ video.name }}</p>
                  <p class="text-sm text-slate-500" data-testid="details">
                    {{ video.format === 'WebM' ? 'WebM' : 'MP4' }} · {{ size(video.sizeBytes) }} ·
                    Added {{ video.uploadedAtUtc | date: 'mediumDate' }}
                  </p>
                  <p class="text-sm">
                    <span
                      class="rounded-full px-2 py-0.5 text-xs font-medium"
                      [class]="statusClass(video)"
                      data-testid="status"
                      >{{ statusLabel(video.status) }}</span
                    >
                    @if (video.rejectionReason) {
                      <span class="text-rose-700" data-testid="reason">
                        {{ video.rejectionReason }}</span
                      >
                    }
                  </p>
                </div>
                <button
                  type="button"
                  class="link text-sm"
                  [attr.aria-label]="'Preview ' + video.name"
                  (click)="show(video)"
                >
                  Preview
                </button>
              </li>
            } @empty {
              <li class="text-slate-600">No videos yet.</li>
            }
          </ul>
        } @else {
          <p class="alert-error" role="alert">
            Your videos could not be loaded. Please try again later.
          </p>
        }
      </section>
    </section>
  `,
})
export class VideosPage {
  private readonly videos = inject(VideosService);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly version = signal(0);

  protected readonly size = formatFileSize;
  protected readonly statusLabel = videoStatusLabel;
  protected readonly uploaded = signal<Video | null>(null);
  protected readonly preview = signal<{ name: string; url: string } | null>(null);
  protected readonly actionError = signal<string | null>(null);
  protected readonly fileProblem = signal<string | null>(null);
  protected readonly form = this.formBuilder.group({
    file: this.formBuilder.control<File | null>(null, Validators.required),
  });
  protected readonly submission = new FormSubmission(this.form, {
    file: 'Choose a video file.',
  });
  protected readonly state = toSignal(
    toObservable(this.version).pipe(switchMap(() => withLoadState(this.videos.list()))),
    { initialValue: { status: 'loading' } as LoadState<Video[]> },
  );

  constructor() {
    inject(DestroyRef).onDestroy(() => this.closePreview());
  }

  protected statusClass(video: Video): string {
    return STATUS_CLASSES[video.status];
  }

  protected fileMessages(): readonly string[] {
    const problem = this.fileProblem();
    return problem ? [problem] : this.submission.messagesFor('file');
  }

  protected choose(input: HTMLInputElement): void {
    const file = input.files?.[0] ?? null;
    const problem = file ? checkVideo(file) : null;
    this.fileProblem.set(problem);
    this.form.controls.file.setValue(problem ? null : file);
    this.form.controls.file.markAsTouched();
    if (problem) {
      input.value = '';
    }
  }

  protected upload(input: HTMLInputElement): void {
    const file = this.form.getRawValue().file;
    this.uploaded.set(null);
    this.submission.submit(
      () => this.videos.upload(file!),
      (video) => {
        this.uploaded.set(video);
        this.form.reset();
        input.value = '';
        this.version.update((version) => version + 1);
      },
    );
  }

  protected show(video: Video): void {
    this.actionError.set(null);
    this.videos.content(video.id).subscribe({
      next: (blob) => {
        this.closePreview();
        this.preview.set({ name: video.name, url: URL.createObjectURL(blob) });
      },
      error: (error: unknown) => this.actionError.set(toFormErrors(error).form),
    });
  }

  protected closePreview(): void {
    const current = this.preview();
    if (current) {
      URL.revokeObjectURL(current.url);
      this.preview.set(null);
    }
  }
}

function checkVideo(file: File): string | null {
  if (!/\.(mp4|webm)$/i.test(file.name)) {
    return 'Only MP4 and WebM videos can be uploaded.';
  }

  return file.size > MAX_VIDEO_BYTES ? 'The video is larger than 10 MB.' : null;
}
