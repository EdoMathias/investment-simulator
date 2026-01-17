export function Card(props: { title: string; children: React.ReactNode }) {
    return (
        <div style={{ border: "1px solid #ddd", borderRadius: 12, padding: 14, minWidth: 260, flex: 1 }}>
            <div style={{ fontWeight: 700, marginBottom: 10 }}>{props.title}</div>
            {props.children}
        </div>
    );
}
