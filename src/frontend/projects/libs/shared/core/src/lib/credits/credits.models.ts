export interface CreditPack {
  readonly code: string;
  readonly name: string;
  readonly credits: number;
}

export type CreditMovementType = 'TopUp' | 'Hold' | 'Charge' | 'Release';

export interface CreditMovement {
  readonly id: string;
  readonly type: CreditMovementType;
  /** Always positive; the type tells which way the credits moved. */
  readonly credits: number;
  readonly label: string;
  readonly createdAtUtc: string;
}

export interface Wallet {
  readonly available: number;
  /** Credits set aside for bookings that have not been played yet. */
  readonly held: number;
  /** Newest first. */
  readonly movements: readonly CreditMovement[];
}

const MOVEMENT_TYPES: Record<CreditMovementType, { label: string; sign: '+' | '−' }> = {
  TopUp: { label: 'Top-up', sign: '+' },
  Hold: { label: 'Hold', sign: '−' },
  Charge: { label: 'Charge', sign: '−' },
  Release: { label: 'Release', sign: '+' },
};

export function movementTypeLabel(type: CreditMovementType): string {
  return MOVEMENT_TYPES[type].label;
}

/** The movement as it changes the available balance, e.g. `+2,000`. */
export function signedCredits(movement: CreditMovement): string {
  return `${MOVEMENT_TYPES[movement.type].sign}${movement.credits.toLocaleString('en-US')}`;
}
