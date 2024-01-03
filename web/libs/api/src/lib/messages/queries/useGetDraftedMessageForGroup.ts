import { messagesClient } from "../client";
import {
   useQuery,
   useQueryClient,
   UseQueryOptions,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { ChatMessageDraft } from "@openapi";

const getDraftedMessageForGroup = async (
   groupId: string
): Promise<ChatMessageDraft> => {
   const { status, data } = await messagesClient.get<ChatMessageDraft>(
      `drafts/${groupId}`,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.NotFound) {
      return null!;
   }

   return data.data!;
};

export const useGetDraftedMessageForGroup = (
   groupId: string,
   options?: UseQueryOptions<
      Omit<
         UseQueryOptions<
            ChatMessageDraft,
            Error,
            ChatMessageDraft,
            [string, string]
         >,
         "initialData"
      > & { initialData?: () => undefined }
   >
) => {
   const client = useQueryClient();
   return useQuery<ChatMessageDraft, Error, ChatMessageDraft, [string, string]>(
      [`chat-message-draft`, groupId],
      // @ts-ignore
      ({ queryKey: [_, id] }) => getDraftedMessageForGroup(id as string),
      // @ts-ignore
      {
         // onSuccess: (data) => console.log("Chat message draft: "),
         ...options,
      }
   );
};
