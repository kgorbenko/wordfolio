import { useState } from "react";
import {
    Alert,
    Box,
    Button,
    Chip,
    Divider,
    IconButton,
    InputAdornment,
    Link,
    MenuItem,
    SnackbarContent,
    TextField,
    Typography,
} from "@mui/material";
import Add from "@mui/icons-material/Add";
import Label from "@mui/icons-material/Label";
import Visibility from "@mui/icons-material/Visibility";
import type { GridColDef, GridSortModel } from "@mui/x-data-grid";

import { AppLayout } from "../../../shared/components/layouts/AppLayout";
import { ContentDataGrid } from "../../../shared/components/ContentDataGrid";
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
        id: "1",
        name: "English Vocabulary",
        entryCount: 4,
        children: [
            { id: "6", name: "Common Words", entryCount: 156 },
            { id: "7", name: "Academic Terms", entryCount: 89 },
            { id: "8", name: "Idioms & Phrases", entryCount: 42 },
        ],
    },
    {
        id: "2",
        name: "Spanish Basics",
        entryCount: 3,
        children: [
            { id: "9", name: "Greetings", entryCount: 24 },
            { id: "10", name: "Travel Phrases", entryCount: 37 },
        ],
    },
    { id: "3", name: "Academic Writing", entryCount: 0 },
    {
        id: "4",
        name: "Japanese",
        entryCount: 2,
        children: [
            { id: "11", name: "Hiragana", entryCount: 46 },
            { id: "12", name: "Katakana", entryCount: 46 },
            { id: "13", name: "JLPT N5 Kanji", entryCount: 103 },
        ],
    },
    {
        id: "5",
        name: "French Literature",
        entryCount: 1,
        children: [{ id: "14", name: "Romantic Era", entryCount: 28 }],
    },
];

const stubUser: NavUser = {
    initials: "K",
    email: "test1@test.com",
};

const columns: GridColDef[] = [
    {
        field: "name",
        headerName: "Vocabulary",
        flex: 1,
        minWidth: 200,
        renderCell: (params) => (
            <TextWithSubtext text={params.row.name} subtext={params.row.desc} />
        ),
    },
    {
        field: "created",
        headerName: "Created At",
        width: 130,
        align: "right",
        headerAlign: "right",
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
        width: 105,
        align: "right",
        headerAlign: "right",
    },
];

const mobileColumns: GridColDef[] = columns.filter(
    (c) => c.field !== "created" && c.field !== "updated"
);

