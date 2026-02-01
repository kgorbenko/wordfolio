import {
    Card,
    CardActionArea,
    CardContent,
    Typography,
    Box,
    Chip,
} from "@mui/material";

import styles from "./EntryListItem.module.scss";

interface EntryListItemProps {
    readonly id: number;
    readonly entryText: string;
    readonly firstDefinition?: string;
    readonly firstTranslation?: string;
    readonly createdAt: Date;
    readonly onClick?: () => void;
}

const formatRelativeTime = (date: Date): string => {
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

    if (diffDays === 0) return "Today";
    if (diffDays === 1) return "Yesterday";
    if (diffDays < 7) return `${diffDays} days ago`;
    if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago`;
    if (diffDays < 365) return `${Math.floor(diffDays / 30)} months ago`;
    return `${Math.floor(diffDays / 365)} years ago`;
};

export const EntryListItem = ({
    entryText,
    firstDefinition,
    firstTranslation,
    createdAt,
    onClick,
}: EntryListItemProps) => {
    return (
        <Card
            className={styles.entryListItem}
            sx={{ "&:hover": { boxShadow: 2 } }}
        >
            <CardActionArea onClick={onClick}>
                <CardContent className={styles.cardContent}>
                    <Box className={styles.contentWrapper}>
                        <Box className={styles.textContent}>
                            <Typography
                                variant="subtitle1"
                                fontWeight={600}
                                className={styles.entryText}
                            >
                                {entryText}
                            </Typography>
                            {firstDefinition && (
                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                    noWrap
                                    className={styles.definition}
                                >
                                    {firstDefinition}
                                </Typography>
                            )}
                            {firstTranslation && (
                                <Typography
                                    className={styles.translation}
                                    variant="body2"
                                    color="text.secondary"
                                    noWrap
                                >
                                    {firstTranslation}
                                </Typography>
                            )}
                        </Box>
                        <Chip
                            className={styles.dateChip}
                            label={formatRelativeTime(createdAt)}
                            size="small"
                            variant="outlined"
                        />
                    </Box>
                </CardContent>
            </CardActionArea>
        </Card>
    );
};
