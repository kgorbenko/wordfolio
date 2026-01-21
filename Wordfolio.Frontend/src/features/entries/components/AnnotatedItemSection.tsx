import { ReactNode } from "react";
import { Box, Typography, Button } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";

import { AnnotatedItemColor } from "../types";
import styles from "./AnnotatedItemSection.module.scss";

interface AnnotatedItemSectionProps {
    readonly title: string;
    readonly color: AnnotatedItemColor;
    readonly emptyMessage: string;
    readonly itemCount: number;
    readonly isLoading?: boolean;
    readonly onAdd: () => void;
    readonly children: ReactNode;
    readonly testId: string;
}

export const AnnotatedItemSection = ({
    title,
    color,
    emptyMessage,
    itemCount,
    isLoading = false,
    onAdd,
    children,
    testId,
}: AnnotatedItemSectionProps) => {
    const titleColor = color === "primary" ? "primary.main" : "secondary.main";

    return (
        <Box className={styles.section} data-testid={`${testId}-section`}>
            <Box className={styles.header}>
                <Typography
                    variant="h6"
                    fontWeight={600}
                    sx={{ color: titleColor }}
                >
                    {title}
                </Typography>
                <Button
                    size="small"
                    startIcon={<AddIcon />}
                    onClick={onAdd}
                    disabled={isLoading}
                    data-testid={`${testId}-add-button`}
                >
                    Add
                </Button>
            </Box>
            {itemCount === 0 ? (
                <Typography
                    variant="body2"
                    color="text.secondary"
                    data-testid={`${testId}-empty-message`}
                >
                    {emptyMessage}
                </Typography>
            ) : (
                <Box className={styles.items}>{children}</Box>
            )}
        </Box>
    );
};
