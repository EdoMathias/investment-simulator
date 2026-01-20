import { useCallback, useEffect, useMemo, useState } from "react";
import type {
    AccountState,
    ApiError,
    InvestmentCompletedEvent,
    InvestmentHistoryItem,
    ActiveInvestment,
} from "../api/types";
import { ENDPOINTS } from "../api/endpoints";
import { get } from "../api/http";
import { useInvestmentCompletedSSE } from "./useInvestmentCompletedSSE";

export function useAccountData(enabled: boolean) {
    const [accountState, setAccountState] = useState<AccountState | null>(null);
    const [stateError, setStateError] = useState<ApiError | null>(null);

    const [history, setHistory] = useState<InvestmentHistoryItem[]>([]);
    const [historyError, setHistoryError] = useState<ApiError | null>(null);

    const refreshState = useCallback(async () => {
        try {
            const newState = await get<AccountState>(ENDPOINTS.state);
            setAccountState(newState);
            setStateError(null);
            return newState;
        } catch (e: any) {
            const apiError: ApiError = e.apiError || {
                code: "FETCH_STATE_FAILED",
                message: e.message ?? "Failed to fetch account state",
            };
            setStateError(apiError);
            throw e;
        }
    }, []);

    const refreshHistory = useCallback(async () => {
        try {
            const newHistory = await get<InvestmentHistoryItem[]>(ENDPOINTS.investmentHistory);
            setHistory(newHistory);
            setHistoryError(null);
            return newHistory;
        } catch (e: any) {
            const apiError: ApiError = e.apiError || {
                code: "FETCH_HISTORY_FAILED",
                message: e.message ?? "Failed to fetch investment history",
            };
            setHistoryError(apiError);
            throw e;
        }
    }, []);

    const refreshAll = useCallback(async () => {
        await Promise.all([refreshState(), refreshHistory()]);
    }, [refreshState, refreshHistory]);

    // Initial load + cleanup when enabled toggles
    useEffect(() => {
        if (enabled) {
            refreshAll();
        } else {
            setAccountState(null);
            setStateError(null);
            setHistory([]);
            setHistoryError(null);
        }
    }, [enabled, refreshAll]);

    // use SSE hook to update state/history when an investment completes
    useInvestmentCompletedSSE(
        (evt: InvestmentCompletedEvent) => {
            setAccountState((prev: AccountState | null) => {
                if (!prev) return prev;

                return {
                    ...prev,
                    balance: evt.BalanceAfter,
                    activeInvestments: prev.activeInvestments.filter(
                        (x: ActiveInvestment) => x.id !== evt.InvestmentId
                    ),
                };
            });

            setHistory((prev: InvestmentHistoryItem[]) => {
                if (prev.some((x: InvestmentHistoryItem) => x.id === evt.InvestmentId)) return prev;

                return [
                    {
                        id: evt.InvestmentId,
                        optionId: evt.OptionId,
                        name: evt.Name,
                        investedAmount: evt.InvestedAmount,
                        returnedAmount: evt.ReturnedAmount,
                        completedAtUtc: evt.CompletedAtUtc,
                        startTimeUtc: evt.StartTimeUtc,
                        endTimeUtc: evt.EndTimeUtc,
                    },
                    ...prev,
                ];
            });
        },
        enabled
    );

    return useMemo(
        () => ({
            accountState,
            history,
            stateError,
            historyError,
            refreshState,
            refreshHistory,
            refreshAll,
        }),
        [
            accountState,
            history,
            stateError,
            historyError,
            refreshState,
            refreshHistory,
            refreshAll,
        ]
    );
}
