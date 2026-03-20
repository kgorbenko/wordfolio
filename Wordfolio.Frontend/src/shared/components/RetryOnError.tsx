import { ErrorState } from "./ErrorState";

interface RetryOnErrorProps {
    readonly title?: string;
    readonly description?: string;
    readonly onRetry: () => void;
}

export const RetryOnError = ({
    title = "Something went wrong",
    description = "We had trouble loading this content. Please try again.",
    onRetry,
}: RetryOnErrorProps) => (
    <ErrorState title={title} description={description} onRetry={onRetry} />
);
