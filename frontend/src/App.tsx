import { useEffect, useMemo, useState, useCallback } from "react";
import type { AccountState, InvestmentHistoryItem } from "./api/types";
import { ENDPOINTS } from "./api/endpoints";
import { ErrorBanner } from "./components/ErrorBanner";
import { Login } from "./pages/login/Login";
import { Investment } from "./pages/investment/Investment";
import { Header } from "./components/Header";
import { useAuth } from "./hooks/useAuth";
import { useInvestmentOptions } from "./hooks/useInvestmentOptions";
import { useInvest } from "./hooks/useInvest";
import { useInvestmentCompletedSSE } from "./hooks/useInvestmentCompletedSSE";
import { get } from "./api/http";

type View = "login" | "investment";

export default function App() {
  const [view, setView] = useState<View>("login");

  // Authentication state
  const { name, setName, isAuthenticated, loading: authLoading, error: authError, login, logout } = useAuth();

  // Investment view state
  const isInvestmentView = view === "investment";

  // State management
  const [state, setState] = useState<AccountState | null>(null);
  const [stateError, setStateError] = useState<string | null>(null);
  const [history, setHistory] = useState<InvestmentHistoryItem[]>([]);
  const [historyError, setHistoryError] = useState<string | null>(null);

  // Fetch state function
  const fetchState = useCallback(async () => {
    try {
      const newState = await get<AccountState>(ENDPOINTS.state);
      setState(newState);
      setStateError(null);
    } catch (e: any) {
      setStateError(e.message ?? "Failed to fetch account state");
    }
  }, []);

  // Fetch history function
  const fetchHistory = useCallback(async () => {
    try {
      const newHistory = await get<InvestmentHistoryItem[]>(ENDPOINTS.investmentHistory);
      setHistory(newHistory);
      setHistoryError(null);
    } catch (e: any) {
      setHistoryError(e.message ?? "Failed to fetch investment history");
    }
  }, []);

  // Initial state and history fetch when entering investment view
  useEffect(() => {
    if (isInvestmentView && isAuthenticated) {
      fetchState();
      fetchHistory();
    } else {
      setState(null);
      setStateError(null);
      setHistory([]);
      setHistoryError(null);
    }
  }, [isInvestmentView, isAuthenticated, fetchState, fetchHistory]);

  // Use SSE to detect investment completions and refresh state/history
  useInvestmentCompletedSSE(
    useCallback(() => {
      // When an investment completes, refresh both state and history
      fetchState();
      fetchHistory();
    }, [fetchState, fetchHistory]),
    isInvestmentView && isAuthenticated
  );

  // Get investment options
  const { options, error: optionsError } = useInvestmentOptions(isInvestmentView);

  // Invest
  const { invest, loading: investLoading, error: investError } = useInvest();

  // Handle invest - refresh state after successful investment
  const handleInvest = useCallback(async (optionId: string) => {
    try {
      await invest(optionId, (newState) => {
        // Update state immediately after investment
        setState(newState);
      });
      // Also refresh state to ensure we have the latest data
      await fetchState();
    } catch {
      // Error is handled by useInvest hook and aggregated below
    }
  }, [invest, fetchState]);

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
        <Investment state={state} options={options || []} history={history} loading={investLoading} onInvest={handleInvest} />
      )}
    </div>
  );
}