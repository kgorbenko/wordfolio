import { useState } from "react";
import {
    Box,
    Button,
    Chip,
    Divider,
    Link,
    MenuItem,
    TextField,
    Typography,
} from "@mui/material";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import SearchIcon from "@mui/icons-material/Search";
import ClearIcon from "@mui/icons-material/Clear";
import { DataGrid } from "@mui/x-data-grid";
import type { GridColDef } from "@mui/x-data-grid";

import { AppLayout } from "../../../shared/components/layouts/AppLayout";
import { EmptyState } from "../../../shared/components/EmptyState";
import { ErrorState } from "../../../shared/components/ErrorState";
import { TextWithSubtext } from "../../../shared/components/TextWithSubtext";
import type {
    NavCollection,
    NavUser,
} from "../../../shared/components/layouts/AppSidebar";
import { TopBarBreadcrumbs } from "../../../shared/components/layouts/TopBarBreadcrumbs";
import type { BreadcrumbItem } from "../../../shared/components/layouts/BreadcrumbNav";

import styles from "./DesignSystemPage.module.scss";

const sampleRows = [
    {
        id: 1,
        name: "Common Words",
        desc: "Frequently used words in daily conversations and writing",
        created: "Jan 5, 2026",
        updated: "Feb 1, 2026",
        count: 156,
    },
    {
        id: 2,
        name: "Academic Terms",
        desc: "Scholarly language for academic reading and writing",
        created: "Jan 12, 2026",
        updated: "Jan 28, 2026",
        count: 89,
    },
    {
        id: 3,
        name: "Idioms & Phrases",
        desc: "Common expressions and idiomatic language",
        created: "Dec 20, 2025",
        updated: "Jan 15, 2026",
        count: 42,
    },
    {
        id: 4,
        name: "Business English",
        desc: "Professional terminology for workplace communication",
        created: "Nov 30, 2025",
        updated: "Jan 10, 2026",
        count: 67,
    },
];

const stubDraftCount = 3;

const stubCollectionData = [
    {
        id: 1,
        name: "English Vocabulary",
        entryCount: 4,
        children: [
            { id: 6, name: "Common Words", entryCount: 156 },
            { id: 7, name: "Academic Terms", entryCount: 89 },
            { id: 8, name: "Idioms & Phrases", entryCount: 42 },
        ],
    },
    {
        id: 2,
        name: "Spanish Basics",
        entryCount: 3,
        children: [
            { id: 9, name: "Greetings", entryCount: 24 },
            { id: 10, name: "Travel Phrases", entryCount: 37 },
        ],
    },
    { id: 3, name: "Academic Writing", entryCount: 0 },
    {
        id: 4,
        name: "Japanese",
        entryCount: 2,
        children: [
            { id: 11, name: "Hiragana", entryCount: 46 },
            { id: 12, name: "Katakana", entryCount: 46 },
            { id: 13, name: "JLPT N5 Kanji", entryCount: 103 },
        ],
    },
    {
        id: 5,
        name: "French Literature",
        entryCount: 1,
        children: [{ id: 14, name: "Romantic Era", entryCount: 28 }],
    },
];

const stubUser: NavUser = {
    initials: "K",
    email: "test1@test.com",
};

const mobileColumnVisibility = {
    created: false,
    updated: false,
};

const SortDescIcon = () => (
    <Box
        component="span"
        className={styles.sortIcon}
        sx={{ color: "text.accent" }}
    >
        ↓
    </Box>
);
const SortAscIcon = () => (
    <Box
        component="span"
        className={styles.sortIcon}
        sx={{ color: "text.accent" }}
    >
        ↑
    </Box>
);

const columns: GridColDef[] = [
    {
        field: "name",
        headerName: "Vocabulary",
        flex: 1,
        minWidth: 200,
        sortable: false,
        renderCell: (params) => (
            <TextWithSubtext text={params.row.name} subtext={params.row.desc} />
        ),
    },
    {
        field: "created",
        headerName: "Created At",
        width: 120,
        align: "right",
        headerAlign: "right",
        sortable: false,
    },
    {
        field: "updated",
        headerName: "Updated At",
        width: 135,
        align: "right",
        headerAlign: "right",
    },
    {
        field: "count",
        headerName: "Entries",
        width: 90,
        align: "right",
        headerAlign: "right",
        sortable: false,
    },
];

const paletteGroups = [
    {
        label: "Backgrounds",
        swatches: [
            { hex: "#1E1E1E", role: "Page" },
            { hex: "#2C2C2C", role: "Surface" },
            { hex: "#323232", role: "Surface Alt" },
            { hex: "#262626", role: "Sidebar" },
            { hex: "#444444", role: "Accent" },
            { hex: "#363636", role: "Toolbar" },
        ],
    },
    {
        label: "Text",
        swatches: [
            { hex: "#FFFFFF", role: "Primary" },
            { hex: "#BBBBBB", role: "Neutral" },
            { hex: "#AAAAAA", role: "Accent" },
            { hex: "#888888", role: "Secondary" },
            { hex: "#777777", role: "Placeholder" },
            { hex: "#666666", role: "Disabled" },
        ],
    },
    {
        label: "Borders",
        swatches: [
            { hex: "#555555", role: "Default" },
            { hex: "#666666", role: "Hover" },
        ],
    },
    {
        label: "Accents",
        swatches: [
            { hex: "#E91E8C", role: "Primary" },
            { hex: "#B5F507", role: "Secondary" },
        ],
    },
    {
        label: "Error",
        swatches: [
            { hex: "#AA5555", role: "Default" },
            { hex: "#CC6666", role: "Hover" },
        ],
    },
];

