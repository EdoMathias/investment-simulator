import { useEffect } from "react";
import { ENDPOINTS } from "../api/endpoints";

export function useInvestmentCompletedSSE(
    onCompleted: () => void,
    enabled: boolean = true
) {
    useEffect(() => {
        if (!enabled) return;

        const es = new EventSource(ENDPOINTS.investmentCompletedStream);

        const handler = () => onCompleted();

        es.addEventListener("investmentCompleted", handler);

        return () => {
            es.removeEventListener("investmentCompleted", handler);
            es.close();
        };
    }, [onCompleted, enabled]);
}