import { useEffect, useMemo, useState } from "react";
import type { AccountState, InvestmentHistoryItem } from "./api/types";
import { ENDPOINTS } from "./api/endpoints";
import { ErrorBanner } from "./components/ErrorBanner";
import { Login } from "./pages/login/Login";
import { Investment } from "./pages/investment/Investment";
import { Header } from "./components/Header";
import { usePolling } from "./hooks/usePolling";
import { useAuth } from "./hooks/useAuth";
import { useInvestmentOptions } from "./hooks/useInvestmentOptions";
import { useInvest } from "./hooks/useInvest";

type View = "login" | "investment";

export default function App() {
  const [view, setView] = useState<View>("login");

  // Authentication state
  const { name, setName, isAuthenticated, loading: authLoading, error: authError, login, logout } = useAuth();

  // Investment view state
  const isInvestmentView = view === "investment";

  // Poll state every 1s on investment view
  const { data: state, error: stateError } = usePolling<AccountState>(
    ENDPOINTS.state,
    isInvestmentView,
    1000
  );

  // Poll history every 3s on investment view
  const { data: history, error: historyError } = usePolling<InvestmentHistoryItem[]>(
    ENDPOINTS.investmentHistory,
    isInvestmentView,
    3000
  );

  // Get investment options
  const { options, error: optionsError } = useInvestmentOptions(isInvestmentView);

  // Invest
  const { invest, loading: investLoading, error: investError } = useInvest();

  // Set default theme to dark
  useEffect(() => {
    document.documentElement.setAttribute('data-theme', 'dark');
  }, []);

  // Update view when authentication state changes
  useEffect(() => {
    if (isAuthenticated) {
      setView("investment");
    } else if (!isAuthenticated && view === "investment") {
      setView("login");
    }
  }, [isAuthenticated]);

  // Handle login
  async function handleLogin() {
    try {
      await login();
    } catch {
      // Error is handled by useAuth hook and aggregated below
    }
  }

  // Handle logout
  function handleLogout() {
    logout();
    setView("login");
  }

  // Aggregate all errors - show the first error that occurs
  const error = useMemo(() => {
    return authError || stateError || historyError || optionsError || investError || null;
  }, [authError, stateError, historyError, optionsError, investError]);

  return (
    <div className="container">
      {view === "investment" && (
        <Header
          userName={state?.userName || null}
          onLogout={handleLogout}
        />
      )}

      <ErrorBanner error={error} />

      {view === "login" ? (
        <Login name={name} setName={setName} loading={authLoading} onLogin={handleLogin} />
      ) : (
        <Investment state={state} options={options || []} history={history || []} loading={investLoading} onInvest={invest} />
      )}
    </div>
  );
}