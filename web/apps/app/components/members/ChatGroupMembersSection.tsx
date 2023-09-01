"use client";
import React, { Fragment } from "react";
import {
   getUserDetails,
   useGetChatGroupDetailsQuery,
   useGetMyClaimsQuery,
   useMembersByCategory,
   USER_DETAILS_KEY,
} from "@web/api";
import { useSearchParams } from "next/navigation";
import { Skeleton } from "@nextui-org/react";
import { useQueryClient } from "@tanstack/react-query";
import { ChatGroupMemberEntry } from "@components/members";

export interface ChatGroupMembersSectionProps {}

const ChatGroupMembersSection = ({}: ChatGroupMembersSectionProps) => {
   const params = useSearchParams();
   const chatGroupId = params.get("c");
   const { data, error, isLoading } = useGetChatGroupDetailsQuery(chatGroupId, {
      enabled: !!chatGroupId,
   });
   const { data: me } = useGetMyClaimsQuery();
   const membersByCategory = useMembersByCategory(
      data?.members,
      data?.chatGroup?.adminIds
   );
   const client = useQueryClient();

   const handlePrefetchUserDetails = async (userId: string) => {
      console.log(`Fetching data for user ${userId} ...`);

      await client.prefetchQuery([USER_DETAILS_KEY, userId], {
         queryFn: ({ queryKey: [_, id] }) => getUserDetails({ userId: id }),
         staleTime: 60 * 1000,
      });
   };

   return (
      <div
         className={`flex flex-col items-start py-2 min-h-[80vh] h-full border-l-1 border-l-default-200 rounded-medium`}
      >
         {!chatGroupId ? (
            <div
               className={`w-full h-full items-center justify-center px-4 py-2`}
            >
               <h2 className={`text-large text-default-400`}>
                  There's no selected chat group.
               </h2>
            </div>
         ) : (
            <Fragment>
               <div className={`w-full px-4 py-2`}>
                  <h2 className={`text-large text-foreground`}>Members</h2>
               </div>
               {Object.entries(membersByCategory).map(
                  ([category, members], id) => (
                     <div key={id} className={`my-2 w-full`}>
                        <div className={`w-full mt-2 px-4`}>
                           <h2 className={`text-xs uppercase text-default-400`}>
                              {category} - {members.length}
                           </h2>
                        </div>
                        {isLoading ? (
                           <div className={`w-full`}>
                              {Array.from({ length: 3 }).map((_, i) => (
                                 <div
                                    key={i}
                                    className={`w-3/5 ml-4 mt-2 flex items-start gap-2`}
                                 >
                                    <Skeleton
                                       className={`rounded-full h-10 w-10 `}
                                    />
                                    <Skeleton
                                       className={`rounded-full h-5 w-4/5`}
                                    />
                                 </div>
                              ))}
                           </div>
                        ) : (
                           <div>
                              {members.map((member) => (
                                 <ChatGroupMemberEntry
                                    onHover={async () =>
                                       await handlePrefetchUserDetails(
                                          member.id
                                       )
                                    }
                                    key={member.id}
                                    member={member}
                                    isMe={
                                       me?.claims?.nameidentifier === member.id
                                    }
                                    myName={me?.claims?.name}
                                    category={category}
                                 />
                              ))}
                           </div>
                        )}
                     </div>
                  )
               )}
            </Fragment>
         )}
      </div>
   );
};

export default ChatGroupMembersSection;
