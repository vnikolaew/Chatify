import { useGetStarredChatGroups } from "@web/api";

export function useIsGroupStarred(groupId: string) {
   const { data: starredGroups } = useGetStarredChatGroups();
   return starredGroups?.some(g => g.id === groupId) ?? false;
}
