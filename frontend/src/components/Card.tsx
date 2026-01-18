export function Card(props: { title: string; children: React.ReactNode }) {
    return (
        <div className="card">
            <div className="cardTitle">{props.title}</div>
            {props.children}
        </div>
    );
}
