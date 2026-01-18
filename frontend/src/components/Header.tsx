import { useState, useEffect, useRef } from 'react';

export function Header(props: {
    userName: string | null;
    onLogout: () => void;
}) {
    const { userName, onLogout } = props;
    const [isDropdownOpen, setIsDropdownOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    // Get first letter of username, or '?' if no username
    const userInitial = userName ? userName.charAt(0).toUpperCase() : '?';

    // Close dropdown when clicking outside
    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setIsDropdownOpen(false);
            }
        }

        if (isDropdownOpen) {
            document.addEventListener('mousedown', handleClickOutside);
        }

        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, [isDropdownOpen]);

    return (
        <header className="app-header">
            <div className="app-header-content">
                <h1 className="app-header-title">Investor</h1>
                <div className="app-header-actions">
                    <div className="profile-dropdown" ref={dropdownRef}>
                        <button
                            className="profile-button"
                            onClick={() => setIsDropdownOpen(!isDropdownOpen)}
                            aria-label="Profile menu"
                            title={userName || 'User'}
                        >
                            {userInitial}
                        </button>
                        {isDropdownOpen && (
                            <div className="profile-dropdown-menu">
                                <div className="profile-dropdown-item profile-dropdown-user">
                                    {userName || 'User'}
                                </div>
                                <button
                                    className="profile-dropdown-item profile-dropdown-button"
                                    onClick={() => {
                                        onLogout();
                                        setIsDropdownOpen(false);
                                    }}
                                >
                                    Logout
                                </button>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </header>
    );
}