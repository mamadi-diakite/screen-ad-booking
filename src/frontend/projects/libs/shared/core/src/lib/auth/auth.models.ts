export interface CurrentUser {
  readonly id: string;
  readonly email: string;
  readonly displayName: string;
  /** "Advertiser" or "AgencyAdmin". */
  readonly role: string;
  /** Set for advertisers only. */
  readonly companyId: string | null;
}

export interface LoginRequest {
  readonly email: string;
  readonly password: string;
}

/** Self sign-up creates an advertiser together with its company. */
export interface RegisterRequest {
  readonly companyName: string;
  readonly displayName: string;
  readonly email: string;
  readonly password: string;
  readonly confirmPassword: string;
}

export interface ChangePasswordRequest {
  readonly currentPassword: string;
  readonly newPassword: string;
  readonly confirmPassword: string;
}

export interface LoginResponse {
  readonly accessToken: string;
  readonly expiresAtUtc: string;
  readonly user: CurrentUser;
}
