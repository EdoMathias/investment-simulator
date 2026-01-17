export function TableCell(props: { children: React.ReactNode; mono?: boolean }) {
    return (
        <td
            style={{
                borderBottom: "1px solid #f1f1f1",
                padding: "8px 6px",
                fontFamily: props.mono ? "ui-monospace, SFMono-Regular, Menlo, monospace" : undefined,
                fontSize: 13,
            }}
        >
            {props.children}
        </td>
    );
}