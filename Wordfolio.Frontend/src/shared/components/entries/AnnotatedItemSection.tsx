import { ReactNode } from "react";
import { Box, Typography, Button } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";

import type { AnnotatedItemColor } from "../../api/types/entries";
import styles from "./AnnotatedItemSection.module.scss";

interface AnnotatedItemSectionProps {
    readonly title: string;
    readonly color: AnnotatedItemColor;
    readonly emptyMessage: string;
    readonly itemCount: number;
    readonly isLoading?: boolean;
    readonly onAdd: () => void;
    readonly children: ReactNode;
}

export const AnnotatedItemSection = ({
    title,
    color,
    emptyMessage,
    itemCount,
    isLoading = false,
    onAdd,
    children,
}: AnnotatedItemSectionProps) => (
    <Box className={styles.section}>
        <Box className={styles.header}>
            <Box
                className={styles.headerLeft}
                sx={{
                    "&::before": {
                        backgroundColor: `${color}.main`,
                    },
                }}
            >
                <Typography variant="overline" className={styles.label}>
                    {title}
                </Typography>
            </Box>
            <Button
                size="small"
                color={color}
                startIcon={<AddIcon />}
                onClick={onAdd}
                disabled={isLoading}
                data-testid="add-button"
            >
                Add
            </Button>
        </Box>
        {itemCount === 0 ? (
            <Typography
                variant="body2"
                color="text.secondary"
                data-testid="empty-message"
            >
                {emptyMessage}
            </Typography>
        ) : (
            <Box className={styles.items}>{children}</Box>
        )}
    </Box>
);
