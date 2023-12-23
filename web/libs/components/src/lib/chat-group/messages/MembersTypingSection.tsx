"use client";
import React, { useMemo, useState } from "react";
import {
   useCurrentChatGroup,
   useCurrentUserId,
   useInterval,
   useGetUsersTyping,
} from "@web/hooks";
import { getMediaUrl, useGetChatGroupDetailsQuery } from "@web/api";
import { Avatar, AvatarGroup } from "@nextui-org/react";
import { AnimatePresence, motion } from "framer-motion";
import { User } from "@openapi";

export interface MembersTypingSectionProps {
}

const MembersTypingSection = ({}: MembersTypingSectionProps) => {
   const groupId = useCurrentChatGroup();
   const usersTyping = useGetUsersTyping(groupId);
   const [typingDotsCount, setTypingDotsCount] = useState(1);
   useInterval(() => setTypingDotsCount((c) => (c + 1) % 4), 750, [
      usersTyping,
   ]);

   const meId = useCurrentUserId();
   const { data: groupDetails } = useGetChatGroupDetailsQuery(groupId);

   const groupMembersById = useMemo<Record<string, User>>(() =>
      groupDetails?.members
         ?.reduce<Record<string, User>>((acc, prev) => {
            acc[prev.id] = prev;
            return acc;
         }, {}), [groupDetails?.members]);

   const members = useMemo(
      () => [...(usersTyping ?? [])]
         .map((u) => groupMembersById[u.userId])
         .filter((m) => m.id !== meId),
      [usersTyping, meId, groupMembersById],
   );

   const membersTypingMessage = useMemo(() => {
      return [...members.map((m) => m.username)].join(", ")
         + (members.length === 1 ? ` is ` : ` are `) + `currently typing`
         + Array.from({ length: typingDotsCount })
            .map((_) => `.`)
            .join(``);
   }, [members, typingDotsCount]);

   if (!members?.length) return <div className={`h-0 w-full`} />;

   return (
      <AnimatePresence mode={`sync`}>
         {members?.length && (
            <motion.div
               key={"users-typing"}
               layout
               initial={{ opacity: 0, height: 0 }}
               animate={{ opacity: 100, height: "auto" }}
               exit={{ opacity: 0, height: 0 }}
               transition={{ duration: 0.1 }}
               className={`text-xs flex items-center gap-1 ml-12 mb-4 text-default-500`}
            >
               <AvatarGroup radius={"full"} size={"sm"} color={"danger"}>
                  {members.map((m, i) => (
                     <Avatar
                        classNames={{
                           img: `w-4 h-4`,
                           base: `w-4 h-4`,
                        }}
                        key={m.id}
                        size={"sm"}
                        src={getMediaUrl(m.profilePicture.mediaUrl)}
                     />
                  ))}
               </AvatarGroup>
               <span>
                  {membersTypingMessage}
               </span>
            </motion.div>
         )}
      </AnimatePresence>
   );
};

export default MembersTypingSection;
