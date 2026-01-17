import { createTheme } from "@mui/material/styles";

export const theme = createTheme({
    palette: {
        mode: "light",
        primary: {
            main: "#6366f1",
            light: "#818cf8",
            dark: "#4f46e5",
            contrastText: "#ffffff",
        },
        secondary: {
            main: "#ec4899",
            light: "#f472b6",
            dark: "#db2777",
            contrastText: "#ffffff",
        },
        error: {
            main: "#ef4444",
            light: "#f87171",
            dark: "#dc2626",
        },
        success: {
            main: "#10b981",
            light: "#34d399",
            dark: "#059669",
        },
        warning: {
            main: "#f59e0b",
            light: "#fbbf24",
            dark: "#d97706",
        },
        background: {
            default: "#f8fafc",
            paper: "#ffffff",
        },
        text: {
            primary: "#1e293b",
            secondary: "#64748b",
        },
        divider: "#e2e8f0",
    },
    typography: {
        fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
        fontSize: 14,
        h1: {
            fontWeight: 700,
            letterSpacing: "-0.025em",
        },
        h2: {
            fontWeight: 700,
            letterSpacing: "-0.025em",
        },
        h3: {
            fontWeight: 600,
            letterSpacing: "-0.01em",
        },
        h4: {
            fontWeight: 600,
        },
        h5: {
            fontWeight: 600,
        },
        h6: {
            fontWeight: 600,
        },
        button: {
            fontWeight: 500,
        },
    },
    shape: {
        borderRadius: 12,
    },
    breakpoints: {
        values: {
            xs: 0,
            sm: 450,
            md: 900,
            lg: 1200,
            xl: 1536,
        },
    },
    components: {
        MuiButton: {
            defaultProps: {
                size: "small",
                disableElevation: true,
            },
            styleOverrides: {
                root: {
                    textTransform: "none",
                    fontWeight: 500,
                    borderRadius: 8,
                    padding: "8px 16px",
                },
                contained: {
                    boxShadow: "none",
                    "&:hover": {
                        boxShadow: "none",
                    },
                },
            },
        },
        MuiTextField: {
            defaultProps: {
                size: "small",
            },
            styleOverrides: {
                root: {
                    margin: 0,
                    "& .MuiOutlinedInput-root": {
                        borderRadius: 8,
                    },
                },
            },
        },
        MuiInputLabel: {
            defaultProps: {
                disableAnimation: true,
            },
            styleOverrides: {
                root: {
                    fontSize: 14,
                },
            },
        },
        MuiFormHelperText: {
            styleOverrides: {
                root: {
                    fontSize: "0.75rem",
                    marginTop: "4px",
                },
            },
        },
        MuiCard: {
            styleOverrides: {
                root: {
                    borderRadius: 16,
                    boxShadow:
                        "0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)",
                    border: "1px solid #e2e8f0",
                },
            },
        },
        MuiFab: {
            styleOverrides: {
                root: {
                    boxShadow:
                        "0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)",
                    "&:hover": {
                        boxShadow:
                            "0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)",
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
        MuiDialog: {
            styleOverrides: {
                paper: {
                    borderRadius: 16,
                },
            },
        },
        MuiDrawer: {
            styleOverrides: {
                paper: {
                    borderRadius: "24px 24px 0 0",
                },
            },
        },
        MuiChip: {
            styleOverrides: {
                root: {
                    borderRadius: 8,
                    fontWeight: 500,
                },
            },
        },
        MuiCheckbox: {
            styleOverrides: {
                root: ({ theme }) => ({
                    color: theme.palette.text.secondary,
                    "&.Mui-checked": {
                        color: theme.palette.primary.main,
                    },
                }),
            },
        },
        MuiSkeleton: {
            styleOverrides: {
                root: {
                    borderRadius: 8,
                },
            },
        },
        MuiBottomNavigation: {
            styleOverrides: {
                root: {
                    borderTop: "1px solid #e2e8f0",
                    backgroundColor: "#ffffff",
                },
            },
        },
    },
});
