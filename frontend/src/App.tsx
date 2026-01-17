import { useEffect, useState } from "react";
import { get, post } from "./api/http";
import type { AccountState, InvestmentHistoryItem, InvestmentOption, LoginRequest } from "./api/types";
import { ENDPOINTS } from "./api/endpoints";
import { ErrorBanner } from "./components/ErrorBanner";
import { Login } from "./pages/login/Login";
import { Investment } from "./pages/investment/Investment";

type View = "login" | "investment";

export default function App() {
  const [view, setView] = useState<View>("login");
  const [name, setName] = useState("");
  const [error, setError] = useState<string | null>(null);

  const [state, setState] = useState<AccountState | null>(null);
  const [options, setOptions] = useState<InvestmentOption[]>([]);
  const [history, setHistory] = useState<InvestmentHistoryItem[]>([]);
  const [loading, setLoading] = useState(false);

  // Poll account state every 1s on console view
  useEffect(() => {
    if (view !== "investment") return;

    let cancelled = false;

    const tick = async () => {
      try {
        const s = await get<AccountState>(ENDPOINTS.state);
        if (!cancelled) setState(s);
      } catch (e: any) {
        if (!cancelled) setError(e.message ?? "Failed to fetch state");
      }
    };

    tick();
    const id = window.setInterval(tick, 1000);

    return () => {
      cancelled = true;
      window.clearInterval(id);
    };
  }, [view]);

  // Load options once on entering console
  useEffect(() => {
    if (view !== "investment") return;

    let cancelled = false;

    (async () => {
      try {
        const opts = await get<InvestmentOption[]>(ENDPOINTS.investmentOptions);
        if (!cancelled) setOptions(opts);
      } catch (e: any) {
        if (!cancelled) setError(e.message ?? "Failed to fetch options");
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [view]);

  // Load and poll history every 1s on investment view
  useEffect(() => {
    if (view !== "investment") return;

    let cancelled = false;

    const tick = async () => {
      try {
        const h = await get<InvestmentHistoryItem[]>(ENDPOINTS.investmentHistory);
        if (!cancelled) setHistory(h);
      } catch (e: any) {
        if (!cancelled) setError(e.message ?? "Failed to fetch history");
      }
    };

    tick();
    const id = window.setInterval(tick, 3000); // every 3s is enough

    return () => {
      cancelled = true;
      window.clearInterval(id);
    };
  }, [view]);

  async function onLogin() {
    setError(null);
    setLoading(true);
    try {
      const body: LoginRequest = { userName: name };
      await post<{ message: string }, LoginRequest>(ENDPOINTS.login, body);
      setView("investment");
    } catch (e: any) {
      setError(e.message ?? "Login failed");
    } finally {
      setLoading(false);
    }
  }

  async function onInvest(optionId: string) {
    setError(null);
    setLoading(true);
    try {
      await post(ENDPOINTS.invest, { optionId });
      // Refresh state to get the new investment
      const s = await get<AccountState>(ENDPOINTS.state);

      // Refresh history to get the new investment
      const h = await get<InvestmentHistoryItem[]>(ENDPOINTS.investmentHistory);
      setHistory(h);
      setState(s);
    } catch (e: any) {
      setError(e.message ?? "Invest failed");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={{ fontFamily: "system-ui, Arial", padding: 16, maxWidth: 980, margin: "0 auto" }}>
      <h2>Investments Simulator</h2>

      <ErrorBanner error={error} />

      {view === "login" ? (
        <Login name={name} setName={setName} loading={loading} onLogin={onLogin} />
      ) : (
        <Investment state={state} options={options} history={history} loading={loading} onInvest={onInvest} />
      )}
    </div>
  );
}