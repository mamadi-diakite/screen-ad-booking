export const MAX_VIDEO_BYTES = 10 * 1024 * 1024;

export type VideoStatus = 'PendingReview' | 'Approved' | 'Rejected';

export interface Video {
  readonly id: string;
  readonly name: string;
  /** `Mp4` or `WebM`. */
  readonly format: string;
  readonly sizeBytes: number;
  readonly status: VideoStatus;
  /** Why the agency rejected the video; `null` otherwise. */
  readonly rejectionReason: string | null;
  readonly uploadedAtUtc: string;
}

const STATUS_LABELS: Record<VideoStatus, string> = {
  PendingReview: 'Pending review',
  Approved: 'Approved',
  Rejected: 'Rejected',
};

export function videoStatusLabel(status: VideoStatus): string {
  return STATUS_LABELS[status];
}
