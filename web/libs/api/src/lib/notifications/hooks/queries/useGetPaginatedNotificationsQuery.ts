import {
   UseQueryOptions,
   useQueryClient,
   useInfiniteQuery,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { notificationsClient } from "../../client";
import {
   UserNotification,
   UserNotificationCursorPagedApiResponse,
} from "@openapi";
import { URLSearchParams } from "next/dist/compiled/@edge-runtime/primitives/url";
import { CursorPaged } from "../../../../../openapi/common/CursorPaged";

export interface GetPaginatedNotificationsModel {
   pageSize: number;
   pagingCursor: string;
}

const getPaginatedNotifications = async ({
   pageSize,
   pagingCursor,
}: GetPaginatedNotificationsModel): Promise<CursorPaged<UserNotification>> => {
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

   return data!.data as CursorPaged<UserNotification>;
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
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   const client = useQueryClient();
   return useInfiniteQuery<
      CursorPaged<UserNotification>,
      Error,
      CursorPaged<UserNotification>,
      any
   >({
      queryKey: [NOTIFICATIONS_KEY, model.pageSize, model.pagingCursor],
      queryFn: () => getPaginatedNotifications(model),
      getNextPageParam: (lastPage: CursorPaged<UserNotification>) => {
         return lastPage.pagingCursor;
      },
      getPreviousPageParam: (_, allPages: any) => allPages.at(-1)?.pagingCursor,
      ...options,
   });
};
