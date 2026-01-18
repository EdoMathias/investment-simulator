import { useEffect, useState } from "react";
import { get } from "../api/http";

/**
 * Hook to poll an endpoint at a given interval
 * @param endpoint - The endpoint to poll
 * @param enabled - Whether to poll the endpoint
 * @param interval - The interval in milliseconds to poll the endpoint
 * @returns The data and error from the endpoint
 */
export function usePolling<T>(
    endpoint: string,
    enabled: boolean,
    interval: number = 1000
) {
    const [data, setData] = useState<T | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!enabled) return;

        let cancelled = false;

        const tick = async () => {
            try {
                const result = await get<T>(endpoint);
                if (!cancelled) {
                    setData(result);
                    setError(null);
                }
            } catch (e: any) {
                if (!cancelled) {
                    setError(e.message ?? "Failed to fetch data");
                }
            }
        };

        tick();
        const id = window.setInterval(tick, interval);

        return () => {
            cancelled = true;
            window.clearInterval(id);
        };
    }, [enabled, endpoint, interval]);

    return { data, error };
}