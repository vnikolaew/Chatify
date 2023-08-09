import { chatGroupsClient } from "../../client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface GetChatGroupAttachmentsModel {
   groupId: string;
   pageSize: number;
   pagingCursor: string;
}

const getChatGroupAttachments = async (model: GetChatGroupAttachmentsModel) => {
   const { groupId, ...request } = model;
   const { status, data } = await chatGroupsClient.get(
      `${groupId}/attachments`,
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

export const useGetChatGroupAttachmentsQuery = (
   model: GetChatGroupAttachmentsModel
) => {
   const client = useQueryClient();

   useQuery({
      queryKey: [`chat-groups`, model.groupId, `attachments`],
      queryFn: () => getChatGroupAttachments(model),
      refetchInterval: 5 * 60 * 1000,
      cacheTime: 60 * 60 * 1000,
   });
};
