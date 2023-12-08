import { useGetChatGroupDetailsQuery, useGetMyFriendsQuery } from "@web/api";
import { useMemo } from "react";

export function useGetNewMemberSuggestions(groupId: string, enabled: boolean = true) {
   const { data: friends, isLoading, isFetching, error } = useGetMyFriendsQuery({ enabled });
   const { data: groupDetails } = useGetChatGroupDetailsQuery(groupId);

   const addMemberSuggestedUsers = useMemo(() => {
      const memberIds = new Set([
         ...(groupDetails?.members ?? []).map((m) => m.id),
      ]);
      return friends?.filter((f) => !memberIds.has(f.id)) ?? [];
   }, [groupDetails?.members, friends]);

   return { addMemberSuggestedUsers, isLoading: isLoading && isFetching };
}
