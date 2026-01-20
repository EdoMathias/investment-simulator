import { useMemo } from "react";
import { Card } from "../../../components/Card";
import type { InvestmentHistoryItem } from "../../../api/types";

function formatMoney(amount: number | null | undefined) {
    if (amount === null || amount === undefined) return "—";
    return `$${amount.toFixed(2)}`;
}

function getLastUpdatedLabel(history: InvestmentHistoryItem[]) {
    const iso = history[0]?.completedAtUtc ?? new Date().toISOString();
    return new Date(iso).toLocaleString();
}

export function BalanceCard(props: {
    balance?: number | null;
    history: InvestmentHistoryItem[];
}) {
    const { balance, history } = props;

    const lastUpdatedLabel = useMemo(() => getLastUpdatedLabel(history), [history]);

    return (
        <Card title="Balance">
            <div className="kpiRow">
                <div className="kpi">{formatMoney(balance)}</div>
            </div>
            <div className="kpiSub">Last updated on {lastUpdatedLabel}</div>
        </Card>
    );
}
