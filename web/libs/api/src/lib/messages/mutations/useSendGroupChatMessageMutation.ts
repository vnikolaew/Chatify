import { messagesClient } from "../client";
import {
   InfiniteData,
   useMutation,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import {
   ChatGroupMessageEntry,
   ObjectApiResponse,
   UserDetailsEntry,
} from "@openapi";
import { CursorPaged } from "../../../../openapi/common/CursorPaged";
import { produce } from "immer";
import { GetMyClaimsResponse } from "../../auth";
import { GET_PAGINATED_GROUP_MESSAGES_KEY } from "../queries";
import { v4 as uuidv4 } from "uuid";

export interface SendGroupChatMessageModel {
   chatGroupId: string;
   content: string;
   files?: Blob[];
   metadata?: Record<string, string>;
}

const sendGroupChatMessage = async (
   model: SendGroupChatMessageModel
): Promise<string> => {
   const { chatGroupId, ...request } = model;

   const formData = new FormData();
   formData.append("content", request.content);
   (request.files ?? []).forEach((file, i) =>
      formData.append(`files`, file, file.name)
   );
   if (request.metadata)
      formData.append("metadata", JSON.stringify(request.metadata));

   const { status, data } = await messagesClient.postForm<ObjectApiResponse>(
      `${chatGroupId}`,
      formData,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data.id as string;
   return uuidv4();
};

export const useSendGroupChatMessageMutation = () => {
   const client = useQueryClient();

   return useMutation<string, Error, SendGroupChatMessageModel, any>(
      sendGroupChatMessage,
      {
         onError: console.error,
         onSuccess: (id, { chatGroupId, content, metadata, files }) => {
            console.log("Chat message sent successfully. Id is " + id);

            const me = client.getQueryData<GetMyClaimsResponse>([
               `me`,
               `claims`,
            ]);
            const meDetails = client.getQueryData<UserDetailsEntry>([
               `user-details`,
               me!.claims["nameidentifier"]!,
            ]);

            // Update client cache with new message:
            client.setQueryData<
               InfiniteData<CursorPaged<ChatGroupMessageEntry>>
            >(GET_PAGINATED_GROUP_MESSAGES_KEY(chatGroupId), (messages) =>
               produce(messages, (draft) => {
                  // draft.pageParams.
                  (
                     draft!.pages[0] as CursorPaged<ChatGroupMessageEntry>
                  ).items.unshift({
                     message: {
                        id,
                        chatGroupId,
                        content,
                        metadata,
                        userId: meDetails.user?.id,
                        createdAt: new Date().toString(),
                        reactionCounts: {},
                     },
                     forwardedMessage: {},
                     repliersInfo: {
                        replierInfos: [],
                        total: 0,
                        lastUpdatedAt: null!,
                     },
                     senderInfo: {
                        profilePictureUrl:
                           meDetails.user?.profilePicture?.mediaUrl,
                        userId: meDetails.user?.id,
                        username: meDetails.user?.username,
                     },
                  });
                  return draft;
               })
            );
         },
         onSettled: (res) => console.log(res),
         // cacheTime: 60 * 60 * 1000,
      }
   );
};
