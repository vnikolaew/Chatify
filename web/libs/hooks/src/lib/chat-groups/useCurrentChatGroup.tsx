import { useSearchParams } from "next/navigation";

export function useCurrentChatGroup(): string | null {
   const params = useSearchParams();
   const chatGroupId = params.get("c");
   return chatGroupId;
}
