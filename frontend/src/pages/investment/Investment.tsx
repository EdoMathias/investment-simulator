import { useState, useEffect } from 'react';
import { Card } from '../../components/Card';
import { TableHeader } from '../../components/TableHeader';
import { TableCell } from '../../components/TableCell';
import type {
  AccountState,
  InvestmentHistoryItem,
  InvestmentOption,
} from '../../api/types';

export function Investment(props: {
  state: AccountState | null;
  options: InvestmentOption[];
  history: InvestmentHistoryItem[];
  loading: boolean;
  onInvest: (optionId: string) => void;
}) {
  const { state, options, history, loading, onInvest } = props;
  const isLoggedIn = !!state?.userName;

  // Update every second to refresh the timer
  const [now, setNow] = useState(Date.now());

  // Update immediately when investments change to avoid stale timestamps
  useEffect(() => {
    if (state?.activeInvestments?.length) {
      setNow(Date.now());
    }
  }, [state?.activeInvestments]);

  useEffect(() => {
    if (!isLoggedIn || !state?.activeInvestments?.length) return;

    const interval = setInterval(() => {
      setNow(Date.now());
    }, 1000);

    return () => clearInterval(interval);
  }, [isLoggedIn, state?.activeInvestments?.length]);

  return (
    <div className="pageConsole">
      <div className="gridTop">
        <Card title="Welcome">
          <div>
            {state?.userName ? `Hello, ${state.userName}!` : 'Loading user...'}
          </div>
        </Card>

        <Card title="Balance">
          <div className="kpiRow">
            <div className="kpi">${state ? state.balance.toFixed(2) : '—'}</div>
          </div>
          <div className="kpiSub">Updates on investment completion</div>
        </Card>
      </div>

      <div className="gridTwo">
        <Card title="Current investments">
          <div className="tableWrap">
            {!isLoggedIn ? (
              <div>Please login first.</div>
            ) : state?.activeInvestments?.length ? (
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
                  {state.activeInvestments.map((inv) => {
                    const endsInSec = Math.max(
                      0,
                      Math.ceil(
                        (new Date(inv.endTimeUtc).getTime() - now) /
                          1000,
                      ),
                    );

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
                    const canAfford =
                      (state?.balance ?? 0) >= opt.requiredAmount;

                    return (
                      <tr className="row" key={opt.id}>
                        <TableCell>{opt.name}</TableCell>
                        <TableCell>${opt.requiredAmount}</TableCell>
                        <TableCell>${opt.expectedReturn}</TableCell>
                        <TableCell>{opt.durationSeconds}s</TableCell>
                        <TableCell>
                          <button
                            className="btn"
                            disabled={loading || isActive || !canAfford}
                            onClick={() => onInvest(opt.id)}
                            title={
                              isActive
                                ? 'Already active'
                                : !canAfford
                                  ? 'Insufficient balance'
                                  : 'Invest'
                            }
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
      </div>

      <div style={{ marginTop: 14 }}>
        <Card title="Completed investments">
          {history.length ? (
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
                {history.map((h) => (
                  <tr className="row" key={h.id}>
                    <TableCell mono>{h.id.slice(0, 8)}…</TableCell>
                    <TableCell>{h.name}</TableCell>
                    <TableCell>${h.investedAmount}</TableCell>
                    <TableCell>${h.expectedReturn}</TableCell>
                    <TableCell>
                      {new Date(h.completedAtUtc).toLocaleString()}
                    </TableCell>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <div className="muted">No completed investments yet.</div>
          )}
        </Card>
      </div>
    </div>
  );
}
