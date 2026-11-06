import { Video } from '@screen-ad-booking/shared/core';
import { vi } from 'vitest';
import { AppHarness } from '../../testing/app-harness';

const pending: Video = {
  id: 'vid-1',
  name: 'Autumn bread offer',
  format: 'Mp4',
  sizeBytes: 3_565_158,
  status: 'PendingReview',
  rejectionReason: null,
  uploadedAtUtc: '2026-11-05T10:00:00Z',
};

const approved: Video = {
  ...pending,
  id: 'vid-2',
  name: 'Croissant week',
  format: 'WebM',
  sizeBytes: 512_000,
  status: 'Approved',
  uploadedAtUtc: '2026-10-20T10:00:00Z',
};

const rejected: Video = {
  ...pending,
  id: 'vid-3',
  name: 'Night promo',
  status: 'Rejected',
  rejectionReason: 'The logo is cut off at the end.',
};

describe('Videos page', () => {
  let app: AppHarness;

  beforeEach(() => (app = new AppHarness()));

  afterEach(() => {
    app.http.verify();
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
  });

  async function openPage(videos: Video[] = [pending, approved, rejected]): Promise<void> {
    app.signIn();
    await app.open('/videos');
    expect(app.text('[role="status"]')).toBe('Loading your videos…');
    app.http.expectOne('/api/videos').flush(videos);
    await app.settle();
  }

  function video(name: string, size = 1024): File {
    const file = new File(['video'], name);
    Object.defineProperty(file, 'size', { value: size });
    return file;
  }

  it('is only for advertisers', async () => {
    app.signInAsAgencyAdmin();
    await app.open('/videos');

    expect(app.router.url).toBe('/');
  });

  it('lists the videos with their format, size, date and review status', async () => {
    await openPage();

    expect(app.texts('[data-testid="name"]')).toEqual([
      'Autumn bread offer',
      'Croissant week',
      'Night promo',
    ]);
    expect(app.texts('[data-testid="details"]').map(collapse)).toEqual([
      'MP4 · 3.4 MB · Added Nov 5, 2026',
      'WebM · 500 KB · Added Oct 20, 2026',
      'MP4 · 3.4 MB · Added Nov 5, 2026',
    ]);
    expect(app.texts('[data-testid="status"]')).toEqual(['Pending review', 'Approved', 'Rejected']);
    expect(app.element.querySelector('[data-testid="status"]')!.className).toContain('amber');
    expect(app.texts('[data-testid="reason"]')).toEqual(['The logo is cut off at the end.']);
  });

  it('says when the library is empty', async () => {
    await openPage([]);

    expect(app.text('[data-testid="videos"] li')).toBe('No videos yet.');
  });

  it('checks the file before sending it', async () => {
    await openPage([]);

    await app.submit();
    expect(app.text('sab-field-errors')).toBe('Choose a video file.');

    await app.choose('file', video('ad.mov'));
    expect(app.text('sab-field-errors')).toBe('Only MP4 and WebM videos can be uploaded.');

    await app.choose('file', video('ad.WEBM', 10 * 1024 * 1024 + 1));
    expect(app.text('sab-field-errors')).toBe('The video is larger than 10 MB.');

    await app.choose('file');
    expect(app.text('sab-field-errors')).toBe('Choose a video file.');
  });

  it('uploads a video, which then waits for review', async () => {
    await openPage([]);

    await app.choose('file', video('autumn-bread-offer.mp4'));
    await app.submit();
    const request = app.http.expectOne({ method: 'POST', url: '/api/videos' });
    expect(((request.request.body as FormData).get('file') as File).name).toBe(
      'autumn-bread-offer.mp4',
    );
    request.flush(pending, { status: 201, statusText: 'Created' });
    await app.settle();

    expect(app.text('form [role="status"]')).toBe(
      '"Autumn bread offer" was added and is waiting for review.',
    );
    app.http.expectOne('/api/videos').flush([pending]);
    await app.settle();
    expect(app.texts('[data-testid="name"]')).toEqual(['Autumn bread offer']);
  });

  it('shows the server message about the file', async () => {
    await openPage([]);

    await app.choose('file', video('fake.mp4'));
    await app.submit();
    app.http
      .expectOne({ method: 'POST', url: '/api/videos' })
      .flush(
        { errors: { file: ['The file content is not an MP4 or WebM video.'] } },
        { status: 400, statusText: 'Bad Request' },
      );
    await app.settle();

    expect(app.text('sab-field-errors')).toBe('The file content is not an MP4 or WebM video.');

    await app.choose('file', video('real.mp4'));
    await app.submit();
    app.http
      .expectOne({ method: 'POST', url: '/api/videos' })
      .flush(
        { title: 'Only advertisers have a video library.' },
        { status: 403, statusText: 'Forbidden' },
      );
    await app.settle();

    expect(app.text('form [role="alert"]')).toBe('Only advertisers have a video library.');
  });

  it('previews a video with the access token, and frees it when closed', async () => {
    const createObjectURL = vi.fn().mockReturnValueOnce('blob:one').mockReturnValueOnce('blob:two');
    const revokeObjectURL = vi.fn();
    vi.stubGlobal('URL', Object.assign(URL, { createObjectURL, revokeObjectURL }));
    await openPage();

    await app.click('[aria-label="Preview Autumn bread offer"]');
    const content = app.http.expectOne('/api/videos/vid-1/content');
    expect(content.request.headers.get('Authorization')).toBe('Bearer token-1');
    content.flush(new Blob(['video']));
    await app.settle();

    expect(app.text('#preview-title')).toBe('Autumn bread offer');
    expect(app.element.querySelector('video')!.getAttribute('src')).toBe('blob:one');

    await app.click('[aria-label="Preview Croissant week"]');
    app.http.expectOne('/api/videos/vid-2/content').flush(new Blob(['video']));
    await app.settle();
    expect(revokeObjectURL).toHaveBeenCalledWith('blob:one');
    expect(app.element.querySelector('video')!.getAttribute('src')).toBe('blob:two');

    await app.click('[data-testid="preview"] button');
    expect(revokeObjectURL).toHaveBeenCalledWith('blob:two');
    expect(app.element.querySelector('[data-testid="preview"]')).toBeNull();

    await app.click('[aria-label="Preview Croissant week"]');
    app.http
      .expectOne('/api/videos/vid-2/content')
      .flush(new Blob(), { status: 404, statusText: 'Not Found' });
    await app.settle();
    expect(app.text('[aria-labelledby="library-title"] [role="alert"]')).toBe(
      'Something went wrong. Please try again.',
    );
  });

  it('frees the preview when leaving the page', async () => {
    const revokeObjectURL = vi.fn();
    vi.stubGlobal(
      'URL',
      Object.assign(URL, { createObjectURL: vi.fn(() => 'blob:one'), revokeObjectURL }),
    );
    await openPage();
    await app.click('[aria-label="Preview Autumn bread offer"]');
    app.http.expectOne('/api/videos/vid-1/content').flush(new Blob(['video']));
    await app.settle();

    await app.open('/');

    expect(revokeObjectURL).toHaveBeenCalledWith('blob:one');
  });

  it('explains when the videos cannot be loaded', async () => {
    app.signIn();
    await app.open('/videos');
    app.http.expectOne('/api/videos').flush(null, { status: 500, statusText: 'Error' });
    await app.settle();

    expect(app.text('[role="alert"]')).toBe(
      'Your videos could not be loaded. Please try again later.',
    );
  });
});

function collapse(text: string): string {
  return text.replace(/\s+/g, ' ');
}
