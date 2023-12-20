import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
//@ts-ignore
import { ChatGroup } from "@openapi";
import { STARRED_CHAT_GROUPS_KEY, STARRED_FEED_KEY } from "../queries";


export interface UnstarChatGroup {
   chatGroupId: string;
}

const unstarChatGroup = async ({ chatGroupId }: UnstarChatGroup) => {
   const { status, data } = await chatGroupsClient.delete(`starred/${chatGroupId}`, {
      headers: {},
   });

   if (status !== HttpStatusCode.Accepted) {
      throw new Error("error");
   }

   return data;
};

export const useUnstarChatGroup = () => {
   const client = useQueryClient();

   return useMutation<any, Error, UnstarChatGroup, any>(
      unstarChatGroup,
      {
         onError: console.error,
         onSuccess: async (_, { chatGroupId }) => {
            client.setQueryData<ChatGroup[]>(STARRED_CHAT_GROUPS_KEY, groups =>
               (groups ?? []).filter(g => g.id !== chatGroupId));

            await client.invalidateQueries(STARRED_FEED_KEY, { refetchType: `none` });
         },
      },
   );
};
