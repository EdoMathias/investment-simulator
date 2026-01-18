import { useState } from "react";
import { post } from "../api/http";
import { ENDPOINTS } from "../api/endpoints";
import type { ApiError, LoginRequest } from "../api/types";

/**
 * Hook to manage authentication state
 * @returns The authentication state
 */
export function useAuth() {
    const [name, setName] = useState("");
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<ApiError | null>(null);

    const login = async () => {
        setError(null);
        setLoading(true);
        try {
            const body: LoginRequest = { userName: name };
            await post<{ message: string }, LoginRequest>(ENDPOINTS.login, body);
            setIsAuthenticated(true);
        } catch (e: any) {
            const apiError: ApiError = e.apiError || {
                code: "LOGIN_FAILED",
                message: e.message ?? "Login failed"
            };
            setError(apiError);
            throw e;
        } finally {
            setLoading(false);
        }
    };

    const logout = async () => {
        setError(null);
        setLoading(true);
        try {
            await post(ENDPOINTS.logout, {});
            setIsAuthenticated(false);
            setName("");
            setError(null);
        } catch (e: any) {
            const apiError: ApiError = e.apiError || {
                code: "LOGOUT_FAILED",
                message: e.message ?? "Logout failed"
            };
            setError(apiError);
            throw e;
        } finally {
            setLoading(false);
        }
    };

    return {
        name,
        setName,
        isAuthenticated,
        loading,
        error,
        login,
        logout,
    };
}