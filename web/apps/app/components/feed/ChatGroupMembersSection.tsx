"use client";
import React, { Fragment, useMemo } from "react";
import { useGetChatGroupDetailsQuery } from "@web/api";
import { useSearchParams } from "next/navigation";
import { Avatar, Badge, Divider, Skeleton } from "@nextui-org/react";
import { User, UserStatus } from "../../../../libs/api/openapi";

export interface ChatGroupMembersSectionProps {}

const ChatGroupMembersSection = ({}: ChatGroupMembersSectionProps) => {
   const params = useSearchParams();
   const chatGroupId = params.get("c");
   const { data, error, isLoading } = useGetChatGroupDetailsQuery(chatGroupId, {
      enabled: !!chatGroupId,
   });
   const membersByCategory = useMemo<Record<string, User[]>>(
      () =>
         (data?.members ?? []).reduce(
            (acc, member: User) => {
               if (data.chatGroup.adminIds.some((id) => id === member.id)) {
                  acc.admins.push(member);
                  return acc;
               } else if (member.status === UserStatus.ONLINE)
                  acc.online.push(member);
               else if (member.status === UserStatus.OFFLINE)
                  acc.offline.push(member);
               else if (member.status === UserStatus.AWAY)
                  acc.away.push(member);
               return acc;
            },
            { admins: [], online: [], offline: [], away: [] }
         ),
      [data]
   );
   console.log(membersByCategory);

   return (
      <div
         className={`flex flex-col items-start py-2 min-h-[80vh] h-full border-l-1 border-l-default-200`}
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
                  <h2 className={`text-large text-foreground`}>
                     Group members
                  </h2>
               </div>
               {/*<Divider*/}
               {/*   className={`w-4/5 h-[1.5px] mb-4 text-default mx-auto`}*/}
               {/*/>*/}
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
                                 <div
                                    key={member.id}
                                    className={`w-4/5 ml-4 mt-3 flex items-start gap-3`}
                                 >
                                    <Badge
                                       color={
                                          member.status === UserStatus.ONLINE
                                             ? "success"
                                             : member.status === UserStatus.AWAY
                                             ? "warning"
                                             : "default"
                                       }
                                       content={""}
                                       classNames={
                                          {
                                             // badge: "h-3 w-3 border-[1px]",
                                          }
                                       }
                                       placement={"bottom-right"}
                                       size={"sm"}
                                       variant={"shadow"}
                                       as={"span"}
                                    >
                                       <Avatar
                                          fallback={
                                             <Skeleton
                                                className={`h-10 w-10 rounded-full`}
                                             />
                                          }
                                          isBordered
                                          radius={"full"}
                                          color={
                                             member.status === UserStatus.ONLINE
                                                ? "success"
                                                : "default"
                                          }
                                          size={"sm"}
                                          className={`aspect-square outline-1 object-cover`}
                                          src={member.profilePicture.mediaUrl}
                                       />
                                    </Badge>
                                    <span className={`text-medium`}>
                                       {member.username}
                                    </span>
                                 </div>
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
