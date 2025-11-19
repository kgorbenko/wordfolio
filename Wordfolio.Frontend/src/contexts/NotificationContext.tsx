import { createContext, useContext } from "react";
import {
    OpenNotificationOptions,
    OpenSuccessNotificationOptions,
    OpenErrorNotificationOptions,
} from "./NotificationProvider";

export interface NotificationContextValue {
    readonly openNotification: (options: OpenNotificationOptions) => void;
    readonly openSuccessNotification: (
        options?: OpenSuccessNotificationOptions
    ) => void;
    readonly openErrorNotification: (
        options?: OpenErrorNotificationOptions
    ) => void;
}

export const NotificationContext = createContext<
    NotificationContextValue | undefined
>(undefined);

export const useNotificationContext = (): NotificationContextValue => {
    const context = useContext(NotificationContext);
    if (context === undefined) {
        throw new Error(
            "useNotificationContext must be used within a NotificationProvider"
        );
    }
    return context;
};
