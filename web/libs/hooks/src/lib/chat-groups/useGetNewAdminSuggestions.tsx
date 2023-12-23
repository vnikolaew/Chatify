import { useGetChatGroupDetailsQuery } from "@web/api";
import { useMemo } from "react";

export function useGetNewAdminSuggestions(groupId: string) {
   const { data: groupDetails } = useGetChatGroupDetailsQuery(groupId);

   const groupAdminIds = useMemo(() =>
         new Set([...(groupDetails?.chatGroup?.adminIds ?? [])]),
      [groupDetails]);

   const addAdminSuggestedUsers = useMemo(() =>
      groupDetails.members.filter((m) => !groupAdminIds.has(m.id)),
      [groupDetails?.members]);

   return addAdminSuggestedUsers;
}
