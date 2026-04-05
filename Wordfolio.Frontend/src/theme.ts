import { createTheme } from "@mui/material/styles";

import type {} from "@mui/x-data-grid/themeAugmentation";

declare module "@mui/material/IconButton" {
    interface IconButtonOwnProps {
        variant?: string;
    }
}

declare module "@mui/material/styles" {
    interface TypeBackground {
        fill: string;
        sidebar: string;
        sidebarHeader: string;
        surfaceAccent: string;
        toolbar: string;
    }

    interface TypeText {
        neutral: string;
        placeholder: string;
        accent: string;
        topbarPrimary: string;
        topbarMuted: string;
    }

    interface TypeAction {
        listSelected: string;
    }
}

const bg = "#141816";
const surface = "#1B201C";
const surfaceAlt = "#242924";
const surfaceSidebar = "#1E1E1E";
const surfaceSidebarHeader = "#1E1E1E";
const surfaceSidebarBorder = "#2C2C2C";
const surfaceAccent = "#3A3A3A";
const surfaceToolbar = "#2A2F2A";

const surfaceInput = "#0E1210";

const border = "#555555";
const borderHover = "#666666";

const textPrimary = "#FFFFFF";
const textSecondary = "#888888";
const textPlaceholder = "#777777";
const textAccent = "#AAAAAA";
const textNeutral = "#BBBBBB";

const accentPrimary = "#E91E8C";
const accentSecondary = "#16DB93";
const accentSecondaryHover = "#18F1A2";

const topbarTextPrimary = "#000000";
const topbarTextMuted = "rgba(0, 0, 0, 0.55)";

const error = "#D95555";
const errorHover = "#E56767";

const overlayHover = "rgba(255, 255, 255, 0.15)";

const inputBorderRest = "rgba(22, 219, 147, 0.62)";
const inputBorderHover = "rgba(22, 219, 147, 0.62)";
const inputBorderFocus = "#16DB93";
const overlayListSelected = "rgba(50, 50, 50, 0.7)";
const overlayErrorHover = "rgba(217, 85, 85, 0.08)";

const defaultBorder = `2px solid ${border}`;

