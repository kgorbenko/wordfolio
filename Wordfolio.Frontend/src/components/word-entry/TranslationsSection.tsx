import {
    Box,
    Typography,
    Checkbox,
    Chip,
    IconButton,
    Collapse,
    alpha,
    useTheme,
} from "@mui/material";
import LanguageIcon from "@mui/icons-material/Language";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import ExpandLessIcon from "@mui/icons-material/ExpandLess";
import { useState } from "react";

import "./TranslationsSection.scss";

export interface TranslationExampleItem {
    readonly id: string;
    readonly exampleText: string;
    readonly source: "Api" | "Custom";
    readonly selected: boolean;
}

export interface TranslationItem {
    readonly id: string;
    readonly translationText: string;
    readonly partOfSpeech: "verb" | "noun" | "adj" | "adv" | null;
    readonly selected: boolean;
    readonly examples: TranslationExampleItem[];
}

interface TranslationsSectionProps {
    readonly translations: TranslationItem[];
    readonly onChange: (translations: TranslationItem[]) => void;
}

const partOfSpeechLabels: Record<string, string> = {
    verb: "verb",
    noun: "noun",
    adj: "adj",
    adv: "adv",
};

export const TranslationsSection = ({
    translations,
    onChange,
}: TranslationsSectionProps) => {
    const theme = useTheme();
    const [isExpanded, setIsExpanded] = useState(true);

    const selectedCount = translations.filter((t) => t.selected).length;

    const handleToggleTranslation = (id: string) => {
        onChange(
            translations.map((t) => {
                if (t.id === id) {
                    const newSelected = !t.selected;
                    return {
                        ...t,
                        selected: newSelected,
                        examples: t.examples.map((ex) => ({
                            ...ex,
                            selected: newSelected ? ex.selected : false,
                        })),
                    };
                }
                return t;
            })
        );
    };

    const handleToggleExample = (translationId: string, exampleId: string) => {
        onChange(
            translations.map((t) => {
                if (t.id === translationId) {
                    return {
                        ...t,
                        examples: t.examples.map((ex) =>
                            ex.id === exampleId
                                ? { ...ex, selected: !ex.selected }
                                : ex
                        ),
                    };
                }
                return t;
            })
        );
    };

    const handleSelectAll = () => {
        const allSelected = translations.every((t) => t.selected);
        onChange(
            translations.map((t) => ({
                ...t,
                selected: !allSelected,
                examples: t.examples.map((ex) => ({
                    ...ex,
                    selected: !allSelected,
                })),
            }))
        );
    };

    if (translations.length === 0) {
        return null;
    }

    return (
        <Box className="translations-section">
            <Box className="header" onClick={() => setIsExpanded(!isExpanded)}>
                <Box className="header-left">
                    <LanguageIcon
                        sx={{ color: "secondary.main", fontSize: 20 }}
                    />
                    <Typography variant="subtitle1" fontWeight={600}>
                        Translations
                    </Typography>
                    <Chip
                        className="counter-chip"
                        label={`${selectedCount}/${translations.length}`}
                        size="small"
                        variant="outlined"
                        color="secondary"
                    />
                </Box>
                <Box className="header-right">
                    <Typography
                        className="select-all"
                        variant="caption"
                        sx={{ color: "secondary.main" }}
                        onClick={(e) => {
                            e.stopPropagation();
                            handleSelectAll();
                        }}
                    >
                        {translations.every((t) => t.selected)
                            ? "Deselect All"
                            : "Select All"}
                    </Typography>
                    <IconButton size="small">
                        {isExpanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                    </IconButton>
                </Box>
            </Box>

            <Collapse in={isExpanded}>
                <Box className="list">
                    {translations.map((trans) => (
                        <Box
                            key={trans.id}
                            className="item"
                            sx={{
                                borderColor: trans.selected
                                    ? "secondary.main"
                                    : "divider",
                                bgcolor: trans.selected
                                    ? alpha(theme.palette.secondary.main, 0.04)
                                    : "background.paper",
                            }}
                        >
                            <Box className="item-content">
                                <Checkbox
                                    checked={trans.selected}
                                    onChange={() =>
                                        handleToggleTranslation(trans.id)
                                    }
                                    size="small"
                                    color="secondary"
                                    sx={{ mt: -0.5, ml: -0.5 }}
                                />
                                <Box className="item-body">
                                    <Box className="pos-wrapper">
                                        {trans.partOfSpeech && (
                                            <Chip
                                                className="pos-chip"
                                                label={
                                                    partOfSpeechLabels[
                                                        trans.partOfSpeech
                                                    ] || trans.partOfSpeech
                                                }
                                                size="small"
                                                sx={{
                                                    bgcolor: alpha(
                                                        theme.palette.secondary
                                                            .main,
                                                        0.1
                                                    ),
                                                    color: "secondary.main",
                                                }}
                                            />
                                        )}
                                    </Box>
                                    <Typography
                                        variant="body2"
                                        sx={{
                                            color: trans.selected
                                                ? "text.primary"
                                                : "text.secondary",
                                            fontWeight: 500,
                                        }}
                                    >
                                        {trans.translationText}
                                    </Typography>

                                    {trans.examples.length > 0 &&
                                        trans.selected && (
                                            <Box className="examples">
                                                {trans.examples.map((ex) => (
                                                    <Box
                                                        key={ex.id}
                                                        className="example-item"
                                                    >
                                                        <Checkbox
                                                            checked={
                                                                ex.selected
                                                            }
                                                            onChange={() =>
                                                                handleToggleExample(
                                                                    trans.id,
                                                                    ex.id
                                                                )
                                                            }
                                                            size="small"
                                                            color="secondary"
                                                            sx={{
                                                                mt: -0.5,
                                                                p: 0.25,
                                                            }}
                                                        />
                                                        <Typography
                                                            className="example-text"
                                                            variant="body2"
                                                            color="text.secondary"
                                                        >
                                                            {ex.exampleText}
                                                        </Typography>
                                                    </Box>
                                                ))}
                                            </Box>
                                        )}
                                </Box>
                            </Box>
                        </Box>
                    ))}
                </Box>
            </Collapse>
        </Box>
    );
};
