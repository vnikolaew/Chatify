"use client";
import React, { useMemo, useState } from "react";
import {
   useCurrentChatGroup,
   useCurrentUserId,
   useInterval,
   useGetUsersTyping,
} from "@hooks";
import { getMediaUrl, useGetChatGroupDetailsQuery } from "@web/api";
import { Avatar, AvatarGroup } from "@nextui-org/react";
import { AnimatePresence, motion } from "framer-motion";

export interface MembersTypingSectionProps {}

const MembersTypingSection = ({}: MembersTypingSectionProps) => {
   const groupId = useCurrentChatGroup();
   const usersTyping = useGetUsersTyping(groupId);
   const [typingDotsCount, setTypingDotsCount] = useState(1);
   useInterval(() => setTypingDotsCount((c) => (c + 1) % 4), 1000, [
      usersTyping,
   ]);

   const meId = useCurrentUserId();
   const { data: groupDetails } = useGetChatGroupDetailsQuery(groupId);

   const members = useMemo(
      () =>
         usersTyping &&
         [...usersTyping]
            .map((u) => groupDetails?.members?.find((_) => _.id === u.userId))
            .filter((m) => m.id !== meId),
      [groupDetails, usersTyping]
   );

   if (!members?.length) return null!;

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
                  {[...members.map((m) => m.username)].join(", ")}{" "}
                  {members.length === 1 ? ` is ` : ` are `} currently typing{" "}
                  {Array.from({ length: typingDotsCount })
                     .map((_) => `.`)
                     .join(``)}
               </span>
            </motion.div>
         )}
      </AnimatePresence>
   );
};

export default MembersTypingSection;
