import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
// @ts-ignore
import { ChatGroup, ChatGroupDetailsEntry, ChatGroupFeedEntry } from "@openapi";
import { STARRED_CHAT_GROUPS_KEY, STARRED_FEED_KEY } from "../queries";
import { produce } from "immer";

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

            // await client.invalidateQueries(STARRED_FEED_KEY, { refetchType: `none` });
            const feed = client.getQueryData<ChatGroupFeedEntry[]>([`feed`])!;
            const group = feed.find(e => e.chatGroup?.id === chatGroupId);

            client.setQueryData<ChatGroupFeedEntry[]>(STARRED_FEED_KEY, feed =>
               produce(feed, (draft: ChatGroupFeedEntry[]) =>
                  [...(draft ?? []), group]
                     .sort((a, b) =>
                        new Date(b.latestMessage?.createdAt!).getTime()
                        - new Date(a.latestMessage?.createdAt!).getTime())));
         },
      },
   );
};
