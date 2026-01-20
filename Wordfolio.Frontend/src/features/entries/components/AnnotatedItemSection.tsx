import { useState, ReactNode } from "react";
import { Box, Typography, Button } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";

import { AnnotatedItemColor } from "../types";
import { AddItemDialog } from "./AddItemDialog";
import styles from "./AnnotatedItemSection.module.scss";

interface AnnotatedItemSectionProps {
    readonly title: string;
    readonly color: AnnotatedItemColor;
    readonly emptyMessage: string;
    readonly dialogTitle: string;
    readonly dialogLabel: string;
    readonly dialogMultiline?: boolean;
    readonly itemCount: number;
    readonly isLoading?: boolean;
    readonly onAdd: (text: string) => void;
    readonly children: ReactNode;
}

export const AnnotatedItemSection = ({
    title,
    color,
    emptyMessage,
    dialogTitle,
    dialogLabel,
    dialogMultiline = false,
    itemCount,
    isLoading = false,
    onAdd,
    children,
}: AnnotatedItemSectionProps) => {
    const [isDialogOpen, setIsDialogOpen] = useState(false);

    const handleAdd = (text: string) => {
        onAdd(text);
        setIsDialogOpen(false);
    };

    const titleColor = color === "primary" ? "primary.main" : "secondary.main";

    return (
        <Box className={styles.section}>
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
                    onClick={() => setIsDialogOpen(true)}
                    disabled={isLoading}
                >
                    Add
                </Button>
            </Box>
            {itemCount === 0 ? (
                <Typography variant="body2" color="text.secondary">
                    {emptyMessage}
                </Typography>
            ) : (
                <Box className={styles.items}>{children}</Box>
            )}
            <AddItemDialog
                open={isDialogOpen}
                title={dialogTitle}
                label={dialogLabel}
                multiline={dialogMultiline}
                onClose={() => setIsDialogOpen(false)}
                onAdd={handleAdd}
            />
        </Box>
    );
};
