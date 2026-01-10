import { createFileRoute } from "@tanstack/react-router";
import { useState, useEffect } from "react";
import {
    Box,
    Container,
    Typography,
    Card,
    CardContent,
    CardActionArea,
    Skeleton,
    alpha,
    useTheme,
} from "@mui/material";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import FolderIcon from "@mui/icons-material/Folder";
import AddIcon from "@mui/icons-material/Add";

import { useUiStore } from "../../stores/uiStore";
import {
    vocabulariesApi,
    VocabularyResponse,
    CollectionResponse,
} from "../../api/vocabulariesApi";
import { entriesApi, EntryResponse } from "../../api/entriesApi";

interface VocabularyWithCollection extends VocabularyResponse {
    collectionName: string;
    entryCount?: number;
}

const DashboardPage = () => {
    const theme = useTheme();
    const { openWordEntry } = useUiStore();

    const [isLoading, setIsLoading] = useState(true);
    const [collections, setCollections] = useState<CollectionResponse[]>([]);
    const [vocabularies, setVocabularies] = useState<
        VocabularyWithCollection[]
    >([]);
    const [recentEntries, setRecentEntries] = useState<EntryResponse[]>([]);

    useEffect(() => {
        const loadData = async () => {
            try {
                const cols = await vocabulariesApi.getCollections();
                setCollections(cols);

                const allVocabs = await vocabulariesApi.getAllVocabularies();
                setVocabularies(allVocabs);

                if (allVocabs.length > 0) {
                    const entries = await entriesApi.getEntries(
                        allVocabs[0].id
                    );
                    setRecentEntries(entries.slice(0, 5));
                }
            } catch {
                // Handle error silently for now
            } finally {
                setIsLoading(false);
            }
        };

        loadData();
    }, []);

    if (isLoading) {
        return (
            <Container maxWidth="lg" sx={{ py: 4 }}>
                <Skeleton
                    variant="text"
                    width={200}
                    height={40}
                    sx={{ mb: 3 }}
                />
                <Box
                    sx={{
                        display: "grid",
                        gridTemplateColumns: {
                            xs: "1fr",
                            sm: "1fr 1fr",
                            md: "1fr 1fr 1fr",
                        },
                        gap: 2,
                    }}
                >
                    {[1, 2, 3].map((i) => (
                        <Skeleton key={i} variant="rounded" height={120} />
                    ))}
                </Box>
            </Container>
        );
    }

    const isEmpty = collections.length === 0 && vocabularies.length === 0;

    return (
        <Container maxWidth="lg" sx={{ py: 4 }}>
            {isEmpty ? (
                <Box
                    sx={{
                        textAlign: "center",
                        py: 8,
                    }}
                >
                    <Box
                        sx={{
                            width: 80,
                            height: 80,
                            borderRadius: "50%",
                            bgcolor: alpha(theme.palette.primary.main, 0.1),
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "center",
                            mx: "auto",
                            mb: 3,
                        }}
                    >
                        <MenuBookIcon
                            sx={{ fontSize: 40, color: "primary.main" }}
                        />
                    </Box>
                    <Typography variant="h4" gutterBottom fontWeight={600}>
                        Welcome to Wordfolio
                    </Typography>
                    <Typography
                        variant="body1"
                        color="text.secondary"
                        sx={{ mb: 4, maxWidth: 400, mx: "auto" }}
                    >
                        Start building your vocabulary by adding your first
                        word. Click the + button to get started.
                    </Typography>
                    <Card
                        sx={{
                            maxWidth: 300,
                            mx: "auto",
                            cursor: "pointer",
                            transition: "all 0.2s ease",
                            "&:hover": {
                                transform: "translateY(-2px)",
                                boxShadow: theme.shadows[4],
                            },
                        }}
                        onClick={() => openWordEntry()}
                    >
                        <CardActionArea sx={{ p: 3 }}>
                            <Box
                                sx={{
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "center",
                                    gap: 1,
                                }}
                            >
                                <AddIcon color="primary" />
                                <Typography
                                    variant="button"
                                    color="primary"
                                    fontWeight={600}
                                >
                                    Add Your First Word
                                </Typography>
                            </Box>
                        </CardActionArea>
                    </Card>
                </Box>
            ) : (
                <>
                    <Typography variant="h4" gutterBottom fontWeight={600}>
                        Dashboard
                    </Typography>

                    <Box sx={{ mb: 4 }}>
                        <Typography
                            variant="h6"
                            sx={{
                                mb: 2,
                                display: "flex",
                                alignItems: "center",
                                gap: 1,
                            }}
                        >
                            <FolderIcon sx={{ color: "primary.main" }} />
                            Your Collections
                        </Typography>
                        <Box
                            sx={{
                                display: "grid",
                                gridTemplateColumns: {
                                    xs: "1fr",
                                    sm: "1fr 1fr",
                                    md: "1fr 1fr 1fr",
                                },
                                gap: 2,
                            }}
                        >
                            {collections.map((collection) => (
                                <Card
                                    key={collection.id}
                                    sx={{
                                        transition: "all 0.2s ease",
                                        "&:hover": {
                                            transform: "translateY(-2px)",
                                            boxShadow: theme.shadows[4],
                                        },
                                    }}
                                >
                                    <CardActionArea>
                                        <CardContent>
                                            <Typography
                                                variant="h6"
                                                fontWeight={600}
                                                noWrap
                                            >
                                                {collection.name}
                                            </Typography>
                                            <Typography
                                                variant="body2"
                                                color="text.secondary"
                                                sx={{
                                                    overflow: "hidden",
                                                    textOverflow: "ellipsis",
                                                    display: "-webkit-box",
                                                    WebkitLineClamp: 2,
                                                    WebkitBoxOrient: "vertical",
                                                    minHeight: 40,
                                                }}
                                            >
                                                {collection.description ||
                                                    "No description"}
                                            </Typography>
                                            <Typography
                                                variant="caption"
                                                color="text.secondary"
                                                sx={{ mt: 1, display: "block" }}
                                            >
                                                {
                                                    vocabularies.filter(
                                                        (v) =>
                                                            v.collectionId ===
                                                            collection.id
                                                    ).length
                                                }{" "}
                                                vocabularies
                                            </Typography>
                                        </CardContent>
                                    </CardActionArea>
                                </Card>
                            ))}
                        </Box>
                    </Box>

                    {recentEntries.length > 0 && (
                        <Box>
                            <Typography
                                variant="h6"
                                sx={{
                                    mb: 2,
                                    display: "flex",
                                    alignItems: "center",
                                    gap: 1,
                                }}
                            >
                                <MenuBookIcon
                                    sx={{ color: "secondary.main" }}
                                />
                                Recent Words
                            </Typography>
                            <Box
                                sx={{
                                    display: "flex",
                                    flexDirection: "column",
                                    gap: 1,
                                }}
                            >
                                {recentEntries.map((entry) => (
                                    <Card key={entry.id}>
                                        <CardContent
                                            sx={{
                                                py: 1.5,
                                                "&:last-child": { pb: 1.5 },
                                            }}
                                        >
                                            <Typography
                                                variant="subtitle1"
                                                fontWeight={600}
                                            >
                                                {entry.entryText}
                                            </Typography>
                                            {entry.definitions[0] && (
                                                <Typography
                                                    variant="body2"
                                                    color="text.secondary"
                                                    noWrap
                                                >
                                                    {
                                                        entry.definitions[0]
                                                            .definitionText
                                                    }
                                                </Typography>
                                            )}
                                            {entry.translations[0] && (
                                                <Typography
                                                    variant="body2"
                                                    color="text.secondary"
                                                    noWrap
                                                    sx={{
                                                        fontStyle: "italic",
                                                    }}
                                                >
                                                    {
                                                        entry.translations[0]
                                                            .translationText
                                                    }
                                                </Typography>
                                            )}
                                        </CardContent>
                                    </Card>
                                ))}
                            </Box>
                        </Box>
                    )}
                </>
            )}
        </Container>
    );
};

export const Route = createFileRoute("/_authenticated/dashboard")({
    component: DashboardPage,
});
