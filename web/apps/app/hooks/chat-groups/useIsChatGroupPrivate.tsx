import { useMemo } from "react";
import { ChatGroupDetailsEntry } from "@openapi";

export function useIsChatGroupPrivate(chatGroupDetails: ChatGroupDetailsEntry) {
   return useMemo(
      () => chatGroupDetails?.chatGroup?.metadata?.private === "true",
      [chatGroupDetails?.chatGroup?.metadata?.private]
   );
}
