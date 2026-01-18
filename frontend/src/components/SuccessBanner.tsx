import { useEffect, useState } from "react";
import "../styles/base.css";

export function SuccessBanner(props: { message: string | null }) {
    const [isVisible, setIsVisible] = useState(false);

    useEffect(() => {
        if (props.message) {
            // Show the toast when message appears
            setIsVisible(true);

            // Auto-dismiss after 5 seconds
            const timer = setTimeout(() => {
                setIsVisible(false);
            }, 5000);

            return () => clearTimeout(timer);
        } else {
            // Hide immediately when message is cleared
            setIsVisible(false);
        }
    }, [props.message]);

    if (!props.message || !isVisible) return null;

    return (
        <div className="successToast">
            <div className="successToastContent">
                <svg
                    className="successIcon"
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
                        d="M6 10L9 13L14 7"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                    />
                </svg>
                <div className="successToastMessage">
                    {props.message}
                </div>
            </div>
        </div>
    );
}