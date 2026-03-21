import { describe, it, expect } from "vitest";
import { render, screen, fireEvent, act } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { PasswordField } from "../../../src/shared/components/PasswordField";

describe("PasswordField", () => {
    it("should render as a password field by default", () => {
        render(<PasswordField label="Password" id="password" />);

        expect(screen.getByLabelText("Password")).toHaveAttribute(
            "type",
            "password"
        );
    });

    it("should toggle to text and back to password on successive clicks", async () => {
        render(<PasswordField label="Password" id="password" />);
        const toggleButton = screen.getByRole("button", {
            name: "toggle password visibility",
        });

        await userEvent.click(toggleButton);
        expect(screen.getByLabelText("Password")).toHaveAttribute(
            "type",
            "text"
        );

        await userEvent.click(toggleButton);
        expect(screen.getByLabelText("Password")).toHaveAttribute(
            "type",
            "password"
        );
    });

    it("should forward standard TextField props", () => {
        render(
            <PasswordField
                label="Password"
                id="password"
                disabled
                error
                helperText="Helper text"
            />
        );

        expect(screen.getByLabelText("Password")).toBeDisabled();
        expect(screen.getByText("Helper text")).toBeInTheDocument();
    });

    it("should have an accessible label on the toggle button", () => {
        render(<PasswordField label="Password" id="password" />);

        expect(
            screen.getByRole("button", { name: "toggle password visibility" })
        ).toBeInTheDocument();
    });

    it("should call preventDefault on mouse down of the toggle button to preserve input focus", async () => {
        render(<PasswordField label="Password" id="password" />);
        const toggleButton = screen.getByRole("button", {
            name: "toggle password visibility",
        });

        let defaultPrevented = true;
        await act(async () => {
            defaultPrevented = fireEvent.mouseDown(toggleButton);
        });

        expect(defaultPrevented).toBe(false);
    });
});
