export function Login(props: {
    name: string;
    setName: (v: string) => void;
    loading: boolean;
    onLogin: () => void;
}) {
    const { name, setName, loading, onLogin } = props;

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter') {
            onLogin();
        }
    };

    return (
        <div className="pageLogin">
            <div className="card loginPanel">
                <h3 className="loginTitle">Welcome</h3>
                <p className="loginHint">Enter your name to continue.</p>

                <input
                    className="input"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="Your name (English letters, min 3)"
                    onKeyDown={handleKeyDown}
                />

                <div className="loginActions">
                    <button className="btn btnPrimary" onClick={onLogin} disabled={loading}>
                        {loading ? "Logging in..." : "Login"}
                    </button>
                </div>
            </div>
        </div>
    );
}
