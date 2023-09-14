"use client";
import { Avatar, ListboxItem } from "@nextui-org/react";
import React, { forwardRef, useCallback } from "react";
import { ChatGroup } from "@openapi";
import { useCurrentUserId } from "@hooks";
import { getMediaUrl, useGetMyFriendsQuery } from "@web/api";

export interface ChatGroupSearchEntryProps {
   chatGroup: ChatGroup;
}

const ChatGroupSearchEntry = forwardRef(
   ({ chatGroup }: ChatGroupSearchEntryProps, ref) => {
      const meId = useCurrentUserId();
      const { data: friends } = useGetMyFriendsQuery();

      const getGroupMediaUrl = useCallback(
         (chatGroup: ChatGroup) => {
            return chatGroup?.metadata?.private === "true" && friends
               ? getMediaUrl(
                    friends.find((f) =>
                       chatGroup.adminIds.some((_) => _ !== meId && _ === f.id)
                    )?.profilePicture.mediaUrl
                 )!
               : getMediaUrl(chatGroup?.picture?.mediaUrl);
         },
         [friends]
      );

      const getGroupName = useCallback(
         (chatGroup: ChatGroup) => {
            return chatGroup?.metadata?.private === "true" && friends
               ? friends.find((f) =>
                    chatGroup.adminIds.some((_) => _ !== meId && _ === f.id)
                 )?.username
               : chatGroup?.name;
         },
         [friends]
      );
      return (
         <ListboxItem
            // variant={`faded`} color={`primary`}
            className={`z-[100] px-2`}
            startContent={
               <Avatar
                  // size={`sm`}
                  className={`w-6 h-6`}
                  radius={`sm`}
                  src={getGroupMediaUrl(chatGroup)}
               />
            }
            key={chatGroup.id}
         >
            <span className={`text-foreground text-small`}>
               {getGroupName(chatGroup)}
            </span>
            {chatGroup?.metadata?.private === "true" && <span>status</span>}
         </ListboxItem>
      );
   }
);

export default ChatGroupSearchEntry;
