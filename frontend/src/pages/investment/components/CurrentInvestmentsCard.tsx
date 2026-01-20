import { Card } from "../../../components/Card";
import { TableHeader } from "../../../components/TableHeader";
import { TableCell } from "../../../components/TableCell";
import type { AccountState } from "../../../api/types";

function secondsUntil(endTimeUtc: string, nowMs: number) {
    const diffMs = new Date(endTimeUtc).getTime() - nowMs;
    return Math.max(0, Math.ceil(diffMs / 1000));
}

export function CurrentInvestmentsCard(props: {
    isLoggedIn: boolean;
    activeInvestments: AccountState["activeInvestments"] | undefined;
    nowMs: number;
}) {
    const { isLoggedIn, activeInvestments, nowMs } = props;

    return (
        <Card title="Current investments">
            <div className="tableWrap">
                {!isLoggedIn ? (
                    <div>Please login first.</div>
                ) : activeInvestments?.length ? (
                    <table className="table">
                        <thead>
                            <tr>
                                <TableHeader>ID</TableHeader>
                                <TableHeader>Name</TableHeader>
                                <TableHeader>Invested</TableHeader>
                                <TableHeader>Return</TableHeader>
                                <TableHeader>Ends in</TableHeader>
                            </tr>
                        </thead>
                        <tbody>
                            {activeInvestments.map((inv) => {
                                const endsInSec = secondsUntil(inv.endTimeUtc, nowMs);

                                return (
                                    <tr className="row" key={inv.id}>
                                        <TableCell mono>{inv.id.slice(0, 8)}…</TableCell>
                                        <TableCell>{inv.name}</TableCell>
                                        <TableCell>${inv.investedAmount}</TableCell>
                                        <TableCell>${inv.expectedReturn}</TableCell>
                                        <TableCell>{endsInSec}s</TableCell>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                ) : (
                    <div>No active investments.</div>
                )}
            </div>
        </Card>
    );
}
