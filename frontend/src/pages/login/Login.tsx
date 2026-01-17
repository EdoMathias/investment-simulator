export function Login(props: {
    name: string;
    setName: (v: string) => void;
    loading: boolean;
    onLogin: () => void;
}) {
    const { name, setName, loading, onLogin } = props;

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
                />

                <div className="loginActions">
                    <button className="btn btnPrimary" onClick={onLogin} disabled={loading}>
                        {loading ? "Logging in..." : "Login"}
                    </button>

                    <span className="loginNote">No password needed.</span>
                </div>
            </div>
        </div>
    );
}
