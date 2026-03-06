import { createParser, EventSourceMessage } from "eventsource-parser";

import { useAuthStore } from "../stores/authStore";

export interface DictionaryDefinition {
    readonly definition: string;
    readonly partOfSpeech: "verb" | "noun" | "adj" | "adv" | null;
    readonly exampleSentences: string[];
}

export interface DictionaryTranslationExample {
    readonly russian: string;
    readonly english: string;
}

export interface DictionaryTranslation {
    readonly translation: string;
    readonly partOfSpeech: "verb" | "noun" | "adj" | "adv" | null;
    readonly examples: DictionaryTranslationExample[];
}

export interface DictionaryResult {
    readonly definitions: DictionaryDefinition[];
    readonly translations: DictionaryTranslation[];
}

export interface DictionaryStreamCallbacks {
    onText?: (text: string) => void;
    onResult?: (result: DictionaryResult) => void;
    onError?: (error: Error) => void;
    onComplete?: () => void;
}

const API_BASE_URL = "/api";

const getAuthHeaders = (): HeadersInit => {
    const authTokens = useAuthStore.getState().authTokens;
    if (!authTokens) {
        throw new Error("Not authenticated");
    }
    return {
        Accept: "text/event-stream",
        Authorization: `${authTokens.tokenType} ${authTokens.accessToken}`,
    };
};

export const dictionaryApi = {
    lookupStream: async (
        text: string,
        callbacks: DictionaryStreamCallbacks,
        signal?: AbortSignal
    ): Promise<void> => {
        let response: Response;

        try {
            response = await fetch(
                `${API_BASE_URL}/dictionary/lookup?text=${encodeURIComponent(text)}`,
                {
                    method: "GET",
                    headers: getAuthHeaders(),
                    signal,
                }
            );
        } catch (error) {
            if (error instanceof Error && error.name === "AbortError") {
                return;
            }
            callbacks.onError?.(
                error instanceof Error ? error : new Error("Network error")
            );
            return;
        }

        if (!response.ok) {
            const errorText = await response.text();
            callbacks.onError?.(new Error(errorText || "Lookup failed"));
            return;
        }

        const reader = response.body?.getReader();
        if (!reader) {
            callbacks.onError?.(new Error("No response body"));
            return;
        }

        const decoder = new TextDecoder();
        let accumulatedText = "";

        const parser = createParser({
            onEvent: (event: EventSourceMessage) => {
                const { event: eventName, data } = event;

                if (eventName === "text") {
                    accumulatedText += data;
                    callbacks.onText?.(accumulatedText);
                } else if (eventName === "result") {
                    try {
                        const result: DictionaryResult = JSON.parse(data);
                        callbacks.onResult?.(result);
                    } catch {
                        callbacks.onError?.(
                            new Error("Failed to parse result")
                        );
                    }
                }
            },
        });

        try {
            while (true) {
                const { done, value } = await reader.read();
                if (done) break;

                const chunk = decoder.decode(value, { stream: true });
                parser.feed(chunk);
            }

            callbacks.onComplete?.();
        } catch (error) {
            if (error instanceof Error && error.name === "AbortError") {
                return;
            }
            callbacks.onError?.(
                error instanceof Error ? error : new Error("Stream failed")
            );
        } finally {
            reader.releaseLock();
        }
    },
};
