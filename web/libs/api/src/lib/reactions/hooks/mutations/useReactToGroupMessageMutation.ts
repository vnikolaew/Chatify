import {
   InfiniteData,
   useMutation,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { reactionsClient } from "../../client";
import {
   ChatGroupMessageEntry,
   ChatMessageReaction,
   ObjectApiResponse,
   // @ts-ignore
} from "@openapi";
import { GetMyClaimsResponse } from "../../../auth";
import { CursorPaged } from "@openapi";
import { produce } from "immer";
import { GET_PAGINATED_GROUP_MESSAGES_KEY } from "../../../messages";
import { GET_ALL_REACTIONS_KEY } from "../queries";

export interface ReactToGroupMessageModel {
   messageId: string;
   groupId: string;
   reactionType: number;
}

const reactToGroupMessage = async (
   model: ReactToGroupMessageModel
): Promise<string> => {
   const { messageId, ...request } = model;
   const { status, data } = await reactionsClient.post<ObjectApiResponse>(
      `${messageId}`,
      request,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data.id;
};

export const useReactToGroupMessageMutation = () => {
   const client = useQueryClient();

   return useMutation<string, Error, ReactToGroupMessageModel, any>({
      mutationFn: reactToGroupMessage,
      onError: console.error,
      onSuccess: (id, { messageId, groupId, reactionType }) => {
         console.log("Reaction Id: " + id);

         const me = client.getQueryData<GetMyClaimsResponse>([`me`, `claims`])!;
         const meId = me.claims!["nameidentifier"];
         const meName = me.claims!["name"];

         client.setQueryData<ChatMessageReaction[]>(
            GET_ALL_REACTIONS_KEY(messageId),
            (reactions) => {
               return [
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
                     message.message.reactionCounts[reactionType.toString()] =
                        Number(
                           message.message?.reactionCounts[
                              reactionType.toString() ?? 0
                           ]
                        ) + 1;
                  }
               });
            }
         );
      },
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
