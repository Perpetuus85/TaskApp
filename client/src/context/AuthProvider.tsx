import {
  createContext,
  useContext,
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import api, { axiosPrivate } from '../api/axios';

const AUTH_STORAGE_KEY = 'taskapp_auth';

type User = Record<string, unknown>;

export type AuthState = {
  accessToken: string | null;
  refreshToken: string | null;
  user: User | null;
  email: string | null;
};

type LoginCredentials = {
  email: string;
  password: string;
};

type RegisterPayload = {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
};

type AuthResponse = {
  accessToken?: string;
  refreshToken?: string;
  token?: string;
  user?: User;
  email?: string;
};

type RefreshPayload = {
  accessToken: string | null;
  refreshToken: string | null;
};

type AuthContextType = {
  auth: AuthState;
  isAuthenticated: boolean;
  isLoading: boolean;
  setAuth: (value: AuthState | ((previous: AuthState) => AuthState)) => void;
  updateTokens: (tokens: Partial<Pick<AuthState, 'accessToken' | 'refreshToken'>>) => void;
  login: (credentials: LoginCredentials) => Promise<AuthState>;
  register: (payload: RegisterPayload) => Promise<AuthState>;
  refreshAccessToken: () => Promise<string | null>;
  logout: () => Promise<void>;
};

const initialAuthState: AuthState = {
  accessToken: null,
  refreshToken: null,
  user: null,
  email: null,
};

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

const readStoredAuth = (): AuthState => {
  if (typeof window === 'undefined') {
    return initialAuthState;
  }

  try {
    const raw = window.localStorage.getItem(AUTH_STORAGE_KEY);
    if (!raw) {
      return initialAuthState;
    }

    const parsed = JSON.parse(raw) as Partial<AuthState>;
    return {
      accessToken: parsed.accessToken ?? null,
      refreshToken: parsed.refreshToken ?? null,
      user: parsed.user ?? null,
      email: parsed.email ?? null,
    };
  } catch {
    return initialAuthState;
  }
};

const toAuthState = (response: AuthResponse, previous: AuthState): AuthState => ({
  accessToken: response.accessToken ?? response.token ?? previous.accessToken,
  refreshToken: response.refreshToken ?? previous.refreshToken,
  user: response.user ?? previous.user,
  email: response.email ?? previous.email,
});

export default function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<AuthState>(() => readStoredAuth());
  const [isLoading, setIsLoading] = useState(true);

  const updateTokens: AuthContextType['updateTokens'] = useCallback((tokens) => {
    setAuth((previous) => ({
      ...previous,
      ...tokens,
    }));
  }, []);

  const refreshAccessToken = useCallback(async () => {
    try {
      const payload: RefreshPayload = {
        accessToken: auth.accessToken,
        refreshToken: auth.refreshToken,
      };

      const response = await api.post<AuthResponse>('/auth/refresh', payload, {
        withCredentials: true,
      });

      const nextAccessToken = response.data.accessToken ?? response.data.token ?? null;
      const nextRefreshToken = response.data.refreshToken ?? null;
      updateTokens({
        accessToken: nextAccessToken,
        refreshToken: nextRefreshToken,
      });

      return nextAccessToken;
    } catch {
      setAuth(initialAuthState);
      return null;
    }
  }, [auth.accessToken, auth.refreshToken, updateTokens]);

  const login = useCallback(async (credentials: LoginCredentials) => {
    const response = await api.post<AuthResponse>('/auth/login', credentials, {
      withCredentials: true,
    });
    const nextState = toAuthState(response.data, initialAuthState);
    setAuth(nextState);
    return nextState;
  }, []);

  const register = useCallback(async (payload: RegisterPayload) => {
    const response = await api.post<AuthResponse>('/auth/register', payload, {
      withCredentials: true,
    });
    const nextState = toAuthState(response.data, initialAuthState);
    setAuth(nextState);
    return nextState;
  }, []);

  const logout = useCallback(async () => {
    try {
      await axiosPrivate.post('/auth/logout', null, {
        withCredentials: true,
      });
    } finally {
      setAuth(initialAuthState);
    }
  }, []);

  useEffect(() => {
    if (typeof window === 'undefined') {
      return;
    }

    window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth));
  }, [auth]);

  useEffect(() => {
    let isMounted = true;

    const initialize = async () => {
      try {
        if (!auth.accessToken && auth.refreshToken) {
          await refreshAccessToken();
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    initialize();

    return () => {
      isMounted = false;
    };
  }, [auth.accessToken, auth.refreshToken, refreshAccessToken]);

  useEffect(() => {
    const requestInterceptor = axiosPrivate.interceptors.request.use(
      (config) => {
        if (!config.headers.Authorization && auth.accessToken) {
          config.headers.Authorization = `Bearer ${auth.accessToken}`;
        }
        return config;
      },
      (error) => Promise.reject(error),
    );

    const responseInterceptor = axiosPrivate.interceptors.response.use(
      (response) => response,
      async (error) => {
        const previousRequest = error?.config as (typeof error.config & { sent?: boolean }) | undefined;
        if (error?.response?.status === 401 && previousRequest && !previousRequest.sent) {
          previousRequest.sent = true;
          const nextToken = await refreshAccessToken();
          if (nextToken) {
            previousRequest.headers.Authorization = `Bearer ${nextToken}`;
            return axiosPrivate(previousRequest);
          }
        }
        return Promise.reject(error);
      },
    );

    return () => {
      axiosPrivate.interceptors.request.eject(requestInterceptor);
      axiosPrivate.interceptors.response.eject(responseInterceptor);
    };
  }, [auth.accessToken, refreshAccessToken]);

  const contextValue = useMemo<AuthContextType>(
    () => ({
      auth,
      isAuthenticated: Boolean(auth.accessToken),
      isLoading,
      setAuth,
      updateTokens,
      login,
      register,
      refreshAccessToken,
      logout,
    }),
    [auth, isLoading, login, logout, refreshAccessToken, register, updateTokens],
  );

  return <AuthContext.Provider value={contextValue}>{children}</AuthContext.Provider>;
}
