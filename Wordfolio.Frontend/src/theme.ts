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
        surfaceAccent: string;
        toolbar: string;
    }

    interface TypeText {
        neutral: string;
        placeholder: string;
        accent: string;
    }

    interface TypeAction {
        listSelected: string;
    }
}

const bg = "#1E1E1E";
const surface = "#2C2C2C";
const surfaceAlt = "#323232";
const surfaceSidebar = "#262626";
const surfaceAccent = "#444444";
const surfaceAccentHover = "#505050";
const surfaceToolbar = "#363636";

const border = "#555555";
const borderHover = "#666666";

const textPrimary = "#FFFFFF";
const textSecondary = "#888888";
const textPlaceholder = "#777777";
const textAccent = "#AAAAAA";
const textNeutral = "#BBBBBB";

const accentPrimary = "#E91E8C";
const accentSecondary = "#B5F507";

const error = "#AA5555";
const errorHover = "#CC6666";

const overlayHover = "rgba(255, 255, 255, 0.05)";
const overlayListSelected = "rgba(66, 66, 66, 0.5)";
const overlayErrorHover = "rgba(170, 85, 85, 0.08)";

const defaultBorder = `2px solid ${border}`;

export const theme = createTheme({
    palette: {
        mode: "dark",
        background: {
            default: bg,
            paper: surface,
            fill: surface,
            sidebar: surfaceSidebar,
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
        fontFamily: "'DM Sans', Arial, sans-serif",
        fontWeightLight: 200,
        fontWeightRegular: 200,
        fontWeightMedium: 200,
        fontWeightBold: 200,
        h1: {
            fontSize: 26,
            fontWeight: 400,
            letterSpacing: "-0.02em",
            "@media (min-width: 900px)": {
                fontSize: 32,
            },
        },
        h2: {
            fontSize: 18,
            fontWeight: 400,
            "@media (min-width: 900px)": {
                fontSize: 22,
            },
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
        borderRadius: 8,
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
                        fontSize: 16,
                    },
                    borderRadius: 8,
                    padding: "6px 16px",
                    height: 32,
                    "&:focus-visible": {
                        boxShadow: `0 0 0 2px ${accentSecondary}`,
                    },
                },
                containedPrimary: {
                    backgroundColor: surfaceAccent,
                    border: defaultBorder,
                    color: textPrimary,
                    "&:hover": {
                        backgroundColor: surfaceAccentHover,
                        borderColor: borderHover,
                    },
                },
                outlinedPrimary: {
                    borderWidth: 2,
                    borderColor: border,
                    color: textNeutral,
                    "&:hover": {
                        borderColor: borderHover,
                    },
                },
                outlinedError: {
                    borderWidth: 2,
                    borderColor: error,
                    color: error,
                    "&:hover": {
                        borderColor: errorHover,
                        backgroundColor: overlayErrorHover,
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
                    borderRadius: 8,
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
                    backgroundColor: surface,
                    borderRadius: 8,
                    fontSize: 13,
                    "@media (min-width: 900px)": {
                        fontSize: 16,
                    },
                    "&.MuiInputBase-multiline": {
                        padding: 0,
                    },
                    "& .MuiOutlinedInput-notchedOutline": {
                        borderColor: border,
                        borderWidth: 2,
                    },
                    "& .MuiOutlinedInput-notchedOutline legend": {
                        maxWidth: 0,
                    },
                    "&:hover .MuiOutlinedInput-notchedOutline": {
                        borderColor: borderHover,
                        borderWidth: 2,
                    },
                    "&.Mui-focused .MuiOutlinedInput-notchedOutline": {
                        borderColor: accentSecondary,
                        borderWidth: 2,
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
                        fontSize: 16,
                    },
                },
                icon: {
                    color: borderHover,
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
                        color: textNeutral,
                    },
                },
            },
        },
        MuiCard: {
            styleOverrides: {
                root: {
                    backgroundColor: surface,
                    borderRadius: 8,
                    border: `1px solid ${surfaceAccent}`,
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
            styleOverrides: {
                root: {
                    borderRadius: 4,
                    height: 24,
                    fontSize: 11,
                    fontWeight: 400,
                    "@media (min-width: 900px)": {
                        fontSize: 14,
                    },
                },
            },
        },
        MuiDialog: {
            styleOverrides: {
                paper: {
                    backgroundColor: surface,
                    borderRadius: 8,
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
                        fontSize: 16,
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
                        borderRight: `1px solid ${surfaceToolbar}`,
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
                    backgroundColor: surfaceToolbar,
                },
            },
        },
        MuiToolbar: {
            styleOverrides: {
                root: {
                    minHeight: "48px",
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
                    fontFamily: "'DM Sans', Arial, sans-serif",
                    backgroundColor: "transparent",
                    "--DataGrid-rowBorderColor": "transparent",
                    "--DataGrid-containerBackground": "transparent",
                    "& .MuiDataGrid-columnHeaders, & .MuiDataGrid-columnHeaders *":
                        {
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
                            backgroundColor: surfaceAccent,
                        },
                    },
                    "&:nth-of-type(even)": {
                        backgroundColor: surface,
                        "&:hover": {
                            backgroundColor: surfaceAccent,
                        },
                    },
                    "&:first-of-type": {
                        borderTop: defaultBorder,
                        borderTopLeftRadius: 8,
                        borderTopRightRadius: 8,
                    },
                    "&:last-of-type": {
                        borderBottom: defaultBorder,
                        borderBottomLeftRadius: 8,
                        borderBottomRightRadius: 8,
                    },
                },
                cell: {
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
