import { useEffect, useRef } from "react";
import { ENDPOINTS } from "../api/endpoints";
import type { InvestmentCompletedEvent } from "../api/types";

export function useInvestmentCompletedSSE(
    onCompleted: (evt: InvestmentCompletedEvent) => void,
    enabled: boolean = true
) {
    const lastTokenRef = useRef<number>(0);
    const onCompletedRef = useRef(onCompleted);
    onCompletedRef.current = onCompleted;

    useEffect(() => {
        if (!enabled) return;

        let es: EventSource | null = null;
        let stopped = false;
        let retryTimer: number | null = null;
        let retries = 0;

        const connect = () => {
            if (stopped) return;

            es = new EventSource(ENDPOINTS.investmentCompletedStream);

            es.addEventListener("investmentCompleted", ((e: MessageEvent) => {
                const evt = JSON.parse(e.data) as InvestmentCompletedEvent;

                // ignore duplicates / out-of-order
                if (evt.Token <= lastTokenRef.current) return;
                lastTokenRef.current = evt.Token;

                onCompletedRef.current(evt);
            }) as EventListener);

            es.onopen = () => {
                retries = 0; // reset backoff after successful connect
            };

            es.onerror = () => {
                // Connection is broken (backend down, proxy reset, etc.)
                // Close and retry.
                es?.close();
                es = null;

                if (stopped) return;

                const delayMs = Math.min(3000, 300 + retries * 300);
                retries++;

                if (retryTimer) window.clearTimeout(retryTimer);
                retryTimer = window.setTimeout(connect, delayMs);
            };
        };

        connect();

        return () => {
            stopped = true;
            if (retryTimer) window.clearTimeout(retryTimer);
            es?.close();
        };
    }, [enabled]);
}