import { chatGroupsClient } from "../../client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface GetChatGroupsFeedModel {
   limit: number;
   offset: number;
}

const getChatGroupsFeed = async (model: GetChatGroupsFeedModel) => {
   const params = new URLSearchParams({
      limit: `${model.limit}`,
      offset: model.offset.toString(),
   });

   const { status, data } = await chatGroupsClient.get(`feed`, {
      headers: {},
      data: model,
      params,
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useGetChatGroupsFeedQuery = (model: GetChatGroupsFeedModel) => {
   const client = useQueryClient();

   useQuery({
      queryKey: [`feed`],
      queryFn: () => getChatGroupsFeed(model),
      refetchInterval: 30 * 1000,
      cacheTime: 60 * 60 * 1000,
   });
};
