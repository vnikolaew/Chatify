import {
   UseQueryOptions,
   useQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { notificationsClient } from "../../client";
import { DEFAULT_CACHE_TIME, DEFAULT_STALE_TIME } from "../../../constants";
import { UserNotification } from "../../../../../openapi";

export interface GetPaginatedNotificationsModel {
   pageSize: number;
   pagingCursor: string;
}

const getPaginatedNotifications = async ({
   pageSize,
   pagingCursor,
}: GetPaginatedNotificationsModel): Promise<UserNotification[]> => {
   const { status, data } = await notificationsClient.get(``, {
      headers: {},
      params: new URLSearchParams({
         pageSize: pageSize.toString(),
         pagingCursor,
      }),
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useGetPaginatedNotificationsQuery = (
   model: GetPaginatedNotificationsModel,
   options?: Omit<
      UseQueryOptions<
         UserNotification[],
         Error,
         UserNotification[],
         (string | number)[]
      >,
      "initialData"
   > & { initialData?: (() => undefined) | undefined }
) => {
   const client = useQueryClient();

   return useQuery({
      queryKey: [`notifications`, model.pageSize, model.pagingCursor],
      queryFn: () => getPaginatedNotifications(model),
      cacheTime: DEFAULT_CACHE_TIME,
      staleTime: DEFAULT_STALE_TIME,
      ...options,
   });
};