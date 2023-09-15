"use client";
import { Avatar, Listbox, ListboxItem, Skeleton } from "@nextui-org/react";
import { ChatGroup, UserStatus } from "@openapi";
import React, { Fragment, useCallback, useMemo } from "react";
import { useCurrentUserId } from "@hooks";
import { getMediaUrl, useGetMyFriendsQuery } from "@web/api";
import { Check } from "lucide-react";

export interface ChatGroupSearchEntriesProps {
   entries: ChatGroup[];
   loading?: boolean;
   selectedEntries?: ChatGroup[];
   setSelectedEntries?: (entries: ChatGroup[]) => void;
   onSelect?: (chatGroup: ChatGroup) => void;
}

const ChatGroupSearchEntries = ({
   entries,
   loading,
   selectedEntries,
   setSelectedEntries,
   onSelect,
}: ChatGroupSearchEntriesProps) => {
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
      [friends, meId]
   );

   const getGroupName = useCallback(
      (chatGroup: ChatGroup) => {
         return chatGroup?.metadata?.private === "true" && friends
            ? friends.find((f) =>
                 chatGroup.adminIds.some((_) => _ !== meId && _ === f.id)
              )?.username
            : chatGroup?.name;
      },
      [friends, meId]
   );

   const normalizeGroup = useCallback(
      (chatGroup: ChatGroup) => {
         return {
            ...chatGroup,
            picture: {
               ...chatGroup.picture,
               mediaUrl: getGroupMediaUrl(chatGroup),
            },
            name: getGroupName(chatGroup),
         };
      },
      [getGroupMediaUrl, getGroupName]
   );

   const isSelected = useCallback(
      (group: ChatGroup) => selectedEntries.some((e) => e.id === group.id),
      [selectedEntries]
   );

   const friendStatuses = useMemo(
      () =>
         entries
            .map((e) => ({
               id: e.id,
               adminIds: e.adminIds,
               isPrivate: e.metadata.private === "true",
            }))
            .reduce<Record<string, UserStatus | null>>(
               (acc, { id, isPrivate, adminIds }) => {
                  if (isPrivate && friends) {
                     acc[id] = friends.find((f) =>
                        adminIds.some((_) => _ !== meId && _ === f.id)
                     ).status;
                  }
                  return acc;
               },
               {} as Record<string, UserStatus | null>
            ),
      [friends, meId, entries]
   );

   const getStatusColor = useCallback((status: UserStatus) => {
      if (!status) return "default";
      switch (status) {
         case UserStatus.AWAY:
            return "bg-warning";
         case UserStatus.ONLINE:
            return "bg-success";
         case UserStatus.OFFLINE:
            return "bg-default";
      }
      return "bg-default";
   }, []);

   const renderSearchEntry = useCallback(
      (chatGroup: ChatGroup) => (
         <ListboxItem
            // variant={`faded`} color={`primary`}
            textValue={chatGroup.id}
            // selectedIcon={<Check size={12} className={`stroke-primary `} />}
            className={`z-[100] px-2 gap-4`}
            startContent={
               loading ? (
                  <Skeleton className={`w-6 h-6 rounded-full`} />
               ) : (
                  <Avatar
                     // size={`sm`}
                     className={`w-6 h-6`}
                     radius={`sm`}
                     src={getGroupMediaUrl(chatGroup)}
                  />
               )
            }
            key={chatGroup.id}
         >
            {loading ? (
               <div className={`flex items-center gap-2`}>
                  <Skeleton className={`w-24 h-3 rounded-full`} />
                  <Skeleton className={`w-12 h-2 rounded-full`} />
               </div>
            ) : (
               <div className={`flex items-center gap-2`}>
                  <span
                     className={`text-foreground text-small ${
                        isSelected(chatGroup) && `text-primary`
                     }`}
                  >
                     {getGroupName(chatGroup)}
                  </span>
                  {chatGroup?.metadata?.private === "true" && (
                     <Fragment>
                        <div
                           className={`w-2 h-2 rounded-full ${getStatusColor(
                              friendStatuses[chatGroup.id]
                           )}`}
                        />
                        <span
                           className={`font-light ml-2 text-small ${
                              isSelected(chatGroup) && `text-primary`
                           }`}
                        >
                           {
                              friends?.find(
                                 (f) => f.username === getGroupName(chatGroup)
                              )?.displayName
                           }
                        </span>
                     </Fragment>
                  )}
               </div>
            )}
         </ListboxItem>
      ),
      [
         getGroupMediaUrl,
         getGroupName,
         getStatusColor,
         friendStatuses,
         friends,
         loading,
         isSelected,
      ]
   );

   return (
      <Listbox
         variant={`solid`}
         selectionMode={`multiple`}
         onSelectionChange={(keys) =>
            setSelectedEntries(
               entries
                  .filter((e) => [...keys].some((_) => _ === e.id))
                  .map(normalizeGroup)
            )
         }
         selectedKeys={selectedEntries?.map((_) => _.id)}
         className={`absolute max-h-[200px] overflow-y-scroll rounded-medium bg-default-100 bg-opacity-100 z-[100] bottom-0 translate-y-[105%]`}
         onAction={(id) => {
            const group = entries.find((e) => e.id === id);
            console.log(group);
            onSelect?.(normalizeGroup(group));
         }}
         itemClasses={{
            base: `gap-4`,
            selectedIcon: `text-primary`,
         }}
         aria-label={`Search entries`}
      >
         {(loading
            ? Array.from({ length: 5 }).map<ChatGroup>(
                 (_, i) => ({ id: i } as unknown as ChatGroup)
              )
            : entries
         ).map(renderSearchEntry)}
      </Listbox>
   );
};

export default ChatGroupSearchEntries;
