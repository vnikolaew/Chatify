import { HubConnection, HubConnectionState } from "@microsoft/signalr";
import {
   IAddedToChatGroup,
   IChatGroupMemberLeft,
   IChatGroupMemberRemoved,
   IChatGroupMemberStartedTyping,
   IChatGroupMemberStoppedTyping,
   IChatGroupMessage,
   IChatGroupMessageEdited,
   IChatGroupMessageReactedTo,
   IChatGroupMessageUnReactedTo,
   IChatGroupNewAdminAdded,
   IFriendInvitationAccepted,
   IFriendInvitationDeclined,
   IReceiveFriendInvitation,
   IUserStatusChanged,
} from "./messages";

export enum HubMethods {
   ReceiveGroupChatMessage = "ReceiveGroupChatMessage",
   ChatGroupMemberStartedTyping = "ChatGroupMemberStartedTyping",
   ChatGroupMemberStoppedTyping = "ChatGroupMemberStoppedTyping",
   ChatGroupMemberRemoved = "ChatGroupMemberRemoved",
   ChatGroupMemberLeft = "ChatGroupMemberLeft",
   ChatGroupNewAdminAdded = "ChatGroupNewAdminAdded",
   ChatGroupMessageRemoved = "ChatGroupMessageRemoved",
   ChatGroupMessageEdited = "ChatGroupMessageEdited",
   ChatGroupMessageReactedTo = "ChatGroupMessageReactedTo",
   ChatGroupMessageUnReactedTo = "ChatGroupMessageUnReactedTo",
   ReceiveFriendInvitation = "ReceiveFriendInvitation",
   FriendInvitationAccepted = "FriendInvitationAccepted",
   FriendInvitationDeclined = "FriendInvitationDeclined",
   AddedToChatGroup = "AddedToChatGroup",
   UserStatusChanged = "UserStatusChanged",
}

export class ChatifyHubClient implements IChatClient {
   private readonly connection: HubConnection;

   constructor(connection: HubConnection) {
      this.connection = connection;
   }

   onAddedToChatGroup(callback: (event: IAddedToChatGroup) => void): void {
      this.connection.on(HubMethods.AddedToChatGroup, callback);
   }

   onChatGroupMemberLeft(
      callback: (event: IChatGroupMemberLeft) => void
   ): void {
      this.connection.on(HubMethods.ChatGroupMemberLeft, callback);
   }

   onChatGroupMemberRemoved(
      callback: (event: IChatGroupMemberRemoved) => void
   ): void {
      this.connection.on(HubMethods.ChatGroupMemberRemoved, callback);
   }

   onChatGroupMemberStartedTyping(
      callback: (event: IChatGroupMemberStartedTyping) => void
   ): void {
      this.connection.on(HubMethods.ChatGroupMemberStartedTyping, callback);
   }

   onChatGroupMemberStoppedTyping(
      callback: (event: IChatGroupMemberStoppedTyping) => void
   ): void {
      this.connection.on(HubMethods.ChatGroupMemberStoppedTyping, callback);
   }

   onChatGroupMessageEdited(
      callback: (event: IChatGroupMessageEdited) => void
   ): void {
      this.connection.on(HubMethods.ChatGroupMessageEdited, callback);
   }

   onChatGroupMessageReactedTo(
      callback: (event: IChatGroupMessageReactedTo) => void
   ): void {
      this.connection.on(HubMethods.ChatGroupMessageReactedTo, callback);
   }

   onChatGroupMessageUnReactedTo(
      callback: (event: IChatGroupMessageUnReactedTo) => void
   ): void {
      this.connection.on(HubMethods.ChatGroupMessageUnReactedTo, callback);
   }

   onChatGroupNewAdminAdded(
      callback: (event: IChatGroupNewAdminAdded) => void
   ): void {
      this.connection.on(HubMethods.ChatGroupNewAdminAdded, callback);
   }

   onFriendInvitationAccepted(
      callback: (event: IFriendInvitationAccepted) => void
   ): void {
      this.connection.on(HubMethods.FriendInvitationAccepted, callback);
   }

   onFriendInvitationDeclined(
      callback: (event: IFriendInvitationDeclined) => void
   ): void {
      this.connection.on(HubMethods.FriendInvitationDeclined, callback);
   }

   onFriendInvitationReceived(
      callback: (event: IReceiveFriendInvitation) => void
   ): void {
      this.connection.on(HubMethods.ReceiveFriendInvitation, callback);
   }

   onReceiveChatGroupMessage(
      callback: (message: IChatGroupMessage) => void
   ): void {
      this.connection.on(HubMethods.ReceiveGroupChatMessage, callback);
   }

   onUserStatusChanged(callback: (event: IUserStatusChanged) => void): void {
      this.connection.on(HubMethods.UserStatusChanged, callback);
   }

   async start(): Promise<void> {
      return this.connection.start();
   }

   get state(): HubConnectionState {
      return this.connection.state;
   }

   async stop(): Promise<void> {
      return this.connection.stop();
   }
}

export interface IChatClient {
   get state(): HubConnectionState;

   onReceiveChatGroupMessage(
      callback: (message: IChatGroupMessage) => void
   ): void;

   onChatGroupMemberStartedTyping(
      callback: (event: IChatGroupMemberStartedTyping) => void
   ): void;

   onChatGroupMemberStoppedTyping(
      callback: (event: IChatGroupMemberStoppedTyping) => void
   ): void;

   onUserStatusChanged(callback: (event: IUserStatusChanged) => void): void;

   onChatGroupMemberLeft(callback: (event: IChatGroupMemberLeft) => void): void;

   onChatGroupMemberRemoved(
      callback: (event: IChatGroupMemberRemoved) => void
   ): void;

   onChatGroupMessageEdited(
      callback: (event: IChatGroupMessageEdited) => void
   ): void;

   onChatGroupMessageReactedTo(
      callback: (event: IChatGroupMessageReactedTo) => void
   ): void;

   onChatGroupMessageUnReactedTo(
      callback: (event: IChatGroupMessageUnReactedTo) => void
   ): void;

   onFriendInvitationReceived(
      callback: (event: IReceiveFriendInvitation) => void
   ): void;

   onFriendInvitationAccepted(
      callback: (event: IFriendInvitationAccepted) => void
   ): void;

   onFriendInvitationDeclined(
      callback: (event: IFriendInvitationDeclined) => void
   ): void;

   onChatGroupNewAdminAdded(
      callback: (event: IChatGroupNewAdminAdded) => void
   ): void;

   onAddedToChatGroup(callback: (event: IAddedToChatGroup) => void): void;

   onClose?(callback: (error?: Error | undefined) => void): void;

   start(): Promise<void>;

   stop(): Promise<void>;
}
