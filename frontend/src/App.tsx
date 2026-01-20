import { useEffect, useMemo, useState, useCallback } from "react";
import type { ApiError } from "./api/types";
import { ErrorBanner } from "./components/ErrorBanner";
import { SuccessBanner } from "./components/SuccessBanner";
import { Login } from "./pages/login/Login";
import { Investment } from "./pages/investment/Investment";
import { Header } from "./components/Header";
import { useAuth } from "./hooks/useAuth";
import { useInvestmentOptions } from "./hooks/useInvestmentOptions";
import { useInvest } from "./hooks/useInvest";
import { useAccountData } from "./hooks/useAccountData";

type View = "login" | "investment";

export default function App() {
  const [view, setView] = useState<View>("login");

  // Authentication hook
  const {
    name,
    setName,
    isAuthenticated,
    loading: authLoading,
    error: authError,
    login,
    logout,
  } = useAuth();

  const isInvestmentView = view === "investment";

  // Account data hook
  const {
    accountState,
    history,
    stateError,
    historyError,
    refreshState,
  } = useAccountData(isInvestmentView && isAuthenticated);

  // Investment options hook
  const { options, error: optionsError } = useInvestmentOptions(isInvestmentView);

  // Invest hook
  const { invest, loading: investLoading, error: investError } = useInvest();

  // Success message
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Handle invest - refresh state after successful investment
  const handleInvest = useCallback(
    async (optionId: string) => {
      try {
        const {
          response: { message },
        } = await invest(optionId);

        setSuccessMessage(message);

        await refreshState();
      } catch {
        // Error is handled by useInvest hook and aggregated below
      }
    },
    [invest, refreshState]
  );

  // Switch view based on authentication state
  useEffect(() => {
    if (isAuthenticated) {
      setView("investment");
    } else if (!isAuthenticated && view === "investment") {
      setView("login");
    }
  }, [isAuthenticated, view]);

  async function handleLogin() {
    try {
      const response = await login();
      if (response) {
        setSuccessMessage(response.message);
      }
    } catch {
      // Error is handled by useAuth hook and aggregated below
    }
  }

  function handleLogout() {
    logout();
    setView("login");
  }

  // Aggregate all errors - shows the first error that occurs
  const error: ApiError | null = useMemo(() => {
    return authError || stateError || historyError || optionsError || investError || null;
  }, [authError, stateError, historyError, optionsError, investError]);

  return (
    <div className="container">
      {view === "investment" && (
        <Header userName={accountState?.userName || null} onLogout={handleLogout} />
      )}

      <ErrorBanner error={error} />
      <SuccessBanner message={successMessage} />

      {view === "login" ? (
        <Login name={name} setName={setName} loading={authLoading} onLogin={handleLogin} />
      ) : (
        <Investment
          state={accountState}
          options={options || []}
          history={history}
          loading={investLoading}
          onInvest={handleInvest}
        />
      )}
    </div>
  );
}