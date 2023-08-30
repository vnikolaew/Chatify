import { useGetChatGroupDetailsQuery } from "@web/api";
import { User } from "@openapi";

export function useGetChatGroupMembers(groupId: string): User[] | null {
   const { data: groupDetails } = useGetChatGroupDetailsQuery(groupId);
   return groupDetails?.members;
}
