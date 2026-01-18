import { useEffect, useState } from "react";
import type { ApiError } from "../api/types";
import "../styles/base.css";

export function ErrorBanner(props: { error: ApiError | null }) {
    const [isVisible, setIsVisible] = useState(false);

    useEffect(() => {
        if (props.error) {
            // Show the toast when error appears
            setIsVisible(true);

            // Auto-dismiss after 5 seconds
            const timer = setTimeout(() => {
                setIsVisible(false);
            }, 5000);

            return () => clearTimeout(timer);
        } else {
            // Hide immediately when error is cleared
            setIsVisible(false);
        }
    }, [props.error]);

    if (!props.error || !isVisible) return null;

    return (
        <div className="errorToast">
            <div className="errorToastContent">
                <svg
                    className="errorIcon"
                    width="20"
                    height="20"
                    viewBox="0 0 20 20"
                    fill="none"
                    xmlns="http://www.w3.org/2000/svg"
                >
                    <path
                        d="M10 18C14.4183 18 18 14.4183 18 10C18 5.58172 14.4183 2 10 2C5.58172 2 2 5.58172 2 10C2 14.4183 5.58172 18 10 18Z"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                    />
                    <path
                        d="M10 6V10"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                    />
                    <path
                        d="M10 14H10.01"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                    />
                </svg>
                <div className="errorToastMessage">
                    <strong>Error:</strong> {props.error.message}
                </div>
            </div>
        </div>
    );
}