export const theme = createTheme({
    palette: {
        mode: "dark",
        background: {
            default: bg,
            paper: surface,
            fill: surface,
            sidebar: surfaceSidebar,
            sidebarHeader: surfaceSidebarHeader,
            surfaceAccent: surfaceAccent,
            toolbar: surfaceToolbar,
        },
        text: {
            primary: textPrimary,
            secondary: textSecondary,
            disabled: borderHover,
            neutral: textNeutral,
            placeholder: textPlaceholder,
            accent: textAccent,
            topbarPrimary: topbarTextPrimary,
            topbarMuted: topbarTextMuted,
        },
        primary: {
            main: accentSecondary,
        },
        secondary: {
            main: accentPrimary,
        },
        error: {
            main: error,
        },
        divider: border,
        action: {
            listSelected: overlayListSelected,
        },
    },
    typography: {
        fontFamily: "'Open Sans', Arial, sans-serif",
        fontWeightLight: 200,
        fontWeightRegular: 200,
        fontWeightMedium: 400,
        fontWeightBold: 400,
        h1: {
            fontSize: 26,
            fontWeight: 300,
            letterSpacing: "-0.02em",
            "@media (min-width: 900px)": {
                fontSize: 32,
            },
        },
        h2: {
            fontSize: 18,
            fontWeight: 300,
            "@media (min-width: 900px)": {
                fontSize: 22,
            },
        },
        h3: {
            fontWeight: 300,
        },
        h4: {
            fontWeight: 300,
        },
        h5: {
            fontWeight: 300,
        },
        h6: {
            fontWeight: 300,
        },
        body1: {
            fontSize: 14,
            "@media (min-width: 900px)": {
                fontSize: 18,
            },
        },
        body2: {
            fontSize: 13,
            "@media (min-width: 900px)": {
                fontSize: 16,
            },
        },
        overline: {
            fontSize: 10,
            fontWeight: 400,
            letterSpacing: "0.08em",
            textTransform: "uppercase",
            lineHeight: 1.6,
            color: textNeutral,
            "@media (min-width: 900px)": {
                fontSize: 12,
            },
        },
    },
    shape: {
        borderRadius: 14,
    },
    components: {
        MuiButton: {
            defaultProps: {
                disableElevation: true,
                disableRipple: true,
            },
            styleOverrides: {
                root: {
                    textTransform: "none",
                    fontSize: 13,
                    "@media (min-width: 900px)": {
                        fontSize: 14,
                    },
                    borderRadius: 14,
                    padding: "6px 16px",
                    height: 32,
                },
                startIcon: {
                    color: "currentColor",
                },
                endIcon: {
                    color: "currentColor",
                },
                contained: {
                    "&.Mui-disabled": {
                        backgroundColor: surfaceAccent,
                        border: `2px solid ${surfaceAccent}`,
                        color: borderHover,
                    },
                },
                outlined: {
                    "&.Mui-disabled": {
                        border: "2px solid rgba(255, 255, 255, 0.12)",
                        color: borderHover,
                    },
                },
                text: {
                    "&.Mui-disabled": {
                        border: "2px solid transparent",
                        color: borderHover,
                    },
                },
                containedPrimary: {
                    backgroundColor: accentSecondary,
                    border: `2px solid ${accentSecondary}`,
                    color: "rgba(0, 0, 0, 0.85)",
                    "&:hover": {
                        backgroundColor: accentSecondaryHover,
                        borderColor: accentSecondaryHover,
                    },
                    "&:focus-visible": {
                        boxShadow: `0 0 0 2px ${accentSecondary}`,
                    },
                },
                outlinedPrimary: {
                    color: accentSecondary,
                    border: `2px solid rgba(22, 219, 147, 0.42)`,
                    "&:hover": {
                        color: accentSecondaryHover,
                        border: `2px solid rgba(22, 219, 147, 0.62)`,
                        backgroundColor: "rgba(22, 219, 147, 0.06)",
                    },
                    "&:focus-visible": {
                        boxShadow: `0 0 0 2px ${accentSecondary}`,
                    },
                },
                textPrimary: {
                    color: accentSecondary,
                    border: "2px solid transparent",
                    "&:hover": {
                        color: accentSecondaryHover,
                        backgroundColor: "rgba(22, 219, 147, 0.06)",
                    },
                    "&:focus-visible": {
                        boxShadow: `0 0 0 2px ${accentSecondary}`,
                    },
                },
                containedError: {
                    backgroundColor: error,
                    border: `2px solid ${error}`,
                    color: "#FFFFFF",
                    "&:hover": {
                        backgroundColor: errorHover,
                        borderColor: errorHover,
                    },
                    "&:focus-visible": {
                        boxShadow: `0 0 0 2px ${error}`,
                    },
                },
                outlinedError: {
                    color: error,
                    border: "2px solid rgba(217, 85, 85, 0.50)",
                    "&:hover": {
                        color: errorHover,
                        border: "2px solid rgba(229, 103, 103, 0.70)",
                        backgroundColor: overlayErrorHover,
                    },
                    "&:focus-visible": {
                        boxShadow: `0 0 0 2px ${error}`,
                    },
                },
                textError: {
                    color: error,
                    border: "2px solid transparent",
                    "&:hover": {
                        color: errorHover,
                        backgroundColor: overlayErrorHover,
                    },
                    "&:focus-visible": {
                        boxShadow: `0 0 0 2px ${error}`,
                    },
                },
            },
        },
        MuiIconButton: {
            defaultProps: {
                size: "small",
            },
            styleOverrides: {
                root: {
                    borderRadius: 14,
                    height: 32,
                    width: 32,
                },
            },
            variants: [
                {
                    props: { variant: "outlined" },
                    style: {
                        border: defaultBorder,
                        color: textNeutral,
                        "&:hover": {
                            borderColor: borderHover,
                        },
                    },
                },
            ],
        },
        MuiOutlinedInput: {
            styleOverrides: {
                root: {
                    backgroundColor: surfaceInput,
                    borderRadius: 14,
                    fontSize: 13,
                    "@media (min-width: 900px)": {
                        fontSize: 14,
                    },
                    "&.MuiInputBase-multiline": {
                        padding: 0,
                    },
                    "&.MuiInputBase-multiline.MuiInputBase-adornedEnd": {
                        paddingRight: "14px",
                    },
                    "& .MuiOutlinedInput-notchedOutline": {
                        borderColor: inputBorderRest,
                        borderWidth: 2,
                    },
                    "& .MuiOutlinedInput-notchedOutline legend": {
                        maxWidth: 0,
                    },
                    "&:hover .MuiOutlinedInput-notchedOutline": {
                        borderColor: inputBorderHover,
                    },
                    "&.Mui-focused .MuiOutlinedInput-notchedOutline": {
                        borderColor: inputBorderFocus,
                    },
                    "&.Mui-error .MuiOutlinedInput-notchedOutline": {
                        borderColor: "rgba(217, 85, 85, 0.50)",
                    },
                    "&.Mui-error:hover .MuiOutlinedInput-notchedOutline": {
                        borderColor: "rgba(229, 103, 103, 0.70)",
                    },
                    "&.Mui-error.Mui-focused .MuiOutlinedInput-notchedOutline":
                        {
                            borderColor: error,
                        },
                    "&.Mui-disabled .MuiOutlinedInput-notchedOutline": {
                        borderColor: "rgba(255, 255, 255, 0.12)",
                    },
                    "&.Mui-disabled": {
                        backgroundColor: surfaceInput,
                    },
                    "&.Mui-disabled .MuiInputAdornment-root .MuiSvgIcon-root": {
                        color: "#666666",
                    },
                    "&.Mui-disabled .MuiInputAdornment-root .MuiIconButton-root":
                        {
                            color: "#666666",
                        },
                },
                input: {
                    height: 32,
                    padding: "0 10px",
                    boxSizing: "border-box",
                    "&::placeholder": {
                        color: textPlaceholder,
                        opacity: 1,
                    },
                    "&.Mui-disabled::placeholder": {
                        color: "#666666",
                        opacity: 1,
                    },
                    "&.MuiInputBase-inputMultiline": {
                        height: "auto",
                        padding: "6px 10px",
                    },
                },
            },
        },
        MuiTextField: {
            defaultProps: {
                size: "small",
            },
        },
        MuiSelect: {
            defaultProps: {
                size: "small",
            },
            styleOverrides: {
                select: {
                    height: 32,
                    padding: "0 10px",
                    boxSizing: "border-box",
                    display: "flex",
                    alignItems: "center",
                    fontSize: 13,
                    "@media (min-width: 900px)": {
                        fontSize: 14,
                    },
                },
                icon: {
                    color: "#666666",
                    ".Mui-focused &": {
                        color: accentSecondary,
                    },
                    ".Mui-error &": {
                        color: error,
                    },
                    ".Mui-disabled &": {
                        color: "#666666",
                    },
                },
            },
        },
        MuiInputLabel: {
            defaultProps: {
                shrink: true,
                disableAnimation: true,
            },
            styleOverrides: {
                root: {
                    fontSize: 10,
                    fontWeight: 400,
                    letterSpacing: "0.08em",
                    textTransform: "uppercase",
                    color: textNeutral,
                    "@media (min-width: 900px)": {
                        fontSize: 12,
                    },
                    position: "relative",
                    transform: "none",
                    marginBottom: 6,
                    "&.Mui-focused": {
                        color: accentSecondary,
                    },
                    "&.Mui-error": {
                        color: error,
                    },
                    "&.Mui-disabled": {
                        color: "#666666",
                    },
                },
            },
        },
        MuiFormHelperText: {
            styleOverrides: {
                root: {
                    color: "#888888",
                    "&.Mui-error": {
                        color: error,
                    },
                    "&.Mui-disabled": {
                        color: "#666666",
                    },
                },
            },
        },
        MuiCard: {
            styleOverrides: {
                root: {
                    backgroundColor: surface,
                    borderRadius: 14,
                    border: defaultBorder,
                    boxShadow: "none",
                },
            },
        },
        MuiCardContent: {
            styleOverrides: {
                root: {
                    padding: "12px 16px",
                    "&:last-child": {
                        paddingBottom: 12,
                    },
                },
            },
        },
        MuiChip: {
            defaultProps: {
                variant: "outlined",
            },
            styleOverrides: {
                root: {
                    borderRadius: 14,
                    height: 26,
                    fontSize: 12,
                    "@media (min-width: 900px)": {
                        fontSize: 13,
                    },
                    fontWeight: 400,
                },
                label: {
                    padding: "0 10px",
                },
                icon: {
                    color: "currentColor",
                    fontSize: 14,
                    marginLeft: 6,
                    marginRight: -2,
                },
                deleteIcon: {
                    color: "currentColor",
                    fontSize: 14,
                    marginLeft: -2,
                    marginRight: 6,
                    opacity: 0.7,
                    "&:hover": {
                        color: "currentColor",
                        opacity: 1,
                    },
                },
                outlined: {
                    border: `2px solid ${border}`,
                    color: textNeutral,
                    backgroundColor: "rgba(255, 255, 255, 0.03)",
                    "&.MuiChip-clickable:hover, &.MuiChip-deletable:hover": {
                        borderColor: borderHover,
                        backgroundColor: "rgba(255, 255, 255, 0.05)",
                    },
                    "&.MuiChip-clickable:focus-visible, &.MuiChip-deletable:focus-visible":
                        {
                            boxShadow: "0 0 0 2px #888888",
                        },
                    "&.Mui-disabled": {
                        border: "2px solid rgba(255, 255, 255, 0.12)",
                        color: "#666666",
                        backgroundColor: "transparent",
                        opacity: 1,
                    },
                },
                outlinedPrimary: {
                    border: "2px solid rgba(22, 219, 147, 0.42)",
                    color: accentSecondary,
                    backgroundColor: "rgba(22, 219, 147, 0.06)",
                    "&.MuiChip-clickable:hover, &.MuiChip-deletable:hover": {
                        border: "2px solid rgba(22, 219, 147, 0.62)",
                        color: accentSecondaryHover,
                        backgroundColor: "rgba(22, 219, 147, 0.08)",
                    },
                    "&.MuiChip-clickable:focus-visible, &.MuiChip-deletable:focus-visible":
                        {
                            boxShadow: `0 0 0 2px ${accentSecondary}`,
                        },
                    "&.Mui-disabled": {
                        border: "2px solid rgba(255, 255, 255, 0.12)",
                        color: "#666666",
                        backgroundColor: "transparent",
                        opacity: 1,
                    },
                },
                outlinedSecondary: {
                    border: "2px solid rgba(233, 30, 140, 0.42)",
                    color: "#E06AAD",
                    backgroundColor: "rgba(233, 30, 140, 0.06)",
                    "&.MuiChip-clickable:hover, &.MuiChip-deletable:hover": {
                        border: "2px solid rgba(233, 30, 140, 0.62)",
                        color: "#E88BC4",
                        backgroundColor: "rgba(233, 30, 140, 0.08)",
                    },
                    "&.MuiChip-clickable:focus-visible, &.MuiChip-deletable:focus-visible":
                        {
                            boxShadow: "0 0 0 2px #E91E8C",
                        },
                    "&.Mui-disabled": {
                        border: "2px solid rgba(255, 255, 255, 0.12)",
                        color: "#666666",
                        backgroundColor: "transparent",
                        opacity: 1,
                    },
                },
                colorError: {
                    "&.MuiChip-outlined": {
                        border: "2px solid rgba(217, 85, 85, 0.50)",
                        color: error,
                        backgroundColor: "rgba(217, 85, 85, 0.08)",
                        "&.MuiChip-clickable:hover, &.MuiChip-deletable:hover":
                            {
                                border: "2px solid rgba(229, 103, 103, 0.70)",
                                color: errorHover,
                                backgroundColor: "rgba(217, 85, 85, 0.10)",
                            },
                        "&.MuiChip-clickable:focus-visible, &.MuiChip-deletable:focus-visible":
                            {
                                boxShadow: `0 0 0 2px ${error}`,
                            },
                        "&.Mui-disabled": {
                            border: "2px solid rgba(255, 255, 255, 0.12)",
                            color: "#666666",
                            backgroundColor: "transparent",
                            opacity: 1,
                        },
                    },
                    "&.MuiChip-filled": {
                        backgroundColor: error,
                        border: `2px solid ${error}`,
                        color: textPrimary,
                        "&.MuiChip-clickable:hover, &.MuiChip-deletable:hover":
                            {
                                backgroundColor: errorHover,
                                borderColor: errorHover,
                            },
                        "&.Mui-disabled": {
                            backgroundColor: surfaceAccent,
                            border: `2px solid ${surfaceAccent}`,
                            color: "#666666",
                            opacity: 1,
                        },
                    },
                },
                filled: {
                    border: `2px solid ${surfaceAccent}`,
                    backgroundColor: surfaceAccent,
                    color: textPrimary,
                    "&.MuiChip-clickable:hover, &.MuiChip-deletable:hover": {
                        backgroundColor: "#444444",
                        borderColor: "#444444",
                    },
                    "&.Mui-disabled": {
                        backgroundColor: surfaceAccent,
                        border: `2px solid ${surfaceAccent}`,
                        color: "#666666",
                        opacity: 1,
                    },
                },
                filledPrimary: {
                    backgroundColor: accentSecondary,
                    border: `2px solid ${accentSecondary}`,
                    color: "rgba(0, 0, 0, 0.85)",
                    "&.MuiChip-clickable:hover, &.MuiChip-deletable:hover": {
                        backgroundColor: accentSecondaryHover,
                        borderColor: accentSecondaryHover,
                    },
                    "&.Mui-disabled": {
                        backgroundColor: surfaceAccent,
                        border: `2px solid ${surfaceAccent}`,
                        color: "#666666",
                        opacity: 1,
                    },
                },
                filledSecondary: {
                    backgroundColor: accentPrimary,
                    border: `2px solid ${accentPrimary}`,
                    color: textPrimary,
                    "&.MuiChip-clickable:hover, &.MuiChip-deletable:hover": {
                        backgroundColor: "#F032A5",
                        borderColor: "#F032A5",
                    },
                    "&.Mui-disabled": {
                        backgroundColor: surfaceAccent,
                        border: `2px solid ${surfaceAccent}`,
                        color: "#666666",
                        opacity: 1,
                    },
                },
            },
        },
        MuiDialog: {
            styleOverrides: {
                paper: {
                    backgroundColor: surface,
                    borderRadius: 14,
                    border: defaultBorder,
                    backgroundImage: "none",
                },
            },
        },
        MuiDialogTitle: {
            styleOverrides: {
                root: {
                    padding: "24px",
                },
            },
        },
        MuiDialogContent: {
            styleOverrides: {
                root: {
                    padding: "16px 24px",
                },
            },
        },
        MuiDialogActions: {
            styleOverrides: {
                root: {
                    padding: "8px 24px 24px 24px",
                },
            },
        },
        MuiMenu: {
            styleOverrides: {
                paper: {
                    backgroundColor: surface,
                    border: defaultBorder,
                },
            },
        },
        MuiMenuItem: {
            styleOverrides: {
                root: {
                    fontSize: 13,
                    "@media (min-width: 900px)": {
                        fontSize: 14,
                    },
                    color: textPrimary,
                    "&:hover": {
                        backgroundColor: surfaceAlt,
                    },
                    "&.Mui-selected": {
                        backgroundColor: surface,
                        "&:hover": {
                            backgroundColor: surfaceToolbar,
                        },
                        "&.Mui-focusVisible": {
                            backgroundColor: surface,
                        },
                    },
                },
            },
        },
        MuiPaper: {
            styleOverrides: {
                root: {
                    backgroundImage: "none",
                },
            },
        },
        MuiDrawer: {
            styleOverrides: {
                root: {
                    "&.MuiDrawer-docked": {
                        width: 278,
                        flexShrink: 0,
                    },
                },
                paper: {
                    "&:not(.MuiDrawer-paperAnchorBottom)": {
                        backgroundColor: surfaceSidebar,
                        borderRight: `1px solid ${surfaceSidebarBorder}`,
                        borderRadius: 0,
                        width: 278,
                    },
                    ".MuiDrawer-docked &": {
                        position: "relative",
                    },
                },
            },
        },
        MuiAppBar: {
            defaultProps: {
                elevation: 0,
            },
            styleOverrides: {
                root: {
                    backgroundColor: accentSecondary,
                },
            },
        },
        MuiToolbar: {
            styleOverrides: {
                root: {
                    minHeight: "48px",
                    "@media (min-width: 600px)": {
                        minHeight: "48px",
                    },
                },
            },
        },
        MuiListItemButton: {
            defaultProps: {
                disableRipple: true,
            },
            styleOverrides: {
                root: {
                    borderRadius: 5,
                    padding: "5px 10px",
                    marginLeft: 8,
                    marginRight: 8,
                    gap: 8,
                    minHeight: "unset",
                    color: textPrimary,
                },
            },
        },
        MuiListItemIcon: {
            styleOverrides: {
                root: {
                    minWidth: "unset",
                    color: textPrimary,
                },
            },
        },
        MuiListItemText: {
            styleOverrides: {
                primary: {
                    fontSize: 13,
                    "@media (min-width: 900px)": {
                        fontSize: 16,
                    },
                    color: "inherit",
                },
            },
        },
        MuiListSubheader: {
            styleOverrides: {
                root: {
                    backgroundColor: "transparent",
                    fontSize: 10,
                    fontWeight: 400,
                    letterSpacing: "0.08em",
                    textTransform: "uppercase",
                    color: textNeutral,
                    "@media (min-width: 900px)": {
                        fontSize: 12,
                    },
                    lineHeight: "12px",
                    padding: "5px 20px",
                },
            },
        },
        MuiInputAdornment: {
            styleOverrides: {
                root: {
                    "& > .MuiSvgIcon-root": {
                        color: textPlaceholder,
                        fontSize: "14px",
                    },
                    "& .MuiIconButton-root": {
                        color: textPlaceholder,
                        padding: "4px",
                        height: "auto",
                        width: "auto",
                        borderRadius: "50%",
                        "& .MuiSvgIcon-root": {
                            fontSize: "14px",
                        },
                    },
                },
            },
        },
        MuiLink: {
            styleOverrides: {
                root: {
                    color: accentSecondary,
                    textDecorationColor: accentSecondary,
                    textDecorationThickness: 1,
                },
            },
        },
        MuiDataGrid: {
            defaultProps: {
                rowHeight: 48,
                columnHeaderHeight: 32,
                disableColumnMenu: true,
                disableColumnResize: true,
                disableRowSelectionOnClick: true,
            },
            styleOverrides: {
                root: {
                    border: "none",
                    fontFamily: "'Open Sans', Arial, sans-serif",
                    backgroundColor: "transparent",
                    "--DataGrid-rowBorderColor": "transparent",
                    "--DataGrid-containerBackground": "transparent",
                    "& .MuiDataGrid-columnHeaders": {
                        borderBottom: "none",
                        backgroundColor: "transparent",
                    },
                    "& .MuiDataGrid-columnHeaders *": {
                        backgroundColor: "transparent",
                    },
                    "& .MuiDataGrid-columnHeader .MuiDataGrid-sortButton": {
                        backgroundColor: "transparent",
                        color: textPlaceholder,
                        padding: "4px",
                        height: "auto",
                        width: "auto",
                        borderRadius: "50%",
                        "& .sort-icon": {
                            fontSize: "12px",
                            "@media (min-width: 900px)": {
                                fontSize: "15px",
                            },
                        },
                        "&:hover": {
                            backgroundColor: overlayHover,
                        },
                    },
                },
                columnHeader: {
                    padding: "0 16px",
                    "&:focus, &:focus-within": {
                        outline: "none",
                    },
                },
                columnHeaderTitle: {
                    fontSize: 10,
                    fontWeight: 400,
                    letterSpacing: "0.08em",
                    textTransform: "uppercase",
                    color: textNeutral,
                    "@media (min-width: 900px)": {
                        fontSize: 12,
                    },
                },
                columnSeparator: {
                    display: "none",
                },
                row: {
                    borderLeft: defaultBorder,
                    borderRight: defaultBorder,
                    "&:nth-of-type(odd)": {
                        backgroundColor: surfaceAlt,
                        "&:hover": {
                            backgroundColor: overlayHover,
                        },
                    },
                    "&:nth-of-type(even)": {
                        backgroundColor: surface,
                        "&:hover": {
                            backgroundColor: overlayHover,
                        },
                    },
                    "&:first-of-type": {
                        boxShadow: `inset 0 2px 0 0 ${border}`,
                        borderTopLeftRadius: 14,
                        borderTopRightRadius: 14,
                    },
                    "&:last-of-type": {
                        boxShadow: `inset 0 -2px 0 0 ${border}`,
                        borderBottomLeftRadius: 14,
                        borderBottomRightRadius: 14,
                    },
                    "&:first-of-type:last-of-type": {
                        boxShadow: `inset 0 2px 0 0 ${border}, inset 0 -2px 0 0 ${border}`,
                    },
                },
                cell: {
                    borderTop: "none",
                    borderBottom: "none",
                    padding: "0 16px",
                    display: "flex",
                    alignItems: "center",
                    color: textPrimary,
                    fontSize: 13,
                    "@media (min-width: 900px)": {
                        fontSize: 14,
                    },
                    "&:focus, &:focus-within": {
                        outline: "none",
                    },
                },
                toolbarContainer: {
                    padding: 0,
                },
                filler: {
                    display: "none",
                },
                overlay: {
                    backgroundColor: bg,
                },
            },
        },
    },
});
