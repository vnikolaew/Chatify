import { useMemo } from "react";
import { ChatGroupDetailsEntry } from "@openapi";
import { useGetChatGroupDetailsQuery } from "@web/api";

export function useIsChatGroupPrivate(chatGroupDetails: ChatGroupDetailsEntry) {
   return useMemo(
      () => chatGroupDetails?.chatGroup?.metadata?.private === "true",
      [chatGroupDetails?.chatGroup?.metadata?.private],
   );
}

export function useIsChatGroupPrivateById(groupId: string) {
   const { data: group } = useGetChatGroupDetailsQuery(groupId);
   return useMemo(
      () => group?.chatGroup?.metadata?.private === "true",
      [group?.chatGroup?.metadata?.private],
   );
}
