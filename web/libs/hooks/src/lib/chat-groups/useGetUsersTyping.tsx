import { useQuery } from "@tanstack/react-query";

export interface IUserTyping {
   userId: string;
   username: string;
   groupId: string;
}

export function useGetUsersTyping(groupId: string) {
   const { data: usersTyping } = useQuery<Set<IUserTyping>>({
      queryKey: [`chat-group`, groupId, `typing`],
   });
   return usersTyping ?? new Set<IUserTyping>();
}
