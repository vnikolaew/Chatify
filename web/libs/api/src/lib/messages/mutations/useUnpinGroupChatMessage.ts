import { messagesClient } from "../client";
import {
   InfiniteData,
   useMutation,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import {
   ChatGroupDetailsEntry,
   ChatGroupMessageEntry,
   ChatMessage,
   PinnedMessage,
} from "@openapi";
import { produce } from "immer";
import { GET_PINNED_GROUP_MESSAGES_KEY } from "../../chat-groups";
import { CursorPaged } from "../../../../openapi/common/CursorPaged";
import { GET_PAGINATED_GROUP_MESSAGES_KEY } from "../queries";

export interface UnpinGroupChatMessageModel {
   messageId: string;
   groupId: string;
}

const unpinGroupChatMessage = async ({
   messageId,
}: UnpinGroupChatMessageModel) => {
   const { status, data } = await messagesClient.delete(`/pins/${messageId}`, {
      headers: {},
      data: {},
   });

   if (
      status === HttpStatusCode.BadRequest ||
      status == HttpStatusCode.NotFound
   ) {
      throw new Error("error");
   }

   return data;
};

export const useUnpinGroupChatMessage = () => {
   const client = useQueryClient();

   return useMutation<any, Error, UnpinGroupChatMessageModel, any>(
      unpinGroupChatMessage,
      {
         onError: (
            _,
            { groupId },
            {
               message,
               oldPinnedMessages,
            }: {
               message: ChatGroupMessageEntry;
               oldPinnedMessages: PinnedMessage[];
            }
         ) => {
            client.setQueryData<ChatGroupDetailsEntry>(
               [`chat-group`, groupId],
               (old: ChatGroupDetailsEntry) =>
                  produce(old, () => oldPinnedMessages)
            );

            client.setQueryData<ChatMessage[]>(
               GET_PINNED_GROUP_MESSAGES_KEY(groupId),
               (old) => {
                  return produce(old, (draft) => {
                     if (message?.message && draft) {
                        draft.push(message.message);
                     }
                     return draft;
                  });
               }
            );
         },
         onMutate: ({ messageId, groupId }) => {
            const message = client
               .getQueryData<InfiniteData<CursorPaged<ChatGroupMessageEntry>>>(
                  GET_PAGINATED_GROUP_MESSAGES_KEY(groupId)
               )
               ?.pages.flatMap((p) => p.items)
               .find((m) => m?.message?.id === messageId);

            client.setQueryData<ChatMessage[]>(
               GET_PINNED_GROUP_MESSAGES_KEY(groupId),
               (old) => {
                  return produce(old, (draft) => {
                     if (message?.message && draft) {
                        draft = draft?.filter((m) => m.id !== messageId);
                     }
                     return draft;
                  });
               }
            );

            let oldPinnedMessages: PinnedMessage[] = [];
            client.setQueryData<ChatGroupDetailsEntry>(
               [`chat-group`, groupId],
               (old: ChatGroupDetailsEntry) => {
                  return produce(old, (draft: ChatGroupDetailsEntry) => {
                     oldPinnedMessages = draft.chatGroup?.pinnedMessages;

                     draft.chatGroup.pinnedMessages =
                        draft.chatGroup?.pinnedMessages?.filter(
                           (m: PinnedMessage) => m.messageId !== messageId
                        );
                     return draft;
                  });
               }
            );

            return { message, oldPinnedMessages };
         },
         onSuccess: (data) => {
            console.log("Chat message unpinned successfully: " + data);
         },
         onSettled: (res) => console.log(res),
      }
   );
};
