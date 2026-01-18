import { useEffect, useState } from "react";
import { get } from "../api/http";
import { ENDPOINTS } from "../api/endpoints";
import type { ApiError, InvestmentOption } from "../api/types";

/**
 * Hook to get investment options
 * @param enabled - Whether to get the investment options if on investment view
 * @returns The investment options and error
 */
export function useInvestmentOptions(enabled: boolean) {
    const [options, setOptions] = useState<InvestmentOption[]>([]);
    const [error, setError] = useState<ApiError | null>(null);

    useEffect(() => {
        if (!enabled) return;

        let cancelled = false;

        (async () => {
            try {
                const opts = await get<InvestmentOption[]>(ENDPOINTS.investmentOptions);
                if (!cancelled) {
                    setOptions(opts);
                    setError(null);
                }
            } catch (e: any) {
                if (!cancelled) {
                    const apiError: ApiError = e.apiError || {
                        code: "FETCH_OPTIONS_FAILED",
                        message: e.message ?? "Failed to fetch options"
                    };
                    setError(apiError);
                }
            }
        })();

        return () => {
            cancelled = true;
        };
    }, [enabled]);

    return { options, error };
}