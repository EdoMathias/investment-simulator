import { useCallback, useEffect, useMemo, useState } from "react";

export function usePagination<T>(items: readonly T[], itemsPerPage: number) {
    const [currentPage, setCurrentPage] = useState(1);

    const totalPages = useMemo(() => {
        return Math.max(1, Math.ceil(items.length / itemsPerPage));
    }, [items.length, itemsPerPage]);

    const pageItems = useMemo(() => {
        const start = (currentPage - 1) * itemsPerPage;
        return items.slice(start, start + itemsPerPage);
    }, [items, currentPage, itemsPerPage]);

    // reset if the list shrinks and current page becomes invalid
    useEffect(() => {
        if (items.length === 0) {
            setCurrentPage(1);
            return;
        }
        if (currentPage > totalPages) setCurrentPage(1);
    }, [items.length, currentPage, totalPages]);

    const goToPage = useCallback(
        (page: number) => {
            const clamped = Math.min(Math.max(page, 1), totalPages);
            setCurrentPage(clamped);
        },
        [totalPages],
    );

    const next = useCallback(() => goToPage(currentPage + 1), [goToPage, currentPage]);
    const prev = useCallback(() => goToPage(currentPage - 1), [goToPage, currentPage]);

    return { currentPage, totalPages, pageItems, goToPage, next, prev };
}
