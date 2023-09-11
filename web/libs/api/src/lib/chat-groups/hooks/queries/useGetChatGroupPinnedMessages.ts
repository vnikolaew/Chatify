import {
   UseQueryOptions,
   useQueryClient,
   useQuery,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import {
   ChatMessage,
   ChatMessageListApiResponse,
   // @ts-ignore
} from "@openapi";
// @ts-ignore
import { sleep } from "../../../utils";
import { chatGroupsClient } from "../../client";

export interface GetChatGroupPinnedMessagesModel {
   groupId: string;
}

const getChatGroupPinnedMessages = async (
   model: GetChatGroupPinnedMessagesModel
): Promise<ChatMessage[]> => {
   const { groupId } = model;
   const { status, data } =
      await chatGroupsClient.get<ChatMessageListApiResponse>(
         `${groupId}/pins`,
         {
            headers: {},
         }
      );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }
   await sleep(2000);

   return data.data as ChatMessage[];
};

export const GET_PINNED_GROUP_MESSAGES_KEY = (groupId: string) => [
   `chat-group`,
   groupId,
   `pinned-messages`,
];

export const useGetChatGroupPinnedMessages = (
   { groupId }: GetChatGroupPinnedMessagesModel,
   options?: Omit<
      UseQueryOptions<ChatMessage[], Error, ChatMessage[], string[]>,
      "initialData"
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   const client = useQueryClient();
   return useQuery<ChatMessage[], Error, ChatMessage[], string[]>({
      queryKey: GET_PINNED_GROUP_MESSAGES_KEY(groupId),
      queryFn: ({ queryKey: [_, groupId, __], pageParam = null! }) => {
         return getChatGroupPinnedMessages({
            groupId,
         });
      },
      enabled: !!groupId,
      ...options,
   });
};
