import {
    Card,
    CardActionArea,
    CardContent,
    Typography,
    Box,
    Chip,
} from "@mui/material";

import "./EntryListItem.scss";

interface EntryListItemProps {
    readonly id: number;
    readonly entryText: string;
    readonly firstDefinition?: string;
    readonly firstTranslation?: string;
    readonly createdAt: string;
    readonly onClick?: () => void;
}

const formatRelativeTime = (dateString: string): string => {
    const date = new Date(dateString);
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
        <Card className="entry-list-item" sx={{ "&:hover": { boxShadow: 2 } }}>
            <CardActionArea onClick={onClick}>
                <CardContent sx={{ py: 2 }}>
                    <Box className="content-wrapper">
                        <Box className="text-content">
                            <Typography
                                variant="subtitle1"
                                fontWeight={600}
                                sx={{ mb: 0.5 }}
                            >
                                {entryText}
                            </Typography>
                            {firstDefinition && (
                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                    noWrap
                                    sx={{ mb: 0.25 }}
                                >
                                    {firstDefinition}
                                </Typography>
                            )}
                            {firstTranslation && (
                                <Typography
                                    className="translation"
                                    variant="body2"
                                    color="text.secondary"
                                    noWrap
                                >
                                    {firstTranslation}
                                </Typography>
                            )}
                        </Box>
                        <Chip
                            className="date-chip"
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
