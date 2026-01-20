import { useCallback, useState } from "react";
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

    const validateUserName = (userName: string) => {
        if (!userName) {
            return "User name is required";
        }
        if (userName.length < 3) {
            return "User name must be at least 3 characters long";
        }
        if (userName.length > 20) {
            return "User name must be less than 20 characters long";
        }
        if (!/^[a-zA-Z]+$/.test(userName)) {
            return "User name must contain only English letters";
        }
        return null;
    };


    const login = useCallback(async () => {
        if (loading) return;

        setError(null);

        const validationMessage = validateUserName(name);
        if (validationMessage) {
            const apiError: ApiError = { code: "INVALID_USERNAME", message: validationMessage };
            setError(apiError);
            throw apiError;
        }

        const userName = name.trim();
        const body: LoginRequest = { userName };

        setLoading(true);
        try {
            const response = await post<{ message: string }, LoginRequest>(ENDPOINTS.login, body);
            setIsAuthenticated(true);
            return response;
        } catch (e: any) {
            const apiError: ApiError = e.apiError || {
                code: "LOGIN_FAILED",
                message: e.message ?? "Login failed",
            };
            setError(apiError);
            throw apiError;
        } finally {
            setLoading(false);
        }
    }, [name, loading]);

    const logout = useCallback(async () => {
        if (loading) return;

        setError(null);
        setLoading(true);
        try {
            await post(ENDPOINTS.logout, {});
            setIsAuthenticated(false);
            setName("");
        } catch (e: any) {
            const apiError: ApiError = e.apiError || {
                code: "LOGOUT_FAILED",
                message: e.message ?? "Logout failed",
            };
            setError(apiError);
            throw apiError;
        } finally {
            setLoading(false);
        }
    }, [loading]);

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