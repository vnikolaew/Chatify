import { useGetChatGroupDetailsQuery } from "@web/api";
import { useMemo } from "react";
import { ChatGroupDetailsEntry } from "@openapi";

// export function useIsChatGroupPrivate(groupId: string) {
//    const { data: details } = useGetChatGroupDetailsQuery(groupId);
//    return useMemo(
//       () => details?.chatGroup?.metadata?.private === "true",
//       [details]
//    );
// }

export function useIsChatGroupPrivate(chaGroupDetails: ChatGroupDetailsEntry) {
   return useMemo(
      () => chaGroupDetails?.chatGroup?.metadata?.private === "true",
      [chaGroupDetails]
   );
}
