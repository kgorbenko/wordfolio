import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor, act } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createRef } from "react";

import {
    EntryForm,
    EntryFormHandle,
} from "../../../../src/features/entries/components/EntryForm";
import {
    createFormValues,
    readFormData,
    getAddDefinitionButton,
    getAddTranslationButton,
    getDeleteDefinitionButton,
    getDeleteTranslationButton,
    getDefinitionInput,
    getTranslationInput,
    getAddDefinitionExampleButton,
    getAddTranslationExampleButton,
    getDeleteDefinitionExampleButton,
    getDeleteTranslationExampleButton,
    getDefinitionExampleInput,
    getTranslationExampleInput,
    getSubmitButton,
    getCancelButton,
} from "./entryFormTestUtils";

describe("EntryForm", () => {
    const defaultProps = {
        onSubmit: vi.fn(),
        onCancel: vi.fn(),
        submitLabel: "Save",
    };

    beforeEach(() => {
        vi.clearAllMocks();
    });

    describe("rendering & props", () => {
        it("renders with default state", () => {
            render(<EntryForm {...defaultProps} />);

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [],
                translations: [],
            });

            expect(screen.getByLabelText("Word or Phrase")).toBeInTheDocument();
            expect(getSubmitButton()).toBeInTheDocument();
            expect(getCancelButton()).toBeInTheDocument();
        });

        it("hides entry text field when showEntryText=false", () => {
            render(<EntryForm {...defaultProps} showEntryText={false} />);

            expect(readFormData()).toEqual({
                entryText: null,
                entryTextError: null,
                definitions: [],
                translations: [],
            });

            expect(
                screen.queryByLabelText("Word or Phrase")
            ).not.toBeInTheDocument();
        });

        it("hides footer when showFooter=false", () => {
            render(<EntryForm {...defaultProps} showFooter={false} />);

            expect(
                screen.queryByRole("button", { name: "Save" })
            ).not.toBeInTheDocument();
            expect(
                screen.queryByRole("button", { name: "Cancel" })
            ).not.toBeInTheDocument();
        });

        it("displays custom submit label", () => {
            render(<EntryForm {...defaultProps} submitLabel="Create Entry" />);

            expect(getSubmitButton("Create Entry")).toBeInTheDocument();
        });

        it("populates form from defaultValues", () => {
            render(
                <EntryForm
                    {...defaultProps}
                    defaultValues={createFormValues({
                        entryText: "hello",
                        definitions: [
                            {
                                text: "a greeting",
                                examples: [{ text: "Hello there!" }],
                            },
                        ],
                        translations: [
                            {
                                text: "hola",
                                examples: [{ text: "¡Hola amigo!" }],
                            },
                        ],
                    })}
                />
            );

            expect(readFormData()).toEqual({
                entryText: "hello",
                entryTextError: null,
                definitions: [
                    {
                        text: "a greeting",
                        error: null,
                        examples: [
                            {
                                text: "Hello there!",
                                error: null,
                            },
                        ],
                    },
                ],
                translations: [
                    {
                        text: "hola",
                        error: null,
                        examples: [
                            {
                                text: "¡Hola amigo!",
                                error: null,
                            },
                        ],
                    },
                ],
            });
        });
    });

    describe("loading state", () => {
        it("disables entry text field when loading", () => {
            render(<EntryForm {...defaultProps} isLoading={true} />);

            expect(screen.getByLabelText("Word or Phrase")).toBeDisabled();
        });

        it("disables add buttons when loading", () => {
            render(<EntryForm {...defaultProps} isLoading={true} />);

            expect(getAddDefinitionButton()).toBeDisabled();
            expect(getAddTranslationButton()).toBeDisabled();
        });

        it("disables footer buttons when loading", () => {
            render(<EntryForm {...defaultProps} isLoading={true} />);

            expect(
                screen.getByRole("button", { name: "Saving..." })
            ).toBeDisabled();
            expect(getCancelButton()).toBeDisabled();
        });

        it("shows 'Saving...' on submit button when loading", () => {
            render(<EntryForm {...defaultProps} isLoading={true} />);

            expect(
                screen.getByRole("button", { name: "Saving..." })
            ).toBeInTheDocument();
            expect(
                screen.queryByRole("button", { name: "Save" })
            ).not.toBeInTheDocument();
        });

        it("disables definition text fields when loading", () => {
            render(
                <EntryForm
                    {...defaultProps}
                    isLoading={true}
                    defaultValues={createFormValues({
                        entryText: "test",
                        definitions: [{ text: "a test" }],
                    })}
                />
            );

            expect(getDefinitionInput(0)).toBeDisabled();
        });
    });

    describe("definition management", () => {
        it("shows empty message when no definitions", () => {
            render(<EntryForm {...defaultProps} />);

            expect(screen.getByText("No definitions yet")).toBeInTheDocument();
        });

        it("adds a definition", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            await user.click(getAddDefinitionButton());

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [
                    {
                        text: "",
                        error: null,
                        examples: [],
                    },
                ],
                translations: [],
            });
            expect(
                screen.queryByText("No definitions yet")
            ).not.toBeInTheDocument();
        });

        it("adds multiple definitions", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            const addDefinitionButton = getAddDefinitionButton();
            await user.click(addDefinitionButton);
            await user.click(addDefinitionButton);
            await user.click(addDefinitionButton);

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [
                    { text: "", error: null, examples: [] },
                    { text: "", error: null, examples: [] },
                    { text: "", error: null, examples: [] },
                ],
                translations: [],
            });
        });

        it("removes a definition", async () => {
            const user = userEvent.setup();
            render(
                <EntryForm
                    {...defaultProps}
                    defaultValues={createFormValues({
                        definitions: [{ text: "def 1" }, { text: "def 2" }],
                    })}
                />
            );

            const deleteButton = getDeleteDefinitionButton(0);
            await user.click(deleteButton);

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [{ text: "def 2", error: null, examples: [] }],
                translations: [],
            });
        });

        it("updates definition text", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            await user.click(getAddDefinitionButton());
            await user.type(getDefinitionInput(0), "a greeting");

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [
                    { text: "a greeting", error: null, examples: [] },
                ],
                translations: [],
            });
        });
    });

    describe("translation management", () => {
        it("shows empty message when no translations", () => {
            render(<EntryForm {...defaultProps} />);

            expect(screen.getByText("No translations yet")).toBeInTheDocument();
        });

        it("adds a translation", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            await user.click(getAddTranslationButton());

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [],
                translations: [
                    {
                        text: "",
                        error: null,
                        examples: [],
                    },
                ],
            });
            expect(
                screen.queryByText("No translations yet")
            ).not.toBeInTheDocument();
        });

        it("adds multiple translations", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            const addTranslationButton = getAddTranslationButton();
            await user.click(addTranslationButton);
            await user.click(addTranslationButton);
            await user.click(addTranslationButton);

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [],
                translations: [
                    { text: "", error: null, examples: [] },
                    { text: "", error: null, examples: [] },
                    { text: "", error: null, examples: [] },
                ],
            });
        });

        it("removes a translation", async () => {
            const user = userEvent.setup();
            render(
                <EntryForm
                    {...defaultProps}
                    defaultValues={createFormValues({
                        translations: [
                            { text: "trans 1" },
                            { text: "trans 2" },
                        ],
                    })}
                />
            );

            const deleteButton = getDeleteTranslationButton(0);
            await user.click(deleteButton);

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [],
                translations: [{ text: "trans 2", error: null, examples: [] }],
            });
        });

        it("updates translation text", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            await user.click(getAddTranslationButton());
            await user.type(getTranslationInput(0), "hola");

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [],
                translations: [{ text: "hola", error: null, examples: [] }],
            });
        });
    });

    describe("example management (within definitions)", () => {
        it("adds an example to a definition", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            await user.click(getAddDefinitionButton());
            await user.click(getAddDefinitionExampleButton(0));

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [
                    {
                        text: "",
                        error: null,
                        examples: [{ text: "", error: null }],
                    },
                ],
                translations: [],
            });
        });

        it("adds multiple examples to a definition", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            await user.click(getAddDefinitionButton());
            await user.click(getAddDefinitionExampleButton(0));
            await user.click(getAddDefinitionExampleButton(0));

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [
                    {
                        text: "",
                        error: null,
                        examples: [
                            { text: "", error: null },
                            { text: "", error: null },
                        ],
                    },
                ],
                translations: [],
            });
        });

        it("removes an example from a definition", async () => {
            const user = userEvent.setup();
            render(
                <EntryForm
                    {...defaultProps}
                    defaultValues={createFormValues({
                        definitions: [
                            {
                                text: "def",
                                examples: [{ text: "ex 1" }, { text: "ex 2" }],
                            },
                        ],
                    })}
                />
            );

            const deleteButton = getDeleteDefinitionExampleButton(0, 0);
            await user.click(deleteButton);

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [
                    {
                        text: "def",
                        error: null,
                        examples: [{ text: "ex 2", error: null }],
                    },
                ],
                translations: [],
            });
        });

        it("updates example text in a definition", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            await user.click(getAddDefinitionButton());
            await user.click(getAddDefinitionExampleButton(0));
            await user.type(getDefinitionExampleInput(0, 0), "Hello there!");

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [
                    {
                        text: "",
                        error: null,
                        examples: [{ text: "Hello there!", error: null }],
                    },
                ],
                translations: [],
            });
        });
    });

    describe("example management (within translations)", () => {
        it("adds an example to a translation", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            await user.click(getAddTranslationButton());
            await user.click(getAddTranslationExampleButton(0));

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [],
                translations: [
                    {
                        text: "",
                        error: null,
                        examples: [{ text: "", error: null }],
                    },
                ],
            });
        });

        it("adds multiple examples to a translation", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            await user.click(getAddTranslationButton());
            await user.click(getAddTranslationExampleButton(0));
            await user.click(getAddTranslationExampleButton(0));

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [],
                translations: [
                    {
                        text: "",
                        error: null,
                        examples: [
                            { text: "", error: null },
                            { text: "", error: null },
                        ],
                    },
                ],
            });
        });

        it("removes an example from a translation", async () => {
            const user = userEvent.setup();
            render(
                <EntryForm
                    {...defaultProps}
                    defaultValues={createFormValues({
                        translations: [
                            {
                                text: "trans",
                                examples: [{ text: "ex 1" }, { text: "ex 2" }],
                            },
                        ],
                    })}
                />
            );

            await user.click(getDeleteTranslationExampleButton(0, 0));

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [],
                translations: [
                    {
                        text: "trans",
                        error: null,
                        examples: [{ text: "ex 2", error: null }],
                    },
                ],
            });
        });

        it("updates example text in a translation", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            await user.click(getAddTranslationButton());
            await user.click(getAddTranslationExampleButton(0));
            await user.type(getTranslationExampleInput(0, 0), "¡Hola amigo!");

            expect(readFormData()).toEqual({
                entryText: "",
                entryTextError: null,
                definitions: [],
                translations: [
                    {
                        text: "",
                        error: null,
                        examples: [{ text: "¡Hola amigo!", error: null }],
                    },
                ],
            });
        });
    });

    describe("form submission", () => {
        it("calls onSubmit with mapped data when valid", async () => {
            const user = userEvent.setup();
            const onSubmit = vi.fn();
            render(
                <EntryForm
                    {...defaultProps}
                    onSubmit={onSubmit}
                    defaultValues={createFormValues({
                        entryText: "hello",
                        definitions: [
                            {
                                text: "a greeting",
                                examples: [{ text: "Hello!" }],
                            },
                        ],
                    })}
                />
            );

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(onSubmit).toHaveBeenCalledWith({
                    entryText: "hello",
                    definitions: [
                        {
                            definitionText: "a greeting",
                            source: "Manual",
                            examples: [
                                { exampleText: "Hello!", source: "Custom" },
                            ],
                        },
                    ],
                    translations: [],
                });
            });
        });

        it("blocks submission when form is invalid", async () => {
            const user = userEvent.setup();
            const onSubmit = vi.fn();
            render(<EntryForm {...defaultProps} onSubmit={onSubmit} />);

            await user.click(getSubmitButton());

            expect(onSubmit).not.toHaveBeenCalled();
        });

        it("maps definitions without id field", async () => {
            const user = userEvent.setup();
            const onSubmit = vi.fn();
            render(
                <EntryForm
                    {...defaultProps}
                    onSubmit={onSubmit}
                    defaultValues={createFormValues({
                        entryText: "test",
                        definitions: [{ text: "test def" }],
                    })}
                />
            );

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(onSubmit).toHaveBeenCalledWith({
                    entryText: "test",
                    definitions: [
                        {
                            definitionText: "test def",
                            source: "Manual",
                            examples: [],
                        },
                    ],
                    translations: [],
                });
            });
        });

        it("maps translations without id field", async () => {
            const user = userEvent.setup();
            const onSubmit = vi.fn();
            render(
                <EntryForm
                    {...defaultProps}
                    onSubmit={onSubmit}
                    defaultValues={createFormValues({
                        entryText: "test",
                        translations: [{ text: "test trans" }],
                    })}
                />
            );

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(onSubmit).toHaveBeenCalledWith({
                    entryText: "test",
                    definitions: [],
                    translations: [
                        {
                            translationText: "test trans",
                            source: "Manual",
                            examples: [],
                        },
                    ],
                });
            });
        });

        it("maps examples without id field", async () => {
            const user = userEvent.setup();
            const onSubmit = vi.fn();
            render(
                <EntryForm
                    {...defaultProps}
                    onSubmit={onSubmit}
                    defaultValues={createFormValues({
                        entryText: "test",
                        definitions: [
                            {
                                text: "test def",
                                examples: [{ text: "example" }],
                            },
                        ],
                    })}
                />
            );

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(onSubmit).toHaveBeenCalledWith({
                    entryText: "test",
                    definitions: [
                        {
                            definitionText: "test def",
                            source: "Manual",
                            examples: [
                                { exampleText: "example", source: "Custom" },
                            ],
                        },
                    ],
                    translations: [],
                });
            });
        });
    });

    describe("validation", () => {
        it("shows error for empty entry text", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "",
                    entryTextError: "Entry text is required",
                    definitions: [],
                    translations: [],
                });
            });
        });

        it("shows error when entry text exceeds 255 characters", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            const longText = "a".repeat(256);
            const entryTextInput = screen.getByLabelText("Word or Phrase");
            await user.type(entryTextInput, longText);

            await user.click(getAddDefinitionButton());
            await user.type(getDefinitionInput(0), "test def");

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: longText,
                    entryTextError: "Entry text must be at most 255 characters",
                    definitions: [
                        { text: "test def", error: null, examples: [] },
                    ],
                    translations: [],
                });
            });
        });

        it("shows error when no definitions or translations", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            const entryTextInput = screen.getByLabelText("Word or Phrase");
            await user.type(entryTextInput, "hello");

            await user.click(getSubmitButton());

            expect(
                screen.getByText(
                    "At least one definition or translation is required"
                )
            ).toBeInTheDocument();
        });

        it("shows error for entry text with leading or trailing whitespace", async () => {
            const user = userEvent.setup();
            const onSubmit = vi.fn();
            render(<EntryForm {...defaultProps} onSubmit={onSubmit} />);

            const entryTextInput = screen.getByLabelText("Word or Phrase");
            await user.type(entryTextInput, "  hello world  ");
            await user.click(getAddDefinitionButton());
            await user.type(getDefinitionInput(0), "a greeting");

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "  hello world  ",
                    entryTextError:
                        "Cannot have leading or trailing whitespace",
                    definitions: [
                        { text: "a greeting", error: null, examples: [] },
                    ],
                    translations: [],
                });
            });
            expect(onSubmit).not.toHaveBeenCalled();
        });

        it("shows error for definition text with leading or trailing whitespace", async () => {
            const user = userEvent.setup();
            render(
                <EntryForm
                    {...defaultProps}
                    defaultValues={createFormValues({
                        entryText: "hello",
                        definitions: [{ text: "  a greeting  " }],
                    })}
                />
            );

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "hello",
                    entryTextError: null,
                    definitions: [
                        {
                            text: "  a greeting  ",
                            error: "Cannot have leading or trailing whitespace",
                            examples: [],
                        },
                    ],
                    translations: [],
                });
            });
        });

        it("shows error for translation text with leading or trailing whitespace", async () => {
            const user = userEvent.setup();
            render(
                <EntryForm
                    {...defaultProps}
                    defaultValues={createFormValues({
                        entryText: "hello",
                        translations: [{ text: "  hola  " }],
                    })}
                />
            );

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "hello",
                    entryTextError: null,
                    definitions: [],
                    translations: [
                        {
                            text: "  hola  ",
                            error: "Cannot have leading or trailing whitespace",
                            examples: [],
                        },
                    ],
                });
            });
        });

        it("shows error for example text with leading or trailing whitespace in definition", async () => {
            const user = userEvent.setup();
            render(
                <EntryForm
                    {...defaultProps}
                    defaultValues={createFormValues({
                        entryText: "hello",
                        definitions: [
                            {
                                text: "a greeting",
                                examples: [{ text: "  Hello!  " }],
                            },
                        ],
                    })}
                />
            );

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "hello",
                    entryTextError: null,
                    definitions: [
                        {
                            text: "a greeting",
                            error: null,
                            examples: [
                                {
                                    text: "  Hello!  ",
                                    error: "Cannot have leading or trailing whitespace",
                                },
                            ],
                        },
                    ],
                    translations: [],
                });
            });
        });

        it("shows error for example text with leading or trailing whitespace in translation", async () => {
            const user = userEvent.setup();
            render(
                <EntryForm
                    {...defaultProps}
                    defaultValues={createFormValues({
                        entryText: "hello",
                        translations: [
                            {
                                text: "hola",
                                examples: [{ text: "  ¡Hola!  " }],
                            },
                        ],
                    })}
                />
            );

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "hello",
                    entryTextError: null,
                    definitions: [],
                    translations: [
                        {
                            text: "hola",
                            error: null,
                            examples: [
                                {
                                    text: "  ¡Hola!  ",
                                    error: "Cannot have leading or trailing whitespace",
                                },
                            ],
                        },
                    ],
                });
            });
        });

        it("shows error for empty definition text", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            const entryTextInput = screen.getByLabelText("Word or Phrase");
            await user.type(entryTextInput, "hello");
            await user.click(getAddDefinitionButton());

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "hello",
                    entryTextError: null,
                    definitions: [
                        {
                            text: "",
                            error: "Definition text is required",
                            examples: [],
                        },
                    ],
                    translations: [],
                });
            });
        });

        it("shows error when definition text exceeds 255 characters", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            const longText = "a".repeat(256);
            const entryTextInput = screen.getByLabelText("Word or Phrase");
            await user.type(entryTextInput, "hello");
            await user.click(getAddDefinitionButton());
            await user.paste(longText);

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "hello",
                    entryTextError: null,
                    definitions: [
                        {
                            text: longText,
                            error: "Definition must be at most 255 characters",
                            examples: [],
                        },
                    ],
                    translations: [],
                });
            });
        });

        it("shows error for empty translation text", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            const entryTextInput = screen.getByLabelText("Word or Phrase");
            await user.type(entryTextInput, "hello");
            await user.click(getAddTranslationButton());

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "hello",
                    entryTextError: null,
                    definitions: [],
                    translations: [
                        {
                            text: "",
                            error: "Translation text is required",
                            examples: [],
                        },
                    ],
                });
            });
        });

        it("shows error when translation text exceeds 255 characters", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            const longText = "a".repeat(256);
            const entryTextInput = screen.getByLabelText("Word or Phrase");
            await user.type(entryTextInput, "hello");
            await user.click(getAddTranslationButton());
            await user.paste(longText);

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "hello",
                    entryTextError: null,
                    definitions: [],
                    translations: [
                        {
                            text: longText,
                            error: "Translation must be at most 255 characters",
                            examples: [],
                        },
                    ],
                });
            });
        });

        it("shows error for empty example text in definition", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            const entryTextInput = screen.getByLabelText("Word or Phrase");
            await user.type(entryTextInput, "hello");
            await user.click(getAddDefinitionButton());
            await user.type(getDefinitionInput(0), "a greeting");
            await user.click(getAddDefinitionExampleButton(0));

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "hello",
                    entryTextError: null,
                    definitions: [
                        {
                            text: "a greeting",
                            error: null,
                            examples: [
                                {
                                    text: "",
                                    error: "Example text is required",
                                },
                            ],
                        },
                    ],
                    translations: [],
                });
            });
        });

        it("shows error for empty example text in translation", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            const entryTextInput = screen.getByLabelText("Word or Phrase");
            await user.type(entryTextInput, "hello");
            await user.click(getAddTranslationButton());
            await user.type(getTranslationInput(0), "hola");
            await user.click(getAddTranslationExampleButton(0));

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "hello",
                    entryTextError: null,
                    definitions: [],
                    translations: [
                        {
                            text: "hola",
                            error: null,
                            examples: [
                                {
                                    text: "",
                                    error: "Example text is required",
                                },
                            ],
                        },
                    ],
                });
            });
        });

        it("shows error when example text exceeds 500 characters", async () => {
            const user = userEvent.setup();
            render(<EntryForm {...defaultProps} />);

            const longText = "a".repeat(501);
            const entryTextInput = screen.getByLabelText("Word or Phrase");
            await user.type(entryTextInput, "hello");
            await user.click(getAddDefinitionButton());
            await user.type(getDefinitionInput(0), "a greeting");
            await user.click(getAddDefinitionExampleButton(0));
            await user.paste(longText);

            await user.click(getSubmitButton());

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "hello",
                    entryTextError: null,
                    definitions: [
                        {
                            text: "a greeting",
                            error: null,
                            examples: [
                                {
                                    text: longText,
                                    error: "Example must be at most 500 characters",
                                },
                            ],
                        },
                    ],
                    translations: [],
                });
            });
        });
    });

    describe("cancel & imperative handle", () => {
        it("calls onCancel when cancel button is clicked", async () => {
            const user = userEvent.setup();
            const onCancel = vi.fn();
            render(<EntryForm {...defaultProps} onCancel={onCancel} />);

            await user.click(getCancelButton());

            expect(onCancel).toHaveBeenCalledTimes(1);
        });

        it("submits valid form via ref.submit()", async () => {
            const onSubmit = vi.fn();
            const formRef = createRef<EntryFormHandle>();
            render(
                <EntryForm
                    ref={formRef}
                    {...defaultProps}
                    onSubmit={onSubmit}
                    defaultValues={createFormValues({
                        entryText: "hello",
                        definitions: [{ text: "a greeting" }],
                    })}
                />
            );

            act(() => {
                formRef.current?.submit();
            });

            await waitFor(() => {
                expect(onSubmit).toHaveBeenCalled();
            });
        });

        it("validates form and shows errors via ref.submit() when invalid", async () => {
            const onSubmit = vi.fn();
            const formRef = createRef<EntryFormHandle>();
            render(
                <EntryForm
                    ref={formRef}
                    {...defaultProps}
                    onSubmit={onSubmit}
                />
            );

            act(() => {
                formRef.current?.submit();
            });

            await waitFor(() => {
                expect(readFormData()).toEqual({
                    entryText: "",
                    entryTextError: "Entry text is required",
                    definitions: [],
                    translations: [],
                });
            });
            expect(onSubmit).not.toHaveBeenCalled();
        });
    });
});
