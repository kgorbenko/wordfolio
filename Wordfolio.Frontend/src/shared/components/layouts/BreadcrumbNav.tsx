import { Box } from "@mui/material";
import { Link } from "@tanstack/react-router";
import ChevronLeftIcon from "@mui/icons-material/ChevronLeft";

import styles from "./BreadcrumbNav.module.scss";

export interface BreadcrumbItem {
    readonly label: string;
    readonly to?: string;
    readonly params?: Record<string, string | number>;
}

interface ProcessedBreadcrumbItem {
    readonly label: string;
    readonly to?: string;
    readonly params?: Record<string, string | number>;
    readonly active: boolean;
    readonly isBackButton: boolean;
}

interface BreadcrumbNavProps {
    readonly items: BreadcrumbItem[];
    readonly truncate?: boolean;
}

const processItems = (
    items: BreadcrumbItem[],
    truncate: boolean
): ProcessedBreadcrumbItem[] => {
    const lastIndex = items.length - 1;

    if (!truncate || items.length <= 1) {
        return items.map((item, index) => ({
            ...item,
            active: index === lastIndex,
            isBackButton: false,
        }));
    }

    const parent = items[lastIndex - 1];

    return [
        {
            label: parent.label,
            to: parent.to,
            params: parent.params,
            active: false,
            isBackButton: true,
        },
        { label: items[lastIndex].label, active: true, isBackButton: false },
    ];
};

export const BreadcrumbNav = ({
    items,
    truncate = false,
}: BreadcrumbNavProps) => {
    const displayItems = processItems(items, truncate);

    return (
        <div className={styles.breadcrumbs}>
            {displayItems.map((crumb, index) => (
                <div
                    key={`${crumb.label}-${index}`}
                    className={styles.breadcrumbItem}
                >
                    {index > 0 && (
                        <Box
                            component="span"
                            className={styles.separator}
                            sx={{ color: "text.secondary" }}
                        >
                            /
                        </Box>
                    )}
                    {crumb.isBackButton && crumb.to ? (
                        <Link
                            to={crumb.to}
                            params={crumb.params}
                            className={styles.backButton}
                        >
                            <Box
                                component="span"
                                className={styles.backButtonInner}
                                sx={{
                                    color: "text.neutral",
                                    "&:hover": { color: "text.primary" },
                                }}
                            >
                                <ChevronLeftIcon sx={{ fontSize: 20 }} />
                                <span className={styles.breadcrumbLabel}>
                                    {crumb.label}
                                </span>
                            </Box>
                        </Link>
                    ) : crumb.to && !crumb.active ? (
                        <Link
                            to={crumb.to}
                            params={crumb.params}
                            className={styles.linkWrapper}
                        >
                            <Box
                                component="span"
                                className={styles.breadcrumbLabel}
                                sx={{
                                    color: "text.neutral",
                                    "&:hover": { color: "text.primary" },
                                }}
                            >
                                {crumb.label}
                            </Box>
                        </Link>
                    ) : (
                        <Box
                            component="span"
                            className={styles.breadcrumbLabel}
                            sx={{ color: "text.primary" }}
                        >
                            {crumb.label}
                        </Box>
                    )}
                </div>
            ))}
        </div>
    );
};
