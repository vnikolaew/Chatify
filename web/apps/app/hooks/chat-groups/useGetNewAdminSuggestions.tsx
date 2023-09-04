import { useGetChatGroupDetailsQuery, useGetMyFriendsQuery } from "@web/api";
import { useMemo } from "react";

export function useGetNewAdminSuggestions(groupId: string) {
   const { data: groupDetails } = useGetChatGroupDetailsQuery(groupId);
   const addAdminSuggestedUsers = useMemo(() => {
      const memberIds = new Set([
         ...(
            groupDetails?.members?.filter((m) =>
               groupDetails.chatGroup.adminIds.every((id) => id !== m.id)
            ) ?? []
         ).map((m) => m.id),
      ]);

      return groupDetails.members.filter((m) => memberIds.has(m.id));
   }, [groupDetails?.members]);

   return addAdminSuggestedUsers;
}
