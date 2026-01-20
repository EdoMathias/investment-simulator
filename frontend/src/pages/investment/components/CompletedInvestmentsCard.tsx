import { Card } from "../../../components/Card";
import { TableHeader } from "../../../components/TableHeader";
import { TableCell } from "../../../components/TableCell";
import type { InvestmentHistoryItem } from "../../../api/types";

export function CompletedInvestmentsCard(props: {
    history: InvestmentHistoryItem[];
    pageItems: InvestmentHistoryItem[];
    currentPage: number;
    totalPages: number;
    onPrev: () => void;
    onNext: () => void;
}) {
    const { history, pageItems, currentPage, totalPages, onPrev, onNext } = props;

    return (
        <Card title="Completed investments">
            {history.length ? (
                <>
                    <div className="tableWrap">
                        <table className="table">
                            <thead>
                                <tr>
                                    <TableHeader>ID</TableHeader>
                                    <TableHeader>Name</TableHeader>
                                    <TableHeader>Invested</TableHeader>
                                    <TableHeader>Return</TableHeader>
                                    <TableHeader>Completed</TableHeader>
                                </tr>
                            </thead>
                            <tbody>
                                {pageItems.map((h) => (
                                    <tr className="row" key={h.id}>
                                        <TableCell mono>{h.id}</TableCell>
                                        <TableCell>{h.name}</TableCell>
                                        <TableCell>${h.investedAmount}</TableCell>
                                        <TableCell>${h.returnedAmount}</TableCell>
                                        <TableCell>{new Date(h.completedAtUtc).toLocaleString()}</TableCell>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>

                    {totalPages > 1 && (
                        <div className="pagination">
                            <button className="btn" onClick={onPrev} disabled={currentPage === 1}>
                                Previous
                            </button>

                            <div className="paginationInfo">
                                Page {currentPage} of {totalPages}
                                <span className="paginationCount">({history.length} total)</span>
                            </div>

                            <button
                                className="btn"
                                onClick={onNext}
                                disabled={currentPage === totalPages}
                            >
                                Next
                            </button>
                        </div>
                    )}
                </>
            ) : (
                <div className="muted">No completed investments yet.</div>
            )}
        </Card>
    );
}
