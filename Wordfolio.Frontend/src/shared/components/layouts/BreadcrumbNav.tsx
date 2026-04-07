import { Box, IconButton } from "@mui/material";
import { Link, useNavigate, useRouter } from "@tanstack/react-router";
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
    const router = useRouter();
    const navigate = useNavigate();
    const displayItems = processItems(items, truncate);

    const handleBack = (
        fallbackTo?: string,
        fallbackParams?: Record<string, string | number>
    ) => {
        if (router.history.canGoBack()) {
            router.history.back();
        } else if (fallbackTo) {
            void navigate({
                to: fallbackTo,
                params: fallbackParams,
                replace: true,
            });
        }
    };

    return (
        <div className={styles.breadcrumbs}>
            {displayItems.map((crumb, index) => (
                <div
                    key={`${crumb.label}-${index}`}
                    className={styles.breadcrumbItem}
                >
                    {index > 0 && !displayItems[index - 1].isBackButton && (
                        <Box
                            component="span"
                            className={styles.separator}
                            sx={{ color: "text.topbarPrimary" }}
                        >
                            /
                        </Box>
                    )}
                    {crumb.isBackButton ? (
                        <IconButton
                            onClick={() => handleBack(crumb.to, crumb.params)}
                            sx={{ color: "text.topbarMuted", mr: 1.5 }}
                        >
                            <ChevronLeftIcon />
                        </IconButton>
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
                                    color: "text.topbarMuted",
                                    "&:hover": { color: "text.topbarPrimary" },
                                }}
                            >
                                {crumb.label}
                            </Box>
                        </Link>
                    ) : (
                        <Box
                            component="span"
                            className={styles.breadcrumbLabel}
                            sx={{ color: "text.topbarPrimary" }}
                        >
                            {crumb.label}
                        </Box>
                    )}
                </div>
            ))}
        </div>
    );
};
