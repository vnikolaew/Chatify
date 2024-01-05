import { chatGroupsClient } from "../../client";
import {
   UseQueryOptions,
   useQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { sleep } from "../../../utils";
// @ts-ignore
import { ChatGroupListApiResponse, ChatGroup } from "@openapi";

export interface SearchChatGroupsByName {
   query: string;
}

const searchChatGroupsByName = async (
   model: SearchChatGroupsByName
): Promise<ChatGroup[]> => {
   const params = new URLSearchParams({
      q: model.query,
   });

   const { status, data } =
      await chatGroupsClient.get<ChatGroupListApiResponse>(`search`, {
         headers: {},
         data: model,
         params,
      });
   await sleep(1000);

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data!;
};

export const useSearchChatGroupsByName = (
   model: SearchChatGroupsByName,
   options?:
      | (Omit<
           UseQueryOptions<unknown, unknown, ChatGroup[], any>,
           "initialData" | "queryKey"
        > & {
           initialData?: (() => undefined) | undefined;
        })
      | undefined
) => {
   const client = useQueryClient();

   return useQuery({
      queryKey: [`chat-groups`, "search", model.query],
      queryFn: () => searchChatGroupsByName(model),
      refetchOnWindowFocus: false,
      refetchInterval: 60 * 5 * 1000,
      gcTime: 60 * 60 * 1000,
      ...options,
   });
};
