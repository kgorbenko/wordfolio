import { Skeleton } from "@mui/material";
import { CardGrid } from "./CardGrid";
import styles from "./ContentSkeleton.module.scss";

interface ContentSkeletonProps {
    readonly variant: "cards" | "list" | "form" | "detail";
}

export const ContentSkeleton = ({ variant }: ContentSkeletonProps) => (
    <>
        {variant === "cards" && (
            <CardGrid>
                {[1, 2, 3].map((i) => (
                    <Skeleton key={i} variant="rounded" height={120} />
                ))}
            </CardGrid>
        )}
        {variant === "list" && (
            <div className={styles.listSkeleton}>
                {[1, 2, 3, 4, 5].map((i) => (
                    <Skeleton key={i} variant="rounded" height={60} />
                ))}
            </div>
        )}
        {variant === "form" && (
            <div className={styles.formSkeleton}>
                <Skeleton variant="rounded" height={56} />
                <Skeleton variant="rounded" height={100} />
                <Skeleton variant="rounded" height={40} width={120} />
            </div>
        )}
        {variant === "detail" && (
            <div className={styles.detailSkeleton}>
                <Skeleton
                    variant="text"
                    height={40}
                    width="60%"
                    style={{ marginBottom: 16 }}
                />
                <Skeleton
                    variant="rounded"
                    height={100}
                    style={{ marginBottom: 24 }}
                />
                <Skeleton variant="rounded" height={200} />
            </div>
        )}
    </>
);
