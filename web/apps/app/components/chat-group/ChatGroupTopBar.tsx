"use client";
import React, { Fragment, useMemo } from "react";
import {
   useCurrentChatGroup,
   useIsChatGroupPrivate, useIsGroupStarred,
   useIsUserLoggedIn,
} from "@hooks";
import {
   getMediaUrl,
   useGetChatGroupDetailsQuery,
   useGetMyClaimsQuery, useGetStarredChatGroups, useStarChatGroup, useUnstarChatGroup,
} from "@web/api";
import { Avatar, AvatarGroup, Skeleton, Spinner, Tooltip } from "@nextui-org/react";
import { UserStatus } from "@openapi";
import {
   EditChatGroupActionButton,
   AddNewMemberActionButton,
   PinnedMessagesActionButton,
} from "@components/chat-group/actions";
import { useTranslations } from "next-intl";
import { Star } from "lucide-react";
import ChatGroupStarSection from "@components/ChatGroupStarSection";

export interface ChatGroupTopBarProps {
}

const ChatGroupTopBar = ({}: ChatGroupTopBarProps) => {
   const { isUserLoggedIn } = useIsUserLoggedIn();

   const chatGroupId = useCurrentChatGroup();

   const t = useTranslations("MainArea.TopBar");
   const { data: me, error: meError } = useGetMyClaimsQuery({
      enabled: isUserLoggedIn,
   });
   const {
      data: chatGroupDetails,
      error,
      isLoading,
   } = useGetChatGroupDetailsQuery(chatGroupId, {
      enabled: !!chatGroupId && isUserLoggedIn,
   });
   const isPrivateGroup = useIsChatGroupPrivate(chatGroupDetails);
   const isUserGroupAdmin = useMemo(
      () =>
         chatGroupDetails?.chatGroup?.adminIds?.some(
            (id) => id === me?.claims?.nameidentifier,
         ),
      [chatGroupDetails?.chatGroup?.adminIds, me?.claims?.nameidentifier],
   );
   const membersOnline = useMemo(
      () =>
         chatGroupDetails?.members?.filter(
            (m) => m.status === UserStatus.ONLINE,
         )?.length,
      [chatGroupDetails?.members],
   );

   const groupPictureUrl = useMemo(() =>
      getMediaUrl(
         isPrivateGroup
            ? chatGroupDetails?.members?.find(
               (m) => m.id !== me?.claims?.nameidentifier,
            )?.profilePicture?.mediaUrl
            : chatGroupDetails?.chatGroup?.picture?.mediaUrl,
      ), [isPrivateGroup, chatGroupDetails?.members, me?.claims?.nameidentifier, chatGroupDetails?.chatGroup?.picture?.mediaUrl]);

   const groupName = useMemo(() => {
      return isPrivateGroup
         ? chatGroupDetails?.members?.filter(
            (m) => m.id !== me?.claims?.nameidentifier,
         )[0]?.username
         : chatGroupDetails?.chatGroup?.name;
   }, [isPrivateGroup, chatGroupDetails?.members, me?.claims?.nameidentifier, chatGroupDetails?.chatGroup?.name]);

   const groupAbout = useMemo(() => {
      return chatGroupDetails?.chatGroup?.about?.length
         ? chatGroupDetails?.chatGroup?.about.length > 30 ? `${chatGroupDetails.chatGroup.about?.substring(
            0,
            30,
         )}...` : chatGroupDetails?.chatGroup?.about
         : t(`ChatGroupNoDescription`);
   }, [chatGroupDetails?.chatGroup.about, t]);

   return (
      <div
         className={`flex border-y border-y-default-200 rounded-medium shadow-large shadow-primary-500 w-full items-center justify-between p-3 gap-2`}
      >
         <div className={`flex h-full ml-4 items-center gap-4`}>
            {chatGroupDetails ? (
               <Fragment>
                  <Avatar
                     fallback={
                        <Skeleton className={`h-10 w-10 rounded-full`} />
                     }
                     isBordered
                     radius={"full"}
                     color={"danger"}
                     size={"md"}
                     className={`aspect-square object-cover`}
                     src={groupPictureUrl}
                  />
                  <div
                     className={`flex flex-col items-start justify-around h-full`}
                  >
                     {isLoading ? (
                        <Fragment>
                           <Skeleton
                              as={"div"}
                              className={`text-medium rounded-full w-20 h-4 text-foreground`}
                           />
                           <Skeleton
                              as={"div"}
                              className={`text-small rounded-full w-10 h-2 text-default-500`}
                           />
                        </Fragment>
                     ) : (
                        <Fragment>
                           <div className={`flex items-center gap-4`}>
                           <span className={`text-medium text-foreground`}>
                              {groupName}
                           </span>
                              <ChatGroupStarSection chatGroupId={chatGroupId} />
                           </div>
                           <span className={`text-xs text-default-500`}>
                              {groupAbout}
                           </span>
                           <div className={`flex mt-1 items-center gap-3`}>
                              <span className={`text-xs text-default-400`}>
                                {t(`MembersCount`, { count: chatGroupDetails.members.length })}
                              </span>
                              <div
                                 className={`bg-default-200 rounded-full w-[4px] h-[4px]`}
                              />
                              <div className={`flex items-center gap-2`}>
                                 <AvatarGroup
                                    renderCount={(count) => (
                                       <div
                                          className={`w-5 flex items-center text-[.5rem] h-5 rounded-full bg-default-300 justify-center`}
                                       >
                                          +{count}
                                       </div>
                                    )}
                                    size={"sm"}
                                    className={`h-3`}
                                    max={3}
                                 >
                                    {chatGroupDetails.members
                                       .filter(
                                          (_) => _.status === UserStatus.ONLINE,
                                       )
                                       .map((user, i) => (
                                          <Avatar
                                             classNames={{
                                                base: "w-4 h-4",
                                             }}
                                             size={"sm"}
                                             color={"success"}
                                             radius={"full"}
                                             className={`w-4 h-4`}
                                             src={user.profilePicture.mediaUrl}
                                             key={user.id}
                                          />
                                       ))}
                                 </AvatarGroup>
                                 <span className={`text-xs text-success-300`}>
                        {t(`MembersOnline`, { count: membersOnline })}
                                 </span>
                              </div>
                           </div>
                        </Fragment>
                     )}
                  </div>
               </Fragment>
            ) : (
               <div>
                  <span className={`text-default-200 text-medium`}>
                     There's no chat group selected.
                  </span>
               </div>
            )}
         </div>
         <div className="mx-2 flex items-center gap-2">
            {isUserGroupAdmin && (
               <EditChatGroupActionButton chatGroup={chatGroupDetails} />
            )}
            <PinnedMessagesActionButton />
            {isUserGroupAdmin && <AddNewMemberActionButton />}
         </div>
      </div>
   );
};

export default ChatGroupTopBar;
