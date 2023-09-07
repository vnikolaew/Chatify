import {
   UseQueryOptions,
   useQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { notificationsClient } from "../../client";
import { DEFAULT_CACHE_TIME, DEFAULT_STALE_TIME } from "../../../constants";
import {
   UserNotification,
   UserNotificationCursorPagedApiResponse,
} from "@openapi";
import { URLSearchParams } from "next/dist/compiled/@edge-runtime/primitives/url";

export interface GetPaginatedNotificationsModel {
   pageSize: number;
   pagingCursor: string;
}

const getPaginatedNotifications = async ({
   pageSize,
   pagingCursor,
}: GetPaginatedNotificationsModel): Promise<UserNotification[]> => {
   const params = new URLSearchParams({ pageSize: pageSize.toString() });
   if (pagingCursor) params.set("pagingCursor", pagingCursor);

   const { status, data } =
      await notificationsClient.get<UserNotificationCursorPagedApiResponse>(
         ``,
         {
            headers: {},
            params,
         }
      );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data!.data;
};

export const NOTIFICATIONS_KEY = `notifications`;

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
      queryKey: [NOTIFICATIONS_KEY, model.pageSize, model.pagingCursor],
      queryFn: () => getPaginatedNotifications(model),
      ...options,
   });
};
