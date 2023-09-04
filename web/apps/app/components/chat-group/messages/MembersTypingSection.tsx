"use client";
import React, { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useCurrentChatGroup } from "@hooks";
import { useGetChatGroupDetailsQuery } from "@web/api";

export interface MembersTypingSectionProps {}

const MembersTypingSection = ({}: MembersTypingSectionProps) => {
   const groupId = useCurrentChatGroup();
   const { data: usersTyping } = useQuery<Set<string>>([
      `chat-group`,
      groupId,
      `typing`,
   ]);
   const { data: groupDetails } = useGetChatGroupDetailsQuery(groupId);

   const memberUsernames = useMemo(
      () =>
         usersTyping &&
         [...usersTyping].map(
            (u) => groupDetails?.members?.find((_) => _.id === u).username
         ),
      [groupDetails, usersTyping]
   );

   if (!memberUsernames?.length) return null!;

   return (
      <div className={`text-xs text-default-300`}>
         {[...memberUsernames].join(", ")} are currently typing ...
      </div>
   );
};

export default MembersTypingSection;
