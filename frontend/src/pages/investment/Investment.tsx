import type {
  AccountState,
  InvestmentHistoryItem,
  InvestmentOption,
} from "../../api/types";

import { usePagination } from "../../hooks/usePagination";
import { useCountdown } from "../../hooks/useCountdown";

import { WelcomeCard } from "./components/WelcomeCard";
import { BalanceCard } from "./components/BalanceCard";
import { CurrentInvestmentsCard } from "./components/CurrentInvestmentsCard";
import { AvailableInvestmentsCard } from "./components/AvailableInvestmentsCard";
import { CompletedInvestmentsCard } from "./components/CompletedInvestmentsCard";

type Props = {
  state: AccountState | null;
  options: InvestmentOption[];
  history: InvestmentHistoryItem[];
  loading: boolean;
  onInvest: (optionId: string) => void;
};

export function Investment({ state, options, history, loading, onInvest }: Props) {
  const isLoggedIn = !!state?.userName;

  const { currentPage, totalPages, pageItems, next, prev } = usePagination(
    history,
    10,
  );

  const hasActive = !!state?.activeInvestments?.length;
  const nowMs = useCountdown(isLoggedIn && hasActive, state?.activeInvestments);

  return (
    <div className="pageConsole">
      <div className="gridTop">
        <WelcomeCard userName={state?.userName} />
        <BalanceCard balance={state?.balance} history={history} />
      </div>

      <div className="gridTwo">
        <CurrentInvestmentsCard
          isLoggedIn={isLoggedIn}
          activeInvestments={state?.activeInvestments}
          nowMs={nowMs}
        />

        <AvailableInvestmentsCard
          options={options}
          state={state}
          loading={loading}
          onInvest={onInvest}
        />
      </div>

      <div style={{ marginTop: 14 }}>
        <CompletedInvestmentsCard
          history={history}
          pageItems={pageItems}
          currentPage={currentPage}
          totalPages={totalPages}
          onPrev={prev}
          onNext={next}
        />
      </div>
    </div>
  );
}
