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
import MenuBookIcon from "@mui/icons-material/MenuBook";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import ExpandLessIcon from "@mui/icons-material/ExpandLess";
import { useState } from "react";

import "./DefinitionsSection.scss";

export interface ExampleItem {
    readonly id: string;
    readonly exampleText: string;
    readonly source: "Api" | "Custom";
    readonly selected: boolean;
}

export interface DefinitionItem {
    readonly id: string;
    readonly definitionText: string;
    readonly partOfSpeech: "verb" | "noun" | "adj" | "adv" | null;
    readonly selected: boolean;
    readonly examples: ExampleItem[];
}

interface DefinitionsSectionProps {
    readonly definitions: DefinitionItem[];
    readonly onChange: (definitions: DefinitionItem[]) => void;
}

const partOfSpeechLabels: Record<string, string> = {
    verb: "verb",
    noun: "noun",
    adj: "adj",
    adv: "adv",
};

export const DefinitionsSection = ({
    definitions,
    onChange,
}: DefinitionsSectionProps) => {
    const theme = useTheme();
    const [isExpanded, setIsExpanded] = useState(true);

    const selectedCount = definitions.filter((d) => d.selected).length;

    const handleToggleDefinition = (id: string) => {
        onChange(
            definitions.map((d) => {
                if (d.id === id) {
                    const newSelected = !d.selected;
                    return {
                        ...d,
                        selected: newSelected,
                        examples: d.examples.map((ex) => ({
                            ...ex,
                            selected: newSelected ? ex.selected : false,
                        })),
                    };
                }
                return d;
            })
        );
    };

    const handleToggleExample = (definitionId: string, exampleId: string) => {
        onChange(
            definitions.map((d) => {
                if (d.id === definitionId) {
                    return {
                        ...d,
                        examples: d.examples.map((ex) =>
                            ex.id === exampleId
                                ? { ...ex, selected: !ex.selected }
                                : ex
                        ),
                    };
                }
                return d;
            })
        );
    };

    const handleSelectAll = () => {
        const allSelected = definitions.every((d) => d.selected);
        onChange(
            definitions.map((d) => ({
                ...d,
                selected: !allSelected,
                examples: d.examples.map((ex) => ({
                    ...ex,
                    selected: !allSelected,
                })),
            }))
        );
    };

    if (definitions.length === 0) {
        return null;
    }

    return (
        <Box className="definitions-section">
            <Box className="header" onClick={() => setIsExpanded(!isExpanded)}>
                <Box className="header-left">
                    <MenuBookIcon
                        sx={{ color: "primary.main", fontSize: 20 }}
                    />
                    <Typography variant="subtitle1" fontWeight={600}>
                        Definitions
                    </Typography>
                    <Chip
                        className="counter-chip"
                        label={`${selectedCount}/${definitions.length}`}
                        size="small"
                        variant="outlined"
                    />
                </Box>
                <Box className="header-right">
                    <Typography
                        className="select-all"
                        variant="caption"
                        sx={{ color: "primary.main" }}
                        onClick={(e) => {
                            e.stopPropagation();
                            handleSelectAll();
                        }}
                    >
                        {definitions.every((d) => d.selected)
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
                    {definitions.map((def) => (
                        <Box
                            key={def.id}
                            className="item"
                            sx={{
                                borderColor: def.selected
                                    ? "primary.main"
                                    : "divider",
                                bgcolor: def.selected
                                    ? alpha(theme.palette.primary.main, 0.04)
                                    : "background.paper",
                            }}
                        >
                            <Box className="item-content">
                                <Checkbox
                                    checked={def.selected}
                                    onChange={() =>
                                        handleToggleDefinition(def.id)
                                    }
                                    size="small"
                                    sx={{ mt: -0.5, ml: -0.5 }}
                                />
                                <Box className="item-body">
                                    <Box className="pos-wrapper">
                                        {def.partOfSpeech && (
                                            <Chip
                                                className="pos-chip"
                                                label={
                                                    partOfSpeechLabels[
                                                        def.partOfSpeech
                                                    ] || def.partOfSpeech
                                                }
                                                size="small"
                                                sx={{
                                                    bgcolor: alpha(
                                                        theme.palette.primary
                                                            .main,
                                                        0.1
                                                    ),
                                                    color: "primary.main",
                                                }}
                                            />
                                        )}
                                    </Box>
                                    <Typography
                                        variant="body2"
                                        sx={{
                                            color: def.selected
                                                ? "text.primary"
                                                : "text.secondary",
                                        }}
                                    >
                                        {def.definitionText}
                                    </Typography>

                                    {def.examples.length > 0 &&
                                        def.selected && (
                                        <Box className="examples">
                                            {def.examples.map((ex) => (
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
                                                                def.id,
                                                                ex.id
                                                            )
                                                        }
                                                        size="small"
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
