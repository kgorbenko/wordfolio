import { useState } from "react";
import { TextField, InputAdornment, IconButton } from "@mui/material";
import type { TextFieldProps } from "@mui/material";
import Visibility from "@mui/icons-material/Visibility";
import VisibilityOff from "@mui/icons-material/VisibilityOff";

type PasswordFieldProps = Omit<TextFieldProps, "type" | "slotProps">;

export const PasswordField = (props: PasswordFieldProps) => {
    const [showPassword, setShowPassword] = useState(false);

    return (
        <TextField
            {...props}
            type={showPassword ? "text" : "password"}
            slotProps={{
                input: {
                    endAdornment: (
                        <InputAdornment position="end">
                            <IconButton
                                aria-label="toggle password visibility"
                                onClick={() => setShowPassword((prev) => !prev)}
                                onMouseDown={(e) => e.preventDefault()}
                                edge="end"
                                tabIndex={-1}
                            >
                                {showPassword ? (
                                    <VisibilityOff />
                                ) : (
                                    <Visibility />
                                )}
                            </IconButton>
                        </InputAdornment>
                    ),
                },
            }}
        />
    );
};
