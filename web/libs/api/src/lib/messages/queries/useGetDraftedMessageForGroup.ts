import { messagesClient } from "../client";
import {
   QueryKey,
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
   return useQuery<ChatMessageDraft, Error, ChatMessageDraft, QueryKey>({
      queryKey: [`chat-message-draft`, groupId] as QueryKey,
      queryFn: ({ queryKey: [_, id] }) =>
         getDraftedMessageForGroup(id as string),
      onSuccess: (data) => console.log("Chat message draft: "),
      ...options,
   });
};
