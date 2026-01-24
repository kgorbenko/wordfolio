import { screen, within } from "@testing-library/react";

import { EntryFormValues } from "../../../../src/features/entries/types";

export interface ExampleData {
    readonly text: string;
    readonly error: string | null;
}

export interface DefinitionData {
    readonly text: string;
    readonly error: string | null;
    readonly examples: ExampleData[];
}

export interface TranslationData {
    readonly text: string;
    readonly error: string | null;
    readonly examples: ExampleData[];
}

export interface FormData {
    readonly entryText: string | null;
    readonly entryTextError: string | null;
    readonly definitions: DefinitionData[];
    readonly translations: TranslationData[];
}

export interface FormDataInput {
    readonly entryText?: string;
    readonly definitions?: Array<{
        readonly text: string;
        readonly examples?: Array<{ readonly text: string }>;
    }>;
    readonly translations?: Array<{
        readonly text: string;
        readonly examples?: Array<{ readonly text: string }>;
    }>;
}

const getTextFieldData = (
    container: HTMLElement
): { text: string; error: string | null } => {
    const input = within(container).getByRole<HTMLInputElement>("textbox");
    const helperText = container.querySelector(".MuiFormHelperText-root");

    return {
        text: input.value,
        error: helperText?.textContent || null,
    };
};

const readEntryTextField = (): {
    text: string | null;
    error: string | null;
} => {
    const field = screen.queryByTestId("entry-text-field");
    if (!field) {
        return { text: null, error: null };
    }
    const data = getTextFieldData(field);
    return { text: data.text, error: data.error };
};

const readItemsFromSection = (sectionTestId: string): DefinitionData[] => {
    const section = screen.queryByTestId(sectionTestId);
    if (!section) {
        return [];
    }

    const cards = within(section).queryAllByTestId("card");
    return cards.map((card) => {
        const textField = within(card).getByTestId("text-field");
        const { text, error } = getTextFieldData(textField);

        const exampleContainers = within(card).queryAllByTestId("example");
        const examples = exampleContainers.map((container) => {
            const exampleTextField =
                within(container).getByTestId("example-text-field");
            return getTextFieldData(exampleTextField);
        });

        return { text, error, examples };
    });
};

export const readFormData = (): FormData => {
    const { text: entryText, error: entryTextError } = readEntryTextField();

    return {
        entryText,
        entryTextError,
        definitions: readItemsFromSection("definitions-section"),
        translations: readItemsFromSection("translations-section"),
    };
};

export const createFormValues = (
    input: FormDataInput = {}
): EntryFormValues => {
    return {
        entryText: input.entryText ?? "",
        definitions: (input.definitions ?? []).map((d, i) => ({
            id: `def-${i}`,
            definitionText: d.text,
            source: "Manual" as const,
            examples: (d.examples ?? []).map((ex, j) => ({
                id: `ex-${i}-${j}`,
                exampleText: ex.text,
                source: "Custom" as const,
            })),
        })),
        translations: (input.translations ?? []).map((t, i) => ({
            id: `trans-${i}`,
            translationText: t.text,
            source: "Manual" as const,
            examples: (t.examples ?? []).map((ex, j) => ({
                id: `ex-${i}-${j}`,
                exampleText: ex.text,
                source: "Custom" as const,
            })),
        })),
    };
};

const getCardsInSection = (sectionTestId: string): HTMLElement[] => {
    const section = screen.getByTestId(sectionTestId);
    return within(section).queryAllByTestId("card");
};

const getExamplesInCard = (card: HTMLElement): HTMLElement[] =>
    within(card).queryAllByTestId("example");

export const getAddDefinitionButton = (): HTMLElement => {
    const section = screen.getByTestId("definitions-section");
    return within(section).getByTestId("add-button");
};

export const getAddTranslationButton = (): HTMLElement => {
    const section = screen.getByTestId("translations-section");
    return within(section).getByTestId("add-button");
};

export const getDeleteDefinitionButton = (index: number): HTMLElement => {
    const cards = getCardsInSection("definitions-section");
    return within(cards[index]).getByTestId("delete-button");
};

export const getDeleteTranslationButton = (index: number): HTMLElement => {
    const cards = getCardsInSection("translations-section");
    return within(cards[index]).getByTestId("delete-button");
};

export const getDefinitionInput = (index: number): HTMLElement => {
    const cards = getCardsInSection("definitions-section");
    const textField = within(cards[index]).getByTestId("text-field");
    return within(textField).getByRole("textbox");
};

export const getTranslationInput = (index: number): HTMLElement => {
    const cards = getCardsInSection("translations-section");
    const textField = within(cards[index]).getByTestId("text-field");
    return within(textField).getByRole("textbox");
};

export const getAddDefinitionExampleButton = (
    defIndex: number
): HTMLElement => {
    const cards = getCardsInSection("definitions-section");
    return within(cards[defIndex]).getByTestId("add-example-button");
};

export const getAddTranslationExampleButton = (
    transIndex: number
): HTMLElement => {
    const cards = getCardsInSection("translations-section");
    return within(cards[transIndex]).getByTestId("add-example-button");
};

export const getDeleteDefinitionExampleButton = (
    defIndex: number,
    exampleIndex: number
): HTMLElement => {
    const cards = getCardsInSection("definitions-section");
    const examples = getExamplesInCard(cards[defIndex]);
    return within(examples[exampleIndex]).getByTestId("example-delete-button");
};

export const getDeleteTranslationExampleButton = (
    transIndex: number,
    exampleIndex: number
): HTMLElement => {
    const cards = getCardsInSection("translations-section");
    const examples = getExamplesInCard(cards[transIndex]);
    return within(examples[exampleIndex]).getByTestId("example-delete-button");
};

export const getDefinitionExampleInput = (
    defIndex: number,
    exampleIndex: number
): HTMLElement => {
    const cards = getCardsInSection("definitions-section");
    const examples = getExamplesInCard(cards[defIndex]);
    const textField = within(examples[exampleIndex]).getByTestId(
        "example-text-field"
    );
    return within(textField).getByRole("textbox");
};

export const getTranslationExampleInput = (
    transIndex: number,
    exampleIndex: number
): HTMLElement => {
    const cards = getCardsInSection("translations-section");
    const examples = getExamplesInCard(cards[transIndex]);
    const textField = within(examples[exampleIndex]).getByTestId(
        "example-text-field"
    );
    return within(textField).getByRole("textbox");
};

export const getSubmitButton = (label: string = "Save"): HTMLElement => {
    return screen.getByRole("button", { name: label });
};

export const getCancelButton = (): HTMLElement => {
    return screen.getByRole("button", { name: "Cancel" });
};
