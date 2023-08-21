import { chatGroupsClient } from "../../client";
import {
   UseQueryOptions,
   useQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { sleep } from "../../../utils";
// @ts-ignore
import { ChatGroup } from "@openapi/models/ChatGroup";

export interface SearchChatGroupsByName {
   query: string;
}

export interface SearchChatGroupsByNameResponse {
   data: ChatGroup[];
}

const searchChatGroupsByName = async (
   model: SearchChatGroupsByName
): Promise<SearchChatGroupsByNameResponse> => {
   const params = new URLSearchParams({
      q: model.query,
   });

   const { status, data } = await chatGroupsClient.get(`search`, {
      headers: {},
      data: model,
      params,
   });
   await sleep(1000);

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useSearchChatGroupsByName = (
   model: SearchChatGroupsByName,
   options?:
      | (Omit<
           UseQueryOptions<
              unknown,
              unknown,
              SearchChatGroupsByNameResponse,
              any
           >,
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
      cacheTime: 60 * 60 * 1000,
      ...options,
   });
};
