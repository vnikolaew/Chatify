import {
   useQueryClient,
   useInfiniteQuery,
   UndefinedInitialDataInfiniteOptions,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { notificationsClient } from "../../client";
import {
   UserNotification,
   UserNotificationCursorPagedApiResponse,
   CursorPaged,
} from "@openapi";

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
   options?: UndefinedInitialDataInfiniteOptions<
      UserNotification[],
      Error,
      UserNotification[],
      (string | number)[]
   >
) => {
   const client = useQueryClient();
   return useInfiniteQuery<
      CursorPaged<UserNotification>,
      Error,
      CursorPaged<UserNotification>,
      any
   >({
      queryKey: [NOTIFICATIONS_KEY],
      initialPageParam: null!,
      queryFn: () => getPaginatedNotifications(model),
      getNextPageParam: (lastPage: CursorPaged<UserNotification>) => {
         return lastPage.pagingCursor;
      },
      getPreviousPageParam: (_, allPages: any) => allPages.at(-1)?.pagingCursor,
      ...options,
   });
};
