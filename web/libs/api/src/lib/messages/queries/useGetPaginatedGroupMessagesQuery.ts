import { messagesClient } from "../client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface GetPaginatedGroupMessagesModel {
   groupId: string;
   pageSize: number;
   pagingCursor: string;
}

const getPaginatedGroupMessages = async (
   model: GetPaginatedGroupMessagesModel
) => {
   const { groupId, ...request } = model;
   const { status, data } = await messagesClient.get(`${groupId}`, {
      headers: {},
      data: request,
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useGetPaginatedGroupMessagesQuery = (
   model: GetPaginatedGroupMessagesModel
) => {
   const client = useQueryClient();

   return useQuery({
      queryKey: [
         `chat-group`,
         model.groupId,
         `messages`,
         model.pageSize,
         model.pagingCursor,
      ],
      queryFn: () => getPaginatedGroupMessages(model),
      cacheTime: 60 * 60 * 1000,
   });
};
