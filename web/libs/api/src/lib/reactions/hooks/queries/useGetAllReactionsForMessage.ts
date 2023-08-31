import {
   UseQueryOptions,
   useQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { reactionsClient } from "../../client";
import {
   ChatMessageReaction,
   ChatMessageReactionListApiResponse,
   // @ts-ignore
} from "@openapi";
import { DEFAULT_CACHE_TIME, DEFAULT_STALE_TIME } from "../../../constants";

export interface GetAllReactionsForMessageModel {
   messageId: string;
}

const getAllReactionsForMessage = async (
   model: GetAllReactionsForMessageModel
): Promise<ChatMessageReaction[]> => {
   const { status, data } =
      await reactionsClient.get<ChatMessageReactionListApiResponse>(
         `${model.messageId}`,
         {
            headers: {},
         }
      );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data!;
};

export const useGetAllReactionsForMessage = (
   messageId: string,
   options?: Omit<
      UseQueryOptions<ChatMessageReaction[], Error, ChatMessageReaction[], any>,
      "initialData" | "queryFn" | "queryKey"
   > & {}
) => {
   const client = useQueryClient();

   return useQuery<ChatMessageReaction[], Error, ChatMessageReaction[], any>(
      [`chat-message`, messageId, `reactions`],
      () => getAllReactionsForMessage({ messageId }),
      {
         onError: console.error,
         cacheTime: DEFAULT_CACHE_TIME,
         staleTime: DEFAULT_STALE_TIME,
         ...options,
      }
   );
};
