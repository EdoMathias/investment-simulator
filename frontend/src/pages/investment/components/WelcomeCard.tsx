import { Card } from "../../../components/Card";

export function WelcomeCard({ userName }: { userName?: string | null }) {
    return (
        <Card title="Welcome">
            <div>{userName ? `Hello, ${userName}!` : "Loading user..."}</div>
        </Card>
    );
}