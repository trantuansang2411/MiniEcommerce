import { apiClient } from "@/shared/api/api-client";
import type { AuthSession } from "@/shared/api/auth-session";
import type { ForgotPasswordRequest, LoginRequest, RegisterRequest, ResetPasswordRequest, VerifyEmailRequest } from "@/modules/auth/types/auth.type";

export const authService = {
  login: (request: LoginRequest) =>
    apiClient<AuthSession>("/user/login", { method: "POST", body: request }),
  logout: () => apiClient<null>("/user/logout", { method: "POST" }),

  register: (request: RegisterRequest) =>
    apiClient<null>("/user/register", { method: "POST", body: request }),

  verifyEmail: (request: VerifyEmailRequest) =>
    apiClient<AuthSession>("/user/verify-email", { method: "POST", body: request }),

  resendVerification: (request: ForgotPasswordRequest) =>
    apiClient<null>("/user/resend-verification", { method: "POST", body: request }),

  forgotPassword: (request: ForgotPasswordRequest) =>
    apiClient<null>("/user/forgot-password", { method: "POST", body: request }),

  resetPassword: (request: ResetPasswordRequest) =>
    apiClient<null>("/user/reset-password", { method: "POST", body: request }),
};
