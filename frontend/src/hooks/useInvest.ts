import { useState } from "react";
import { post } from "../api/http";
import { ENDPOINTS } from "../api/endpoints";
import type { ApiError, InvestmentResponse } from "../api/types";

/**
 * Hook to invest in an option
 */
export function useInvest() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<ApiError | null>(null);

    const invest = async (optionId: string) => {
        setError(null);
        setLoading(true);
        try {
            const response = await post<InvestmentResponse, { optionId: string }>(
                ENDPOINTS.invest,
                { optionId }
            );

            return { response };
        } catch (e: any) {
            const apiError: ApiError =
                e.apiError || { code: "INVEST_FAILED", message: e.message ?? "Invest failed" };
            setError(apiError);
            throw e;
        } finally {
            setLoading(false);
        }
    };

    return { invest, loading, error };
}