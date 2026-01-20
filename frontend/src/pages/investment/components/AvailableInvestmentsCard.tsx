import { Card } from "../../../components/Card";
import { TableHeader } from "../../../components/TableHeader";
import { TableCell } from "../../../components/TableCell";
import type { AccountState, InvestmentOption } from "../../../api/types";

export function AvailableInvestmentsCard(props: {
    options: InvestmentOption[];
    state: AccountState | null;
    loading: boolean;
    onInvest: (optionId: string) => void;
}) {
    const { options, state, loading, onInvest } = props;

    return (
        <Card title="Available investments">
            <div className="tableWrap">
                {options.length ? (
                    <table className="table">
                        <thead>
                            <tr>
                                <TableHeader>Name</TableHeader>
                                <TableHeader>Required</TableHeader>
                                <TableHeader>Return</TableHeader>
                                <TableHeader>Duration</TableHeader>
                                <TableHeader>Action</TableHeader>
                            </tr>
                        </thead>
                        <tbody>
                            {options.map((opt) => {
                                const isActive = !!state?.activeInvestments?.some(
                                    (a) => a.optionId === opt.id,
                                );
                                const canAfford = (state?.balance ?? 0) >= opt.requiredAmount;

                                const disabled = loading || isActive || !canAfford;
                                const title = isActive
                                    ? "Already active"
                                    : !canAfford
                                        ? "Insufficient balance"
                                        : "Invest";

                                return (
                                    <tr className="row" key={opt.id}>
                                        <TableCell>{opt.name}</TableCell>
                                        <TableCell>${opt.requiredAmount}</TableCell>
                                        <TableCell>${opt.expectedReturn}</TableCell>
                                        <TableCell>{opt.durationSeconds}s</TableCell>
                                        <TableCell>
                                            <button
                                                className="btn"
                                                disabled={disabled}
                                                onClick={() => onInvest(opt.id)}
                                                title={title}
                                            >
                                                Invest
                                            </button>
                                        </TableCell>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                ) : (
                    <div>Loading options…</div>
                )}
            </div>
        </Card>
    );
}
