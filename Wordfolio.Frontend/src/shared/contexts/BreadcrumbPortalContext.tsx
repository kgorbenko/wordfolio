import { createContext, useContext } from "react";

export const BreadcrumbPortalContext = createContext<HTMLElement | null>(null);

export const useBreadcrumbPortal = () => useContext(BreadcrumbPortalContext);
