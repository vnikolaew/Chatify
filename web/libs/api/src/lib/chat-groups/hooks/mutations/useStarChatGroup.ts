import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
// @ts-ignore
import { ChatGroup, ChatGroupDetailsEntry } from "@openapi";
import { STARRED_CHAT_GROUPS_KEY, STARRED_FEED_KEY } from "../queries";

export interface StarChatGroup {
   chatGroupId: string;
}

const starChatGroup = async ({ chatGroupId }: StarChatGroup) => {
   const { status, data } = await chatGroupsClient.post(`starred/${chatGroupId}`, {
      headers: {},
   });

   if (status !== HttpStatusCode.Accepted) {
      throw new Error("error");
   }

   return data;
};

export const useStarChatGroup = () => {
   const client = useQueryClient();

   return useMutation<any, Error, StarChatGroup, any>(
      starChatGroup,
      {
         onError: console.error,
         onSuccess: async (_, { chatGroupId }) => {
            const starredChatGroup = client.getQueryData<ChatGroupDetailsEntry>([`chat-group`, chatGroupId]);

            client.setQueryData<ChatGroup[]>(STARRED_CHAT_GROUPS_KEY, groups =>
               [...(groups ?? []), starredChatGroup.chatGroup]);
            await client.invalidateQueries(STARRED_FEED_KEY, { refetchType: `none` });
         },
      },
   );
};
