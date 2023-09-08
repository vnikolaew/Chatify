"use client";
import React, { useMemo } from "react";
import {
   Avatar,
   AvatarProps,
   Badge,
   Button,
   Input,
   Link,
   Spinner,
} from "@nextui-org/react";
import { AlertCircle, UserIcon } from "lucide-react";
import { useUserHandle } from "@hooks";
import {
   useAcceptFriendInviteMutation,
   useDeclineFriendInviteMutation,
   useFindUserByHandleQuery,
   useSendFriendInviteMutation,
} from "@web/api";
import { FriendInvitationStatus, UserStatus } from "@openapi";
import moment from "moment";

export interface PageProps {}

const FriendsPage = ({}: PageProps) => {
   const { validationState, userHandle, setUserHandle, errorMessage } =
      useUserHandle();
   const {
      isLoading,
      refetch: search,
      error,
      isFetching,
      data: user,
   } = useFindUserByHandleQuery(userHandle, { enabled: false });
   const noUserFound = useMemo(
      () => !isLoading && !isFetching && !user,
      [isLoading, isFetching, user]
   );

   const {
      mutateAsync: sendFriendInvite,
      isLoading: inviteLoading,
      error: inviteError,
   } = useSendFriendInviteMutation();

   const statusColor = useMemo<AvatarProps["color"]>(() => {
      if (!user) return "default";
      switch (user.user.status) {
         case UserStatus.AWAY:
            return "warning";
         case UserStatus.ONLINE:
            return "success";
         case UserStatus.OFFLINE:
            return "default";
      }
      return "default";
   }, [user]);

   const handleClick = async () => {
      await search();
   };

   const handleSendFriendInvite = async (userId: string) => {
      console.log(userId);
      await sendFriendInvite({ userId });
   };

   console.log(user);
   // @ts-ignore
   return (
      <section
         className={`w-full min-h-[60vh] mt-12 flex flex-col items-center`}
      >
         <div className="flex flex-col w-1/3 gap-2">
            <h1 className={`text-2xl text-foreground`}>Find new friends</h1>
            <h2 className={`text-default-400 font-normal text-small`}>
               You can add a new Chatify friend with their handle.
            </h2>
            <div className={`w-full mt-2 flex flex-col items-center gap-1`}>
               <Input
                  value={userHandle}
                  onValueChange={setUserHandle}
                  placeholder={`User#0000`}
                  // @ts-ignore
                  validationState={validationState}
                  errorMessage={errorMessage}
                  startContent={
                     <UserIcon className={`fill-foreground`} size={16} />
                  }
                  variant={"flat"}
                  description={
                     "Use a handle to find the user you are searching for."
                  }
                  classNames={{
                     input: `pl-3`,
                  }}
                  className={`w-full shadow-md`}
                  size={"md"}
                  color={"default"}
                  isClearable
                  type={"text"}
               />
               <Button
                  isLoading={isLoading && isFetching}
                  onPress={handleClick}
                  spinner={<Spinner color={`white`} size={`sm`} />}
                  isDisabled={!!errorMessage || userHandle.length === 0}
                  className={`self-end px-6 disabled:cursor-not-allowed ${
                     userHandle.length === 0 && `hover:cursor-not-allowed`
                  }`}
                  disabled={userHandle.length === 0}
                  variant={"shadow"}
                  color={"primary"}
                  size={"md"}
               >
                  {isLoading && isFetching ? "Searching ..." : "Find users"}
               </Button>
            </div>
            {user && (
               <div
                  className={`w-full mt-12 border-b-1 border-b-default-100  rounded-md pb-2 px-6 transition-background duration-100 self-center flex items-center justify-between`}
               >
                  <div className={` flex items-center gap-4`}>
                     <Badge
                        content={""}
                        shape={"circle"}
                        placement={"bottom-right"}
                        variant={"solid"}
                        color={statusColor}
                        size={`sm`}
                     >
                        <Avatar
                           src={user.user?.profilePicture.mediaUrl}
                           color={"danger"}
                           size={`md`}
                           isBordered={true}
                        />
                     </Badge>
                     <div
                        className={`flex flex-col items-start justify-evenly gap-1 `}
                     >
                        <h2 className={`text-foreground text-small`}>
                           {user.user?.displayName}
                        </h2>
                        <h3 className={`text-default-300 text-xs`}>
                           {user.user?.userHandle}{" "}
                           {user.friendInvitation?.status ===
                              FriendInvitationStatus.PENDING &&
                              ` - Sent you an invite ${moment(
                                 new Date(user.friendInvitation.createdAt)
                              ).fromNow()}`}
                           {user.friendsRelation &&
                              ` - Friends since ${moment(
                                 new Date(user.friendsRelation.createdAt)
                              ).format("DD/MM/YYYY")}`}
                        </h3>
                     </div>
                  </div>
                  {user?.friendInvitation &&
                     user.friendInvitation?.status ===
                        FriendInvitationStatus.PENDING && (
                        <FriendInvitationSection
                           userId={user?.user.id}
                           inviteId={user.friendInvitation?.id}
                        />
                     )}
                  {user?.friendsRelation && (
                     <Button
                        as={Link}
                        href={`/?c=${user?.friendsRelation.groupId}`}
                        size={"sm"}
                        className={`px-6`}
                        variant={`flat`}
                        color={`warning`}
                     >
                        Go to DM
                     </Button>
                  )}
                  {!user?.friendsRelation && !user.friendInvitation && (
                     <Button
                        onPress={async (_) =>
                           await handleSendFriendInvite(user.user?.id)
                        }
                        isLoading={inviteLoading}
                        spinner={<Spinner color={`white`} size={`sm`} />}
                        isDisabled={inviteLoading}
                        size={"sm"}
                        variant={`flat`}
                        color={`danger`}
                     >
                        {inviteLoading ? "Loading ..." : "Send request"}
                     </Button>
                  )}
               </div>
            )}
            {noUserFound && (
               <div
                  className={`w-full gap-2 flex items-center justify-center text-center mt-8`}
               >
                  <AlertCircle
                     className={`stroke-default-400`}
                     size={28}
                     // color={"white"}
                  />
                  <span className={`text-default-400`}>
                     No user was found with the given handle.
                  </span>
               </div>
            )}
         </div>
      </section>
   );
};

interface FriendInvitationSectionProps {
   inviteId: string;
   userId: string;
}

const FriendInvitationSection = ({
   inviteId,
   userId,
}: FriendInvitationSectionProps) => {
   const {
      mutateAsync: acceptInvite,
      isLoading: acceptLoading,
      error: acceptError,
   } = useAcceptFriendInviteMutation();
   const {
      mutateAsync: declineInvite,
      isLoading,
      error,
   } = useDeclineFriendInviteMutation();

   return (
      <div className={`flex items-center gap-2`}>
         <Button
            onPress={async (_) => await acceptInvite({ inviteId, userId })}
            isLoading={acceptLoading}
            spinner={<Spinner size={`sm`} color={`white`} />}
            isDisabled={acceptLoading}
            className={`px-8 py-0`}
            size={`sm`}
            radius={"md"}
            variant={"light"}
            color={"success"}
         >
            Accept
         </Button>
         <Button
            onPress={async (_) => await declineInvite({ inviteId })}
            isLoading={isLoading}
            isDisabled={isLoading}
            radius={"md"}
            spinner={<Spinner size={`sm`} color={`white`} />}
            size={`sm`}
            className={`px-8 py-0`}
            variant={"light"}
            color={"danger"}
         >
            Decline
         </Button>
      </div>
   );
};

export default FriendsPage;
