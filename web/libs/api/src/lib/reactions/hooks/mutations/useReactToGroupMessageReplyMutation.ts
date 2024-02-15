import {
   InfiniteData,
   useMutation,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { reactionsClient } from "../../client";
import { ReactToGroupMessageModel } from "./useReactToGroupMessageMutation";
import { GetMyClaimsResponse } from "../../../auth";
import { ChatMessageReaction, ChatMessageReply, CursorPaged } from "@openapi";
import { GET_ALL_REACTIONS_KEY } from "../queries";
import { GET_PAGINATED_GROUP_MESSAGE_REPLIES_KEY } from "../../../messages";
import { produce } from "immer";

export interface ReactToGroupMessageReplyModel
   extends ReactToGroupMessageModel {}

const reactToGroupMessageReply = async (
   model: ReactToGroupMessageReplyModel
) => {
   const { messageId, ...request } = model;
   const { status, data } = await reactionsClient.post(
      `replies/${messageId}`,
      request,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useReactToGroupMessageReplyMutation = () => {
   const client = useQueryClient();

   return useMutation<any, Error, ReactToGroupMessageModel, any>({
      mutationFn: reactToGroupMessageReply,
      onError: console.error,
      onSuccess: (id, { messageId, groupId, reactionType }) => {
         console.log(
            "Successfully reacted to chat group message reply. Reaction Id: " +
               id
         );

         const me = client.getQueryData<GetMyClaimsResponse>([`me`, `claims`])!;
         const meId = me.claims!["nameidentifier"];
         const meName = me.claims!["name"];

         // Update message reactions count:
         client.setQueryData<ChatMessageReaction[]>(
            GET_ALL_REACTIONS_KEY(messageId),
            (reactions) => [
               ...(reactions ?? []).filter((r) => r.userId !== meId),
               {
                  chatGroupId: groupId,
                  createdAt: new Date().toString(),
                  id,
                  messageId,
                  reactionCode: reactionType,
                  userId: meId,
                  username: meName,
               },
            ]
         );

         // Update message replies' reactions count:
         client.setQueryData<InfiniteData<CursorPaged<ChatMessageReply>>>(
            GET_PAGINATED_GROUP_MESSAGE_REPLIES_KEY(messageId),
            (old) =>
               produce(
                  old,
                  (replies: InfiniteData<CursorPaged<ChatMessageReply>>) => {
                     const reply = replies.pages
                        .flatMap((_) => _.items)
                        .find((r) => r.id === messageId);

                     if (!reply) return replies;
                     reply.reactionCounts[reactionType.toString()] =
                        Number(
                           reply?.reactionCounts[reactionType.toString() ?? 0]
                        ) + 1;
                     return replies;
                  }
               )
         );
      },
      onSettled: (res) => console.log(res),
   });
};
