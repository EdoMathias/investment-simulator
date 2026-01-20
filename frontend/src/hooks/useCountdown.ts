import { useEffect, useState } from "react";

/**
 * Countdown hook
 * @param enabled - Whether the countdown is enabled
 * @param bumpKey - Key to bump the countdown
 */
export function useCountdown(enabled: boolean, bumpKey?: unknown) {
    const [nowMs, setNowMs] = useState(() => Date.now());

    useEffect(() => {
        if (enabled) setNowMs(Date.now());
    }, [enabled, bumpKey]);

    useEffect(() => {
        if (!enabled) return;

        const id = window.setInterval(() => setNowMs(Date.now()), 1000);
        return () => window.clearInterval(id);
    }, [enabled]);

    return nowMs;
}