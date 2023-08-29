"use client";
import React from "react";
import {
   getMediaUrl,
   useGetMyClaimsQuery,
   useGetPaginatedGroupMessagesQuery,
} from "@web/api";
import { Avatar, Button, Link, ScrollShadow, Tooltip } from "@nextui-org/react";
import moment from "moment";
import { ChatGroupMemberInfoCard } from "@components/members";

export interface ChatMessagesSectionProps {
   groupId: string;
}

const ChatMessagesSection = ({ groupId }: ChatMessagesSectionProps) => {
   const {
      data: messages,
      isLoading,
      error,
      refetch,
      fetchNextPage,
      hasNextPage,
   } = useGetPaginatedGroupMessagesQuery(
      {
         groupId,
         pageSize: 10,
         pagingCursor: null!,
      },
      { enabled: !!groupId }
   );
   const { data: me } = useGetMyClaimsQuery();
   console.log(`Messages: `, messages);

   const fetchMoreMessages = async () => {
      await fetchNextPage();
      // await refetch({
      //    queryKey: [`chat-group`, groupId, `messages`, 10, null!],
      //    exact: false,
      // });
   };

   return (
      <section className={`w-full`}>
         <ScrollShadow style={{}} className={`w-full`} orientation={"vertical"}>
            <div className={`flex px-4 w-full max-h-[70vh] flex-col gap-4`}>
               {messages?.pages
                  ?.flatMap((p) => p.items)
                  .reverse()
                  ?.map((message, i) => (
                     <div
                        className={`flex px-2 py-1 rounded-lg gap-3 items-center transition-background duration-100 hover:bg-default-200 ${
                           message.senderInfo.userId ===
                              me.claims.nameidentifier && "flex-row-reverse"
                        }`}
                        key={message.message.id}
                     >
                        <Avatar
                           className={`w-10 h-10`}
                           size={"md"}
                           radius={"md"}
                           color={"warning"}
                           isBordered
                           src={getMediaUrl(
                              message.senderInfo.profilePictureUrl
                           )}
                        />
                        <div
                           className={`flex flex-col justify-evenly self-start items-start gap-2`}
                        >
                           <div className={`items-center flex gap-2`}>
                              <Tooltip
                                 delay={500}
                                 closeDelay={300}
                                 placement={"right"}
                                 content={
                                    <ChatGroupMemberInfoCard
                                       userId={message.senderInfo.userId}
                                    />
                                 }
                              >
                                 <Link
                                    className={`text-small cursor-pointer`}
                                    underline={"hover"}
                                    color={`foreground`}
                                 >
                                    {message.senderInfo.username}
                                 </Link>
                              </Tooltip>
                              <time
                                 className={`text-xs font-light text-default-500`}
                              >
                                 {moment(
                                    new Date(message.message.createdAt)
                                 ).format("HH:MM DD/MM/YYYY")}
                              </time>
                           </div>
                           <span className={`text-small text-foreground-500`}>
                              {message.message.content}
                           </span>
                        </div>
                     </div>
                  ))}
            </div>
         </ScrollShadow>
         <div className={`mt-4`}>
            <Button
               onPress={fetchMoreMessages}
               variant={"light"}
               color={"primary"}
            >
               Fetch more
            </Button>
         </div>
      </section>
   );
};

export default ChatMessagesSection;
