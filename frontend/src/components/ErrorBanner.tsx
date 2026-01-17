export function ErrorBanner(props: { error: string | null }) {
    if (!props.error) return null;

    return (
        <div style={{ background: "red", color: "white", padding: 10, borderRadius: 8, marginBottom: 12 }}>
            <strong>Error:</strong> {props.error}
        </div>
    );
}