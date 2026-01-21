import { useState, useCallback, useRef, useEffect } from "react";

import { dictionaryApi } from "../../../api/dictionaryApi";
import { mapDictionaryResult } from "../api/mappers";
import { LookupState, UseWordLookupResult } from "../types";

const DEBOUNCE_DELAY_MS = 500;
const MIN_WORD_LENGTH = 2;

interface UseWordLookupOptions {
    readonly onError?: (message: string) => void;
}

export function useWordLookup(
    options?: UseWordLookupOptions
): UseWordLookupResult {
    const [word, setWordState] = useState("");
    const [lookupState, setLookupState] = useState<LookupState>({
        status: "idle",
    });

    const abortControllerRef = useRef<AbortController | null>(null);
    const debounceTimerRef = useRef<NodeJS.Timeout | null>(null);

    const performLookup = useCallback(
        async (searchWord: string) => {
            if (abortControllerRef.current) {
                abortControllerRef.current.abort();
            }

            abortControllerRef.current = new AbortController();
            setLookupState({ status: "loading", streamingText: "" });

            await dictionaryApi.lookupStream(
                searchWord,
                {
                    onText: (text) =>
                        setLookupState({
                            status: "loading",
                            streamingText: text,
                        }),
                    onResult: (response) => {
                        const result = mapDictionaryResult(response);
                        setLookupState({ status: "success", result });
                    },
                    onError: (error) => {
                        if (error.name !== "AbortError") {
                            setLookupState({ status: "error" });
                            options?.onError?.("Failed to look up word");
                        }
                    },
                    onComplete: () => {
                        setLookupState((current) => {
                            if (current.status === "loading") {
                                return { status: "empty" };
                            }
                            return current;
                        });
                    },
                },
                abortControllerRef.current.signal
            );
        },
        [options]
    );

    const setWord = useCallback(
        (value: string) => {
            setWordState(value);

            if (debounceTimerRef.current) {
                clearTimeout(debounceTimerRef.current);
            }

            if (value.trim().length >= MIN_WORD_LENGTH) {
                debounceTimerRef.current = setTimeout(() => {
                    performLookup(value.trim());
                }, DEBOUNCE_DELAY_MS);
            } else {
                setLookupState({ status: "idle" });
            }
        },
        [performLookup]
    );

    const clear = useCallback(() => {
        setWordState("");
        setLookupState({ status: "idle" });
        if (abortControllerRef.current) {
            abortControllerRef.current.abort();
        }
        if (debounceTimerRef.current) {
            clearTimeout(debounceTimerRef.current);
        }
    }, []);

    const reset = useCallback(() => {
        clear();
    }, [clear]);

    useEffect(() => {
        return () => {
            if (abortControllerRef.current) {
                abortControllerRef.current.abort();
            }
            if (debounceTimerRef.current) {
                clearTimeout(debounceTimerRef.current);
            }
        };
    }, []);

    return {
        word,
        lookupState,
        setWord,
        clear,
        reset,
    };
}
