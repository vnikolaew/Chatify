"use client";
import React from "react";
import Link from "next/link";
import { useGetChatGroupDetailsQuery } from "@web/api";
import { useDisclosure } from "@nextui-org/react";
import { redirect, useSearchParams } from "next/navigation";
import { ChatGroupTopBar, ChatMessagesSection, Toast } from "@web/components";
import { useCurrentChatGroup, useIsUserLoggedIn } from "@web/hooks";

function IndexPage() {
   let { isUserLoggedIn } = useIsUserLoggedIn();
   console.log({ isUserLoggedIn });
   if (!isUserLoggedIn) return redirect(`/signin`);

   const params = useSearchParams();
   const isNew = params.get("new") === "true";

   const {
      isOpen: isSuccessModalOpen,
      onOpenChange: onSuccessModalOpenChange,
   } = useDisclosure({ defaultOpen: true });

   const chatGroupId = useCurrentChatGroup();

   const {
      data: chatGroupDetails,
      error,
      isLoading,
   } = useGetChatGroupDetailsQuery(chatGroupId, {
      enabled: !!chatGroupId && isUserLoggedIn,
   });

   return (
      <div
         className={`flex min-h-[60vh] text-xl flex-col items-center shadow-gray-300 w-full rounded-md mb-6`}
      >
         <ChatGroupTopBar />
         {isNew && chatGroupDetails && (
            <Toast
               className={`bg-success-300`}
               autoFocus={false}
               header={"Success!"}
               isOpen={isSuccessModalOpen}
               onOpenChange={onSuccessModalOpenChange}
            >
               <p>
                  You successfully created{" "}
                  <strong className={`text-small mr-1`}>
                     {chatGroupDetails?.chatGroup?.name}
                  </strong>
                  chat group.
               </p>
            </Toast>
         )}
         {!isUserLoggedIn ? (
            <UserNotLoggedInSection />
         ) : (
            <div
               className={`self-center w-full flex flex-col items-center mt-4`}
            >
               <ChatMessagesSection groupId={chatGroupId} />
            </div>
         )}
      </div>
   );
}

const UserNotLoggedInSection = () => (
   <div className={`mt-12`}>
      <h2 className={`font-bold`}>You are currently not logged in.</h2>
      <div className={`flex justify-center items-center gap-8`}>
         <Link className={`hover:underline text-blue-600`} href={`/signup`}>
            {" "}
            Sign Up
         </Link>
         <Link className={`hover:underline text-blue-600`} href={`/signin`}>
            Sign In
         </Link>
      </div>
   </div>
);

export default IndexPage;
