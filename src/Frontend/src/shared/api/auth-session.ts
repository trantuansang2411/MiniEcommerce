"use client";

import type { AuthUser } from "@/modules/auth/types/auth.type";

const legacySessionKey = "mini-project-auth";
const channelName = "ministore-auth";

export type AuthSession = AuthUser & {
  accessToken: string;
};

export type AuthEvent =
  | { type: "LOGIN" | "TOKEN_REFRESHED"; session: AuthSession }
  | { type: "LOGOUT" };

let memorySession: AuthSession | null = null;
let channel: BroadcastChannel | null = null;
const listeners = new Set<(event: AuthEvent) => void>();

function getChannel() {
  if (channel || typeof window === "undefined" || !("BroadcastChannel" in window)) return channel;

  channel = new BroadcastChannel(channelName);
  channel.onmessage = (message: MessageEvent<AuthEvent>) => notify(message.data);
  return channel;
}

function notify(event: AuthEvent) {
  for (const listener of listeners) listener(event);
}

export function getSession(): AuthSession | null {
  if (typeof window !== "undefined") window.sessionStorage.removeItem(legacySessionKey);
  return memorySession;
}

export function saveSession(session: AuthSession) {
  memorySession = session;
}

export function clearSession() {
  memorySession = null;
  if (typeof window !== "undefined") window.sessionStorage.removeItem(legacySessionKey);
}

export function publishAuthEvent(event: AuthEvent) {
  notify(event);
  getChannel()?.postMessage(event);
}

export function subscribeToAuthEvents(listener: (event: AuthEvent) => void) {
  listeners.add(listener);
  getChannel();
  return () => listeners.delete(listener);
}
