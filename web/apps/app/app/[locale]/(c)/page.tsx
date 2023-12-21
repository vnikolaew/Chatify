"use client";
import React from "react";
import Link from "next/link";
import { useGetChatGroupDetailsQuery } from "@web/api";
import { useIsUserLoggedIn } from "@hooks";
import { useDisclosure } from "@nextui-org/react";
import { useCurrentChatGroup } from "@hooks";
import ChatGroupTopBar from "@components/chat-group/ChatGroupTopBar";
import { redirect, useSearchParams } from "next/navigation";
import { ChatMessagesSection } from "@components/chat-group";
import Toast from "@components/common/Toast";
import { cookies } from "next/headers";

function IndexPage() {
   let { isUserLoggedIn } = useIsUserLoggedIn();
   isUserLoggedIn = isUserLoggedIn || !!cookies().has(
      process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME,
   );

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
