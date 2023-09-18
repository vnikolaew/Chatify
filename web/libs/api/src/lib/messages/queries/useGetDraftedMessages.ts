import { messagesClient } from "../client";
import {
   useQuery,
   useQueryClient,
   UseQueryOptions,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { ChatMessageDraft, ChatMessageDraftListApiResponse } from "@openapi";

const getDraftedMessages = async (): Promise<ChatMessageDraft[]> => {
   const { status, data } =
      await messagesClient.get<ChatMessageDraftListApiResponse>(`drafts`, {
         headers: {},
      });

   if (status === HttpStatusCode.NotFound) {
      throw new Error("error");
   }

   return data.data!;
};

export const useGetDraftedMessages = (
   options?: UseQueryOptions<
      Omit<
         UseQueryOptions<
            ChatMessageDraft[],
            Error,
            ChatMessageDraft[],
            [string]
         >,
         "initialData"
      > & { initialData?: () => undefined }
   >
) => {
   const client = useQueryClient();
   return useQuery<ChatMessageDraft[], Error, ChatMessageDraft[], [string]>(
      [`chat-message-drafts`],
      // @ts-ignore
      {
         queryFn: getDraftedMessages,
         onError: console.error,
         onSuccess: (data: ChatMessageDraft[]) =>
            console.log("Chat message drafts: " + data),
         onSettled: (res: any) => console.log(res),
         ...options,
      }
   );
};
