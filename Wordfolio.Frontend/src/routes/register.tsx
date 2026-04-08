import { createFileRoute } from "@tanstack/react-router";
import { z } from "zod";

import { RegisterPage } from "../features/auth/pages/RegisterPage";

const registerSearchSchema = z.object({
    redirect: z.string().optional(),
});

export const Route = createFileRoute("/register")({
    component: RegisterPage,
    validateSearch: registerSearchSchema,
});
