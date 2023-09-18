"use client";
import React, { Fragment, useMemo } from "react";
import { User, UserStatus } from "@openapi";
import { Avatar, Badge, Skeleton, Tooltip } from "@nextui-org/react";
import ChatGroupMemberInfoCard from "./ChatGroupMemberInfoCard";
import { getMediaUrl, useGetMyClaimsQuery } from "@web/api";

export interface ChatGroupMemberEntryProps {
  member: User;
  category: string;
  onHover?: React.MouseEventHandler<HTMLDivElement>;
}

const ChatGroupMemberEntry = ({
                                member,
                                onHover,
                                category
                              }: ChatGroupMemberEntryProps) => {
  const { data: me } = useGetMyClaimsQuery();
  const isMe = useMemo(
    () => member.id === me?.claims?.nameidentifier,
    [member.id, me?.claims?.nameidentifier]
  );

  const innerDiv = (
    <div
      onMouseEnter={onHover}
      className={`w-4/5 py-1 px-3 ml-4 mt-2 rounded-md transition-background duration-100 hover:bg-default-100 cursor-pointer flex items-center gap-3`}
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
        placement={"bottom-right"}
        size={"sm"}
        variant={"shadow"}
        as={"span"}
      >
        <Avatar
          fallback={<Skeleton className={`h-10 w-10 rounded-full`} />}
          isBordered
          radius={"full"}
          color={
            category === "admins"
              ? "secondary"
              : member.status === UserStatus.ONLINE
                ? "success"
                : "default"
          }
          size={"sm"}
          className={`aspect-square outline-1 object-cover`}
          src={getMediaUrl(member.profilePicture.mediaUrl)}
        />
      </Badge>
      <span className={`text-small`}>
            {isMe ? <Fragment>{member.username} <b>(you)</b></Fragment> : member.username}
         </span>
    </div>
  );

  return isMe ? (
    innerDiv
  ) : (
    <Tooltip
      delay={500}
      closeDelay={300}
      placement={"left"}
      key={member.id}
      content={<ChatGroupMemberInfoCard userId={member.id} />}
    >
      {innerDiv}
    </Tooltip>
  );
};

export default ChatGroupMemberEntry;
