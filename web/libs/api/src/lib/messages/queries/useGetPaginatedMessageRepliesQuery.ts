import { messagesClient } from "../client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
export interface GetPaginatedMessageRepliesModel {
   messageId: string;
   pageSize: number;
   pagingCursor: string;
}

const getPaginatedMessageReplies = async (
   model: GetPaginatedMessageRepliesModel
) => {
   const { messageId, ...request } = model;
   const { status, data } = await messagesClient.get(`replies/${messageId}`, {
      headers: {},
      data: request,
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useGetPaginatedMessageRepliesQuery = (
   model: GetPaginatedMessageRepliesModel
) => {
   const client = useQueryClient();

   return useQuery({
      queryKey: [`message-replies`, model.pageSize, model.pagingCursor],
      queryFn: () => getPaginatedMessageReplies(model),
      cacheTime: 60 * 60 * 1000,
   });
};
