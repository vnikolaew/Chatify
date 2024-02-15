"use client";
import React from "react";
import Link from "next/link";
import { useGetChatGroupDetailsQuery } from "@web/api";
import { useDisclosure } from "@nextui-org/react";
import { useSearchParams } from "next/navigation";
import { ChatGroupTopBar, ChatMessagesSection, Toast } from "@web/components";
import { useCurrentChatGroup, useIsUserLoggedIn } from "@web/hooks";

function IndexPage() {
   let { isUserLoggedIn } = useIsUserLoggedIn();
   // if (!isUserLoggedIn) return redirect(`/signin`);

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
         className={`mb-6 flex min-h-[60vh] w-full flex-col items-center rounded-md text-xl shadow-gray-300`}
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
               className={`mt-4 flex w-full flex-col items-center self-center`}
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
      <div className={`flex items-center justify-center gap-8`}>
         <Link className={`text-blue-600 hover:underline`} href={`/signup`}>
            {" "}
            Sign Up
         </Link>
         <Link className={`text-blue-600 hover:underline`} href={`/signin`}>
            Sign In
         </Link>
      </div>
   </div>
);

export default IndexPage;
