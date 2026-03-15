import { createFileRoute } from "@tanstack/react-router";

import { DesignSystemPage } from "../features/design-system/pages/DesignSystemPage";

export const Route = createFileRoute("/design-system")({
    component: DesignSystemPage,
});
