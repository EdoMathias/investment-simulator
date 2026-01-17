export function TableHeader(props: { children: React.ReactNode }) {
    return (
        <th style={{ textAlign: "left", borderBottom: "1px solid #eee", padding: "8px 6px", fontSize: 12 }}>
            {props.children}
        </th>
    );
}