import { createFileRoute } from "@tanstack/react-router";
import { z } from "zod";

import { LoginPage } from "../pages/LoginPage";

const loginSearchSchema = z.object({
    message: z.string().optional(),
});

export const Route = createFileRoute("/login")({
    component: LoginPage,
    validateSearch: loginSearchSchema,
});
