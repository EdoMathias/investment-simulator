import { useEffect, useMemo, useState, useCallback } from "react";
import type { AccountState, ApiError, InvestmentHistoryItem } from "./api/types";
import { ENDPOINTS } from "./api/endpoints";
import { ErrorBanner } from "./components/ErrorBanner";
import { SuccessBanner } from "./components/SuccessBanner";
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
  const [stateError, setStateError] = useState<ApiError | null>(null);
  const [history, setHistory] = useState<InvestmentHistoryItem[]>([]);
  const [historyError, setHistoryError] = useState<ApiError | null>(null);

  // Success message state
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Fetch state function
  const fetchState = useCallback(async () => {
    try {
      const newState = await get<AccountState>(ENDPOINTS.state);
      setState(newState);
      setStateError(null);
    } catch (e: any) {
      const apiError: ApiError = e.apiError || {
        code: "FETCH_STATE_FAILED",
        message: e.message ?? "Failed to fetch account state"
      };
      setStateError(apiError);
    }
  }, []);

  // Fetch history function
  const fetchHistory = useCallback(async () => {
    try {
      const newHistory = await get<InvestmentHistoryItem[]>(ENDPOINTS.investmentHistory);
      setHistory(newHistory);
      setHistoryError(null);
    } catch (e: any) {
      const apiError: ApiError = e.apiError || {
        code: "FETCH_HISTORY_FAILED",
        message: e.message ?? "Failed to fetch investment history"
      };
      setHistoryError(apiError);
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
      // Store previous history to detect new completions
      const previousHistoryIds = new Set(history.map(h => h.id));

      // When an investment completes, refresh both state and history
      fetchState();
      fetchHistory().then(() => {
        // After fetching, compare to find the newly completed investment
        get<InvestmentHistoryItem[]>(ENDPOINTS.investmentHistory).then(newHistory => {
          const completedInvestment = newHistory.find(h => !previousHistoryIds.has(h.id));
          if (completedInvestment) {
            console.log("completedInvestment", completedInvestment);
            setSuccessMessage(
              `Investment "${completedInvestment.name}" completed! Return: $${completedInvestment.returnedAmount.toFixed(2)}`
            );
          }
        });
      });
    }, [fetchState, fetchHistory, history]),
    isInvestmentView && isAuthenticated
  );

  // Get investment options
  const { options, error: optionsError } = useInvestmentOptions(isInvestmentView);

  // Invest
  const { invest, loading: investLoading, error: investError } = useInvest();

  // Handle invest - refresh state after successful investment
  const handleInvest = useCallback(async (optionId: string) => {
    try {
      // Find the investment option name before investing
      const investmentOption = options?.find(opt => opt.id === optionId);
      const investmentName = investmentOption?.name || "Investment";

      await invest(optionId, (newState) => {
        // Update state immediately after investment
        setState(newState);
      });

      // Show success message
      setSuccessMessage(`Investment "${investmentName}" started successfully!`);

      // Also refresh state to ensure we have the latest data
      await fetchState();
    } catch {
      // Error is handled by useInvest hook and aggregated below
    }
  }, [invest, fetchState, options]);

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
      <SuccessBanner message={successMessage} />

      {view === "login" ? (
        <Login name={name} setName={setName} loading={authLoading} onLogin={handleLogin} />
      ) : (
        <Investment state={state} options={options || []} history={history} loading={investLoading} onInvest={handleInvest} />
      )}
    </div>
  );
}