import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";

import { EmptyState } from "./EmptyState";

interface RetryOnErrorProps {
    readonly title?: string;
    readonly description?: string;
    readonly onRetry: () => void;
}

export const RetryOnError = ({
    title = "Something went wrong",
    description = "We had trouble loading this content. Please try again.",
    onRetry,
}: RetryOnErrorProps) => {
    return (
        <EmptyState
            icon={
                <ErrorOutlineIcon sx={{ fontSize: 40, color: "error.main" }} />
            }
            title={title}
            description={description}
            actionLabel="Retry"
            onAction={onRetry}
        />
    );
};
