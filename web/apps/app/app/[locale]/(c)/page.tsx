"use client";
import React, { useEffect } from "react";
import Link from "next/link";
import { useGetChatGroupDetailsQuery, useGetMyClaimsQuery } from "@web/api";
import { useIsUserLoggedIn } from "@hooks";
import { useDisclosure } from "@nextui-org/react";
import { useCurrentChatGroup } from "@hooks";
import ChatGroupTopBar from "@components/chat-group/ChatGroupTopBar";
import { useSearchParams } from "next/navigation";
import { ChatMessagesSection } from "@components/chat-group";
import Toast from "@components/common/Toast";

function IndexPage() {
   const params = useSearchParams();
   const isNew = params.get("new") === "true";
   const { data: me } = useGetMyClaimsQuery();

   const {
      isOpen: isSuccessModalOpen,
      onOpenChange: onSuccessModalOpenChange,
   } = useDisclosure({ defaultOpen: true });

   const chatGroupId = useCurrentChatGroup();
   const { isUserLoggedIn } = useIsUserLoggedIn();
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
