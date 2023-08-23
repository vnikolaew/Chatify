import {
   useQuery,
   useQueryClient,
   UseQueryOptions,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { notificationsClient } from "../../client";
import { DEFAULT_CACHE_TIME, DEFAULT_STALE_TIME } from "../../../constants";
import { User, UserNotification } from "@openapi";

const getUnreadNotifications = async (): Promise<UserNotification[]> => {
   const { status, data } = await notificationsClient.get(`unread`, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useGetUnreadNotificationsQuery = (
   options?: Omit<
      UseQueryOptions<UserNotification[], Error, UserNotification[], string[]>,
      "initialData"
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   const client = useQueryClient();

   return useQuery({
      queryKey: [`notifications`, `unread`],
      queryFn: () => getUnreadNotifications(),
      cacheTime: DEFAULT_CACHE_TIME,
      staleTime: DEFAULT_STALE_TIME,
      ...options,
   });
};
