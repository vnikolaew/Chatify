import { Media } from "@openapi";

interface IChatGroupMessage {
   chatGroupId: string;
   attachments: Media[];
   senderId: string;
   messageId: string;
   senderUsername: string;
   content: string;
   timestamp: string;
}

export interface IChatGroupMemberStartedTyping {
   chatGroupId: string;
   userId: string;
   username: string;
   timestamp: string;
}

export interface IChatGroupMemberStoppedTyping {
   chatGroupId: string;
   userId: string;
   username: string;
   timestamp: string;
}

export interface IUserStatusChanged {
   userId: string;
   username: string;
   newStatus: number;
   timestamp: string;
}

export interface IChatGroupMemberLeft {
   chatGroupId: string;
   userId: string;
   username: string;
   timestamp: string;
}

export interface IChatGroupMemberRemoved {
   chatGroupId: string;
   removedById: string;
   userId: string;
   username: string;
   timestamp: string;
}

export interface IChatGroupMemberAdded {
   chatGroupId: string;
   addedById: string;
   userId: string;
   username: string;
   timestamp: string;
}

export interface IChatGroupMessageRemoved {
   chatGroupId: string;
   messageId: string;
   userId: string;
   username: string;
   timestamp: string;
}

export interface IChatGroupMessageEdited {
   chatGroupId: string;
   messageId: string;
   userId: string;
   newContent: string;
   timestamp: string;
   metadata: { [key: string]: string };
}

export interface IChatGroupMessageReactedTo {
   chatGroupId: string;
   messageId: string;
   messageReactionId: string;
   userId: string;
   reactionType: number;
   timestamp: string;
   metadata: { [key: string]: string };
}

export interface IChatGroupMessageUnReactedTo {
   chatGroupId: string;
   messageId: string;
   messageReactionId: string;
   userId: string;
   reactionType: number;
   timestamp: string;
   metadata: { [key: string]: string };
}

export interface IReceiveFriendInvitation {
   inviterId: string;
   inviterUsername: string;
   timestamp: string;
   metadata: { [key: string]: string };
}

export interface IFriendInvitationResponded {
   inviteeId: string;
   inviteeUsername: string;
   timestamp: string;
   metadata: { [key: string]: string };
}

export interface IAddedToChatGroup {
   chatGroupId: string;
   addedById: string;
   addedByUsername: string;
   timestamp: string;
   metadata: { [key: string]: string };
}

export interface IChatGroupNewAdminAdded {
   chatGroupId: string;
   adminId: string;
   adminUsername: string;
   timestamp: string;
   metadata: { [key: string]: string };
}

export interface IFriendInvitationAccepted extends IFriendInvitationResponded {}

export interface IFriendInvitationDeclined extends IFriendInvitationResponded {}
