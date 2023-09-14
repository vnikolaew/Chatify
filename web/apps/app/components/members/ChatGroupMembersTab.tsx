"use client";
import React, { useMemo } from "react";
import AddNewGroupAdminActionButton from "@components/members/AddNewGroupAdminActionButton";
import { Skeleton } from "@nextui-org/react";
import { ChatGroupMemberEntry } from "@components/members/index";
import {
   getUserDetails,
   useGetChatGroupDetailsQuery,
   useMembersByCategory,
   USER_DETAILS_KEY,
} from "@web/api";
import { useCurrentUserId } from "@hooks";
import { useQueryClient } from "@tanstack/react-query";

export interface ChatGroupMembersTabProps {
   chatGroupId: string;
}

const ChatGroupMembersTab = ({ chatGroupId }: ChatGroupMembersTabProps) => {
   const { data, error, isLoading } = useGetChatGroupDetailsQuery(chatGroupId, {
      enabled: !!chatGroupId,
   });

   const meId = useCurrentUserId();
   const isCurrentUserGroupAdmin = useMemo(
      () => data && meId && data?.chatGroup?.adminIds?.some((_) => _ === meId),
      [data, meId]
   );
   const client = useQueryClient();
   const membersByCategory = useMembersByCategory(
      data?.members,
      data?.chatGroup?.adminIds
   );

   const handlePrefetchUserDetails = async (userId: string) => {
      console.log(`Fetching data for user ${userId} ...`);

      await client.prefetchQuery([USER_DETAILS_KEY, userId], {
         queryFn: ({ queryKey: [_, id] }) => getUserDetails({ userId: id }),
         staleTime: 60 * 1000,
      });
   };

   return (
      <div>
         {Object.entries(membersByCategory).map(([category, members], id) => (
            <div key={id} className={`my-2 w-full`}>
               <div
                  className={`w-full flex items-center justify-between gap-2 mt-4 px-4`}
               >
                  <h2 className={`text-xs uppercase text-default-400`}>
                     {category} - {members.length}
                  </h2>
                  {category === "admins" && true && (
                     <AddNewGroupAdminActionButton />
                  )}
               </div>
               {isLoading ? (
                  <div className={`w-full`}>
                     {Array.from({ length: 3 }).map((_, i) => (
                        <div
                           key={i}
                           className={`w-3/5 ml-4 mt-2 flex items-start gap-2`}
                        >
                           <Skeleton className={`rounded-full h-10 w-10 `} />
                           <Skeleton className={`rounded-full h-5 w-4/5`} />
                        </div>
                     ))}
                  </div>
               ) : (
                  <div className={`min-h-[24px]`}>
                     {members.map((member) => (
                        <ChatGroupMemberEntry
                           onHover={async () =>
                              await handlePrefetchUserDetails(member.id)
                           }
                           key={member.id}
                           member={member}
                           category={category}
                        />
                     ))}
                  </div>
               )}
            </div>
         ))}
      </div>
   );
};

export default ChatGroupMembersTab;
