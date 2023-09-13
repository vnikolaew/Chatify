import { chatGroupsClient } from "../../client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import {
   ChatGroupAttachment,
   ChatGroupAttachmentCursorPagedApiResponse,
   CursorPaged,
} from "@openapi";

export interface GetChatGroupAttachmentsModel {
   groupId: string;
   pageSize: number;
   pagingCursor: string;
}

const getChatGroupAttachments = async (
   model: GetChatGroupAttachmentsModel
): Promise<CursorPaged<ChatGroupAttachment>> => {
   const { groupId, ...request } = model;
   const { status, data } =
      await chatGroupsClient.get<ChatGroupAttachmentCursorPagedApiResponse>(
         `${groupId}/attachments`,
         {
            headers: {},
            data: request,
         }
      );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data as CursorPaged<ChatGroupAttachment>;
};

export const useGetChatGroupAttachmentsQuery = (
   model: GetChatGroupAttachmentsModel
) => {
   const client = useQueryClient();

   return useQuery({
      queryKey: [`chat-groups`, model.groupId, `attachments`],
      queryFn: () => getChatGroupAttachments(model),
      refetchInterval: 5 * 60 * 1000,
   });
};