const paletteGroups = [
    {
        label: "Backgrounds",
        swatches: [
            { hex: "#141816", role: "Page" },
            { hex: "#1B201C", role: "Surface" },
            { hex: "#242924", role: "Surface Alt" },
            { hex: "#1E1E1E", role: "Sidebar" },
            { hex: "#3A3A3A", role: "Accent" },
            { hex: "#2A2F2A", role: "Toolbar" },
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
            { hex: "#555555", role: "Neutral Default" },
            { hex: "#666666", role: "Neutral Hover" },
            { hex: "rgba(22, 219, 147, 0.62)", role: "Input Rest" },
            { hex: "rgba(22, 219, 147, 0.62)", role: "Input Hover" },
            { hex: "#16DB93", role: "Input Focus" },
        ],
    },
    {
        label: "Accents",
        swatches: [
            { hex: "#B5F507", role: "Lime / Action" },
            { hex: "#E91E8C", role: "Magenta / Brand" },
        ],
    },
    {
        label: "Error",
        swatches: [
            { hex: "#D95555", role: "Default" },
            { hex: "#E56767", role: "Hover" },
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
    const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set(["1"]));
    const [selectedCollectionId, setSelectedCollectionId] = useState<
        string | null
    >(null);
    const [selectedVocabularyId, setSelectedVocabularyId] = useState<
        string | null
    >("6");
    const [gridSortModel, setGridSortModel] = useState<GridSortModel>([
        { field: "updated", sort: "desc" },
    ]);
    const [gridFilter, setGridFilter] = useState("");

    const toggleCollection = (id: string) => {
        setExpandedIds((prev) => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id);
            else next.add(id);
            return next;
        });
    };

    const selectCollection = (id: string) => {
        setSelectedCollectionId(id);
        setSelectedVocabularyId(null);
    };

    const selectVocabulary = (childId: string) => {
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
                                            key={`${swatch.hex}-${swatch.role}`}
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
                            <Typography variant="overline" display="block">
                                Primary
                            </Typography>
                            <Box className={styles.buttonsRow}>
                                <Button variant="contained">Add Entry</Button>
                                <Button variant="outlined">Cancel</Button>
                                <Button variant="text">View</Button>
                            </Box>
                            <Typography variant="overline" display="block">
                                Error
                            </Typography>
                            <Box className={styles.buttonsRow}>
                                <Button variant="contained" color="error">
                                    Delete
                                </Button>
                                <Button variant="outlined" color="error">
                                    Remove
                                </Button>
                                <Button variant="text" color="error">
                                    Discard
                                </Button>
                            </Box>
                            <Typography variant="overline" display="block">
                                With Icon
                            </Typography>
                            <Box className={styles.buttonsRow}>
                                <Button variant="contained" startIcon={<Add />}>
                                    Add Entry
                                </Button>
                                <Button variant="outlined" startIcon={<Add />}>
                                    Add Entry
                                </Button>
                                <Button variant="text" startIcon={<Add />}>
                                    Add Entry
                                </Button>
                            </Box>
                            <Typography variant="overline" display="block">
                                Disabled
                            </Typography>
                            <Box className={styles.buttonsRow}>
                                <Button variant="contained" disabled>
                                    Add Entry
                                </Button>
                                <Button variant="outlined" disabled>
                                    Cancel
                                </Button>
                                <Button variant="text" disabled>
                                    View
                                </Button>
                            </Box>
                            <Box className={styles.buttonsRow}>
                                <Button
                                    variant="contained"
                                    color="error"
                                    disabled
                                >
                                    Delete
                                </Button>
                                <Button
                                    variant="outlined"
                                    color="error"
                                    disabled
                                >
                                    Remove
                                </Button>
                                <Button variant="text" color="error" disabled>
                                    Discard
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
                                helperText="This is a helpful hint."
                            />
                            <TextField
                                label="Input with Icon"
                                placeholder="Enter a value..."
                                fullWidth
                                slotProps={{
                                    input: {
                                        endAdornment: (
                                            <InputAdornment position="end">
                                                <IconButton
                                                    edge="end"
                                                    tabIndex={-1}
                                                >
                                                    <Visibility />
                                                </IconButton>
                                            </InputAdornment>
                                        ),
                                    },
                                }}
                            />
                            <TextField
                                select
                                label="Select"
                                defaultValue="custom"
                                fullWidth
                            >
                                <MenuItem value="custom">Custom</MenuItem>
                                <MenuItem value="api">API</MenuItem>
                            </TextField>
                            <TextField
                                label="Textarea"
                                placeholder="A feeling of great pleasure and happiness brought about by something good."
                                fullWidth
                                multiline
                                rows={3}
                            />
                            <Typography
                                variant="overline"
                                display="block"
                                className={styles.formSectionLabel}
                            >
                                Error States
                            </Typography>
                            <TextField
                                label="Input with Icon"
                                placeholder="Enter a value..."
                                fullWidth
                                error
                                helperText="This field is required"
                                slotProps={{
                                    input: {
                                        endAdornment: (
                                            <InputAdornment position="end">
                                                <IconButton
                                                    edge="end"
                                                    tabIndex={-1}
                                                >
                                                    <Visibility />
                                                </IconButton>
                                            </InputAdornment>
                                        ),
                                    },
                                }}
                            />
                            <TextField
                                select
                                label="Select"
                                defaultValue="custom"
                                fullWidth
                                error
                                helperText="Please select a valid option"
                            >
                                <MenuItem value="custom">Custom</MenuItem>
                                <MenuItem value="api">API</MenuItem>
                            </TextField>
                            <Typography
                                variant="overline"
                                display="block"
                                className={styles.formSectionLabel}
                            >
                                Disabled States
                            </Typography>
                            <TextField
                                label="Text Input"
                                placeholder="Enter a name..."
                                fullWidth
                                disabled
                            />
                            <TextField
                                label="Input with Icon"
                                placeholder="Enter a value..."
                                fullWidth
                                disabled
                                slotProps={{
                                    input: {
                                        endAdornment: (
                                            <InputAdornment position="end">
                                                <IconButton edge="end" disabled>
                                                    <Visibility />
                                                </IconButton>
                                            </InputAdornment>
                                        ),
                                    },
                                }}
                            />
                            <TextField
                                select
                                label="Select"
                                defaultValue="custom"
                                fullWidth
                                disabled
                            >
                                <MenuItem value="custom">Custom</MenuItem>
                                <MenuItem value="api">API</MenuItem>
                            </TextField>
                            <TextField
                                label="Textarea"
                                placeholder="A feeling of great pleasure and happiness brought about by something good."
                                multiline
                                rows={3}
                                fullWidth
                                disabled
                            />
                        </Box>
                    </Box>

                    <Box className={styles.section}>
                        <Typography
                            variant="h2"
                            className={styles.sectionTitle}
                        >
                            Data Grid
                        </Typography>
                        <Divider className={styles.sectionDivider} />
                        <ContentDataGrid
                            rows={sampleRows}
                            desktopColumns={columns}
                            mobileColumns={mobileColumns}
                            onRowClick={() => {}}
                            actionLabel="+ Add Vocabulary"
                            onAction={() => {}}
                            searchPlaceholder="Search vocabularies..."
                            sortModel={gridSortModel}
                            onSortModelChange={setGridSortModel}
                            filterValue={gridFilter}
                            onFilterValueChange={setGridFilter}
                        />
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
                        <Box className={styles.typographyStack}>
                            <Typography variant="overline" display="block">
                                Outlined
                            </Typography>
                            <Box className={styles.chipsRow}>
                                <Chip label="Default" />
                                <Chip label="Primary" color="primary" />
                                <Chip label="Secondary" color="secondary" />
                                <Chip label="Error" color="error" />
                                <Chip label="Disabled" disabled />
                            </Box>

                            <Typography variant="overline" display="block">
                                Filled — Selected / Active
                            </Typography>
                            <Box className={styles.chipsRow}>
                                <Chip label="Default" variant="filled" />
                                <Chip
                                    label="Primary"
                                    variant="filled"
                                    color="primary"
                                />
                                <Chip
                                    label="Secondary"
                                    variant="filled"
                                    color="secondary"
                                />
                                <Chip
                                    label="Error"
                                    variant="filled"
                                    color="error"
                                />
                                <Chip
                                    label="Disabled"
                                    variant="filled"
                                    disabled
                                />
                            </Box>

                            <Typography variant="overline" display="block">
                                Clickable
                            </Typography>
                            <Box className={styles.chipsRow}>
                                <Chip label="Default" onClick={() => {}} />
                                <Chip
                                    label="Primary"
                                    color="primary"
                                    onClick={() => {}}
                                />
                                <Chip
                                    label="Secondary"
                                    color="secondary"
                                    onClick={() => {}}
                                />
                                <Chip
                                    label="Error"
                                    color="error"
                                    onClick={() => {}}
                                />
                            </Box>

                            <Typography variant="overline" display="block">
                                Deletable
                            </Typography>
                            <Box className={styles.chipsRow}>
                                <Chip label="Default" onDelete={() => {}} />
                                <Chip
                                    label="Primary"
                                    color="primary"
                                    onDelete={() => {}}
                                />
                                <Chip
                                    label="Error"
                                    color="error"
                                    onDelete={() => {}}
                                />
                            </Box>

                            <Typography variant="overline" display="block">
                                With Leading Icon
                            </Typography>
                            <Box className={styles.chipsRow}>
                                <Chip
                                    label="Primary"
                                    color="primary"
                                    icon={<Label />}
                                />
                                <Chip
                                    label="Error"
                                    color="error"
                                    icon={<Label />}
                                />
                            </Box>

                            <Typography variant="overline" display="block">
                                Size Hierarchy — Chip 26px vs Button 32px
                            </Typography>
                            <Box
                                className={styles.chipsRow}
                                sx={{ alignItems: "center" }}
                            >
                                <Chip label="Tag" />
                                <Button variant="outlined">Action</Button>
                            </Box>
                        </Box>
                    </Box>

                    <Box className={styles.section}>
                        <Typography
                            variant="h2"
                            className={styles.sectionTitle}
                        >
                            Alerts
                        </Typography>
                        <Divider className={styles.sectionDivider} />
                        <Box className={styles.typographyStack}>
                            {(
                                [
                                    "standard",
                                    "filled",
                                    "outlined",
                                ] as const
                            ).map((variant) => (
                                <Box key={variant}>
                                    <Typography
                                        variant="overline"
                                        display="block"
                                        mb={1}
                                    >
                                        {variant.charAt(0).toUpperCase() +
                                            variant.slice(1)}
                                    </Typography>
                                    <Box
                                        sx={{
                                            display: "flex",
                                            flexDirection: "column",
                                            gap: 1,
                                        }}
                                    >
                                        {(
                                            [
                                                "error",
                                                "warning",
                                                "info",
                                                "success",
                                            ] as const
                                        ).map((severity) => (
                                            <Alert
                                                key={severity}
                                                variant={variant}
                                                severity={severity}
                                            >
                                                {severity
                                                    .charAt(0)
                                                    .toUpperCase() +
                                                    severity.slice(1)}{" "}
                                                — This is a {severity} message.
                                            </Alert>
                                        ))}
                                    </Box>
                                </Box>
                            ))}
                            <Box>
                                <Typography
                                    variant="overline"
                                    display="block"
                                    mb={1}
                                >
                                    With Close Action
                                </Typography>
                                <Box
                                    sx={{
                                        display: "flex",
                                        flexDirection: "column",
                                        gap: 1,
                                    }}
                                >
                                    {(
                                        [
                                            "error",
                                            "warning",
                                            "info",
                                            "success",
                                        ] as const
                                    ).map((severity) => (
                                        <Alert
                                            key={severity}
                                            severity={severity}
                                            onClose={() => {}}
                                        >
                                            {severity.charAt(0).toUpperCase() +
                                                severity.slice(1)}{" "}
                                            — Dismissible alert.
                                        </Alert>
                                    ))}
                                </Box>
                            </Box>
                        </Box>
                    </Box>

                    <Box className={styles.section}>
                        <Typography
                            variant="h2"
                            className={styles.sectionTitle}
                        >
                            Snackbar
                        </Typography>
                        <Divider className={styles.sectionDivider} />
                        <Box className={styles.typographyStack}>
                            <Box>
                                <Typography
                                    variant="overline"
                                    display="block"
                                    mb={1}
                                >
                                    Neutral SnackbarContent
                                </Typography>
                                <Box
                                    sx={{
                                        display: "flex",
                                        flexDirection: "column",
                                        gap: 1,
                                        maxWidth: 600,
                                    }}
                                >
                                    <SnackbarContent message="This is a neutral snackbar message." />
                                    <SnackbarContent
                                        message="Neutral snackbar with an action button."
                                        action={
                                            <Button
                                                color="secondary"
                                                size="small"
                                            >
                                                Undo
                                            </Button>
                                        }
                                    />
                                </Box>
                            </Box>
                            <Box>
                                <Typography
                                    variant="overline"
                                    display="block"
                                    mb={1}
                                >
                                    Notification-style (Alert in Snackbar)
                                </Typography>
                                <Box
                                    sx={{
                                        display: "flex",
                                        flexDirection: "column",
                                        gap: 1,
                                        maxWidth: 600,
                                    }}
                                >
                                    {(
                                        [
                                            "success",
                                            "error",
                                            "warning",
                                            "info",
                                        ] as const
                                    ).map((severity) => (
                                        <Alert
                                            key={severity}
                                            severity={severity}
                                            onClose={() => {}}
                                        >
                                            {severity.charAt(0).toUpperCase() +
                                                severity.slice(1)}{" "}
                                            — Operation completed successfully.
                                        </Alert>
                                    ))}
                                </Box>
                            </Box>
                        </Box>
                    </Box>
                </Box>
            </Box>
        </AppLayout>
    );
};
