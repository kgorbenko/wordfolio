import { Box } from "@mui/material";
import ChevronLeftIcon from "@mui/icons-material/ChevronLeft";

import styles from "./BreadcrumbNav.module.scss";

export interface BreadcrumbItem {
    readonly label: string;
    readonly to?: string;
}

interface ProcessedBreadcrumbItem {
    readonly label: string;
    readonly to?: string;
    readonly active: boolean;
    readonly isBackButton: boolean;
}

interface BreadcrumbNavProps {
    readonly items: BreadcrumbItem[];
    readonly truncate?: boolean;
}

const processItems = (items: BreadcrumbItem[], truncate: boolean): ProcessedBreadcrumbItem[] => {
    const lastIndex = items.length - 1;

    if (!truncate || items.length <= 2) {
        return items.map((item, index) => ({
            ...item,
            active: index === lastIndex,
            isBackButton: false,
        }));
    }

    const home = { ...items[0], active: false, isBackButton: false };
    const parent = items[lastIndex - 1];
    const active = { ...items[lastIndex], active: true, isBackButton: false };
    const backButton: ProcessedBreadcrumbItem = { label: "back", to: parent.to, active: false, isBackButton: true };

    if (items.length === 3) return [home, backButton, active];

    return [home, { label: "...", active: false, isBackButton: false }, backButton, active];
};

export const BreadcrumbNav = ({ items, truncate = false }: BreadcrumbNavProps) => {
    const displayItems = processItems(items, truncate);

    return (
        <div className={styles.breadcrumbs}>
            {displayItems.map((crumb, index) => (
                <div key={`${crumb.label}-${index}`} className={styles.breadcrumbItem}>
                    {index > 0 && (
                        <Box component="span" className={styles.separator} sx={{ color: "text.secondary" }}>/</Box>
                    )}
                    {crumb.isBackButton ? (
                        <Box
                            component="a"
                            href={crumb.to}
                            className={styles.backButton}
                            sx={{ color: "text.neutral", "&:hover": { color: "text.primary" } }}
                            aria-label="Go back"
                        >
                            <ChevronLeftIcon sx={{ fontSize: 16 }} />
                        </Box>
                    ) : (
                        <Box
                            component={crumb.active || !crumb.to ? "span" : "a"}
                            href={crumb.active ? undefined : crumb.to}
                            className={styles.breadcrumbLabel}
                            sx={{
                                color: crumb.active ? "text.primary" : "text.neutral",
                                "&:hover": {
                                    color: crumb.to ? "text.primary" : undefined,
                                },
                            }}
                        >
                            {crumb.label}
                        </Box>
                    )}
                </div>
            ))}
        </div>
    );
};
