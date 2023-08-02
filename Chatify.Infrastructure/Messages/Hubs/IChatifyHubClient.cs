using Chatify.Infrastructure.Messages.Hubs.Models.Client;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;

namespace Chatify.Infrastructure.Messages.Hubs;

public interface IChatifyHubClient
{
    Task ReceiveMessage(string username, string message);
    Task ReceiveGroupChatMessage(ReceiveGroupChatMessage receiveGroupChatMessage);

    Task ChatGroupMemberStartedTyping(ChatGroupMemberStartedTyping chatGroupMemberStartedTyping);

    Task ChatGroupMemberStoppedTyping(ChatGroupMemberStoppedTyping chatGroupMemberStartedTyping);

    Task UserStatusChanged(UserStatusChanged userStatusChanged);

    // Task ChatGroupMemberJoined(ChatGroupMemberJoined chatGroupMemberJoined);

    Task ChatGroupMemberRemoved(ChatGroupMemberRemoved chatGroupMemberRemoved);

    Task ChatGroupMemberLeft(ChatGroupMemberLeft chatGroupMemberLeft);

    Task ChatGroupNewAdminAdded(ChatGroupNewAdminAdded chatGroupMemberLeft);

    Task ChatGroupMessageRemoved(ChatGroupMessageRemoved chatGroupMessageRemoved);

    Task ChatGroupMessageEdited(ChatGroupMessageEdited chatGroupMessageEdited);

    Task ChatGroupMessageReactedTo(ChatGroupMessageReactedTo chatGroupMessageReactedTo);

    Task ChatGroupMessageUnReactedTo(ChatGroupMessageUnReactedTo chatGroupMessageUnReactedTo);

    Task ReceiveFriendInvitation(ReceiveFriendInvitation receiveFriendInvitation);

    Task FriendInvitationAccepted(FriendInvitationAccepted friendInvitationAccepted);

    Task FriendInvitationDeclined(FriendInvitationDeclined friendInvitationDeclined);

    Task AddedToChatGroup(AddedToChatGroup addedToChatGroup);
}