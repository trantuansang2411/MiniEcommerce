import { clearSession, getSession, publishAuthEvent, saveSession } from "@/shared/api/auth-session";
import type { ApiError, ApiResponse } from "@/shared/types/api";
import type { AuthSession } from "@/shared/api/auth-session";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5114/api";

type RequestOptions = Omit<RequestInit, "body"> & {
  body?: unknown;
  authenticated?: boolean;
  retryAfterRefresh?: boolean;
};

function createError(statusCode: number, message: string, errors?: Record<string, string[]>) {
  const error = new Error(message) as ApiError;
  error.statusCode = statusCode;
  error.errors = errors;
  return error;
}

let refreshPromise: Promise<AuthSession | null> | null = null;

async function withRefreshLock<T>(action: () => Promise<T>): Promise<T> {
  if (typeof navigator === "undefined" || !("locks" in navigator)) return action();
  return navigator.locks.request("ministore-auth-refresh", { mode: "exclusive" }, action);
}

async function requestRefreshToken(): Promise<AuthSession | null> {
  const accessTokenBeforeLock = getSession()?.accessToken ?? null;

  return withRefreshLock(async () => {
    const sessionAfterLock = getSession();
    if (sessionAfterLock && sessionAfterLock.accessToken !== accessTokenBeforeLock) return sessionAfterLock;

    const response = await fetch(`${apiBaseUrl}/user/refresh-token`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
    });

    if (!response.ok) return null;

    const body = (await response.json()) as ApiResponse<AuthSession>;
    saveSession(body.data);
    publishAuthEvent({ type: "TOKEN_REFRESHED", session: body.data });
    return body.data;
  });
}

export function refreshAccessToken(): Promise<AuthSession | null> {
  if (!refreshPromise) {
    refreshPromise = requestRefreshToken().finally(() => { refreshPromise = null; });
  }

  return refreshPromise;
}

export async function apiClient<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { body, authenticated = false, retryAfterRefresh = true, headers, ...init } = options;
  const session = authenticated ? getSession() : null;
  const requestHeaders = new Headers(headers);

  if (body !== undefined && !(body instanceof FormData)) {
    requestHeaders.set("Content-Type", "application/json");
  }

  if (authenticated && session?.accessToken) {
    requestHeaders.set("Authorization", `Bearer ${session.accessToken}`);
  }

  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: requestHeaders,
    body: body instanceof FormData ? body : body === undefined ? undefined : JSON.stringify(body),
    credentials: "include",
  });

  if (response.status === 401 && authenticated && retryAfterRefresh && session) {
    const refreshedSession = await refreshAccessToken();
    if (refreshedSession) {
      return apiClient<T>(path, { ...options, retryAfterRefresh: false });
    }

    clearSession();
    publishAuthEvent({ type: "LOGOUT" });
  }

  const responseBody = await response.json().catch(() => null) as ApiResponse<T> | { title?: string; errors?: Record<string, string[]> } | null;

  if (!response.ok) {
    const message = responseBody && "message" in responseBody
      ? responseBody.message
      : responseBody?.title ?? "Request failed.";
    const errors = responseBody && "errors" in responseBody ? responseBody.errors : undefined;
    throw createError(response.status, message, errors);
  }

  return (responseBody as ApiResponse<T>).data;
}
