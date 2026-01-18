import { useState } from "react";
import { get, post } from "../api/http";
import { ENDPOINTS } from "../api/endpoints";
import type { AccountState, ApiError, InvestmentHistoryItem } from "../api/types";

/**
 * Hook to invest in an option
 */
export function useInvest() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<ApiError | null>(null);

    const invest = async (
        optionId: string,
        onSuccess?: (state: AccountState, history: InvestmentHistoryItem[]) => void
    ) => {
        setError(null);
        setLoading(true);
        try {
            await post(ENDPOINTS.invest, { optionId });

            // Refresh state and history to get the new investment
            const [state, history] = await Promise.all([
                get<AccountState>(ENDPOINTS.state),
                get<InvestmentHistoryItem[]>(ENDPOINTS.investmentHistory),
            ]);

            if (onSuccess) {
                onSuccess(state, history);
            }

            return { state, history };
        } catch (e: any) {
            const apiError: ApiError = e.apiError || {
                code: "INVEST_FAILED",
                message: e.message ?? "Invest failed"
            };
            setError(apiError);
            throw e;
        } finally {
            setLoading(false);
        }
    };

    return { invest, loading, error };
}