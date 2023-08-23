import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { notificationsClient } from "../../client";
import { DEFAULT_CACHE_TIME, DEFAULT_STALE_TIME } from "../../../constants";

const getUnreadNotifications = async () => {
   const { status, data } = await notificationsClient.get(`unread`, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useGetUnreadNotificationsQuery = () => {
   const client = useQueryClient();

   return useQuery({
      queryKey: [`notifications`, `unread`],
      queryFn: () => getUnreadNotifications(),
      cacheTime: DEFAULT_CACHE_TIME,
      staleTime: DEFAULT_STALE_TIME,
   });
};
