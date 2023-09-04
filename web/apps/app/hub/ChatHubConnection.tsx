"use client";
import {
   HttpTransportType,
   HubConnection,
   HubConnectionBuilder,
   LogLevel,
   RetryContext,
} from "@microsoft/signalr";
import { createContext, FC, PropsWithChildren, useContext } from "react";
import { ChatifyHubClient, IChatClient } from "@web/api";

const hubConnection = new HubConnectionBuilder()
   .withUrl(`${process.env.NEXT_PUBLIC_BACKEND_API_URL!}/chat`!, {
      withCredentials: true,
      transport:
         HttpTransportType.ServerSentEvents |
         HttpTransportType.WebSockets |
         HttpTransportType.LongPolling,
   })
   .configureLogging(LogLevel.Information)
   .withAutomaticReconnect({
      nextRetryDelayInMilliseconds(retryContext: RetryContext): number | null {
         return Math.pow(2, retryContext.previousRetryCount);
      },
   })
   .build();

const HubContext = createContext<HubConnection>(null!);

export const useHubConnection = () => useContext(HubContext);

export const HubConnectionProvider: FC<PropsWithChildren> = ({ children }) => (
   <HubContext.Provider value={hubConnection}>{children}</HubContext.Provider>
);

const ChatClientContext = createContext<IChatClient>(null!);

export const useChatifyClientContext = () => useContext(ChatClientContext);

export const ChatClientProvider: FC<PropsWithChildren> = ({ children }) => (
   <ChatClientContext.Provider value={new ChatifyHubClient(hubConnection)}>
      {children}
   </ChatClientContext.Provider>
);
