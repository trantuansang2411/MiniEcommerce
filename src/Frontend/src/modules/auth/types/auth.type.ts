export type Role = "Admin" | "Manager" | "Staff" | "User";

export type AuthUser = {
  userId: string;
  email: string;
  role?: Role;
};

export type LoginRequest = {
  email: string;
  password: string;
};

export type RegisterRequest = LoginRequest;

export type ForgotPasswordRequest = { email: string };

export type VerifyEmailRequest = ForgotPasswordRequest & { code: string };

export type ResetPasswordRequest = VerifyEmailRequest & { newPassword: string };
