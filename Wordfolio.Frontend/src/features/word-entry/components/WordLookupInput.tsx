import { forwardRef } from "react";
import { TextField, InputAdornment, IconButton } from "@mui/material";
import ClearIcon from "@mui/icons-material/Clear";

interface WordLookupInputProps {
    readonly value: string;
    readonly onChange: (value: string) => void;
    readonly onClear: () => void;
}

export const WordLookupInput = forwardRef<
    HTMLInputElement,
    WordLookupInputProps
>(({ value, onChange, onClear }, ref) => {
    return (
        <TextField
            inputRef={ref}
            fullWidth
            placeholder="Enter word or phrase..."
            value={value}
            onChange={(e) => onChange(e.target.value)}
            autoComplete="off"
            slotProps={{
                input: {
                    endAdornment: value && (
                        <InputAdornment position="end">
                            <IconButton
                                onClick={onClear}
                                size="small"
                                edge="end"
                            >
                                <ClearIcon fontSize="small" />
                            </IconButton>
                        </InputAdornment>
                    ),
                },
            }}
        />
    );
});
