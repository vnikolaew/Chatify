"use client";
import React from "react";
import { Button, Input, Spinner, User } from "@nextui-org/react";
import { UserIcon } from "lucide-react";
import { useUserHandle } from "@hooks";
import {
   useFindUserByHandleQuery,
   useSendFriendInviteMutation,
} from "@web/api";

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
   const {
      mutateAsync: sendFriendInvite,
      isLoading: inviteLoading,
      error: inviteError,
   } = useSendFriendInviteMutation();

   const handleClick = async () => {
      console.log(userHandle);
      await search();
   };

   const handleSendFriendInvite = async (userId: string) => {
      console.log(userId);
      await sendFriendInvite({ userId });
   };

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
                  className={`w-full mt-8 border-b-1 border-b-default-100  rounded-md pb-2 px-6 transition-background duration-100 self-center flex items-center justify-between`}
               >
                  <User
                     avatarProps={{
                        src: user.user?.profilePicture.mediaUrl,
                        size: "md",
                        color: "danger",
                        isBordered: true,
                     }}
                     classNames={{
                        name: `text-medium`,
                        description: `text-xs`,
                     }}
                     className={`gap-4`}
                     name={user.user?.displayName}
                     description={user.user?.userHandle}
                  />
                  {user?.friendsRelation ? (
                     <div className={`text-default-300 text-xs`}>
                        You are already friends.
                     </div>
                  ) : (
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
         </div>
      </section>
   );
};

export default FriendsPage;
