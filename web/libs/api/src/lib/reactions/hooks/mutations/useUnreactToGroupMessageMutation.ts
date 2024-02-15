import {
   InfiniteData,
   useMutation,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { reactionsClient } from "../../client";
// @ts-ignore
import { ChatGroupMessageEntry, ChatMessageReaction } from "@openapi";
import { CursorPaged } from "../../../../../openapi/common/CursorPaged";
import { produce } from "immer";
import { GET_PAGINATED_GROUP_MESSAGES_KEY } from "../../../messages";
import { GET_ALL_REACTIONS_KEY } from "../queries";

export interface UnreactToGroupMessageModel {
   messageId: string;
   groupId: string;
   messageReactionId: string;
}

const unreactToGroupMessage = async (model: UnreactToGroupMessageModel) => {
   const { messageReactionId, ...request } = model;

   const { status, data } = await reactionsClient.delete(
      `${messageReactionId}`,
      {
         headers: {},
         data: request,
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useUnreactToGroupMessageMutation = () => {
   const client = useQueryClient();

   return useMutation<string, Error, UnreactToGroupMessageModel, any>({
      mutationFn: unreactToGroupMessage,
      onError: console.error,
      onSuccess: (data, { messageReactionId, messageId, groupId }) => {
         console.log("Successfully unreacted to chat group message: " + data);

         const reaction = client
            .getQueryData<ChatMessageReaction[]>(
               GET_ALL_REACTIONS_KEY(messageId)
            )!
            .find((r) => r.id === messageReactionId);

         client.setQueryData<ChatMessageReaction[]>(
            GET_ALL_REACTIONS_KEY(messageId),
            (reactions) => {
               return [
                  ...(reactions ?? []).filter(
                     (r) => r.id !== messageReactionId
                  ),
               ];
            }
         );

         client.setQueryData<InfiniteData<CursorPaged<ChatGroupMessageEntry>>>(
            GET_PAGINATED_GROUP_MESSAGES_KEY(groupId),
            (old) => {
               // Update reaction counts for the message:
               return produce(old, (draft) => {
                  const message = draft!.pages
                     .flatMap((p) => p.items)
                     .find(
                        (m) => m.message?.id === messageId
                     ) as ChatGroupMessageEntry;

                  if (message) {
                     message.message.reactionCounts[
                        reaction.reactionCode.toString()
                     ] =
                        Number(
                           message.message?.reactionCounts[
                              reaction.reactionCode.toString() ?? 0
                           ]
                        ) - 1;
                  }
               });
            }
         );
      },
      onSettled: (res) => console.log(res),
   });
};
