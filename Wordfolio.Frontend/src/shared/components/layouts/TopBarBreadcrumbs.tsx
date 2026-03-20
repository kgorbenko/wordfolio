import { createPortal } from "react-dom";
import { useMediaQuery, useTheme } from "@mui/material";

import { useBreadcrumbPortal } from "../../contexts/BreadcrumbPortalContext";
import { BreadcrumbNav } from "./BreadcrumbNav";
import type { BreadcrumbItem } from "./BreadcrumbNav";

interface TopBarBreadcrumbsProps {
    readonly items: BreadcrumbItem[];
}

export const TopBarBreadcrumbs = ({ items }: TopBarBreadcrumbsProps) => {
    const portalNode = useBreadcrumbPortal();
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));

    if (!portalNode || items.length === 0) {
        return null;
    }

    return createPortal(
        <BreadcrumbNav items={items} truncate={isMobile} />,
        portalNode
    );
};