const ColorSwatch = ({ hex, role }: { hex: string; role: string }) => (
    <Box className={styles.swatchContainer}>
        <Box
            className={styles.swatchBox}
            sx={{ bgcolor: hex, border: "1px solid rgba(255,255,255,0.08)" }}
        />
        <Typography
            variant="body2"
            component="span"
            className={styles.swatchHex}
            color="text.neutral"
        >
            {hex}
        </Typography>
        <Typography
            variant="body2"
            component="span"
            className={styles.swatchRole}
            color="text.secondary"
        >
            {role}
        </Typography>
    </Box>
);

export const DesignSystemPage = () => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));
    const [expandedIds, setExpandedIds] = useState<Set<number>>(new Set([1]));
    const [selectedCollectionId, setSelectedCollectionId] = useState<
        number | null
    >(null);
    const [selectedVocabularyId, setSelectedVocabularyId] = useState<
        number | null
    >(6);

    const toggleCollection = (id: number) => {
        setExpandedIds((prev) => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id);
            else next.add(id);
            return next;
        });
    };

    const selectCollection = (id: number) => {
        setSelectedCollectionId(id);
        setSelectedVocabularyId(null);
    };

    const selectVocabulary = (childId: number) => {
        setSelectedVocabularyId(childId);
        setSelectedCollectionId(null);
    };

    const stubBreadcrumbs: BreadcrumbItem[] = [
        { label: "Home", to: "#" },
        { label: "English Vocabulary" },
        { label: "Common Words", to: "#" },
        { label: "Word Details" },
    ];

    const stubCollections: NavCollection[] = stubCollectionData.map((c) => ({
        ...c,
        active: c.id === selectedCollectionId,
        expanded: expandedIds.has(c.id),
        activeChildId: selectedVocabularyId ?? undefined,
        onClick: () => selectCollection(c.id),
        onExpand: () => toggleCollection(c.id),
        onChildClick: selectVocabulary,
    }));

    return (
        <AppLayout
            draftCount={stubDraftCount}
            collections={stubCollections}
            user={stubUser}
            onAddEntry={() => {}}
            onDraftsClick={() => {}}
        >
            <TopBarBreadcrumbs items={stubBreadcrumbs} />
            <Box
                className={styles.pageWrapper}
                sx={{
                    bgcolor: "background.default",
                    p: { xs: "20px 16px", md: "40px 48px" },
                }}
            >
                <Box className={styles.content}>
                    <Typography variant="h1" className={styles.pageTitle}>
                        Design System
                    </Typography>

                    <Box className={styles.section}>
                        <Typography
                            variant="h2"
                            className={styles.sectionTitle}
                        >
                            Typography Scale
                        </Typography>
                        <Divider className={styles.sectionDivider} />
                        <Box className={styles.typographyStack}>
                            <Typography variant="h1">
                                Page Title — 26 / 32px
                            </Typography>
                            <Typography variant="h2">
                                Section Heading — 18 / 22px
                            </Typography>
                            <Typography variant="body1">
                                Body text — 14 / 18px. A feeling of great
                                pleasure and happiness brought about by
                                something good.
                            </Typography>
                            <Typography variant="body2" color="text.secondary">
                                Description text — 13 / 16px secondary. Inactive
                                navigation item or description text.
                            </Typography>
                            <Typography variant="overline">
                                Column Header — 10 / 12px uppercase
                            </Typography>
                            <Link href="/design-system" variant="body1">
                                Link — 14 / 18px accent underline
                            </Link>
                        </Box>
                    </Box>

                    <Box className={styles.section}>
                        <Typography
                            variant="h2"
                            className={styles.sectionTitle}
                        >
                            Color Palette
                        </Typography>
                        <Divider className={styles.sectionDivider} />
                        {paletteGroups.map((group) => (
                            <Box key={group.label}>
                                <Typography
                                    variant="overline"
                                    display="block"
                                    mb={1}
                                >
                                    {group.label}
                                </Typography>
                                <Box className={styles.paletteGroup}>
                                    {group.swatches.map((swatch) => (
                                        <ColorSwatch
                                            key={swatch.hex}
                                            {...swatch}
                                        />
                                    ))}
                                </Box>
                            </Box>
                        ))}
                    </Box>

                    <Box className={styles.section}>
                        <Typography
                            variant="h2"
                            className={styles.sectionTitle}
                        >
                            Buttons
                        </Typography>
                        <Divider className={styles.sectionDivider} />
                        <Box className={styles.typographyStack}>
                            <Box className={styles.buttonsRow}>
                                <Button variant="contained">Add Entry</Button>
                                <Button variant="outlined">Cancel</Button>
                                <Button variant="outlined" color="error">
                                    Delete
                                </Button>
                            </Box>
                            <Box className={styles.buttonsRow}>
                                <Button variant="contained" disabled>
                                    Add Entry
                                </Button>
                                <Button variant="outlined" disabled>
                                    Cancel
                                </Button>
                            </Box>
                        </Box>
                    </Box>

                    <Box className={styles.section}>
                        <Typography
                            variant="h2"
                            className={styles.sectionTitle}
                        >
                            Form Inputs
                        </Typography>
                        <Divider className={styles.sectionDivider} />
                        <Box className={styles.formInputs}>
                            <TextField
                                label="Text Input"
                                placeholder="Enter a name..."
                                fullWidth
                            />
                            <TextField
                                label="Textarea"
                                placeholder="A feeling of great pleasure and happiness brought about by something good."
                                multiline
                                rows={3}
                                fullWidth
                            />
                            <Box className={styles.formRow}>
                                <TextField
                                    select
                                    label="Select"
                                    defaultValue="custom"
                                    className={styles.demoSelect}
                                >
                                    <MenuItem value="custom">Custom</MenuItem>
                                    <MenuItem value="api">API</MenuItem>
                                </TextField>
                                <TextField
                                    select
                                    label="Status"
                                    defaultValue="active"
                                    className={styles.demoSelect}
                                >
                                    <MenuItem value="active">Active</MenuItem>
                                    <MenuItem value="archived">
                                        Archived
                                    </MenuItem>
                                </TextField>
                            </Box>
                        </Box>
                    </Box>

                    <Box className={styles.section}>
                        <Typography
                            variant="h2"
                            className={styles.sectionTitle}
                        >
                            Search Toolbar
                        </Typography>
                        <Divider className={styles.sectionDivider} />
                        <Box className={styles.searchToolbar}>
                            <TextField
                                placeholder="Search vocabularies..."
                                fullWidth
                                slotProps={{
                                    input: {
                                        startAdornment: (
                                            <Box
                                                component="span"
                                                className={styles.searchIcon}
                                                sx={{
                                                    color: "text.placeholder",
                                                }}
                                            >
                                                <SearchIcon />
                                            </Box>
                                        ),
                                        endAdornment: (
                                            <Box
                                                component="span"
                                                className={styles.clearIcon}
                                                sx={{
                                                    color: "text.placeholder",
                                                }}
                                            >
                                                <ClearIcon />
                                            </Box>
                                        ),
                                    },
                                }}
                            />
                            <Button
                                variant="contained"
                                className={styles.searchAddButton}
                            >
                                + Add Vocabulary
                            </Button>
                        </Box>
                    </Box>

                    <Box className={styles.section}>
                        <Typography
                            variant="h2"
                            className={styles.sectionTitle}
                        >
                            Grid
                        </Typography>
                        <Divider className={styles.sectionDivider} />
                        <Box className={styles.gridWrapper}>
                            <DataGrid
                                rows={sampleRows}
                                columns={columns}
                                rowHeight={isMobile ? 48 : 52}
                                hideFooter
                                columnVisibilityModel={
                                    isMobile
                                        ? mobileColumnVisibility
                                        : undefined
                                }
                                initialState={{
                                    sorting: {
                                        sortModel: [
                                            { field: "updated", sort: "desc" },
                                        ],
                                    },
                                }}
                                slots={{
                                    columnSortedDescendingIcon: SortDescIcon,
                                    columnSortedAscendingIcon: SortAscIcon,
                                }}
                            />
                        </Box>
                    </Box>

                    <Box className={styles.section}>
                        <Typography
                            variant="h2"
                            className={styles.sectionTitle}
                        >
                            Empty &amp; Error States
                        </Typography>
                        <Divider className={styles.sectionDivider} />
                        <Box className={styles.statesRow}>
                            <Box className={styles.stateItem}>
                                <Typography variant="overline" display="block">
                                    Empty State
                                </Typography>
                                <EmptyState />
                            </Box>
                            <Box className={styles.stateItem}>
                                <Typography variant="overline" display="block">
                                    Error State
                                </Typography>
                                <ErrorState />
                            </Box>
                            <Box className={styles.stateItem}>
                                <Typography variant="overline" display="block">
                                    Error State with Retry
                                </Typography>
                                <ErrorState onRetry={() => {}} />
                            </Box>
                        </Box>
                    </Box>

                    <Box className={styles.section}>
                        <Typography
                            variant="h2"
                            className={styles.sectionTitle}
                        >
                            Chips
                        </Typography>
                        <Divider className={styles.sectionDivider} />
                        <Box className={styles.chipsRow}>
                            <Chip label="Custom" size="small" />
                            <Chip label="API" size="small" color="primary" />
                            <Chip
                                label="Active"
                                size="small"
                                color="secondary"
                            />
                            <Chip label="Error" size="small" color="error" />
                        </Box>
                    </Box>
                </Box>
            </Box>
        </AppLayout>
    );
};
