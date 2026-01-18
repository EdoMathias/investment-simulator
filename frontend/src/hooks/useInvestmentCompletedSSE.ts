import { useEffect, useRef } from "react";
import { ENDPOINTS } from "../api/endpoints";
import type { InvestmentCompletedEvent } from "../api/types";

export function useInvestmentCompletedSSE(
    onCompleted: (evt: InvestmentCompletedEvent) => void,
    enabled: boolean = true
) {
    const lastTokenRef = useRef<number>(0);

    useEffect(() => {
        if (!enabled) return;

        const es = new EventSource(ENDPOINTS.investmentCompletedStream);

        const handler = (e: MessageEvent) => {
            // data is JSON string from the server
            const evt = JSON.parse(e.data) as InvestmentCompletedEvent;

            // ignore duplicates / out-of-order
            if (evt.Token <= lastTokenRef.current) return;
            lastTokenRef.current = evt.Token;

            onCompleted(evt);
        };

        es.addEventListener("investmentCompleted", handler as EventListener);

        return () => {
            es.removeEventListener("investmentCompleted", handler as EventListener);
            es.close();
        };
    }, [onCompleted, enabled]);
}