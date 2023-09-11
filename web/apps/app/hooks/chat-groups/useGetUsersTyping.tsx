import { useQuery } from "@tanstack/react-query";
import { IUserTyping } from "../../hub/ChatifyHubInitializer";

export function useGetUsersTyping(groupId: string) {
   const { data: usersTyping } = useQuery<Set<IUserTyping>>([
      `chat-group`,
      groupId,
      `typing`,
   ]);

   return usersTyping ?? new Set<IUserTyping>();
}
