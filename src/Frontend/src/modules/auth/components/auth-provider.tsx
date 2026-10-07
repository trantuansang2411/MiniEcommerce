"use client";

import { createContext, useCallback, useContext, useEffect, useState } from "react";
import { clearSession, publishAuthEvent, saveSession, subscribeToAuthEvents, type AuthSession } from "@/shared/api/auth-session";
import type { Role } from "@/modules/auth/types/auth.type";
import { refreshAccessToken } from "@/shared/api/api-client";
import { authService } from "@/modules/auth/api/auth.service";

type AuthContextValue = {
  session: AuthSession | null;
  isReady: boolean;
  setSession: (session: AuthSession) => void;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

function attachRole(session: AuthSession | null): AuthSession | null {
  if (!session || session.role) return session;

  try {
    const payload = JSON.parse(atob(session.accessToken.split(".")[1].replace(/-/g, "+").replace(/_/g, "/"))) as Record<string, string>;
    const role = payload.role ?? payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
    return { ...session, role: role as Role | undefined };
  } catch {
    return session;
  }
}

export function AuthProvider({ children }: Readonly<{ children: React.ReactNode }>) {
  const [session, setCurrentSession] = useState<AuthSession | null>(null);
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    let isMounted = true;
    const unsubscribe = subscribeToAuthEvents((event) => {
      if (!isMounted) return;
      if (event.type === "LOGOUT") {
        setCurrentSession(null);
        return;
      }

      saveSession(event.session);
      setCurrentSession(attachRole(event.session));
    });

    void refreshAccessToken()
      .then((nextSession) => { if (isMounted) setCurrentSession(attachRole(nextSession)); })
      .catch(() => { if (isMounted) setCurrentSession(null); })
      .finally(() => { if (isMounted) setIsReady(true); });

    return () => {
      isMounted = false;
      unsubscribe();
    };
  }, []);

  const setSession = useCallback((nextSession: AuthSession) => {
    const hydratedSession = attachRole(nextSession)!;
    saveSession(hydratedSession);
    setCurrentSession(hydratedSession);
    publishAuthEvent({ type: "LOGIN", session: hydratedSession });
  }, []);

  const logout = useCallback(() => {
    void authService.logout().catch(() => undefined).finally(() => {
      clearSession();
      publishAuthEvent({ type: "LOGOUT" });
    });
  }, []);

  return <AuthContext.Provider value={{ session, isReady, setSession, logout }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used inside AuthProvider.");
  return context;
}
